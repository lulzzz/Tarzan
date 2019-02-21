using System;
using Tarzan.Nfx.Model.Core;

namespace Tarzan.Nfx.Model.Observable
{
    public class EmailMessage : ObservableObject
    {
        public override string Type => "email-message";
        public bool IsMultipart { get; set; }
        public string[] ReceivedLines { get; set; }
        public string ContentType { get; set; }
        public DateTime Date { get; set; }
        public string FromRef { get; set; }
        public string[] ToRefs { get; set; }
        public string[] CcRefs { get; set; }
        public string Subject { get; set; }
        public AdditionalHeaderFields AdditionalHeaderFields { get; set; }
        public BodyMultipart[] BodyMultipart { get; set; }
    }

    public class AdditionalHeaderFields
    {
        public string ContentDisposition { get; set; }
        public string XMailer { get; set; }
        public string XOriginatingIP { get; set; }
    }

    public class BodyMultipart
    {
        public string ContentType { get; set; }
        public string ContentDisposition { get; set; }
        public string Body { get; set; }
        public string BodyRawRef { get; set; }
    }

    public class EmailAddr : ObservableObject
    {
        public override string Type => "email-addr";

        public string Value { get; set; }
        public string Display_name { get; set; }
    }

    public class File : ObservableObject
    {
        public override string Type => "file";

        public string Name { get; set; }
        public string MagicNumberHex { get; set; }
        public Hashes Hashes { get; set; }
    }
}
