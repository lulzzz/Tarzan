using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Dashboard.DataAccess.Mock
{
    public class FlowsDataAccess : ITableDataAccess<Flow, Guid>
    {
        List<Flow> m_data;
        public FlowsDataAccess(IHostingEnvironment hostingEnvironment)
        {
            var path = Path.Combine(hostingEnvironment.ContentRootPath, "DataAccess", "Mock", "testbed-12jun-000.json");
            using (var r = new StreamReader(path))
            {
                var json = r.ReadToEnd();                                
                var items = JsonConvert.DeserializeObject<List<Flow>>(json);
                m_data = items;
            }
        }

        public IEnumerable<Flow> FetchRange(int start, int count)        
        {
            return m_data.Skip(start).Take(count);
        }  

        public Flow FetchItem(Guid id)
        {
            return m_data.FirstOrDefault(x => x.FlowId.Equals(id));
        }

        public int Count()
        {
            return m_data.Count;
        }
    }
}