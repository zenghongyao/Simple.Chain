using System.Reflection;
using Simple.Tron.ABI.FunctionEncoding.Attributes;

namespace Simple.Tron.ABI.FunctionEncoding.AttributeEncoding
{
    public class ParameterAttributeValue
    {
        public ParameterAttribute ParameterAttribute { get; set; }
        public object Value { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }
}