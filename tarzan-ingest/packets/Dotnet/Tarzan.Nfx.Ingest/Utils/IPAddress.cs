using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Ingest.Utils
{
    public class InternetAddress : System.Net.IPAddress
    {
        public InternetAddress(byte[] address) : base(address)
        {
        }

        public InternetAddress(long newAddress) : base(newAddress)
        {
        }

        public InternetAddress(byte[] address, long scopeid) : base(address, scopeid)
        {
        }

        
        public InternetAddress(System.Net.IPAddress address) : base(address.GetAddressBytes())
        {
        }

        public static implicit operator InternetAddress(string ipString)
        {
            return new InternetAddress(Parse(ipString));
        }
    }
}
