using FileOptics.Interface;
using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
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
            Bridge.AppendNode(nHeader, root);

            /* Magic number : char[13] */
            Bridge.AppendNode(
                new InfoNode("Magic number", "binary",
                    InfoType.Generic,
                    new GenericInfo("Magic Number", ""),
                    DataType.Critical,
                    0x00, 0x0C),
                nHeader);

            stream.Seek(0x0D, SeekOrigin.Begin);

            #region Header
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

            bool isSSE = version >= 12;
            //if (version != 12) throw new Exception("Expected Special Edition save file.");

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

            /* Header end */
            nHeader.DataEnd = stream.Position - 1;
            #endregion

            /* Screenshot */
            pos = stream.Position;
            byte[] shotbuffer = new byte[4 * shotwidth * shotheight];
            if (stream.Read(shotbuffer, 0, shotbuffer.Length) != shotbuffer.Length)
                throw new Exception("Screenshot data ended earlier than expected.");
            for (int i = 0; i < shotbuffer.Length; i += 4)
            {
                ib[0] = shotbuffer[i];
                ib[1] = shotbuffer[i + 1];
                ib[2] = shotbuffer[i + 2];

                shotbuffer[i] = ib[2];
                shotbuffer[i + 1] = ib[1];
                shotbuffer[i + 2] = ib[0];
            }

            Bitmap bmp = new Bitmap((int)shotwidth, (int)shotheight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            BitmapData bdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            IntPtr pNative = bdata.Scan0;
            Marshal.Copy(shotbuffer, 0, pNative, shotbuffer.Length);

            bmp.UnlockBits(bdata);

            Bridge.AppendNode(
                new InfoNode("Screenshot data", "binary",
                    InfoType.Image,
                    bmp,
                    DataType.Critical,
                    pos, stream.Position - 1),
                root);

            /* 'The meat' */
            System.Windows.Forms.TreeNode parent = null;
            if (comp == 2)
            {
                /* Decomp size : uint32*/
                pos = stream.Position;
                uint decompsize = ReadUInt32(stream, ref ib);
                Bridge.AppendNode(
                    new InfoNode("Decompressed size", "int",
                        InfoType.Generic,
                        new GenericInfo("Decompressed size", decompsize.ToString()),
                        DataType.Critical,
                        pos, stream.Position - 1),
                    root);

                /* Comp size : uint32*/
                pos = stream.Position;
                uint compsize = ReadUInt32(stream, ref ib);
                Bridge.AppendNode(
                    new InfoNode("Compressed size", "int",
                        InfoType.Generic,
                        new GenericInfo("Compressed size", compsize.ToString()),
                        DataType.Critical,
                        pos, stream.Position - 1),
                    root);

                /* Decompressed data */
                pos = stream.Position;
                byte[] compdata = new byte[compsize];
                byte[] decompdata = new byte[decompsize];
                if (stream.Read(compdata, 0, compdata.Length) != compdata.Length)
                    throw new Exception("Compressed data ended earlier than expected.");

                LZ4Codec.Decode(compdata, 0, compdata.Length, decompdata, 0, decompdata.Length);
                InfoNode nDeComp =
                    new InfoNode("Decompressed data", "block-purple",
                        InfoType.BinaryMainFocus,
                        decompdata,
                        DataType.Critical,
                        pos, stream.Length - 1);
                Bridge.AppendNode(nDeComp, root);
                parent = nDeComp;

                stream = new MemoryStream(decompdata);
            }
            else
            {
                //pos = stream.Position;
                //InfoNode nUnComp =
                //    new InfoNode("Uncompressed data", "block-purple",
                //        InfoType.Generic,
                //        new GenericInfo("Uncompressed data", "Contains all save data in an uncompressed format."),
                //        DataType.Critical,
                //        pos, stream.Length - 1);
                //parent = nUnComp;
                parent = root;
            }

            /* Form version : uint8 */
            pos = stream.Position;
            uint formversion = ReadUInt8(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Form version", "int",
                    InfoType.Generic,
                    new GenericInfo("Form version", formversion.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            #region Plugin info
            /* Plugin info */
            pos = stream.Position;
            InfoNode nPluginInfo = new InfoNode("Plugin info", "block",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    pos, 0);
            Bridge.AppendNode(nPluginInfo, parent);

            /* Plugin info size : uint32 */
            pos = stream.Position;
            uint plugininfosize = ReadUInt32(stream, ref ib);
            Bridge.AppendNode(new InfoNode("Plugin info size", "int",
                    InfoType.Generic,
                    new GenericInfo("Plugin info size", plugininfosize.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nPluginInfo);

            /* Plugin count : uint8 */
            pos = stream.Position;
            uint plugincount = ReadUInt8(stream, ref ib);
            Bridge.AppendNode(
                new InfoNode("Plugin count", "int",
                    InfoType.Generic,
                    new GenericInfo("Plugin count", plugincount.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                nPluginInfo);

            /* Plugins */
            pos = stream.Position;
            InfoNode nPlugins = new InfoNode("Plugins", "info",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    pos, 0);
            Bridge.AppendNode(nPlugins, nPluginInfo);

            for (int i = 0; i < plugincount; i++)
            {
                pos = stream.Position;
                string plugin = ReadWString(stream, ref ib);
                Bridge.AppendNode(new InfoNode(plugin, "str", InfoType.None, null, DataType.Critical, pos, stream.Position - 1), nPlugins);
            }

            nPlugins.DataEnd = nPluginInfo.DataEnd = stream.Position - 1;
            #endregion

            #region Light plugin info
            if (isSSE)
            {
                /* Plugin info */
                pos = stream.Position;
                InfoNode nLightPluginInfo = new InfoNode("Light Plugin info", "block",
                        InfoType.None,
                        null,
                        DataType.Critical,
                        pos, 0);
                Bridge.AppendNode(nLightPluginInfo, parent);

                /* Plugin count : uint16 */
                pos = stream.Position;
                uint lightplugincount = ReadUInt16(stream, ref ib);
                Bridge.AppendNode(
                    new InfoNode("Plugin count", "int",
                        InfoType.Generic,
                        new GenericInfo("Plugin count", lightplugincount.ToString()),
                        DataType.Critical,
                        pos, stream.Position - 1),
                    nLightPluginInfo);

                /* Plugins */
                pos = stream.Position;
                InfoNode nLightPlugins = new InfoNode("Plugins", "info",
                        InfoType.None,
                        null,
                        DataType.Critical,
                        pos, 0);
                Bridge.AppendNode(nLightPlugins, nLightPluginInfo);

                for (int i = 0; i < lightplugincount; i++)
                {
                    pos = stream.Position;
                    string plugin = ReadWString(stream, ref ib);
                    Bridge.AppendNode(new InfoNode(plugin, "str", InfoType.None, null, DataType.Critical, pos, stream.Position - 1), nPlugins);
                }

                nLightPlugins.DataEnd = nLightPluginInfo.DataEnd = stream.Position - 1;
            }
            #endregion

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

        uint ReadUInt8(Stream stream, ref byte[] buffer)
        {
            if (stream.Read(buffer, 0, 1) != 1)
                throw new Exception("Data ended earlier than expected.");

            return (uint)buffer[0];
        }

        public void Write(RootInfoNode root, Stream sout)
        {
            Bridge.SimpleWrite(root, sout);
        }
    }
}
