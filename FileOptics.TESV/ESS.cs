using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileOptics.TESV
{
    [ModuleAttrib("TESV Save File", 0x20, new byte[] { 0x54, 0x45, 0x53, 0x56, 0x5F, 0x53, 0x41, 0x56, 0x45, 0x47, 0x41, 0x4D, 0x45 })]
    public class ESS : IModule
    {
        public bool CanRead(Stream stream)
        {
            return true;
        }

        public bool Read(RootInfoNode root, Stream stream)
        {
            InfoNode nHeader =
                new InfoNode("Header", "block",
                    InfoType.Generic,
                    new GenericInfo("Header", "Contains all metadata and pointer information for the save. Save headers are of variable-size as seen by the 'Header size' field."),
                    DataType.Critical,
                    0x00, 0x10);

            /* Magic number : char[13] */
            Bridge.AppendNode(nHeader, root);
            Bridge.AppendNode(
                new InfoNode("Magic number", "binary",
                    InfoType.Generic,
                    new GenericInfo("Magic Number", ""),
                    DataType.Critical,
                    0x00, 0x0C),
                nHeader);

            stream.Seek(0x0D, SeekOrigin.Begin);

            /* Header size : uint32 */
            byte[] ib = new byte[8];
            uint hsize = ReadUInt32(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Header size", "int",
                    InfoType.Generic,
                    new GenericInfo("Header size", $"{hsize} bytes"),
                    DataType.Critical,
                    0x0D, 0x0D + 0x03),
                nHeader);

            /* Version : uint32 */
            long pos = stream.Position;
            uint version = ReadUInt32(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Version", "int",
                    InfoType.Generic,
                    new GenericInfo("Version", version.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            if (version != 12) throw new Exception("Expected Special Edition save file.");

            /* Save number : uint32 */
            pos = stream.Position;
            uint savenumber = ReadUInt32(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Save number", "int",
                    InfoType.Generic,
                    new GenericInfo("Save number", savenumber.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Player name : wstring */
            pos = stream.Position;
            string playername = ReadWString(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Player name", "str",
                    InfoType.Generic,
                    new GenericInfo("Player name", playername),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Player level : uint32 */
            pos = stream.Position;
            uint level = ReadUInt32(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Player level", "int",
                    InfoType.Generic,
                    new GenericInfo("Player level", level.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Player location : wstring */
            pos = stream.Position;
            string playerloc = ReadWString(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Player location", "str",
                    InfoType.Generic,
                    new GenericInfo("Player location", playerloc),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Game date : wstring */
            pos = stream.Position;
            string gamedate = ReadWString(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Game date", "str",
                    InfoType.Generic,
                    new GenericInfo("Game date", gamedate),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Race ID : wstring */
            pos = stream.Position;
            string raceid = ReadWString(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Race ID", "str",
                    InfoType.Generic,
                    new GenericInfo("Race ID", raceid),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Player sex : uint16 */
            pos = stream.Position;
            uint sex = ReadUInt16(stream, ref ib);
            string sexs = "N/A";
            if (sex == 0) sexs = "Male";
            else if (sex == 1) sexs = "Female";
            Bridge.AppendNode(
                new InfoNode("Player sex", "int",
                    InfoType.Generic,
                    new GenericInfo("Player sex", $"{sex}, {sexs}"),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Player current exp : float32 */
            pos = stream.Position;
            uint curexpi = ReadUInt32(stream, ref ib);
            float curexp = FloatConverter.Convert(curexpi);
            Bridge.AppendNode(
                new InfoNode("Player current exp", "binary",
                    InfoType.Generic,
                    new GenericInfo("Player current exp", curexp.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Player required exp : float32 */
            pos = stream.Position;
            uint reqexpi = ReadUInt32(stream, ref ib);
            float reqexp = FloatConverter.Convert(reqexpi);
            Bridge.AppendNode(
                new InfoNode("Player required exp", "binary",
                    InfoType.Generic,
                    new GenericInfo("Player required exp", reqexp.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Filetime : FILETIME(UInt64) */
            pos = stream.Position;
            ulong filetime = ReadUInt64(stream, ref ib);
            DateTime dfiletime = DateTime.FromFileTime((long)filetime);
            Bridge.AppendNode(
                new InfoNode("Filetime", "binary",
                    InfoType.Generic,
                    new GenericInfo("Filetime", $"{dfiletime:R}\r\n{dfiletime:u}"),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Shot width : uint32 */
            pos = stream.Position;
            uint shotwidth = ReadUInt32(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Screenshot width", "int",
                    InfoType.Generic,
                    new GenericInfo("Screenshot width", shotwidth.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Shot height : uint32 */
            pos = stream.Position;
            uint shotheight = ReadUInt32(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Screenshot height", "int",
                    InfoType.Generic,
                    new GenericInfo("Screenshot height", shotheight.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* Compression type : uint16 */
            pos = stream.Position;
            uint comp = ReadUInt16(stream, ref ib);
            string comps = "N/A";
            if (comp == 0) comps = "None";
            else if (comp == 1) comps = "zLib";
            else if (comp == 2) comps = "L4Z";
            Bridge.AppendNode(
                new InfoNode("Compression type", "int",
                    InfoType.Generic,
                    new GenericInfo("Compression type", $"{comp}, {comps}"),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nHeader);

            /* End */
            nHeader.DataEnd = stream.Position - 1;

            return true;
        }

        private string ReadWString(Stream stream, ref byte[] ib)
        {
            int size = (int)ReadUInt16(stream, ref ib);

            byte[] sb = new byte[size];
            if (stream.Read(sb, 0, size) != size)
                throw new Exception("Data ended earlier than expected.");

            return Encoding.ASCII.GetString(sb);
        }

        ulong ReadUInt64(Stream stream, ref byte[] buffer)
        {
            if (stream.Read(buffer, 0, 8) != 8)
                throw new Exception("Data ended earlier than expected.");

            return
                  (ulong)buffer[7] << 0x38
                | (ulong)buffer[6] << 0x30
                | (ulong)buffer[5] << 0x28
                | (ulong)buffer[4] << 0x20
                | (ulong)buffer[3] << 0x18
                | (ulong)buffer[2] << 0x10
                | (ulong)buffer[1] << 0x08
                | (ulong)buffer[0];
        }

        uint ReadUInt32(Stream stream, ref byte[] buffer)
        {
            if (stream.Read(buffer, 0, 4) != 4)
                throw new Exception("Data ended earlier than expected.");

            return (uint)buffer[3] << 0x18 | (uint)buffer[2] << 0x10 | (uint)buffer[1] << 0x08 | (uint)buffer[0];
        }

        uint ReadUInt16(Stream stream, ref byte[] buffer)
        {
            if (stream.Read(buffer, 0, 2) != 2)
                throw new Exception("Data ended earlier than expected.");

            return (uint)buffer[1] << 0x08 | (uint)buffer[0];
        }

        public void Write(RootInfoNode root, Stream sout)
        {
            Bridge.SimpleWrite(root, sout);
        }
    }
}
