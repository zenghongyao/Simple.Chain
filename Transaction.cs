using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Simple.Tron
{
    public class Transaction
    {
        public bool visible { get; set; }
        public string txID { get; set; }
        public JToken raw_data { get; set; }
        public string raw_data_hex { get; set; }
        public string[] signature { get; set; }

        public JArray ret { get; set; }

        public bool IsSuccess()
        {
            foreach (var item in this.ret)
            {
                return item.Value<string>("contractRet") == "SUCCESS";
            }
            return false;
        }

        /// <summary>
        /// 获取合约参数数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TransferFunction> GetContractParameterData()
        {
            if (this.raw_data != null)
            {
                foreach (JToken item in this.raw_data["contract"])
                {
                    JToken parameter = item.Value<JToken>("parameter").Value<JToken>("value");
                    yield return new TransferFunction().DecodeInput(parameter.Value<string>("data"));
                }
            }
        }

        public static implicit operator Transaction(string jsonString)
        {
            Transaction response = new Transaction();
            JObject obj = JObject.Parse(jsonString);
            response.visible = obj.Value<bool>("visible");
            response.txID = obj.Value<string>("txID");
            response.raw_data_hex = obj.Value<string>("raw_data_hex");
            response.raw_data = obj.Value<JToken>("raw_data");
            if (obj.ContainsKey("signature"))
            {
                response.signature = obj.Value<JArray>("signature").Select(c => c.Value<string>()).ToArray();
            }
            if (obj.ContainsKey("ret"))
            {
                response.ret = obj.Value<JArray>("ret");
            }
            return response;
        }

    }
}
