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
        public static BigInteger ToBigNumber(this decimal value)
        {
            return Convert.ToInt64(value / 0.000001M);
        }
    }
}
