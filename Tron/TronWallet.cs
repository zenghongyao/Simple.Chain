using Google.Protobuf;
using Grpc.Core;
using Nethereum.ABI;
using Nethereum.Contracts;
using Nethereum.Contracts.Standards.ERC20.ContractDefinition;
using Nethereum.Signer;
using Newtonsoft.Json;
using Simple.Chain.Crypto;
using Simple.Chain.Protocol;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Simple.Chain.Tron
{
    /// <summary>
    /// 钱包相关
    /// </summary>
    public class TronWallet : IWallet
    {
        private readonly Wallet.WalletClient _wallet;
        public TronWallet(string rpcURL)
        {
            Uri uri = new(rpcURL);
            Channel channel = new Channel(uri.Host, uri.Port, ChannelCredentials.Insecure);
            _wallet = new Wallet.WalletClient(channel);
        }

        public async Task<string> CreateAccountAsync(string privateKey, string address)
        {
            string owner_address = privateKey.ToAddress();

            byte[] ownerAddress = Base58Encoder.DecodeFromBase58Check(owner_address);
            byte[] acountAddress = Base58Encoder.DecodeFromBase58Check(address);

            AccountCreateContract contract = new()
            {
                AccountAddress = ByteString.CopyFrom(acountAddress),
                OwnerAddress = ByteString.CopyFrom(ownerAddress)
            };
            Transaction transaction = await _wallet.CreateAccountAsync(contract);

            SignTransaction(transaction, privateKey);

            var result = await _wallet.BroadcastTransactionAsync(transaction);

            if (!result.Result) throw new ChainException($"广播失败：{result.Message.ToStringUtf8()}", address: owner_address);

            return transaction.GetTxID();
        }


        public async Task<AccountInfo> GetAccountAsync(string address)
        {
            byte[] ownerAddress = Base58Encoder.DecodeFromBase58Check(address);
            Account request = new()
            {
                Address = ByteString.CopyFrom(ownerAddress),
            };
            var response = await _wallet.GetAccountAsync(request);
            var resource = _wallet.GetAccountResource(request);
            return new AccountInfo
            {
                Address = address,
                Balance = response.Balance.ToSum(),
                FreeNetUsed = resource.NetLimit + resource.FreeNetLimit - resource.NetUsed - resource.FreeNetUsed,
                Energy = resource.EnergyLimit - resource.EnergyUsed
            };
        }

        public async Task<AccountInfo> GetAccountAsync(string address, string contract_address)
        {
            byte[] ownerAddress = Base58Encoder.DecodeFromBase58Check(address);
            byte[] contractAddress = Base58Encoder.DecodeFromBase58Check(contract_address);
            ABIEncode abiEncode = new();
            string encode = "0x70a08231" + abiEncode.GetABIParamsEncoded(new BalanceOfFunction() { Owner = address.ToHexAddress(false) }).ToHex();

            TriggerSmartContract contract = new()
            {
                ContractAddress = ByteString.CopyFrom(contractAddress),
                OwnerAddress = ByteString.CopyFrom(ownerAddress),
                Data = ByteString.CopyFrom(encode.HexToByteArray()),
            };
            TransactionExtention transactionExtention = await _wallet.TriggerConstantContractAsync(contract);
            if (transactionExtention.ConstantResult.Count == 0) throw new ChainException("合约执行错误", address: address);
            var result = transactionExtention.ConstantResult[0];
            string hex = "0000000000000000000000000000000000000000000000000000000000000000" + result.ToByteArray().ToHex();
            TransferFunction balanceOf = new TransferFunction().DecodeInput(hex);
            return new AccountInfo
            {
                Address = address,
                Balance = balanceOf.Value.ToSum()
            };
        }

        /// <summary>
        /// 转账签名
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="privateKey"></param>
        private void SignTransaction(Transaction transaction, string privateKey)
        {
            var ecKey = new ECKey(privateKey.HexToByteArray(), true);
            var hash = transaction.RawData.ToByteArray().ToSHA256Hash();
            var sign = ecKey.Sign(hash).ToByteArray();
            transaction.Signature.Add(ByteString.CopyFrom(sign));
        }

        public async Task<string> TransferAsync(string privateKey, string to_address, decimal amount)
        {
            string owner_address = privateKey.ToAddress();
            //检查余额
            AccountInfo account = await this.GetAccountAsync(owner_address);
            if (account.Balance < amount) throw new ChainException($"余额不足：{account.Balance}", address: owner_address);
            byte[] fromAddress = Base58Encoder.DecodeFromBase58Check(owner_address);
            byte[] toAddress = Base58Encoder.DecodeFromBase58Check(to_address);
            TransferContract transfer = new()
            {
                Amount = amount.FromSum(),
                OwnerAddress = ByteString.CopyFrom(fromAddress),
                ToAddress = ByteString.CopyFrom(toAddress)
            };
            //创建交易
            Transaction transaction = await _wallet.CreateTransactionAsync(transfer);
            //签名
            SignTransaction(transaction, privateKey);
            //广播
            var result = await _wallet.BroadcastTransactionAsync(transaction);
            if (!result.Result) throw new ChainException($"广播失败：{result.Message.ToStringUtf8()}", address: owner_address);
            return transaction.GetTxID();
        }


        public async Task<string> TransferAsync(string privateKey, string to_address, string contract_address, decimal amount)
        {
            ABIEncode abiEncode = new();
            string encode = "0xa9059cbb" + abiEncode.GetABIParamsEncoded(new TransferFunction() { To = to_address.ToHexAddress(false), Value = amount.FromSum() }).ToHex();
            string owner_address = privateKey.ToAddress();
            AccountInfo account = await this.GetAccountAsync(owner_address, contract_address);
            if (account.Balance < amount) throw new ChainException($"余额不足：{account}", address: owner_address);
            byte[] fromAddress = Base58Encoder.DecodeFromBase58Check(owner_address);
            byte[] contractAddress = Base58Encoder.DecodeFromBase58Check(contract_address);
            TriggerSmartContract contract = new()
            {
                ContractAddress = ByteString.CopyFrom(contractAddress),
                OwnerAddress = ByteString.CopyFrom(fromAddress),
                Data = ByteString.CopyFrom(encode.HexToByteArray()),
            };
            TransactionExtention transactionExtention = await _wallet.TriggerConstantContractAsync(contract);
            if (!transactionExtention.Result.Result) throw new ChainException($"转账失败，{transactionExtention.Result.Message.ToStringUtf8()}", address: owner_address);
            Transaction transaction = transactionExtention.Transaction;
            transaction.RawData.FeeLimit = 30M.FromSum();
            //签名
            SignTransaction(transaction, privateKey);
            //广播
            var result = await _wallet.BroadcastTransactionAsync(transaction);
            if (!result.Result) throw new ChainException($"广播失败：{result.Message.ToStringUtf8()}", address: owner_address);
            return transaction.GetTxID();
        }

        /// <summary>
        /// 获取交易信息
        /// </summary>
        /// <param name="txId"></param>
        /// <returns></returns>
        public Task<TransactionInfo> GetTransactionInfoAsync(string txId)
        {
            byte[] bytes = txId.HexToByteArray();
            var info = _wallet.GetTransactionInfoById(new BytesMessage() { Value = ByteString.CopyFrom(bytes) });
            if (info.Log.Count == 0)
            {
                return Task.FromResult(new TransactionInfo
                {
                    BlockNumber = info.BlockNumber,
                    Status = info.Result == Protocol.TransactionInfo.Types.code.Sucess,
                    Timestamp = info.BlockTimeStamp,
                    TransactionHash = txId,
                    Gas = info.Fee.ToSum(),
                });
            }
            else
            {
                var log = info.Log[0];
                string address = log.Address.ToByteArray().ToHex().ToBase58Address();
                string data = log.Data.ToByteArray().ToHex();
                BigInteger value = BigInteger.Parse(data, NumberStyles.HexNumber);
                string from = log.Topics[1].ToByteArray().ToHex().ToBase58Address();
                string to = log.Topics[2].ToByteArray().ToHex().ToBase58Address();
                return Task.FromResult(new TransactionInfo()
                {
                    BlockNumber = info.BlockNumber,
                    ContractAddress = address,
                    Value = value.ToSum(),
                    From = from,
                    To = to,
                    Status = info.Result == Protocol.TransactionInfo.Types.code.Sucess,
                    Gas = info.Fee.ToSum(),
                    Timestamp = info.BlockTimeStamp,
                    TransactionHash = txId,
                });
            }

        }

        /// <summary>
        /// 获取最新区块内容
        /// </summary>
        /// <returns></returns>
        public long GetBlockNumber()
        {
            Block block = _wallet.GetNowBlock(new EmptyMessage());
            return block.BlockHeader.RawData.Number;
        }
        /// <summary>
        /// 合约事件监听
        /// </summary>
        /// <param name="@event"></param>
        public void ContractEvent(string contract_address, string eventname, Action<TransactionEvent> @event)
        {

        }

        public decimal GetBalance(string address)
        {
            var account = GetAccountAsync(address).Result;
            return account.Balance;
        }

        public decimal GetBalance(string address, string contractAddress)
        {
            var account = GetAccountAsync(address, contractAddress).Result;
            return account.Balance;
        }

        public AccountInfo GenerateAddress()
        {
            EthECKey ecKey = EthECKey.GenerateKey();
            Nethereum.Web3.Accounts.Account account = new(ecKey.GetPrivateKeyAsBytes());
            return new AccountInfo() { Address = account.Address.ToBase58Address(), PrviateKey = account.PrivateKey };
        }


        public async Task ContractEventAsync(string abi, string contract_address, string eventname, Action<TransactionEvent> @event)
        {
            long number = 0;
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);
                    long blockNumber = GetBlockNumber();
                    if (blockNumber == number) continue;
                    number = blockNumber;
                    Console.WriteLine($"当前区块：{blockNumber}");
                    Block block = await _wallet.GetBlockByNumAsync(new NumberMessage { Num = blockNumber });
                    foreach (Transaction item in block.Transactions)
                    {
                        try
                        {
                            string txId = item.GetTxID();
                            if (item.RawData.Contract.Count == 0) continue;
                            var data = item.RawData.Contract[0];
                            //TransferContract TriggerSmartContract
                            switch (data.Type)
                            {
                                case Transaction.Types.Contract.Types.ContractType.TransferContract:
                                case Transaction.Types.Contract.Types.ContractType.TriggerSmartContract:
                                    {
                                        string hex = data.Parameter.ToByteArray().ToHex();

                                        var parameter = hex.GetContractParameter();
                                        if (parameter == null) continue;
                                        if (parameter.Value > int.MaxValue) continue;
                                        @event(new TransactionEvent { TransactionHash = txId, ContractAddress = parameter.ContractAddress, Block = blockNumber, From = parameter.From, To = parameter.To, Value = parameter.Value });
                                        //Console.WriteLine(hex);
                                        //Console.WriteLine($"交易时间：{item.RawData.Timestamp} 交易哈希：{txId} 合约：{parameter.ContractAddress} 发送地址：{parameter.From} 接收地址：{parameter.To} 金额：{parameter.Value.ToSum()}");
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
