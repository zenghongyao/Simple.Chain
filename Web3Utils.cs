using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Chain
{
    public static class Web3Utils
    {
        public static Dictionary<string, string> MethodID = new Dictionary<string, string>
        {
            {"transfer","a9059cbb" },
            {"transferFrom","23b872dd" }
        };
        public static string GetMethodID(string eventname)
        {
            eventname = eventname.ToLower();
            if (!MethodID.ContainsKey(eventname)) return string.Empty;
            return MethodID[eventname];

        }

        /// <summary>
        /// 获取合约参数数据
        /// </summary>
        /// <returns></returns>
        public static string GetContractParameter(string data, out string to_address, out BigInteger amount)
        {
            //23b872dd transferFrom
            //a9059cbb transfer
            if (data.StartsWith("0x"))
            {
                data = data[2..];
            }
            string from_address = string.Empty;
            to_address = string.Empty;
            amount = 0;
            if (data.Length == 8) return from_address;
            if (data.StartsWith("23b872dd"))
            {
                TransferFromFunction transfer = new TransferFromFunction().DecodeInput(data);
                from_address = transfer.From;
                to_address = transfer.To;
                amount = transfer.Value;
            }
            else if (data.StartsWith("a9059cbb"))
            {
                TransferFunction transfer = new TransferFunction().DecodeInput(data);
                to_address = transfer.To;
                amount = transfer.Value;
            }
            return from_address;
        }
    }
}
