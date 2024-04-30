using Newtonsoft.Json;

namespace Simple.Chain
{
    /// <summary>
    /// 账户信息
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 私钥
        /// </summary>
        public string PrviateKey { get; set; }

        /// <summary>
        /// gas
        /// </summary>
        public decimal Gas { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public long create_time { get; set; }

        public long Energy { get; set; }
        public long FreeNetUsed { get; set; }

    }
}
