using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Simple.Tron.ABI;
using Simple.Tron.ABI.FunctionEncoding;
using Simple.Tron.Contracts;
using Simple.Tron.Crypto;
using System.Net.Http.Headers;

namespace Simple.Tron
{
    /// <summary>
    /// 钱包相关
    /// </summary>
    public class Wallet : TronService
    {
        public Wallet() : this(DEFAULT_BASE_URL)
        {

        }
        public Wallet(string baseUrl) : base(baseUrl)
        {

        }

        private const string CREATE_ACCOUNT = "/wallet/createaccount";
        public Transaction CreateAccount(string owner_address, string account_address)
        {
            var client = new RestClient(baseUrl);
            RestRequest request = new RestRequest(CREATE_ACCOUNT, Method.Post);
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
            var client = new RestClient(baseUrl);
            var request = new RestRequest(GET_ACCOUNT, Method.Post);
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
        /// 创建一笔转账 TRX 的 Transaction.
        /// </summary>
        /// <param name="owner_address"></param>
        /// <param name="to_address"></param>
        /// <param name="amount"></param>
        public Transaction CreateTransaction(string owner_address, string to_address, long amount)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"owner_address",owner_address },
                {"to_address",to_address },
                {"amount",amount },
                {"visible",true }
            };
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseUrl + CREATE_TRANSACTION),
                Headers =
                {
                    { "accept", "application/json" },
                },
                Content = new StringContent(JsonConvert.SerializeObject(parameters))
                {
                    Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
                }
            };
            using (var response = client.Send(request))
            {
                response.EnsureSuccessStatusCode();
                var content = response.Content.ReadAsStringAsync().Result;
                return content;
            }
        }

        private const string TRIGGER_SMART_CONTRACT = "/wallet/triggersmartcontract";
        /// <summary>
        /// 合约
        /// </summary>
        /// <param name="owner_address"></param>
        /// <param name="to_address"></param>
        /// <param name="contract_address"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Transaction TriggerSmartContract(string owner_address, string to_address, string contract_address, decimal amount)
        {
            var functionABI = ABITypedRegistry.GetFunctionABI<TransferFunction>();
            byte[] callerAddressBytes = Base58Encoder.DecodeFromBase58Check(to_address);
            var toAddressBytes = new byte[20];
            Array.Copy(callerAddressBytes, 1, toAddressBytes, 0, toAddressBytes.Length);

            var toAddressHex = "0x" + toAddressBytes.ToHex();

            var trc20Transfer = new TransferFunction
            {
                To = toAddressHex,
                TokenAmount = Convert.ToInt64(amount / 0.000001M),
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
            var client = new RestClient(baseUrl);
            var request = new RestRequest(TRIGGER_SMART_CONTRACT, Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(parameters), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            JObject obj = JObject.Parse(response.Content);
            return obj.Value<JToken>("transaction").ToString();
        }
        /// <summary>
        /// 获取签名（此方法调用测试环境）
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public Transaction GetTransactionSign(Transaction transaction, string privateKey)
        {
            var client = new RestClient("http://47.243.127.195:9090");
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
            var client = new RestClient(baseUrl);
            var request = new RestRequest(BROADCAST_TRANSACTION, Method.Post);
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
            var client = new RestClient(baseUrl);
            var request = new RestRequest(GET_TRANSACTION_BY_ID, Method.Post);
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
            var client = new RestClient(baseUrl);
            var request = new RestRequest(GET_TRANSACTION_INFOBYID, Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(new { value }), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            return JsonConvert.DeserializeObject<TransactionInfo>(response.Content);
        }
    }
}
