using FileOptics.Interface;
using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FileOptics.TESV
{
    [ModuleAttrib("TESV Save File", 0x20, new byte[] { 0x54, 0x45, 0x53, 0x56, 0x5F, 0x53, 0x41, 0x56, 0x45, 0x47, 0x41, 0x4D, 0x45 })]
    public class ESS : IModule
    {
        const int MAX_CHANGEFORMS = 30;

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

            #region File location table
            /* File location table */
            pos = stream.Position;
            InfoNode nFLT = new InfoNode("File location table", "block",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    pos, 0);
            Bridge.AppendNode(nFLT, parent);

            uint formIDArrayCountOffset = ReadUInt32Basic(stream, "FormIDArrayCountOffset", nFLT, ref ib);
            uint unknownTable3Offset = ReadUInt32Basic(stream, "UnknownTable3Offset", nFLT, ref ib);
            uint globalDataTable1Offset = ReadUInt32Basic(stream, "GlobalDataTable1Offset", nFLT, ref ib);
            uint globalDataTable2Offset = ReadUInt32Basic(stream, "GlobalDataTable2Offset", nFLT, ref ib);
            uint changeFormsOffset = ReadUInt32Basic(stream, "ChangeFormsOffset", nFLT, ref ib);
            uint globalDataTable3Offset = ReadUInt32Basic(stream, "GlobalDataTable3Offset", nFLT, ref ib);
            uint globalDataTable1Count = ReadUInt32Basic(stream, "GlobalDataTable1Count", nFLT, ref ib);
            uint globalDataTable2Count = ReadUInt32Basic(stream, "GlobalDataTable2Count", nFLT, ref ib);
            uint globalDataTable3Count = ReadUInt32Basic(stream, "GlobalDataTable3Count", nFLT, ref ib);
            uint changeFormCount = ReadUInt32Basic(stream, "ChangeFormCount", nFLT, ref ib);

            pos = stream.Position;
            stream.Seek(15 * 4, SeekOrigin.Current);
            Bridge.AppendNode(
                new InfoNode("Unused", "binary",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    pos, stream.Position - 1),
                nFLT);

            nFLT.DataEnd = stream.Position - 1;
            #endregion

            /* Global data table 1 & 2 */
            ReadGlobalDataTable(stream, parent, 1, globalDataTable1Count, ref ib);
            ReadGlobalDataTable(stream, parent, 2, globalDataTable2Count, ref ib);

            #region Change forms
            pos = stream.Position;
            InfoNode nCH = new InfoNode("Change form table", "block",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    pos, 0);
            Bridge.AppendNode(nCH, parent);

            {
                int i, oi;
                for (i = 0; i < changeFormCount && i < MAX_CHANGEFORMS; i++)
                {
                    pos = stream.Position;
                    InfoNode nCHEntry = new InfoNode("Change form", "block-trueblue",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            pos, 0);
                    Bridge.AppendNode(nCHEntry, nCH);
                    ReadChangeForm(stream, nCHEntry, ref ib);
                    nCHEntry.DataEnd = stream.Position - 1;
                }
                pos = stream.Position;
                oi = i;
                for (; i < changeFormCount; i++)
                {
                    ReadChangeForm(stream, null, ref ib);
                }
                if (pos != stream.Position)
                {
                    Bridge.AppendNode(
                        new InfoNode($"Skipping {i - oi} entries...", "null",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            pos, stream.Position - 1),
                        nCH);
                }
            }

            nCH.DataEnd = stream.Position - 1;
            #endregion

            /* Global data table 3 */
            ReadGlobalDataTable(stream, parent, 3, globalDataTable3Count, ref ib);

            return true;
        }

        void ReadChangeForm(Stream stream, TreeNode parent, ref byte[] buffer)
        {
            uint formid = ReadFormIDBasic(stream, "Form ID", parent, ref buffer);
            uint changeflags = ReadUInt32Basic(stream, "Change flags", parent, ref buffer);

            /* Type : uint8 */
            long pos = stream.Position;
            uint type = ReadUInt8(stream, ref buffer);
            int varlen = (int)(type >> 6);
            int rtype = (int)(type & 0x3F);

            string varlens = "uint8";
            if (varlen == 1) varlens = "uint16";
            else if (varlen == 2) varlens = "uint32";
            else if (varlen == 3) varlens = "uint64";

            Bridge.AppendNode(
                new InfoNode("Type", "byte",
                    InfoType.Generic,
                    new GenericInfo("Type", $"Data length field type: {varlen} ({varlens})\r\nForm type: {rtype} (0x{rtype:X2})"),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);


            /* Version : uint8 */
            pos = stream.Position;
            uint version = ReadUInt8(stream, ref buffer);

            Bridge.AppendNode(
                new InfoNode("Version", "byte",
                    InfoType.Generic,
                    new GenericInfo("Version", version.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);


            /* Data length : varlen */
            pos = stream.Position;
            uint datalen = 0;
            if (varlen == 0) datalen = ReadUInt8(stream, ref buffer);
            else if (varlen == 1) datalen = ReadUInt16(stream, ref buffer);
            else if (varlen == 2) datalen = ReadUInt32(stream, ref buffer);

            Bridge.AppendNode(
                new InfoNode("Data length", "int",
                    InfoType.Generic,
                    new GenericInfo("Data length", datalen.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            /* Decompressed length : varlen */
            pos = stream.Position;
            uint decomplen = 0;
            if (varlen == 0) decomplen = ReadUInt8(stream, ref buffer);
            else if (varlen == 1) decomplen = ReadUInt16(stream, ref buffer);
            else if (varlen == 2) decomplen = ReadUInt32(stream, ref buffer);

            Bridge.AppendNode(
                new InfoNode("Decompiled length", "int",
                    InfoType.Generic,
                    new GenericInfo("Decompiled length", decomplen.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            /* Data */
            SkipBasic(stream, "Change form data", "", parent, datalen);
        }

        uint ReadVarLen()
        {
            return 0;
        }

        void ReadGlobalDataTable(Stream stream, TreeNode parent, uint num, uint count, ref byte[] buffer)
        {
            long pos = stream.Position;
            InfoNode nGDT1 = new InfoNode("Global data table " + num, "block",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    pos, 0);
            Bridge.AppendNode(nGDT1, parent);

            for (int i = 0; i < count; i++)
            {
                ReadGlobalDataEntry(stream, nGDT1, ref buffer);
            }

            /* Glitched counter for 1005 */
            if (num == 3) {
                if (ReadUInt32(stream, ref buffer) == 1005)
                {
                    stream.Seek(-4, SeekOrigin.Current);
                    ReadGlobalDataEntry(stream, nGDT1, "block-orange", ref buffer);
                }
                else stream.Seek(-4, SeekOrigin.Current);
            }

            nGDT1.DataEnd = stream.Position - 1;
        }

        private void ReadGlobalDataEntry(Stream stream, TreeNode parent, ref byte[] buffer) { ReadGlobalDataEntry(stream, parent, "block-trueblue", ref buffer); }
        private void ReadGlobalDataEntry(Stream stream, TreeNode parent, string imgkey, ref byte[] buffer)
        {
            long pos = stream.Position;
            InfoNode nEntry = new InfoNode("Entry", imgkey,
                    InfoType.None,
                    null,
                    DataType.Critical,
                    pos, 0);
            Bridge.AppendNode(nEntry, parent);

            uint type = ReadUInt32Basic(stream, "Entry type", nEntry, ref buffer);
            nEntry.Text = GetGlobalDataTypeName(type);
            uint len = ReadUInt32Basic(stream, "Entry length", nEntry, ref buffer);

            SkipBasic(stream, "Data", "", nEntry, len);

            nEntry.DataEnd = stream.Position - 1;
        }

        string GetGlobalDataTypeName(uint type)
        {
            switch (type)
            {
                case 0: return "Misc Stats";
                case 1: return "Player Location";
                case 2: return "TES";
                case 3: return "Global Variables";
                case 4: return "Created Objects";
                case 5: return "Effects";
                case 6: return "Weather";
                case 7: return "Audio";
                case 8: return "SkyCells";
                case 100: return "Process Lists";
                case 101: return "Combat";
                case 102: return "Interface";
                case 103: return "Actor Causes";
                case 104: return "Unknown 104";
                case 105: return "Detection Manager";
                case 106: return "Location MetaData";
                case 107: return "Quest Static Data";
                case 108: return "StoryTeller";
                case 109: return "Magic Favorites";
                case 110: return "PlayerControls";
                case 111: return "Story Event Manager";
                case 112: return "Ingredient Shared";
                case 113: return "MenuControls";
                case 114: return "MenuTopicManager";
                case 1000: return "Temp Effects";
                case 1001: return "Papyrus";
                case 1002: return "Anim Objects";
                case 1003: return "Timer";
                case 1004: return "Synchronized Animations";
                case 1005: return "Main";
                default: return "Unknown";
            }
        }

        void SkipBasic(Stream stream, string name, string desc, TreeNode parent, uint amount)
        {
            long pos = stream.Position;
            stream.Seek(amount, SeekOrigin.Current);

            Bridge.AppendNode(
                new InfoNode(name, "info",
                    InfoType.Generic,
                    new GenericInfo(name, desc),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);
        }

        uint ReadUInt32Basic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            uint ret = ReadUInt32(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "int",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }

        uint ReadFormIDBasic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            uint ret = ReadFormID(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "int",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }

        private string ReadWString(Stream stream, ref byte[] ib)
        {
            int size = (int)ReadUInt16(stream, ref ib);

            byte[] sb = new byte[size];
            if (stream.Read(sb, 0, size) != size)
                throw new Exception("Data ended earlier than expected.");

            return Encoding.ASCII.GetString(sb);
        }

        uint ReadFormID(Stream stream, ref byte[] buffer)
        {
            if (stream.Read(buffer, 0, 3) != 3)
                throw new Exception("Data ended earlier than expected.");

            return (uint)buffer[2] << 0x10 | (uint)buffer[1] << 0x08 | (uint)buffer[0];
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
