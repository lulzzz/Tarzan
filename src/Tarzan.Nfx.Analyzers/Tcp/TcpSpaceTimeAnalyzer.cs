using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tarzan.Nfx.Analyzers.Tcp
{

    public class TcpSpaceTimeAnalyze : IComputeAction
    {
        [InstanceResource]
        protected readonly IIgnite m_ignite;



        public void Invoke()
        {
            throw new NotImplementedException();
        }
    }
}
