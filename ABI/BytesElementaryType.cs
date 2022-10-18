using Simple.Tron.ABI.Decoders;
using Simple.Tron.ABI.Encoders;

namespace Simple.Tron.ABI
{
    public class BytesElementaryType : ABIType
    {
        public BytesElementaryType(string name, int size) : base(name)
        {
            Decoder = new BytesElementaryTypeDecoder(size);
            Encoder = new BytesElementaryTypeEncoder(size);
        }
    }
}