using Kaitai;
using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tarzan.Nfx.Model;
using Tarzan.Nfx.Packets.IoT;
using Xunit;

namespace Tarzan.Nfx.PacketDecoders.Tests
{
    public class MqttDecodeTest
    {
        [Theory]
        [InlineData(@"Resources\mqtt\08_disconnect_request.raw")]
        [InlineData(@"Resources\mqtt\07_ping_response.raw")]
        [InlineData(@"Resources\mqtt\06_ping_request.raw")]
        [InlineData(@"Resources\mqtt\05_publish_message.raw")]
        [InlineData(@"Resources\mqtt\04_subscribe_ack.raw")]
        [InlineData(@"Resources\mqtt\03_subscribe_request.raw")]
        [InlineData(@"Resources\mqtt\02_connect_ack_command.raw")]
        [InlineData(@"Resources\mqtt\01_connect_command.raw")]
        public void ParseMqttMessage(string filename)
        {
            var path = PacketProvider.GetFullPath(filename);
            var bytes = File.ReadAllBytes(path);
            var tlsPacket = new MqttPacket(new KaitaiStream(bytes));
        }
    }
}
