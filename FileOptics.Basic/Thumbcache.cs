using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileOptics.Basic
{
    [ModuleAttrib("Windows Thumbnail Cache File (CMMM)", 0x18, new byte[] { 0x43, 0x4D, 0x4D, 0x4D })]
    public class Thumbcache : IModule
    {
        static readonly byte[] CMMM = new byte[] { 0x43, 0x4D, 0x4D, 0x4D };

        public bool CanRead(Stream stream)
        {
            stream.Seek(0x04, SeekOrigin.Begin);

            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int ver = BitConverter.ToInt32(buffer, 0);

            return
                //ver == 0x14 || //Vista
                ver == 0x15 || //Windows 7
                ver == 0x1F;   //Windows 8.1
        }

        public bool CompareBytes(byte[] bt1, byte[] bt2)
        {
            if (bt1.Length != bt2.Length) return false;

            for (int i = 0; i < bt1.Length; i++)
                if (bt1[i] != bt2[i])
                    return false;

            return true;
        }

        public bool Read(RootInfoNode root, Stream stream)
        {
            Bridge.AppendNode(
                new InfoNode("CMMM Header",
                    InfoType.Generic,
                    new GenericInfo("CMMM Header", "Contains information about the file type version and cache type."),
                    DataType.Critical,
                    0x00, 0x17),
                root);

            stream.Seek(0x04, SeekOrigin.Begin);

            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int version = BitConverter.ToInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            int ctype = BitConverter.ToInt32(buffer, 0);

            stream.Seek(0x0C, SeekOrigin.Current); //Skip three 32bit integers

            int entrylen, stringlen, paddinglen, datalen, seek, databegin, dataend, entrybegin;
            byte[] cbuffer = new byte[0x0C];
            long tempseek;
            while (stream.Read(buffer, 0, 4) == 4 && CompareBytes(buffer, CMMM))
            {
                entrybegin = (int)(stream.Position - 4);
                if (stream.Read(buffer, 0, 4) != 4)
                    throw new Exception("Data ended earlier than expected.");
                entrylen = BitConverter.ToInt32(buffer, 0);

                tempseek = stream.Position;
                if (stream.Seek(0x08, SeekOrigin.Current) != tempseek + 0x08) //Skip CRC64 hash
                    throw new Exception("Data ended earlier than expected.");

                if (version == 0x14)
                {
                    tempseek = stream.Position;
                    if (stream.Seek(0x08, SeekOrigin.Current) != tempseek + 0x08) //Skip extension
                        throw new Exception("Data ended earlier than expected.");
                }

                if (stream.Read(cbuffer, 0, 0x0C) != 0x0C)
                    throw new Exception("Data ended earlier than expected.");
                stringlen = BitConverter.ToInt32(cbuffer, 0x00);
                paddinglen = BitConverter.ToInt32(cbuffer, 0x04);
                datalen = BitConverter.ToInt32(cbuffer, 0x08);

                tempseek = stream.Position;
                seek = (version == 0x1F ? 0x0C : 0x04) + 0x10;
                if (stream.Seek(seek, SeekOrigin.Current) != tempseek + seek) //Skip unknown integers and two CRC64 checksums
                    throw new Exception("Data ended earlier than expected.");

                byte[] uidb = new byte[stringlen];
                if (stream.Read(uidb, 0, stringlen) != stringlen)
                    throw new Exception("Data ended earlier than expected.");
                string uniqueid = Encoding.Unicode.GetString(uidb);

                if (uniqueid.Length == 0 || uniqueid[0] == '\0') uniqueid = "NULL";

                tempseek = stream.Position;
                if (stream.Seek(paddinglen, SeekOrigin.Current) != tempseek + paddinglen) //Skip padding
                    throw new Exception("Data ended earlier than expected.");

                tempseek = stream.Position;
                seek = (entrybegin + entrylen) - (int)stream.Position;
                databegin = (int)stream.Position;
                if (stream.Seek(seek, SeekOrigin.Current) != tempseek + seek) //Skip data
                    throw new Exception("Data ended earlier than expected.");
                dataend = (int)stream.Position - 1;

                if (datalen > 0)
                    Bridge.AppendNode(
                        new InfoNode(uniqueid,
                            InfoType.Delegate,
                            new object[] {
                                (Action<object[]>) delegate(object[] data) {
                                    string path = ((string)data[2]);
                                    int begin = ((int)data[0]);
                                    int len = ((int)data[1]);

                                    byte[] imgdata = new byte[len];
                                    using(FileStream nfs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        nfs.Seek(begin, SeekOrigin.Begin);
                                        nfs.Read(imgdata, 0, len);
                                    }

                                    Bridge.ShowInfo(InfoType.ImageStream, new MemoryStream(imgdata, false));
                                },
                                new object[3] { databegin, datalen, root.FilePath }
                            },
                            DataType.Ancillary,
                            entrybegin, dataend),
                        root);
                else
                    Bridge.AppendNode(
                        new InfoNode(uniqueid,
                            InfoType.None,
                            null,
                            DataType.Ancillary,
                            entrybegin, dataend),
                        root);
            }

            if (stream.Position < stream.Length)
            {
                Bridge.AppendNode(
                    new InfoNode("EOF",
                        InfoType.Generic,
                        new GenericInfo("EOF", "End Of File data; serves no function or purpose."),
                        DataType.Useless,
                        stream.Position - 4, stream.Length - 1),
                    root);
            }

            return true;
        }

        public void Write(RootInfoNode root, Stream sout)
        {
            throw new NotImplementedException();
        }
    }
}
