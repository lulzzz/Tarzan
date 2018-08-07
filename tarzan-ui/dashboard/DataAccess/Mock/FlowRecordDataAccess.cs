using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tarzan.UI.Server.Models;

namespace Tarzan.UI.Server.DataAccess.Mock
{
    public class FlowRecordDataAccess : IFlowRecordDataAccess
    {
        List<FlowRecord> m_data;
        public FlowRecordDataAccess(IHostingEnvironment hostingEnvironment)
        {
            var path = Path.Combine(hostingEnvironment.ContentRootPath, "DataAccess", "Mock", "testbed-12jun-000.json");
            using (var r = new StreamReader(path))
            {
                var json = r.ReadToEnd();                                
                var items = JsonConvert.DeserializeObject<List<FlowRecord>>(json);
                m_data = items;
            }
        }

        public IEnumerable<FlowRecord> GetFlowRecords(int start = 0, int length = Int32.MaxValue)        
        {
            return m_data.Skip(start).Take(length);
        }  

        public FlowRecord GetFlowRecord(Guid id)
        {
            return m_data.FirstOrDefault(x => x.FlowId.Equals(id));
        }

        public int RecordCount()
        {
            return m_data.Count;
        }
    }
}