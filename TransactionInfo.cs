using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tron
{
    public class TransactionInfo
    {
        public string id { get; set; }
        public int fee { get; set; }
        public decimal Gas
        {
            get
            {
                return this.fee.ToNumber();
            }
        }
        public int blockNumber { get; set; }

        public long blockTimeStamp { get; set; }

        public string[] contractResult { get; set; }
        public string contract_address { get; set; }
        public TransactionInfo_Receipt receipt { get; set; }
        public TransactionInfo_Log[] log { get; set; }

        /// <summary>
        /// 转出地址
        /// </summary>
        public string From_Address
        {
            get
            {
                return log[0].topics[1][24..].ToBase58Address();
            }
        }
        /// <summary>
        /// 转入地址
        /// </summary>
        public string To_Address
        {
            get
            {
                return log[0].topics[2][24..].ToBase58Address();
            }
        }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount
        {
            get
            {
                string hex = log[0].topics[1] + log[0].data;
                TransferFunction transaction = new TransferFunction().DecodeInput(hex);
                return transaction.Value.ToNumber();
            }
        }
        /// <summary>
        /// 是否交易成功
        /// </summary>
        public bool Success
        {
            get
            {
                return this.receipt.result == "SUCCESS";
            }
        }
    }

    public class TransactionInfo_Receipt
    {
        public int energy_fee { get; set; }
        public int energy_usage_total { get; set; }
        public int net_usage { get; set; }
        public string result { get; set; }
    }

    public class TransactionInfo_Log
    {
        public string address { get; set; }
        public string[] topics { get; set; }
        public string data { get; set; }
    }
}
