using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.RestServer.Models
{
    class RestHandler
    {
        public delegate JObject RestHandlerCallback(JObject args, string username);
        public RestHandlerCallback Callback;
        public bool RequireToken;

        public RestHandler(RestHandlerCallback callback)
        {
            Callback = callback;
            RequireToken = true;
        }

        public RestHandler(RestHandlerCallback callback, bool requireToken)
        {
            Callback = callback;
            RequireToken = requireToken;
        }
    }
}
