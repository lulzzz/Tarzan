using System;
using Xunit;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Model.Observable;

namespace Tarzan.Nfx.Model.Tests
{
    public class ObservableTest
    {
        [Fact]
        public void ArtifactJsonLoad()
        {
            var json = @"{ 'type': 'artifact','mime_type': 'image/jpeg','payload_bin': 'VBORw0KGgoAAAANSUhEUgAAADI=='}";
            var artifact = Artifact.FromJson(json);
        }

        [Fact]
        public void NetworkTrafficJsonLoad()
        {
            var json = @"
            {
	            'type': 'network-traffic',
                'src_ref': '0',
                'dst_ref': '1',
                'protocols': [
                  'ipv4',
                  'tcp'
                ],
                'src_byte_count': 147600,
                'src_packets': 100,
                'ipfix': {
		            'minimum_ip_total_length': 32,
      	            'maximum_ip_total_length': 2556
	            }
            }";
            var artifact = NetworkTraffic.FromJson(json);
        }
    }
}