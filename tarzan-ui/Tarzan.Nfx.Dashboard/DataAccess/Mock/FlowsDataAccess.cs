using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tarzan.Nfx.Model;

namespace Tarzan.UI.Server.DataAccess.Mock
{
    public class FlowsDataAccess : IFlowsDataAccess
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

        public IEnumerable<Flow> GetFlowRecords(int start = 0, int length = Int32.MaxValue)        
        {
            return m_data.Skip(start).Take(length);
        }  

        public Flow GetFlowRecord(Guid id)
        {
            return m_data.FirstOrDefault(x => x.FlowId.Equals(id));
        }

        public int RecordCount()
        {
            return m_data.Count;
        }
    }
}