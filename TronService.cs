using RestSharp;

namespace Simple.Tron
{
    public abstract class TronService
    {
        /// <summary>
        /// DEFAULT_BASE_URL
        /// </summary>
        protected const string DEFAULT_BASE_URL = "https://api.trongrid.io";
        /// <summary>
        /// baseUrl
        /// </summary>
        protected string baseUrl;

        public TronService() : this(DEFAULT_BASE_URL)
        {

        }
        public TronService(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }
    }
}