using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Simple.Tron
{
    public class Transaction
    {
        public bool visible { get; set; }
        public string txID { get; set; }
        public object raw_data { get; set; }
        public string raw_data_hex { get; set; }
        public string[] signature { get; set; }


        public static implicit operator Transaction(string jsonString)
        {
            Transaction response = new Transaction();
            JObject obj = JObject.Parse(jsonString);
            response.visible = obj.Value<bool>("visible");
            response.txID = obj.Value<string>("txID");
            response.raw_data_hex = obj.Value<string>("raw_data_hex");
            response.raw_data = JsonConvert.DeserializeObject<object>(obj.Value<JToken>("raw_data").ToString());
            if (obj.ContainsKey("signature"))
            {
                response.signature = obj.Value<JArray>("signature").Select(c => c.Value<string>()).ToArray();
            }
            return response;
        }
    }
}
