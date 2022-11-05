using Newtonsoft.Json;

namespace Simple.Chain
{
    /// <summary>
    /// 交易记录
    /// </summary>
    public class TransactionInfo
    {
        /// <summary>
        /// 交易哈希
        /// </summary>
        public string TransactionHash { get; set; }
        /// <summary>
        /// from
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// to
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// 合约地址
        /// </summary>
        public string ContractAddress { get; set; }
        /// <summary>
        /// gas
        /// </summary>
        public decimal Gas { get; set; }
        /// <summary>
        /// gas limit
        /// </summary>
        public decimal GasPrice { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Value { get; set; }
        /// <summary>
        /// 区块高度
        /// </summary>
        public int BlockNumber { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }
}
