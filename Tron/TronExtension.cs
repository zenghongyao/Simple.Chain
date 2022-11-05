using Google.Protobuf;
using Nethereum.Web3.Accounts;
using Simple.Chain.Crypto;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Chain.Tron
{
    public static class TronExtension
    {
        const decimal sum = 0.000001M;
        public static BigInteger ToBigNumber(this decimal value)
        {
            return Convert.ToInt64(value / sum);
        }

        public static long FromSum(this decimal value)
        {
            return Convert.ToInt64(value / sum);
        }
        public static decimal ToSum(this long value)
        {
            return value * sum;
        }
        public static decimal ToSum(this BigInteger value)
        {
            if (value.Sign == -1) return 0;
            return (long)value * sum;
        }
        public static decimal ToNumber(this string value)
        {
            decimal.TryParse(value, out decimal amount);
            return amount * sum;
        }

        public static decimal ToNumber(this long value)
        {
            return value * sum;
        }

        public static decimal ToNumber(this int value)
        {
            return value * sum;
        }
        public static decimal ToNumber(this BigInteger value)
        {
            try
            {
                decimal val = (decimal)value;
                return val * sum;
            }
            catch (Exception)
            {
                Console.WriteLine($"转换失败：{value}");
                return 0;
            }
        }

        /// <summary>
        /// 转换base58格式地址
        /// </summary>
        /// <param name="hex_address"></param>
        /// <returns></returns>
        public static string ToBase58Address(this string hex_address)
        {
            if (hex_address.StartsWith("0x"))
            {
                hex_address = hex_address[2..];
            }
            return Base58Encoder.EncodeFromHex(hex_address, 0x41);
        }
        /// <summary>
        /// 通过私钥获取地址
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string ToAddress(this string privateKey)
        {
            Account account = new(privateKey);
            return account.Address.ToBase58Address();
        }
        /// <summary>
        /// 将波场hex地址转出eth hex地址
        /// </summary>
        /// <param name="tron_address"></param>
        /// <returns></returns>
        public static string ToEthAddress(this string tron_address)
        {
            if (tron_address.StartsWith("41"))
            {
                tron_address = tron_address[2..];
            }
            return "0x" + tron_address;
        }
        /// <summary>
        /// 转换hex格式地址
        /// </summary>
        /// <param name="base58_address"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string ToHexAddress(this string base58_address, bool prefix = true)
        {
            byte[] address = Base58Encoder.DecodeFromBase58Check(base58_address);
            string hex_address = string.Concat(address.Select(b => b.ToString("x2")).ToArray());
            return prefix ? hex_address : hex_address[2..];
        }

        public static string GetTxID(this Protocol.Transaction transaction)
        {
            return transaction.RawData.ToByteArray().ToSHA256Hash().ToHex();
        }


        public static ContractParameter GetContractParameter(this string hex)
        {
            if (hex.Length == 232)
            {
                string from_address = hex.Substring(4, 42);
                string contract_address = hex.Substring(50, 42);
                string method = hex.Substring(96, 8);
                string params_0 = hex.Substring(104, 64);
                string params_1 = hex.Substring(168, 64);
                BigInteger value = BigInteger.Parse(params_1, NumberStyles.HexNumber);
                try
                {
                    return new ContractParameter
                    {
                        ContractAddress = contract_address.ToBase58Address(),
                        To = params_0.ToBase58Address(),
                        From = from_address.ToBase58Address(),
                        Value = value,
                        MethodID = method,
                    };
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return null;
        }
    }
}
