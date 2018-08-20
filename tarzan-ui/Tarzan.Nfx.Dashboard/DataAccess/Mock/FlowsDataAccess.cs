using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Mock
{
    public class FlowsDataAccess : ITableDataAccess<PacketFlow, Guid>
    {
        List<PacketFlow> m_data;
        public FlowsDataAccess(IHostingEnvironment hostingEnvironment)
        {
            var path = Path.Combine(hostingEnvironment.ContentRootPath, "DataAccess", "Mock", "testbed-12jun-000.json");
            using (var r = new StreamReader(path))
            {
                var json = r.ReadToEnd();                                
                var items = JsonConvert.DeserializeObject<List<PacketFlow>>(json);
                m_data = items;
            }
        }

        public IEnumerable<PacketFlow> FetchRange(int start, int count)        
        {
            return m_data.Skip(start).Take(count);
        }  

        public PacketFlow FetchItem(Guid id)
        {
            return m_data.FirstOrDefault(x => x.FlowId.Equals(id));
        }

        public int Count()
        {
            return m_data.Count;
        }
    }
}