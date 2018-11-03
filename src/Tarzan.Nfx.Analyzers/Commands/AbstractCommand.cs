using Apache.Ignite.Core;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tarzan.Nfx.Analyzers.Commands
{
    public abstract class AbstractCommand
    {
        protected CommandOption m_clusterOption;
        public AbstractCommand(CommandOption clusterOption)
        {
            this.m_clusterOption = clusterOption;
        }
        public abstract void Configuration(CommandLineApplication command);

        protected IgniteClient CreateIgniteClient()
        {
            return new IgniteClient(m_clusterOption.Values);
        }
    }
}
