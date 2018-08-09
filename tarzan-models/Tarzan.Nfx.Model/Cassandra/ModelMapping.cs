using Cassandra.Mapping;
using System;
using System.Collections.Generic;
using System.Text;
using Tarzan.Nfx.Model;
namespace Tarzan.Nfx.Model.Cassandra
{
    public class ModelMapping : Mappings
    {
        public static void Register(MappingConfiguration global)
        {
            global.Define(Capture.Mapping);
            global.Define(Flow.Mapping);
            global.Define(Host.Mapping);
            global.Define(Service.Mapping);
            global.Define(Dns.Mapping);
        }
    }
}
