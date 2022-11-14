using Simple.Chain.Eth;
using Simple.Chain.Tron;

namespace Simple.Chain
{
    public static class WalletProvider
    {
        /// <summary>
        /// 获取钱包
        /// </summary>
        /// <param name="chain">链：bsc=币安、erc=以太坊 trc=波场</param>
        /// <returns></returns>
        public static IWallet GetWalletClient(string chain, string rpcURL)
        {
            return chain.ToLower() switch
            {
                "bsc" or "erc" => new EthWallet(rpcURL),
                "trc" => new TronWallet(rpcURL),
                _ => throw new ChainException($"{chain}未实现"),
            };
        }
    }
}
