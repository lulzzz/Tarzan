using Microsoft.Extensions.CommandLineUtils;
using System;

namespace Tarzan.Nfx.Ingest
{
    public interface IApplicationCommand
    {
        string Name { get; }
        void ExecuteCommand(CommandLineApplication target);
    }
}