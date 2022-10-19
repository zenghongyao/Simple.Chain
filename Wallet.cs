using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Simple.Tron.ABI;
using Simple.Tron.ABI.FunctionEncoding;
using Simple.Tron.Contracts;
using Simple.Tron.Crypto;

namespace Simple.Tron
{
    /// <summary>
    /// 钱包相关
    /// </summary>
    public class Wallet
    {
        /// <summary>
        /// 默认主网
        /// </summary>
        private const string DEFAULT_MAINNET = "https://api.trongrid.io";
        /// <summary>
        /// 默认测试网络
        /// </summary>
        private const string DEFAULT_TESTNET = "https://api.shasta.trongrid.io";

        private readonly string baseUrl;
        private readonly string testUrl;
        private readonly string? proxyUrl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainnet">主网</param>
        /// <param name="testnet">测试（建议自行部署）</param>
        /// <param name="proxy">代理地址</param>
        public Wallet(string mainnet = DEFAULT_MAINNET, string testnet = DEFAULT_TESTNET, string? proxy = null)
        {
            this.baseUrl = mainnet;
            this.testUrl = testnet;
            this.proxyUrl = proxy;
        }

        /// <summary>
        /// 创建请求客户端
        /// </summary>
        /// <param name="requesturi"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private RestClient CreateClient(string requesturi, Method method, out RestRequest request)
        {
            RestClient client;
            if (string.IsNullOrWhiteSpace(proxyUrl))
            {
                client = new RestClient(baseUrl);
                request = new RestRequest(GET_ACCOUNT, Method.Post);
            }
            else
            {
                Uri uri = new Uri(proxyUrl);
                Console.WriteLine($"{uri.Scheme}://{uri.Host}");
                Console.WriteLine($"{uri.PathAndQuery}{baseUrl}{requesturi}");
                client = new RestClient($"{uri.Scheme}://{uri.Host}");
                request = new RestRequest($"{uri.PathAndQuery}{baseUrl}{requesturi}", Method.Post);
            }
            return client;
        }

        private const string CREATE_ACCOUNT = "/wallet/createaccount";
        public Transaction CreateAccount(string owner_address, string account_address)
        {
            RestClient client = CreateClient(CREATE_ACCOUNT, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"owner_address",owner_address },
                {"account_address",account_address },
            };
            request.AddParameter("application/json", JsonConvert.SerializeObject(parameters), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            if (!response.IsSuccessful) throw new TronException(response);
            return response.Content;
        }

        private const string GET_ACCOUNT = "/wallet/getaccount";
        /// <summary>
        /// 查询一个账号的信息, 包括余额, TRC10 余额, 质押资源, 权限等.
        /// </summary>
        /// <param name="address"></param>
        public string GetAccount(string address)
        {
            RestClient client = CreateClient(GET_ACCOUNT, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"address",address },
                {"visible",true }
            };
            request.AddParameter("application/json", JsonConvert.SerializeObject(parameters), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            if (!response.IsSuccessful) throw new TronException(response);
            return response.Content;
        }

        private const string CREATE_TRANSACTION = "/wallet/createtransaction";

        /// <summary>
        /// TRX转账
        /// </summary>
        /// <param name="owner_address"></param>
        /// <param name="to_address"></param>
        /// <param name="amount"></param>
        public Transaction Transaction(string owner_address, string to_address, decimal amount)
        {
            RestClient client = CreateClient(CREATE_TRANSACTION, Method.Post, out RestRequest request);
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"owner_address",owner_address },
                {"to_address",to_address },
                {"amount",amount.ToBigNumber() },
                {"visible",true }
            };
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(parameters), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            if (!response.IsSuccessful) throw new TronException(response);

            return response.Content;
        }
        /// <summary>
        /// 合约
        /// </summary>
        /// <param name="owner_address"></param>
        /// <param name="to_address"></param>
        /// <param name="contract_address"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Transaction Transaction(string owner_address, string to_address, string contract_address, decimal amount)
        {
            var functionABI = ABITypedRegistry.GetFunctionABI<TransferFunction>();
            byte[] callerAddressBytes = Base58Encoder.DecodeFromBase58Check(to_address);
            var toAddressBytes = new byte[20];
            Array.Copy(callerAddressBytes, 1, toAddressBytes, 0, toAddressBytes.Length);

            var toAddressHex = "0x" + toAddressBytes.ToHex();

            var trc20Transfer = new TransferFunction
            {
                To = toAddressHex,
                TokenAmount = amount.ToBigNumber(),
            };

            var encodedHex = new FunctionCallEncoder().EncodeRequest(trc20Transfer, functionABI.Sha3Signature);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"owner_address",owner_address },
                {"contract_address",contract_address },
                {"function_selector","transfer(address,uint256)" },
                {"fee_limit",1000000000 },
                {"parameter",encodedHex },
                {"visible",true },
            };
            //执行合约
            Transaction transaction = this.TriggerSmartContract(parameters);
            //广播
            this.BroadcastTransaction(transaction);
            return transaction;
        }

        private const string TRIGGER_SMART_CONTRACT = "/wallet/triggersmartcontract";
        /// <summary>
        /// 合约
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Transaction TriggerSmartContract(Dictionary<string, object> parameters)
        {
            RestClient client = CreateClient(TRIGGER_SMART_CONTRACT, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(parameters), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            JObject obj = JObject.Parse(response.Content);
            return obj.Value<JToken>("transaction").ToString();
        }
        /// <summary>
        /// 获取签名（此方法调用测试环境）
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private Transaction GetTransactionSign(Transaction transaction, string privateKey)
        {
            var client = new RestClient(testUrl);
            var request = new RestRequest("/wallet/gettransactionsign", Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(new
            {
                transaction,
                privateKey
            }), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            return response.Content;
        }

        private const string BROADCAST_TRANSACTION = "/wallet/broadcasttransaction";
        /// <summary>
        /// 广播
        /// </summary>
        /// <param name="transaction"></param>
        public void BroadcastTransaction(Transaction transaction)
        {
            RestClient client = CreateClient(BROADCAST_TRANSACTION, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(transaction), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
        }
        private const string GET_TRANSACTION_BY_ID = "/wallet/gettransactionbyid";
        /// <summary>
        /// 按交易哈希查询交易
        /// </summary>
        /// <param name="value"></param>
        public Transaction GetTransactionById(string value)
        {
            RestClient client = CreateClient(GET_TRANSACTION_BY_ID, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(new { value }), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            return response.Content;
        }

        private const string GET_TRANSACTION_INFOBYID = "/wallet/gettransactioninfobyid";
        /// <summary>
        /// 按交易哈希查询交易
        /// </summary>
        /// <param name="value"></param>
        public TransactionInfo GetTransactionInfoById(string value)
        {
            RestClient client = CreateClient(GET_TRANSACTION_INFOBYID, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(new { value }), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            return JsonConvert.DeserializeObject<TransactionInfo>(response.Content);
        }
    }
}
