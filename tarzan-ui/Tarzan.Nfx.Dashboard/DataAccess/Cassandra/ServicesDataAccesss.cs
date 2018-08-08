﻿using Cassandra;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Cassandra
{
    internal class ServicesDataAccesss : TableDataAccess<Service, string>
    {
        public ServicesDataAccesss(ISession session) :base(session, "services", nameof(Service.Name).ToLowerInvariant())
        {
        }
    }
}