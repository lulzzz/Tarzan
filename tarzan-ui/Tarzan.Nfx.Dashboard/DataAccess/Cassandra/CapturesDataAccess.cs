using Cassandra;
using System;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    public class CapturesDataAccess : TableDataAccess<Capture, Guid>
    {
        public CapturesDataAccess(ISession session) : base(session,nameof(Capture.Id).ToLowerInvariant())
        {
        }
    }
}
