using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tron
{
    public class ETHWallet
    {

        public readonly string _mainnet;
        public ETHWallet(string mainnet)
        {
            this._mainnet = mainnet;
        }
        /// <summary>
        /// 转账
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="to_address"></param>
        /// <param name="contractAddress">合约地址</param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Task<TransactionReceipt> Transfer(string privateKey, string to_address, string contractAddress, decimal amount)
        {
            Account account = new(privateKey);
            Web3 web3 = new(account, _mainnet);
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = to_address,
                Value = Web3.Convert.ToWei(amount)
            };
            return transferHandler.SendRequestAndWaitForReceiptAsync(contractAddress, transfer);
        }

        /// <summary>
        /// 获取账户合约余额
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="contractAddress"></param>
        /// <returns></returns>
        public async Task<decimal> GetBalance(string privateKey, string contractAddress)
        {
            Account account = new(privateKey);
            Web3 web3 = new Web3(account, _mainnet);
            var balanceOf = new BalanceOfFunction()
            {
                Owner = account.Address,
            };
            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = await balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOf);
            return Web3.Convert.FromWei(balance);
        }
    }
}
