/*============================================================================
MIT License

Copyright (c) 2021 akichko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
============================================================================*/
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
