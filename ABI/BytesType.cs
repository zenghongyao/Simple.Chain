using Simple.Tron.ABI.Decoders;
using Simple.Tron.ABI.Encoders;

namespace Simple.Tron.ABI
{
    public class BytesType : ABIType
    {
        public BytesType() : base("bytes")
        {
            Decoder = new BytesTypeDecoder();
            Encoder = new BytesTypeEncoder();
        }

        public override int FixedSize => -1;
    }
}