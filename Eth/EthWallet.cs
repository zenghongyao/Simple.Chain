using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Numerics;

namespace Simple.Chain.Eth
{
    public class EthWallet : IWallet
    {

        public readonly string _mainnet;
        public EthWallet(string mainnet)
        {
            _mainnet = mainnet;
        }

        public async Task<string> TransferAsync(string privateKey, string to_address, string contract_address, decimal amount)
        {
            Account account = new(privateKey);
            Web3 web3 = new(account, _mainnet);
            decimal balance = GetBalance(account.Address, contract_address);
            if (amount > balance) throw new ChainException("余额不足", address: account.Address);
            var nonce = await account.NonceService.GetNextNonceAsync();
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            TransferFunction transfer = new()
            {
                FromAddress = account.Address,
                To = to_address,
                GasPrice = new HexBigInteger(5000000000),
                Value = new HexBigInteger(Web3.Convert.ToWei(amount)),
                Nonce = nonce,
            };
            var gas = await transferHandler.EstimateGasAsync(contract_address, transfer);
            transfer.Gas = gas;
            balance = GetBalance(account.Address);
            if (balance < Web3.Convert.FromWei(gas.Value)) throw new ChainException("燃料资金不足", address: account.Address);
            //签名
            var encoded = await transferHandler.SignTransactionAsync(contract_address, transfer);
            //广播
            return await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);
        }
        /// <summary>
        /// 转账
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="to_address"></param>
        /// <param name="amount"></param>
        public async Task<string> TransferAsync(string privateKey, string to_address, decimal amount)
        {
            Account account = new(privateKey);
            Web3 web3 = new(account, _mainnet);
            decimal balance = GetBalance(account.Address);
            if (balance < amount) throw new ChainException("余额不足", address: account.Address);
            var nonce = await account.NonceService.GetNextNonceAsync();
            TransactionInput input = new()
            {
                From = account.Address,
                To = to_address,
                Gas = new HexBigInteger(21000),
                GasPrice = new HexBigInteger(20000000000),
                Value = new HexBigInteger(Web3.Convert.ToWei(amount)),
                Nonce = nonce,
            };
            //签名
            string encoded = await web3.Eth.TransactionManager.SignTransactionAsync(input);
            //广播
            return await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync("0x" + encoded);
        }

        /// <summary>
        /// 获取合约余额
        /// </summary>
        /// <param name="address"></param>
        /// <param name="contractAddress"></param>
        /// <returns></returns>
        public decimal GetBalance(string address, string contractAddress)
        {
            Web3 web3 = new(_mainnet);
            var balanceOf = new BalanceOfFunction()
            {
                Owner = address,
            };
            var balanceHandler = web3.Eth.GetContractQueryHandler<BalanceOfFunction>();
            var balance = balanceHandler.QueryAsync<BigInteger>(contractAddress, balanceOf).Result;
            return Web3.Convert.FromWei(balance);
        }
        /// <summary>
        /// 获取余额
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public decimal GetBalance(string address)
        {
            Web3 web3 = new(_mainnet);
            var balance = web3.Eth.GetBalance.SendRequestAsync(address).Result;
            return Web3.Convert.FromWei(balance.Value);
        }

        public async Task<TransactionInfo> GetTransactionInfoAsync(string txId)
        {
            Web3 web3 = new(_mainnet);
            try
            {
                Transaction transaction = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txId);
                TransactionReceipt receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
                BlockWithTransactions block = await web3.Eth.Blocks.GetBlockWithTransactionsByHash.SendRequestAsync(receipt.BlockHash);
                Web3Utils.GetContractParameter(transaction.Input, out string to_address, out BigInteger amount);
                return new TransactionInfo
                {
                    BlockNumber = (int)receipt.BlockNumber.Value,
                    From = receipt.From,
                    To = to_address,
                    ContractAddress = receipt.To,
                    Gas = Web3.Convert.FromWei(receipt.GasUsed.Value * transaction.GasPrice),
                    GasPrice = Web3.Convert.FromWei(receipt.CumulativeGasUsed.Value),
                    TransactionHash = receipt.TransactionHash,
                    Value = Web3.Convert.FromWei(amount),
                    Timestamp = (long)(block.Timestamp.Value * 1000),
                    Status = receipt.Status.Value == 1
                };
            }
            catch (Exception ex)
            {
                throw new ChainException(ex.Message, address: string.Empty, hash: txId);
            }
        }
        /// <summary>
        /// 监听合约事件
        /// </summary>
        /// <param name="abi"></param>
        /// <param name="contract_address"></param>
        /// <param name="eventname">事件名称</param>
        /// <param name="event"></param>
        public async Task ContractEventAsync(string abi, string contract_address, string eventname, Action<TransactionEvent> @event)
        {
            Web3 web3 = new(_mainnet);
            Contract contract = web3.Eth.GetContract(abi, contract_address);
            Event contractevent = contract.GetEvent(eventname);
            HexBigInteger blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            ulong block = ulong.Parse(blockNumber.Value.ToString());
            while (true)
            {
                var filter = contractevent.CreateFilterInput(new BlockParameter(block), BlockParameter.CreateLatest());
                var logs = contractevent.GetAllChangesDefaultAsync(filter).Result;
                if (logs.Count == 0)
                {
                    continue;
                }
                foreach (var item in logs)
                {
                    @event(new TransactionEvent()
                    {
                        Block = (long)block,
                        TransactionHash = item.Log.TransactionHash,
                        From = (string)item.Event[0].Result,
                        To = (string)item.Event[1].Result,
                        Value = (BigInteger)item.Event[2].Result
                    });
                }
                block++;
            }

            //using (var client = new StreamingWebSocketClient(_mainnet))
            //{
            //    var subscription = new EthLogsObservableSubscription(client);
            //    var filterTransfers = Event<TransferEventDTO>.GetEventABI().CreateFilterInput(contract_address);

            //    subscription.GetSubscribeResponseAsObservable().Subscribe(subscriptionId =>
            //        Console.WriteLine("Block Header subscription Id: " + subscriptionId));

            //    subscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
            //    {
            //        BigInteger value = BigInteger.Parse(log.Data[2..], NumberStyles.HexNumber);
            //        if (log.Topics.Length > 2)
            //        {
            //            string from_address = log.Topics[1].ToString().Replace("000000000000000000000000", "");
            //            string to_address = log.Topics[2].ToString().Replace("000000000000000000000000", "");
            //            @event(new TransactionEvent
            //            {
            //                Block = (long)log.BlockNumber.Value,
            //                TransactionHash = log.TransactionHash,
            //                Value = value,
            //                From = from_address,
            //                To = to_address
            //            });
            //        }
            //    });

            //    bool subscribed = true;

            //    subscription.GetUnsubscribeResponseAsObservable().Subscribe(response =>
            //    {
            //        subscribed = false;
            //        Console.WriteLine("Block Header unsubscribe result: " + response);
            //    });
            //    try
            //    {
            //        await client.StartAsync();
            //        await subscription.SubscribeAsync(filterTransfers);
            //        while (subscribed) await Task.Delay(TimeSpan.FromSeconds(1));
            //    }
            //    catch (Exception ex)
            //    {

            //        throw;
            //    }
            //}
        }

        public AccountInfo GenerateAddress()
        {
            EthECKey ecKey = EthECKey.GenerateKey();
            Account account = new(ecKey.GetPrivateKeyAsBytes());
            return new AccountInfo() { Address = account.Address, PrviateKey = account.PrivateKey };
        }

        public Task<AccountInfo> GetAccountAsync(string address)
        {
            return Task.FromResult(new AccountInfo()
            {
                Address = address,
                Balance = this.GetBalance(address),
                Gas = 0.000255515M,
            });
        }

        public Task<AccountInfo> GetAccountAsync(string address, string contract_address)
        {
            return Task.FromResult(new AccountInfo()
            {
                Address = address,
                Balance = this.GetBalance(address),
                Gas = 0.000255515M,
            });
        }

        public Task<string> CreateAccountAsync(string privateKey, string address)
        {
            throw new NotImplementedException();
        }

        public void ContractEvent(string contract_address, string eventname, Action<TransactionEvent> @event)
        {
            throw new NotImplementedException();
        }
    }
}
