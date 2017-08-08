using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileOptics.Basic
{
    /* *
     * Based on ISO/IEC 15948:2003 (E)
     * https://www.w3.org/TR/PNG/
     * */

    [ModuleAttrib("PNG Image File", 0x10, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    public class PNG : IModule
    {
        //public static byte[] MAGIC = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public bool CanRead(Stream stream)
        {
            return true; //The magic number will take care of this
            //byte[] buffer = new byte[MAGIC.Length];
            //if (stream.Read(buffer, 0, MAGIC.Length) != MAGIC.Length)
            //    return false;

            //int i = 0;
            //for (; i < MAGIC.Length; i++)
            //    if (buffer[i] != MAGIC[i])
            //        break;
            //return i >= MAGIC.Length;
        }

        public bool Read(RootInfoNode root, Stream stream)
        {
            //return true;
            try
            {
                Bridge.AppendNode(
                    new InfoNode("Magic number",
                        InfoType.Generic,
                        new GenericInfo("Magic Number", "An array of bytes that allows programs to determine that the specific file is a PNG."),
                        DataType.Critical,
                        0x00, 0x07),
                    root);

                stream.Seek(0x08, SeekOrigin.Begin);

                byte[] buffer = new byte[4];
                string name = null;
                uint length = 0;
                long start, end;
                bool Ancillary, Private, STC;
                do
                {
                    start = stream.Position;
                    if (stream.Read(buffer, 0, 4) != 4)
                        throw new Exception("Data ended earlier than expected.");
                    length = (uint)buffer[0] << 0x18 | (uint)buffer[1] << 0x10 | (uint)buffer[2] << 0x08 | (uint)buffer[3];
                    //length = BitConverter.ToUInt32(buffer, 0); //Big endian

                    if (stream.Read(buffer, 0, 4) != 4)
                        throw new Exception("Data ended earlier than expected.");
                    name = Encoding.ASCII.GetString(buffer);
                    Ancillary = (buffer[0] & 0x20) == 0x20;
                    Private   = (buffer[1] & 0x20) == 0x20;
                    STC       = (buffer[3] & 0x20) == 0x20;


                    if (stream.Seek(length, SeekOrigin.Current) != start + length + 4 + 4)
                        throw new Exception("Data ended earlier than expected.");

                    if (stream.Read(buffer, 0, 4) != 4) //CRC32r
                        throw new Exception("Data ended earlier than expected.");

                    end = stream.Position - 1;

                    StringBuilder sb = new StringBuilder();
                    switch (name)
                    {
                        default:
                            sb.AppendLine("Unknown chunk type.");
                            break;
                    }

                    sb.AppendLine(Ancillary ? "\r\nThis chunk is ancillary." : "\r\nThis chunk is critical.");
                    sb.AppendLine(Private ? "This chunk is a private chunk." : "This chunk is a public chunk.");
                    sb.AppendLine(STC ? "This chunk is safe to copy." : "This chunk is not safe to copy.");
                    sb.Append(String.Format("This chunk has a CRC32 of {0:X2}{1:X2}{2:X2}{3:X2}", buffer[0], buffer[1], buffer[2], buffer[3]));

                    Bridge.AppendNode(
                        new InfoNode(name,
                            InfoType.Generic,
                            new GenericInfo(name + " Chunk", sb.ToString()),
                            DataType.Critical,
                            start, end),
                        root);
                } while (name != "IEND");

                if (stream.Position < stream.Length)
                {
                    Bridge.AppendNode(
                        new InfoNode("EOF",
                            InfoType.Generic,
                            new GenericInfo("EOF", "End Of File data; serves no function or purpose to the PNG image."),
                            DataType.Useless,
                            stream.Position, stream.Length - 1),
                        root);
                }

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
            //throw new NotImplementedException();
        }

        public void Write(RootInfoNode root, Stream sout)
        {
            throw new NotImplementedException();
        }
    }
}
