using Simple.Tron.ABI.Decoders;
using Simple.Tron.ABI.Encoders;

namespace Simple.Tron.ABI
{
    public class BoolType : ABIType
    {
        public BoolType() : base("bool")
        {
            Decoder = new BoolTypeDecoder();
            Encoder = new BoolTypeEncoder();
        }
    }
}