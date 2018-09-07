using Apache.Ignite.Core.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Model;

namespace Tarzan.Nfx.Ingest.Ignite
{
    public class HttpObjectSerializer : IBinarySerializer
    {
        public void ReadBinary(object obj, IBinaryReader reader)
        {
            var http = (HttpObject)obj;
            http.FlowUid = reader.ReadString(nameof(HttpObject.FlowUid));
            http.ObjectIndex = reader.ReadString(nameof(HttpObject.ObjectIndex));

            http.Client = reader.ReadString(nameof(HttpObject.Client));
            http.Server = reader.ReadString(nameof(HttpObject.Server));
            http.Host = reader.ReadString(nameof(HttpObject.Host));
            http.Method = reader.ReadString(nameof(HttpObject.Method));
            
            http.Password = reader.ReadString(nameof(HttpObject.Password));
            http.Referrer = reader.ReadString(nameof(HttpObject.Referrer));
            http.RequestBodyLength = reader.ReadInt(nameof(HttpObject.RequestBodyLength));
            http.RequestContentType = reader.ReadString(nameof(HttpObject.RequestContentType));
            http.ResponseBodyLength = reader.ReadInt(nameof(HttpObject.ResponseBodyLength));
            http.ResponseContentType = reader.ReadString(nameof(HttpObject.ResponseContentType));
            http.StatusCode = reader.ReadString(nameof(HttpObject.StatusCode));
            http.StatusMessage = reader.ReadString(nameof(HttpObject.StatusMessage));
            http.Timestamp = reader.ReadLong(nameof(HttpObject.Timestamp));
            http.Uri = reader.ReadString(nameof(HttpObject.Uri));
            http.UserAgent = reader.ReadString(nameof(HttpObject.UserAgent));
            http.Username = reader.ReadString(nameof(HttpObject.Username));
            http.Version = reader.ReadString(nameof(HttpObject.Version));
            http.RequestBodyChunks = reader.ReadArray<byte[]>(nameof(HttpObject.RequestBodyChunks)).ToList();
            http.ResponseBodyChunks = reader.ReadArray<byte[]>(nameof(HttpObject.ResponseBodyChunks)).ToList();
        }

        public void WriteBinary(object obj, IBinaryWriter writer)
        {
            var http = (HttpObject)obj;
            writer.WriteString(nameof(HttpObject.FlowUid), http.FlowUid);
            writer.WriteString(nameof(HttpObject.ObjectIndex), http.ObjectIndex);

            writer.WriteString(nameof(HttpObject.Client), http.Client);
            writer.WriteString(nameof(HttpObject.Server), http.Server);
            writer.WriteString(nameof(HttpObject.Host), http.Host);
            writer.WriteString(nameof(HttpObject.Method), http.Method);
            writer.WriteString(nameof(HttpObject.Password), http.Password);
            writer.WriteString(nameof(HttpObject.Referrer), http.Referrer);
            writer.WriteInt(nameof(HttpObject.RequestBodyLength), http.RequestBodyLength);
            writer.WriteString(nameof(HttpObject.RequestContentType), http.RequestContentType);
            writer.WriteInt(nameof(HttpObject.ResponseBodyLength), http.ResponseBodyLength);
            writer.WriteString(nameof(HttpObject.ResponseContentType), http.ResponseContentType);
            
            writer.WriteString(nameof(HttpObject.StatusCode), http.StatusCode);
            writer.WriteString(nameof(HttpObject.StatusMessage), http.StatusMessage);
            writer.WriteLong(nameof(HttpObject.Timestamp), http.Timestamp);
            writer.WriteString(nameof(HttpObject.Uri), http.Uri);
            writer.WriteString(nameof(HttpObject.UserAgent), http.UserAgent);
            writer.WriteString(nameof(HttpObject.Username), http.Username);
            writer.WriteString(nameof(HttpObject.Version), http.Version);
            writer.WriteArray(nameof(HttpObject.RequestBodyChunks), http.RequestBodyChunks.ToArray());
            writer.WriteArray(nameof(HttpObject.ResponseBodyChunks), http.ResponseBodyChunks.ToArray());
        }
    }
}
