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
        public int blockNumber { get; set; }
        public long blockTimeStamp { get; set; }
        public string[] contractResult { get; set; }
        public string contract_address { get; set; }
        public Receipt receipt { get; set; }
        public List<Log> log { get; set; }
    }

    public class Receipt
    {
        public int energy_fee { get; set; }
        public int energy_usage_total { get; set; }
        public int net_fee { get; set; }
        public string result { get; set; }
    }

    public class Log
    {
        public string address { get; set; }
        public string[] topics { get; set; }
        public string data { get; set; }
    }
}
