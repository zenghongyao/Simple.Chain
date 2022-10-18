using Simple.Tron.ABI.Decoders;
using Simple.Tron.ABI.Encoders;

namespace Simple.Tron.ABI
{
    public class AddressType : ABIType
    {
        public AddressType() : base("address")
        {
            //this will need to be only a string type one, converting to hex
            Decoder = new AddressTypeDecoder();
            Encoder = new AddressTypeEncoder();
        }
    }
}