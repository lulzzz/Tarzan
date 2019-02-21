using System;
using Tarzan.Nfx.Model.Core;

namespace Tarzan.Nfx.Model.Observable
{
    /// <summary>
    /// The X.509 Certificate Object represents the properties of an X.509 certificate, as defined by ITU recommendation X.509.
    /// STIX Reference: 
    /// <seealso cref="http://docs.oasis-open.org/cti/stix/v2.0/cs01/part4-cyber-observable-objects/stix-v2.0-cs01-part4-cyber-observable-objects.html#_Toc496716295"/>
    /// </summary>
    public class X509Certificate : ObservableObject
    {
        public string Issuer { get; set; }
        public DateTime ValidityNotBefore { get; set; }
        public DateTime ValidityNotAfter { get; set; }
        public string Subject { get; set; }
                  
        public override string Type => "x509-certificate";
    }

}
