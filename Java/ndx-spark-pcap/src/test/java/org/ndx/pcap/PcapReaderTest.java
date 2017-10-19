package org.ndx.pcap;
import java.util.*;
import junit.framework.TestCase;
import java.io.DataInputStream;
import java.io.FileInputStream;
import org.ndx.model.PacketModel;
public class PcapReaderTest extends TestCase {

        public PcapReaderTest(String name) {
                super( name );
        }

        public void testReadHttpCapFile() throws Exception
        {
                String inputFile = getClass().getResource("http.cap").getFile();
                FileInputStream inputStream = new FileInputStream(inputFile);
                DataInputStream dataStream = new DataInputStream(inputStream);
                PcapReader reader = new PcapReader(dataStream);
                Iterator<PacketModel.RawFrame> itr = reader.iterator();
                int count = 0;
                while(itr.hasNext()) {
                        PacketModel.RawFrame frame = itr.next();
                        System.out.println(frame);
                        count++;
                }
                assertEquals(count, 40);
        }
}