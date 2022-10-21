using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tron
{
    /// <summary>
    /// 转账哈希内容
    /// </summary>
    public class TransactionBlockInfo
    {
        /// <summary>
        /// 区块
        /// </summary>
        public int block { get; set; }
        /// <summary>
        /// 交易时间戳
        /// </summary>
        public long timestamp { get; set; }
        /// <summary>
        /// 合约钱包
        /// </summary>
        public string contract { get; set; }
        /// <summary>
        /// 动作名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 交易哈希
        /// </summary>
        public string transaction { get; set; }
        /// <summary>
        /// 内容包含：
        /// from
        /// to
        /// value
        /// </summary>
        public Dictionary<string, string> result { get; set; }
        /// <summary>
        /// 资源节点
        /// </summary>
        public string resourceNode { get; set; }
        /// <summary>
        /// 未经证实
        /// </summary>
        public bool unconfirmed { get; set; }
    }
}
