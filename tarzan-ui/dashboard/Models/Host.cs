using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tarzan.UI.Server.Models
{
    public class Host
    {
        public string Address { get; set; }
        public string Hostname { get; set; }
        public int UpFlows { get; set; }
        public int DownFlows { get; set; }
        public long OctetsSent { get; set; }
        public long OctetsRecv { get; set; }
        public long PacketsSent { get; set; }
        public long PacketsRecv { get; set; }
    }
}
