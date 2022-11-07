using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;

namespace Simple.Chain
{
    /// <summary>
    /// 链钱包
    /// </summary>
    public interface IWallet
    {
        /// <summary>
        /// 随机生成钱包地址
        /// </summary>
        /// <returns></returns>
        AccountInfo GenerateAddress();

        /// <summary>
        /// 获取账户信息
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<AccountInfo> GetAccountAsync(string address);

        /// <summary>
        /// 检查合约账户信息
        /// </summary>
        /// <param name="address"></param>
        /// <param name="contract_address"></param>
        /// <returns></returns>
        Task<AccountInfo> GetAccountAsync(string address, string contract_address);

        /// <summary>
        /// 创建账户
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="address"></param>
        Task<string> CreateAccountAsync(string privateKey, string address);

        /// <summary>
        /// 合约转账
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="to_address"></param>
        /// <param name="contractAddress"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<string> TransferAsync(string privateKey, string to_address, string contractAddress, decimal amount);
        /// <summary>
        /// 转账
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="to_address"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<string> TransferAsync(string privateKey, string to_address, decimal amount);

        /// <summary>
        /// 查询余额
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        decimal GetBalance(string address);
        /// <summary>
        /// 查询合约余额
        /// </summary>
        /// <param name="address"></param>
        /// <param name="contractAddress"></param>
        /// <returns></returns>
        decimal GetBalance(string address, string contractAddress);
        /// <summary>
        /// 获取交易信息
        /// </summary>
        /// <param name="txId"></param>
        /// <returns></returns>
        Task<TransactionInfo> GetTransactionInfoAsync(string txId);

        /// <summary>
        /// 监听合约事件
        /// </summary>
        /// <param name="abi"></param>
        /// <param name="contract_address"></param>
        /// <param name="eventname"></param>
        /// <param name="event"></param>
        Task ContractEventAsync(string abi, string contract_address, string eventname, Action<TransactionEvent> @event);
    }
}
