using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.CommandLineUtils;

namespace Tarzan.Nfx.Analyzers.Commands
{
    class ExtractDnsCommand : AbstractCommand
    {
        public static string Name { get; private set; } = "extract-dns";

        public ExtractDnsCommand(CommandOption clusterOption) : base(clusterOption)
        {
        }

        public override void Configuration(CommandLineApplication command)
        {
            throw new NotImplementedException();
        }
    }
}
