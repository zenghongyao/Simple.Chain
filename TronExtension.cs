using Simple.Tron.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tron
{
    public static class TronExtension
    {
        const decimal sum = 0.000001M;
        public static BigInteger ToBigNumber(this decimal value)
        {
            return Convert.ToInt64(value / sum);
        }

        public static decimal ToNumber(this string value)
        {
            decimal.TryParse(value, out decimal amount);
            return amount * sum;
        }

        public static decimal ToNumber(this BigInteger value)
        {
            long v = (long)value;
            return v * sum;
        }

        /// <summary>
        /// 转换base58格式地址
        /// </summary>
        /// <param name="hex_address"></param>
        /// <returns></returns>
        public static string ToBase58Address(this string hex_address)
        {
            return Base58Encoder.EncodeFromHex(hex_address, 0x41);
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
            return prefix ? hex_address : hex_address.Substring(2, hex_address.Length - 2);
        }
    }
}
