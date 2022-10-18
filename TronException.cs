using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tron
{
    public class TronException : Exception
    {
        public TronException(RestResponse response) : base(response.Content)
        {

        }

    }
}
