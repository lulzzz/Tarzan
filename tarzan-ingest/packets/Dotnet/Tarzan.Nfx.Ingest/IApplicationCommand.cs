using Microsoft.Extensions.CommandLineUtils;
using System;

namespace Tarzan.Nfx.Ingest
{
    public interface IApplicationCommand
    {
        string Name { get; }
        Action<CommandLineApplication> Configuration { get; }
    }
}