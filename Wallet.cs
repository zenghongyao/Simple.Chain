using Nethereum.ABI;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

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
                request = new RestRequest(requesturi, method);
            }
            else
            {
                Uri uri = new Uri(proxyUrl);
                client = new RestClient($"{uri.Scheme}://{uri.Host}");
                request = new RestRequest($"{uri.PathAndQuery}{baseUrl}{requesturi}", Method.Post);
            }
            return client;
        }

        private const string CREATE_ACCOUNT = "/wallet/createaccount";
        /// <summary>
        /// 创建一个账号
        /// </summary>
        /// <param name="owner_address"></param>
        /// <param name="account_address"></param>
        /// <returns></returns>
        /// <exception cref="TronException"></exception>
        public string CreateAccount(string owner_address, string account_address)
        {
            RestClient client = CreateClient(CREATE_ACCOUNT, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"owner_address",owner_address },
                {"account_address",account_address },
                {"visible",true }
            };
            request.AddParameter("application/json", JsonConvert.SerializeObject(parameters), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            if (!response.IsSuccessful) throw new TronException(response);
            //            {
            //                "visible":true,
            //    "txID":"6c2231ba18fff2189b0044138ebe6dac1ed28e74f0742a929aabb48f64c5da0a",
            //    "raw_data":{
            //                    "contract":[
            //                        {
            //                        "parameter":{
            //                            "value":{
            //                                "owner_address":"TR1sVFGgpuryLDE6YvsxStzzdTcEL4bTnm",
            //                        "account_address":"TZ8HBcH8rz3iFGfwwJATYQaPftf7jTg9qE"
            //                            },
            //                    "type_url":"type.googleapis.com/protocol.AccountCreateContract"
            //                        },
            //                "type":"AccountCreateContract"
            //                        }
            //        ],
            //        "ref_block_bytes":"1906",
            //        "ref_block_hash":"23f1314e28130ea2",
            //        "expiration":1667046669000,
            //        "timestamp":1667046609978
            //    },
            //    "raw_data_hex":"0a021906220823f1314e28130ea240c8f5e59dc2305a6612640a32747970652e676f6f676c65617069732e636f6d2f70726f746f636f6c2e4163636f756e74437265617465436f6e7472616374122e0a1541a50ad85d4465b8095c5f60c082d476da2630bbaa121541fe0220a7773a9ef5aa0975f07a23990ccc51f41c70baa8e29dc230"
            //}
            return response.Content;
        }

        /// <summary>
        /// 随机生成钱包
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public string Generate(out string privateKey)
        {
            EthECKey ecKey = EthECKey.GenerateKey();
            privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            return ecKey.GetPublicAddress().ToLower();
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
        /// <param name="privateKey"></param>
        /// <param name="to_address"></param>
        /// <param name="amount"></param>
        public string Transaction(string privateKey, string owner_address, string to_address, decimal amount)
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
            Transaction transaction = response.Content;
            //签名
            transaction = this.GetTransactionSign(transaction, privateKey);
            //广播
            this.BroadcastTransaction(transaction, out string hex);
            return hex;
        }
        /// <summary>
        /// 合约
        /// </summary>
        /// <param name="owner_address"></param>
        /// <param name="to_address"></param>
        /// <param name="contract_address"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public string Transaction(string privateKey, string owner_address, string to_address, string contract_address, decimal amount)
        {
            ABIEncode abiEncode = new();
            string encodedHex = abiEncode.GetABIParamsEncoded(new TransferFunction() { To = to_address.ToHexAddress(false), Value = amount.ToBigNumber() }).ToHex();
            Dictionary<string, object> parameters = new()
            {
                {"owner_address",owner_address },
                {"contract_address",contract_address },
                {"function_selector","transfer(address,uint256)" },
                {"fee_limit",50000000 },
                {"parameter",encodedHex },
                {"visible",true },
            };
            //执行合约
            Transaction transaction = this.TriggerSmartContract(parameters);
            //签名
            transaction = this.GetTransactionSign(transaction, privateKey);
            //广播
            this.BroadcastTransaction(transaction, out string hex);
            return hex;
        }

        private const string TRIGGER_SMART_CONTRACT = "/wallet/triggersmartcontract";
        /// <summary>
        /// 执行合约
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
        public bool BroadcastTransaction(Transaction transaction, out string hex)
        {
            RestClient client = CreateClient(BROADCAST_TRANSACTION, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(transaction), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            JObject obj = JObject.Parse(response.Content);
            bool success = obj.ContainsKey("result") ? obj.Value<bool>("result") : false;
            if (!success) throw new TronException(response.Content);
            hex = obj.Value<string>("txid");
            return success;
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
        private const string GETTRANSACTIONINFOBYID = "/wallet/gettransactioninfobyid";
        /// <summary>
        /// 获取交易信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TransactionInfo GetTransactionInfo(string value)
        {
            RestClient client = CreateClient(GETTRANSACTIONINFOBYID, Method.Post, out RestRequest request);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(new { value }), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<TransactionInfo>(response.Content);
        }
    }
}
