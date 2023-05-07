using System;
using System.Net;

namespace AtelierMisaka.Utils
{
    internal class TimeOutWebClient : WebClient
    {
        int _timeout = 15000;

        public TimeOutWebClient(int timeout = 15000)
        {
            _timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var req = base.GetWebRequest(address);
            req.Timeout = _timeout;
            return req;
        }
    }
}
