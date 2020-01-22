using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.RestServer.Models
{
    class RestException : Exception
    {
        public RestException(string message) : base(message)
        {
        }

        public RestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
