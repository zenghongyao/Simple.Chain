using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Chain
{
    public class TransactionEvent
    {
        /// <summary>
        /// 交易哈希
        /// </summary>
        public string TransactionHash { get; set; }
        /// <summary>
        /// 区块高度
        /// </summary>
        public long Block { get; set; }
        /// <summary>
        /// 转出地址
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 转入地址
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public BigInteger Value { get; set; }
    }
}
