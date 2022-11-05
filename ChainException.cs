using RestSharp;

namespace Simple.Chain
{
    public class ChainException : Exception
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 交易哈希
        /// </summary>
        public string Hash { get; set; }

        public ChainException(string message) : base(message)
        {

        }

        public ChainException(string message, string address) : base(message)
        {
            this.Address = address;
        }

        public ChainException(string message, string address, string hash) : base(message)
        {
            this.Address = address;
            this.Hash = hash;
        }
        public override string Message => string.IsNullOrWhiteSpace(Address) ? $"{base.Message}，哈希:{Hash}" : $"{base.Message}，地址：{Address}，哈希:{Hash}";
    }
}
