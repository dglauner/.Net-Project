using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E33DGLAUNER
{
    public class SecureCommunicationException :  ApplicationException
    {
        public string addressee { get; set; }

        public SecureCommunicationException()
        {
        }

        public SecureCommunicationException(string message) : base(message)
        {
        }

        public SecureCommunicationException(string message, Exception inner) : base(message, inner)
        {
        }

        public SecureCommunicationException(string message, Exception inner, string addr) : base(message, inner)
        {
            this.addressee = addr;
        }
    }
}
