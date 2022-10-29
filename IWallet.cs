using Nethereum.RPC.Eth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tron
{
    public interface IWallet
    {
        Task<TransactionReceipt> Transfer(string privateKey, string to_address, string contractAddress, decimal amount);
        Task<decimal> GetBalance(string privateKey, string contractAddress);
    }
}
