using FileOptics.Interface;
using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
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
            if (isSSE && formversion > 77) ///TODO: Is formversion > 77 correct in this if statement???
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
                    Bridge.AppendNode(new InfoNode(plugin, "str", InfoType.None, null, DataType.Critical, pos, stream.Position - 1), nLightPlugins);
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
                    InfoType.Table,
                    new TableInfo(View.Details, new string[] { "Change form type", "Count" }, null),
                    DataType.Critical,
                    pos, 0);
            Bridge.AppendNode(nCH, parent);

            Dictionary<ChangeFormType, int> formcount = new Dictionary<ChangeFormType, int>();
            ChangeFormType[] cftvals = (ChangeFormType[])Enum.GetValues(typeof(ChangeFormType));
            foreach (ChangeFormType c in cftvals)
                formcount[c] = 0;

            /* Scope - Form cataloging */
            {
                int i = 0;
                int oi = i;
                long opos = stream.Position;
                int ACHR = 0;
                bool ACHRwithBASEOBJECT = false;
                for (; i < changeFormCount; i++)
                {
                    pos = stream.Position;
                    InfoNode nCHEntry = new InfoNode("Change form", "block-trueblue",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            pos, 0);

                    ChangeForm form = ReadChangeForm(stream, nCHEntry, ref ib);
                    formcount[form.Type]++;

                    /* White list specific change forms against skipping */
                    if (i < 3
                        || form.RefID == 0x400014
                        || (form.Type == ChangeFormType.ACHR && ACHR++ < 3)
                        || (form.Type == ChangeFormType.ACHR && form.HasFlag(ChangeFormFlagACHR.CHANGE_REFR_BASEOBJECT) && !ACHRwithBASEOBJECT && (ACHRwithBASEOBJECT = true))
                        )
                    {
                        if (i - oi > 1) //At this point if i - oi == 1, the previous record was accounted for in the structure tree, we didn't skip anything.
                            Bridge.AppendNode(
                                new InfoNode($"Skipping {i - oi - 1} entries...", "info",
                                    InfoType.None,
                                    null,
                                    DataType.Critical,
                                    opos, stream.Position - 1),
                                nCH);

                        opos = stream.Position;
                        oi = i;

                        nCHEntry.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nCHEntry, nCH);
                    }
                }
                if (i - oi > 0) //At this point if i - oi == 1, a record WAS actually skipped
                {
                    Bridge.AppendNode(
                        new InfoNode($"Skipping {i - oi - 1} entries...", "info",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            opos, stream.Position - 1),
                        nCH);
                }
            }

            /* Scope - Parsing */
            {
                long lastpos = stream.Position;

                foreach (TreeNode n in nCH.Nodes)
                {
                    if (!(n is InfoNode)) continue;
                    InfoNode info = (InfoNode)n;
                    if (info.SecondaryInfo.Count < 1) continue;
                    ChangeForm form = (ChangeForm)info.SecondaryInfo[0];

                    ((GenericInfo)((InfoNode)info.Nodes[1]).Info).Body += ParseChangeFlags(form.ChangeFlags, form.Type);

                    switch (form.Type)
                    {
                        case ChangeFormType.ACHR:
                            ReadACHRForm(stream, form, (InfoNode)n.LastNode, ref ib);
                            break;
                    }
                }

                stream.Seek(lastpos, SeekOrigin.Begin);
            }

            /* Scope - Form count */
            {
                int i = 0, total = 0;
                ListViewItem[] items = new ListViewItem[cftvals.Length + 1];

                foreach (KeyValuePair<ChangeFormType, int> kv in formcount)
                {
                    total += kv.Value;
                    items[i++] = new ListViewItem(new string[] { Enum.GetName(typeof(ChangeFormType), kv.Key), kv.Value.ToString() });
                }

                items[i++] = new ListViewItem(new string[] { "", $"{total} total" });

                ((TableInfo)nCH.Info).Items = items;
            }

            nCH.DataEnd = stream.Position - 1;
            #endregion

            /* Global data table 3 */
            ReadGlobalDataTable(stream, parent, 3, globalDataTable3Count, ref ib);

            /* FormID array */
            pos = stream.Position;
            InfoNode nFIA = new InfoNode("FormID Array", "block",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    pos, 0);

            uint fidcount = ReadUInt32Basic(stream, "Array count", nFIA, ref ib); //broken???

            {
                int i = 0;
                for (; i < fidcount && i < 10; i++)
                    ReadFormIDBasic(stream, "FormID", nFIA, ref ib);
                if (i < fidcount)
                {
                    uint skipcount = (uint)(fidcount - i) - 1;
                    if(skipcount > 0)
                        SkipBasic(stream, $"Skipping {skipcount} entries...", "", nFIA, skipcount * 4);
                    ReadFormIDBasic(stream, "FormID", nFIA, ref ib);
                }
            }

            nFIA.DataEnd = stream.Position - 1;
            Bridge.AppendNode(nFIA, parent);

            return true;
        }

        void ReadACHRForm(Stream stream, ChangeForm form, InfoNode parent, ref byte[] buffer)
        {
            stream.Seek(parent.DataStart, SeekOrigin.Begin);

            long pos;
            if (form.HasFlag(ChangeFormFlagACHR.CHANGE_REFR_MOVE))
            {
                pos = stream.Position;
                InfoNode nRef = new InfoNode("CHANGE_REFR_MOVE", "block-trueblue",
                        InfoType.None,
                        null,
                        DataType.Critical,
                        pos, 0);

                ReadRefIDBasic(stream, "World cell", nRef, ref buffer);
                ReadFloatBasic(stream, "Position 1 (X?)", nRef, ref buffer);
                ReadFloatBasic(stream, "Position 2 (Y?)", nRef, ref buffer);
                ReadFloatBasic(stream, "Position 3 (Z?)", nRef, ref buffer);
                ReadFloatBasic(stream, "Rotation 1 (Pitch?)", nRef, ref buffer);
                ReadFloatBasic(stream, "Rotation 2 (Roll?)", nRef, ref buffer);
                ReadFloatBasic(stream, "Rotation 3 (Yaw?)", nRef, ref buffer);

                nRef.DataEnd = stream.Position - 1;
                Bridge.AppendNode(nRef, parent);
            }
            if (form.HasFlag(ChangeFormFlagACHR.CHANGE_REFR_PROMOTED))
            {
                pos = stream.Position;
                InfoNode nRef = new InfoNode("CHANGE_REFR_PROMOTED", "block-trueblue",
                        InfoType.None,
                        null,
                        DataType.Critical,
                        pos, 0);

                ReadRefIDBasic(stream, "World cell", nRef, ref buffer);
                ReadUInt16Basic(stream, "Unknown signed integer", nRef, ref buffer);
                ReadUInt16Basic(stream, "Unknown signed integer", nRef, ref buffer);

                nRef.DataEnd = stream.Position - 1;
                Bridge.AppendNode(nRef, parent);
            }
            if (form.HasFlag(ChangeFormFlagACHR.CHANGE_REFR_HAVOK_MOVE))
            {
                pos = stream.Position;
                InfoNode nRef = new InfoNode("CHANGE_REFR_HAVOK_MOVE", "block-trueblue",
                        InfoType.None,
                        null,
                        DataType.Critical,
                        pos, 0);

                uint datalen = ReadVarLenBasic(stream, "Data length", nRef, ref buffer);
                SkipBasic(stream, "Data", "", nRef, datalen);

                nRef.DataEnd = stream.Position - 1;
                Bridge.AppendNode(nRef, parent);
            }

            /* Unknown data */
            ReadUInt32Basic(stream, "Unknown int", parent, ref buffer);
            ReadUInt32Basic(stream, "Unknown data", parent, ref buffer);

            if (form.HasFlag(ChangeFormFlagACHR.CHANGE_FORM_FLAGS))
            {
                pos = stream.Position;
                InfoNode nRef = new InfoNode("CHANGE_FORM_FLAGS", "block-trueblue",
                        InfoType.None,
                        null,
                        DataType.Critical,
                        pos, 0);

                uint extraflags = ReadChangeFormFlags(stream, "Flags", nRef, ref buffer);
                ReadUInt16Basic(stream, "Unknown short", nRef, ref buffer);

                nRef.DataEnd = stream.Position - 1;
                Bridge.AppendNode(nRef, parent);
            }
            if (form.HasFlag(ChangeFormFlagACHR.CHANGE_REFR_SCALE))
            {
                ReadFloatBasic(stream, "CHANGE_REFR_SCALE", parent, ref buffer);
            }

            uint hasextradata =
                ((uint)ChangeFormFlagACHR.CHANGE_REFR_EXTRA_OWNERSHIP)
                | ((uint)ChangeFormFlagACHR.CHANGE_REFR_PROMOTED)
                | ((uint)ChangeFormFlagACHR.CHANGE_ACTOR_EXTRA_PACKAGE_DATA)
                | ((uint)ChangeFormFlagACHR.CHANGE_ACTOR_EXTRA_MERCHANT_CONTAINER)
                | ((uint)ChangeFormFlagACHR.CHANGE_ACTOR_EXTRA_DISMEMBERED_LIMBS)
                | ((uint)ChangeFormFlagACHR.CHANGE_REFR_EXTRA_ACTIVATING_CHILDREN)
                | ((uint)ChangeFormFlagACHR.CHANGE_REFR_EXTRA_ENCOUNTER_ZONE)
                | ((uint)ChangeFormFlagACHR.CHANGE_REFR_EXTRA_CREATED_ONLY)
                | (unchecked((uint)ChangeFormFlagACHR.CHANGE_REFR_EXTRA_GAME_ONLY))
                ;

            if ((form.ChangeFlags & hasextradata) != 0)
            {
                pos = stream.Position;
                InfoNode nExtraBlock = new InfoNode("EXTRA data array", "block-trueblue",
                        InfoType.None,
                        null,
                        DataType.Critical,
                        pos, 0);

                uint count = ReadVarLenBasic(stream, "Extra data count", nExtraBlock, ref buffer);

                for (int i = 0; i < count; i++)
                {
                    pos = stream.Position;
                    InfoNode nExtra = new InfoNode("Extra data block", "block-trueblue",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            pos, 0);

                    ReadExtraDataEntry(stream, nExtra, ref buffer);

                    nExtra.DataEnd = stream.Position - 1;
                    Bridge.AppendNode(nExtra, nExtraBlock);
                }

                nExtraBlock.DataEnd = stream.Position - 1;
                Bridge.AppendNode(nExtraBlock, parent);
            }
            if (form.HasFlag(ChangeFormFlagACHR.CHANGE_REFR_INVENTORY) || form.HasFlag(ChangeFormFlagACHR.CHANGE_REFR_LEVELED_INVENTORY))
            {
                pos = stream.Position;
                InfoNode nRef = new InfoNode("Inventory", "block-trueblue",
                        InfoType.None,
                        null,
                        DataType.Critical,
                        pos, 0);

                uint count = ReadVarLenBasic(stream, "Item count", nRef, ref buffer);

                for (int i = 0; i < count; i++)
                {
                    pos = stream.Position;
                    InfoNode nItem = new InfoNode("Item", "block-trueblue",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            pos, 0);

                    uint itemid = ReadRefIDBasic(stream, "Item ID", nItem, ref buffer);
                    ReadSInt32Basic(stream, "Item count (signed uint32)", nItem, ref buffer);

                    uint excount = ReadVarLenBasic(stream, "Extra data array count", nItem, ref buffer);
                    for (int j = 0; j < excount; j++)
                    {
                        pos = stream.Position;
                        InfoNode nExtraBlock = new InfoNode("EXTRA data array", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        uint exdatacount = ReadVarLenBasic(stream, "Extra data count", nExtraBlock, ref buffer);

                        for (int k = 0; k < exdatacount; k++)
                        {
                            pos = stream.Position;
                            InfoNode nExtra = new InfoNode("Extra data block", "block-trueblue",
                                    InfoType.None,
                                    null,
                                    DataType.Critical,
                                    pos, 0);

                            ReadExtraDataEntry(stream, nExtra, ref buffer);

                            nExtra.DataEnd = stream.Position - 1;
                            Bridge.AppendNode(nExtra, nExtraBlock);
                        }

                        nExtraBlock.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nExtraBlock, nItem);
                    }

                    nItem.DataEnd = stream.Position - 1;
                    Bridge.AppendNode(nItem, nRef);
                }

                nRef.DataEnd = stream.Position - 1;
                Bridge.AppendNode(nRef, parent);
            }
        }

        private void ReadExtraDataEntry(Stream stream, InfoNode parent, ref byte[] buffer)
        {
            uint extype = ReadUInt8Basic(stream, "Data type", parent, ref buffer);

            uint count;
            long pos;
            switch (extype)
            {
                case 22:
                    parent.Text = "Worn";
                    break;
                case 23:
                    parent.Text = "WornLeft";
                    break;
                case 24:
                    parent.Text = "PackageStartLocation";

                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);

                    break;
                case 25:
                    parent.Text = "Package";

                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int", parent, ref buffer);
                    SkipBasic(stream, "Unknown data", "", parent, 3);

                    break;
                case 26:
                    parent.Text = "TresPassPackage";

                    uint id = ReadRefIDBasic(stream, "RefID", parent, ref buffer);

                    if (id > 0) { } ///TODO: EXTRA TresPassPackage

                    break;
                case 27:
                    parent.Text = "RunOncePacks";

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("Array item", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadUInt8Basic(stream, "Unknown byte", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    break;
                case 28:
                    parent.Text = "ReferenceHandle";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 29:
                    parent.Text = "Unknown29";
                    break;
                case 30:
                    parent.Text = "LevCreaModifier";
                    ReadUInt32Basic(stream, "Mod", parent, ref buffer);
                    break;
                case 31:
                    parent.Text = "Ghost";
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    break;
                case 33:
                    parent.Text = "Ownership";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 34:
                    parent.Text = "Global";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 35:
                    parent.Text = "Rank";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 36:
                    parent.Text = "Count";
                    ReadUInt16Basic(stream, "Count", parent, ref buffer);
                    break;
                case 37:
                    parent.Text = "Health";
                    ReadFloatBasic(stream, "Health", parent, ref buffer);
                    break;
                case 39:
                    parent.Text = "TimeLeft";
                    ReadUInt32Basic(stream, "Time", parent, ref buffer);
                    break;
                case 40:
                    parent.Text = "Charge";
                    ReadFloatBasic(stream, "Charge", parent, ref buffer);
                    break;
                case 42:
                    parent.Text = "Lock";

                    SkipBasic(stream, "Unknown data", "", parent, 2);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int", parent, ref buffer);

                    break;
                case 43:
                    parent.Text = "Teleport";

                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);

                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);

                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);

                    break;
                case 44:
                    parent.Text = "MapMarker";
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    break;
                case 45:
                    parent.Text = "LeveledCreature";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt32Basic(stream, "NPC change flags", parent, ref buffer);
                    break;
                case 46:
                    parent.Text = "LeveledItem";
                    ReadUInt32Basic(stream, "Unknown int", parent, ref buffer);
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    break;
                case 47:
                    parent.Text = "Scale";
                    ReadFloatBasic(stream, "Scale", parent, ref buffer);
                    break;
                case 49:
                    parent.Text = "NonActorMagicCaster";

                    pos = stream.Position;
                    InfoNode nCaster = new InfoNode("NonActorMagicCasterData", "block-trueblue",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            pos, 0);

                    ReadUInt32Basic(stream, "Unknown int", nCaster, ref buffer);
                    ReadRefIDBasic(stream, "RefID", nCaster, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int", nCaster, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int", nCaster, ref buffer);
                    ReadRefIDBasic(stream, "RefID", nCaster, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", nCaster, ref buffer);

                    nCaster.DataEnd = stream.Position - 1;
                    Bridge.AppendNode(nCaster, parent);

                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);

                    break;
                case 50:
                    parent.Text = "NonActorMagicTarget";

                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);

                    pos = stream.Position;
                    InfoNode nTarget = new InfoNode("NonActorMagicCasterData", "block-trueblue",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            pos, 0);

                    count = ReadVarLenBasic(stream, "Item count", nTarget, ref buffer);

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("MagicTarget", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadUInt8Basic(stream, "Unknown byte", nItem, ref buffer);
                        ReadVarLenBasic(stream, "Unknown vsval", nItem, ref buffer);
                        uint datalength = ReadVarLenBasic(stream, "Data length", nItem, ref buffer);
                        SkipBasic(stream, "Data", "", nItem, datalength);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, nTarget);
                    }

                    nTarget.DataEnd = stream.Position - 1;
                    Bridge.AppendNode(nTarget, parent);

                    break;
                case 52:
                    parent.Text = "PlayerCrimeList";

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("Crime", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);
                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    break;
                case 56:
                    parent.Text = "ItemDropper";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 61:
                    parent.Text = "CannotWear";
                    break;
                case 62:
                    parent.Text = "ExtraPoison";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int", parent, ref buffer);
                    break;
                case 68:
                    parent.Text = "FriendHits";

                    count = ReadVarLenBasic(stream, "Float count", parent, ref buffer);

                    for (int i = 0; i < count; i++)
                        ReadFloatBasic(stream, "Unknown float", parent, ref buffer);

                    break;
                case 69:
                    parent.Text = "HeadingTarget";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 72:
                    parent.Text = "StartingWorldOrCell";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 73:
                    parent.Text = "Hotkey";
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    break;
                case 76:
                    parent.Text = "InfoGeneralTopic";
                    ReadWStringBasic(stream, "Unknown string", parent, ref buffer);
                    SkipBasic(stream, "Unknown data", "", parent, 5);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 77:
                    parent.Text = "HasNoRumors";
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    break;
                case 79:
                    parent.Text = "TerminalState";
                    SkipBasic(stream, "Unknown data", "", parent, 2);
                    break;
                case 83:
                    parent.Text = "unknown83";
                    ReadUInt32Basic(stream, "Unknown int", parent, ref buffer);
                    break;
                case 84:
                    parent.Text = "CanTalkToPlayer";
                    ReadUInt8Basic(stream, "Flag", parent, ref buffer);
                    break;
                case 85:
                    parent.Text = "ObjectHealth";
                    ReadFloatBasic(stream, "Health", parent, ref buffer);
                    break;
                case 88:
                    parent.Text = "ModelSwap";
                    ReadRefIDBasic(stream, "ModelID", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int", parent, ref buffer);
                    break;
                case 89:
                    parent.Text = "Radius";
                    ReadUInt32Basic(stream, "Radius", parent, ref buffer);
                    break;
                case 91:
                    parent.Text = "FactionChanges";

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("Faction change", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadRefIDBasic(stream, "Faction ID", nItem, ref buffer);
                        ReadSInt8Basic(stream, "Rank", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    ReadRefIDBasic(stream, "Faction ID 2", parent, ref buffer);
                    ReadSInt8Basic(stream, "Rank 2", parent, ref buffer);

                    break;
                case 92:
                    parent.Text = "DismemberedLimbs";

                    ReadUInt16Basic(stream, "Unknown int16", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("DismemberedLimbData", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        SkipBasic(stream, "Unknown data", "", nItem, 4);
                        uint count2 = ReadVarLenBasic(stream, "RefID count", nItem, ref buffer);

                        for (int j = 0; j < count2; j++)
                            ReadRefIDBasic(stream, "RefID", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    break;
                case 93:
                    parent.Text = "ActorCause";
                    ReadUInt32Basic(stream, "Actor Cause ID", parent, ref buffer);
                    break;
                case 101:
                    parent.Text = "CombatStyle";
                    ReadRefIDBasic(stream, "Combat style", parent, ref buffer);
                    break;
                case 104:
                    parent.Text = "OpenCloseActivateRef";
                    ReadRefIDBasic(stream, "Ref ID", parent, ref buffer);
                    break;
                case 106:
                    parent.Text = "Ammo";
                    ReadRefIDBasic(stream, "Ammo ID", parent, ref buffer);
                    ReadUInt32Basic(stream, "Ammo count?", parent, ref buffer);
                    break;
                case 108:
                    parent.Text = "PackageData";
                    count = ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    if (count > 0) { } ///TODO: EXTRA PackageData
                    break;
                case 111:
                    parent.Text = "SayTopicInfoOnceADay";

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("Array item", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadRefIDBasic(stream, "Info ID", nItem, ref buffer);
                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);
                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    break;
                case 112:
                    parent.Text = "EncounterZone";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 113:
                    parent.Text = "SayToTopicInfo";

                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("SayToTopicInfoData", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadWStringBasic(stream, "Text 1", nItem, ref buffer);
                        ReadWStringBasic(stream, "Text 2", nItem, ref buffer);
                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);
                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);
                        ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    ReadUInt16Basic(stream, "Unknown int16", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);

                    break;
                case 120:
                    parent.Text = "GuardedRefData";

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("Array item", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);
                        ReadUInt8Basic(stream, "Unknown byte", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    break;
                case 133:
                    parent.Text = "AshPileRef";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 135:
                    parent.Text = "FollowerSwimBreadcrumbs";

                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("Array item", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadFloatBasic(stream, "Unknown float", nItem, ref buffer);
                        ReadFloatBasic(stream, "Unknown float", nItem, ref buffer);
                        ReadFloatBasic(stream, "Unknown float", nItem, ref buffer);
                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadFloatBasic(stream, "Unknown float", nItem, ref buffer);
                        ReadFloatBasic(stream, "Unknown float", nItem, ref buffer);
                        ReadFloatBasic(stream, "Unknown float", nItem, ref buffer);
                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadUInt8Basic(stream, "Unknown byte", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    break;
                case 136:
                    parent.Text = "AliasInstanceArray";

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("Array item", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    break;
                case 140:
                    parent.Text = "PromotedRef";

                    count = ReadVarLenBasic(stream, "RefID count", parent, ref buffer);

                    for (int j = 0; j < count; j++)
                        ReadRefIDBasic(stream, "RefID", parent, ref buffer);

                    break;
                case 142:
                    parent.Text = "OutfitItem";
                    ReadRefIDBasic(stream, "Item ID", parent, ref buffer);
                    break;
                case 146:
                    parent.Text = "SceneData";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 149:
                    parent.Text = "FromAlias";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);
                    break;
                case 150:
                    parent.Text = "ShouldWear";
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    break;
                case 152:
                    parent.Text = "AttachedArrows3D";

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("AttachedArrows3DData", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        uint RefID = ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        if (RefID > 0)
                        {
                            uint unk = ReadUInt16Basic(stream, "Unknown integer", nItem, ref buffer);

                            if (unk != 0xFFFF)
                                SkipBasic(stream, "Unknown data", "", nItem, 4 + 4 * 8);
                        }

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    ReadUInt32Basic(stream, "Unknown data", parent, ref buffer);

                    break;
                case 153:
                    parent.Text = "TextDisplayData";

                    uint fid1 = ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    uint fid2 = ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    int unks = ReadSInt32Basic(stream, "Unknown signed int32", parent, ref buffer);

                    if (unks == -2 && fid1 == 0 && fid2 == 0)
                        ReadWStringBasic(stream, "Text", parent, ref buffer);

                    break;
                case 155:
                    parent.Text = "Enchantment";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt16Basic(stream, "Unknown int16", parent, ref buffer);
                    break;
                case 156:
                    parent.Text = "Soul";
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    break;
                case 157:
                    parent.Text = "ForcedTarget";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 159:
                    parent.Text = "UniqueID";
                    ReadUInt32Basic(stream, "ID 32", parent, ref buffer);
                    ReadUInt16Basic(stream, "ID 16", parent, ref buffer);
                    break;
                case 160:
                    parent.Text = "Flags";
                    ReadUInt32Basic(stream, "Flags", parent, ref buffer);
                    break;
                case 161:
                    parent.Text = "RefrPath";

                    for (int i = 0; i < 3 * 6; i++)
                        ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    for (int i = 0; i < 4; i++)
                        ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);

                    break;
                case 164:
                    parent.Text = "ForcedLandingMarker";
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    break;
                case 169:
                    parent.Text = "Interaction";
                    ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadUInt8Basic(stream, "Unknown byte", parent, ref buffer);
                    break;
                case 174:
                    parent.Text = "GroupConstraint";
                    ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);
                    ReadRefIDBasic(stream, "RefID", parent, ref buffer);
                    ReadWStringBasic(stream, "String 1", parent, ref buffer);
                    ReadWStringBasic(stream, "String 2", parent, ref buffer);
                    for (int i = 0; i < 6; i++)
                        ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    ReadUInt32Basic(stream, "Unknown int32", parent, ref buffer);
                    ReadFloatBasic(stream, "Unknown float", parent, ref buffer);
                    break;
                case 175:
                    parent.Text = "ScriptedAnimDependence";

                    count = ReadVarLenBasic(stream, "Item count", parent, ref buffer);

                    pos = stream.Position;

                    for (int i = 0; i < count; i++)
                    {
                        pos = stream.Position;
                        InfoNode nItem = new InfoNode("Array item", "block-trueblue",
                                InfoType.None,
                                null,
                                DataType.Critical,
                                pos, 0);

                        ReadRefIDBasic(stream, "RefID", nItem, ref buffer);
                        ReadUInt32Basic(stream, "Unknown int32", nItem, ref buffer);

                        nItem.DataEnd = stream.Position - 1;
                        Bridge.AppendNode(nItem, parent);
                    }

                    break;
                case 176:
                    parent.Text = "CachedScale";
                    ReadFloatBasic(stream, "Scale 1", parent, ref buffer);
                    ReadFloatBasic(stream, "Scale 2", parent, ref buffer);
                    break;
                default:
                    parent.Text = "Unknown data type";
                    break;
            }
        }

        string ParseChangeFlags(uint flags, ChangeFormType type)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();

            Type flagenum = ChangeFormFlagEnums.GetEnum(type);
            uint[] flagset = (uint[])Enum.GetValues(flagenum);

            foreach (uint flag in flagset)
                if ((flags & flag) == flag)
                    sb.AppendLine(Enum.GetName(flagenum, (int)flag));

            return sb.ToString();
        }

        ChangeForm ReadChangeForm(Stream stream, TreeNode parent, ref byte[] buffer)
        {
            uint refid = ReadRefIDBasic(stream, "RefID", parent, ref buffer);
            uint changeflags = ReadChangeFormFlags(stream, "Change flags", parent, ref buffer);

            /* Type : uint8 */
            long pos = stream.Position;
            uint type = ReadUInt8(stream, ref buffer);
            int varlen = (int)(type >> 6);
            int rtype = (int)(type & 0x3F);

            string typename = Enum.GetName(typeof(ChangeFormType), (ChangeFormType)rtype);
            if (parent != null) parent.Text = refid == 0x400014 ? typename + " (Player)" : typename;

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
                new InfoNode("Decompressed length", "int",
                    InfoType.Generic,
                    new GenericInfo("Decompressed length", decomplen.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            /* Data */
            SkipBasic(stream, "Change form data", "", parent, datalen);

            ChangeForm ret = new ChangeForm() { RefID = refid, ChangeFlags = changeflags, Type = (ChangeFormType)rtype };
            ((InfoNode)parent).SecondaryInfo.Add(ret);

            return ret;
        }

        private uint ReadChangeFormFlags(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            uint ret = ReadUInt32(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "int",
                    InfoType.Generic,
                    new GenericInfo(name, Convert.ToString(ret, 2).PadLeft(8 * 4, '0')),
                    DataType.Critical,
                    pos, stream.Position - 1)
                { SecondaryInfo = new List<object>(new object[] { ret }) },
                parent);

            return ret;
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
            if (num == 3)
            {
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

        int ReadSInt32Basic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            int ret = unchecked((int)ReadUInt32(stream, ref ib));

            Bridge.AppendNode(
                new InfoNode(name, "int",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }

        uint ReadUInt16Basic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            uint ret = ReadUInt16(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "int",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }

        uint ReadUInt8Basic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            uint ret = ReadUInt8(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "byte",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }


        int ReadSInt8Basic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            int ret = unchecked((int)ReadUInt8(stream, ref ib));

            Bridge.AppendNode(
                new InfoNode(name, "byte",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }

        float ReadFloatBasic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            float ret = ReadFloat(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "binary",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }

        string ReadWStringBasic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            string ret = ReadWString(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "str",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }

        uint ReadVarLenBasic(Stream stream, string name, InfoNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            uint ret = ReadVarLen(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "binary",
                    InfoType.Generic,
                    new GenericInfo(name, ret.ToString()),
                    DataType.Critical,
                    pos, stream.Position - 1),
                parent);

            return ret;
        }

        uint ReadVarLen(Stream stream, ref byte[] buffer)
        {
            if (stream.Read(buffer, 0, 1) != 1)
                throw new Exception("Data ended earlier than expected.");

            if ((buffer[0] & 0x03) == 0)
                return (uint)(buffer[0] >> 2);
            if ((buffer[0] & 0x03) == 1)
            {
                if (stream.Read(buffer, 1, 1) != 1)
                    throw new Exception("Data ended earlier than expected.");
                return (uint)(((uint)buffer[1] << 0x08 | (uint)buffer[0]) >> 2);
            }
            if ((buffer[0] & 0x03) == 2)
            {
                if (stream.Read(buffer, 1, 3) != 1)
                    throw new Exception("Data ended earlier than expected.");
                return (uint)(((uint)buffer[3] << 0x18 | (uint)buffer[2] << 0x10 | (uint)buffer[1] << 0x08 | (uint)buffer[0]) >> 2);
            }
            if ((buffer[0] & 0x03) == 3)
            {
                if (stream.Read(buffer, 1, 7) != 1)
                    throw new Exception("Data ended earlier than expected.");

                ulong val =
                  (ulong)buffer[7] << 0x38
                | (ulong)buffer[6] << 0x30
                | (ulong)buffer[5] << 0x28
                | (ulong)buffer[4] << 0x20
                | (ulong)buffer[3] << 0x18
                | (ulong)buffer[2] << 0x10
                | (ulong)buffer[1] << 0x08
                | (ulong)buffer[0];

                return (uint)(val >> 2); ///TODO: Update signature to ULONG
            }

            return 0;
        }

        uint ReadRefIDBasic(Stream stream, string name, TreeNode parent, ref byte[] ib)
        {
            long pos = stream.Position;
            uint ret = ReadRefID(stream, ref ib);

            Bridge.AppendNode(
                new InfoNode(name, "int",
                    InfoType.Generic,
                    new GenericInfo(name, $"{ret:X6}"),
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
                    new GenericInfo(name, $"{ret:X8}"),
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

        uint ReadRefID(Stream stream, ref byte[] buffer) //BIG ENDIAN
        {
            if (stream.Read(buffer, 0, 3) != 3)
                throw new Exception("Data ended earlier than expected.");

            return (uint)buffer[0] << 0x10 | (uint)buffer[1] << 0x08 | (uint)buffer[2];
        }

        uint ReadFormID(Stream stream, ref byte[] buffer) //BIG ENDIAN
        {
            return ReadUInt32(stream, ref buffer);
        }

        float ReadFloat(Stream stream, ref byte[] buffer)
        {
            if (stream.Read(buffer, 0, 4) != 4)
                throw new Exception("Data ended earlier than expected.");

            return FloatConverter.Convert((uint)buffer[3] << 0x18 | (uint)buffer[2] << 0x10 | (uint)buffer[1] << 0x08 | (uint)buffer[0]);
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
