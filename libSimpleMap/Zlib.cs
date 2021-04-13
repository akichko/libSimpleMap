using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;


namespace libSimpleMap
{
    public class Zlib
    {
        public byte[] Compress(byte[] inBytes)
        {
            byte[] outBytes;

            using (MemoryStream ms = new MemoryStream())
            using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress, true))
            {
                byte[] size = BitConverter.GetBytes((int)inBytes.Length);
                ms.Write(size, 0, 4);
                ds.Write(inBytes, 0, inBytes.Length);
                ds.Dispose();

                //ms.Position = 0;
                //outBytes = new byte[ms.Length];
                //ms.Read(outBytes, 0, outBytes.Length);

                outBytes = ms.ToArray();
            }
            //解凍テスト
            //byte[] orgBytes = Decompress(outBytes);

            return outBytes;
        }

        public byte[] Decompress(byte[] inBytes)
        {
            byte[] outBytes;

            using (MemoryStream msIn = new MemoryStream(inBytes))
            using (MemoryStream msOut = new MemoryStream())
            using (DeflateStream ds = new DeflateStream(msIn, CompressionMode.Decompress))

            {
                byte[] sizeBuf = new byte[4];
                msIn.Read(sizeBuf, 0, 4);
                int size = BitConverter.ToInt32(sizeBuf, 0);

                //msIn.Position = 2;
                ds.CopyTo(msOut);

                //outBytes = new byte[msOut.Length];
                //msOut.Read(outBytes, 0, outBytes.Length);
                outBytes = msOut.ToArray();
            }
            return outBytes;
        }

    }

}
