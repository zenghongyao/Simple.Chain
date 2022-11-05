using System.Numerics;

namespace Simple.Chain.Tron
{
    public class ContractParameter
    {
        public string ContractAddress { get; set; }
        public string MethodID { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public BigInteger Value { get; set; }
    }
}
