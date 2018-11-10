using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Tarzan.Nfx.PacketDecoders.Tests
{
    public class PacketProvider
    {
        public static string GetFullPath(string resourcePath)
        {
            var location = typeof(HttpDecodeTest).GetTypeInfo().Assembly.Location;
            var dirPath = Path.GetDirectoryName(location);
            return Path.Combine(dirPath, resourcePath.Replace('\\', Path.DirectorySeparatorChar));
        }
        public static IList<RawCapture> LoadPacketsFromResourceFolder(string filename)
        {
            var packetList = new List<RawCapture>();
            var device = new CaptureFileReaderDevice(GetFullPath(filename));
            device.Open();
            RawCapture packet;
            while ((packet = device.GetNextPacket()) != null)
            {
                packetList.Add(packet);
            }
            device.Close();
            return packetList;
        }
    }
}
