using Simple.Tron.ABI.Decoders;
using Simple.Tron.ABI.Encoders;

namespace Simple.Tron.ABI
{
    public class Bytes32Type : ABIType
    {
        public Bytes32Type(string name) : base(name)
        {
            Decoder = new Bytes32TypeDecoder();
            Encoder = new Bytes32TypeEncoder();
        }
    }
}