using Google.Protobuf;
using Newtonsoft.Json.Linq;
using Simple.Chain.Crypto;

namespace Simple.Chain.Tron
{
    internal class TronTransaction
    {
        public bool visible { get; set; }
        public string txID { get; set; }
        public JToken raw_data { get; set; }
        public string raw_data_hex { get; set; }
        public string[] signature { get; set; }

        public JArray ret { get; set; }

        public bool IsSuccess()
        {
            foreach (var item in ret)
            {
                return item.Value<string>("contractRet") == "SUCCESS";
            }
            return false;
        }



        public static implicit operator TronTransaction(string jsonString)
        {
            TronTransaction response = new TronTransaction();
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
