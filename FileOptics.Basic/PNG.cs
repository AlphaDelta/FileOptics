using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

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

        System.Windows.Forms.Panel pInfoPalette = null;
        System.Windows.Forms.ListView lvInfoPalette = null;
        public bool Read(RootInfoNode root, Stream stream)
        {
            root.Info = root.FilePath;
            root.IType = InfoType.ImageFile;

            //root.ImageKey = root.SelectedImageKey = "image";

            Bridge.AppendNode(
                new InfoNode("Magic number", "binary",
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
                Private = (buffer[1] & 0x20) == 0x20;
                STC = (buffer[3] & 0x20) == 0x20;


                if (stream.Seek(length, SeekOrigin.Current) != start + length + 4 + 4)
                    throw new Exception("Data ended earlier than expected.");

                if (stream.Read(buffer, 0, 4) != 4) //CRC32r
                    throw new Exception("Data ended earlier than expected.");

                end = stream.Position - 1;

                StringBuilder sb = new StringBuilder();
                DataType dtype = (Ancillary ? DataType.Metadata : DataType.Critical);
                switch (name)
                {
                    case "IHDR":
                        sb.AppendLine("The IHDR chunk contains the header data of a PNG file. It contains information about the width, height, but depth, colour type, compression method, filter method, and interlacing method of the image."); break;
                    case "PLTE":
                        sb.AppendLine("The PLTE chunk contains an array of up to 256 8-bit RGB colours used for simple images that contain less than 256 colours in total."); break;
                    case "IDAT":
                        sb.AppendLine("The IDAT chunk typically contains the compressed image data. There may be more than one IDAT chunk, and if the image has been fully rendered before reaching a new IDAT chunk it will be ignored."); break;
                    case "IEND":
                        sb.AppendLine("The IEND chunk marks the end of a PNG file or data stream. It typically has no contents."); break;
                    case "tRNS":
                        sb.AppendLine("The tRNS chunk specifies a 16-bit RGB colour to be used as a completely transparent colour in truecolor images, a 16-bit grey color to be used as a completely transparent colour in greyscale images, or contains an array of up to 256 bytes controlling the alpha value of palette entries with the same index.");
                        dtype = DataType.Ancillary;
                        break;
                    case "cHRM":
                        sb.AppendLine("The cHRM chunk specifies chromatic data. If an sRGB or iCCP chunk is also present it will override a cHRM chunk.");
                        dtype = DataType.Ancillary;
                        break;
                    case "gAMA":
                        sb.AppendLine("The gAMA chunk controls the image gamma. It should contain a single 4 byte unsigned integer, representing gamma times 100,000.");
                        dtype = DataType.Ancillary;
                        break;
                    case "iCCP":
                        sb.AppendLine("The iCCP chunk contains an embedded ICC profile. The data includes a null-terminated string representing the profile's name, followed by a single byte for the compression method, and finally the compressed ICC profile.");
                        dtype = DataType.Ancillary;
                        break;
                    case "sRGB":
                        sb.AppendLine("The sRGB chunk specifies that the image samples conform to the sRGB colour space and contains a single byte specifying a rendering intent defined in ICC-1 and ICC-1A.");
                        dtype = DataType.Ancillary;
                        break;
                    case "sBIT":
                        sb.AppendLine("The sBIT chunk specifies the original the original amount of significant bits of the image. Its size in bytes should be equal to the amount of channels in the image.");
                        dtype = DataType.Ancillary;
                        break;
                    case "tEXt":
                        sb.AppendLine("The tEXT chunk is a metadata chunk that contains a null-terminated keyword of up to 80 characters (including the null character) followed by an arbitrary amount of Latin-1 text data defined in ISO-8859-1."); break;
                    case "zTXt":
                        sb.AppendLine("The zTXt chunk is a metadata chunk that contains a null-terminated keyword of up to 80 characters (including the null character) followed by a single byte specifying a compression method (currently only 0x00 is used, standardized as the same compression used for IDAT data), then followed by an arbitrary amount of compressed data."); break;
                    case "iTXt":
                        sb.AppendLine("The iTXt chunk is a metadata chunk that contains used to contain text data of an arbitrary encoding which may or may not be compressed."); break;
                    case "hIST":
                        sb.AppendLine("The hIST chunk is a metadata chunk that contains information about the frequency of palette colours, it contains an array of 16-bit integers and an entry for each palette entry."); break;
                    case "pHYs":
                        sb.AppendLine("The pHYs chunk specifies the intended pixel size or aspect ratio for displaying the image, its contents should be exactly 9 bytes in length.");
                        dtype = DataType.Ancillary;
                        break;
                    case "sPLT":
                        sb.AppendLine("The sPLT chunk specifies a suggested palette and contains a null-terminated palette name of up to 80 characters (including the null character) followed by a single byte specifying the bit depth, followed by an array, each entry contains an RGBA color as well as a 16-bit integer for frequency.");
                        dtype = DataType.Ancillary;
                        break;
                    case "bKGD":
                        sb.AppendLine("The bKGD chunk specifies a suggested background color to display the image against.");
                        dtype = DataType.Ancillary;
                        break;
                    case "tIME":
                        sb.AppendLine("The tIME chunk is a metadata chunk that contains the last modification time of the image. It consists of a 16-bit integer for the year, followed by a byte each for the month, day, hour, minute, and second."); break;
                    default:
                        sb.AppendLine("Unknown chunk type.");
                        break;
                }

                sb.AppendLine(Ancillary ? "\r\nThis chunk is ancillary." : "\r\nThis chunk is critical.");
                sb.AppendLine(Private ? "This chunk is a private chunk." : "This chunk is a public chunk.");
                sb.AppendLine(STC ? "This chunk is safe to copy." : "This chunk is not safe to copy.");
                sb.Append(String.Format("This chunk has a CRC32 of {0:X2}{1:X2}{2:X2}{3:X2}", buffer[0], buffer[1], buffer[2], buffer[3]));

                Bridge.AppendNode(
                    new InfoNode(name, "block",
                        InfoType.Generic,
                        new GenericInfo(name + " Chunk", sb.ToString()),
                        dtype,
                        start, end) { SelectedImageKey = "block-trueblue" },
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

            byte colortype = 0xFF;
            foreach (InfoNode n in root.Nodes)
            {
                if(n.Text == "Magic number") continue;

                int datastart = (int)(n.DataStart + 8);
                int datalength = (int)((n.DataEnd - 4) - datastart + 1);

                Bridge.AppendNode(new InfoNode("Chunk length", "int", InfoType.None, null, DataType.Critical, n.DataStart, n.DataStart + 3), n);
                Bridge.AppendNode(new InfoNode("Chunk name", "str", InfoType.None, null, DataType.Critical, n.DataStart + 4, n.DataStart + 7), n);

                if (datalength < 1)
                {
                    Checksum(n);
                    continue;
                }

                #region IHDR
                if (n.Text == "IHDR")
                {
                    if (datalength != 0x0D)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error", "error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse header information. IHDR chunk must have a length of 0x0D (13 decimal)."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] hdrb = new byte[0x0D];
                    stream.Read(hdrb, 0, 0x0D);

                    uint xdim = (uint)hdrb[0] << 0x18 | (uint)hdrb[1] << 0x10 | (uint)hdrb[2] << 0x08 | (uint)hdrb[3];
                    uint ydim = (uint)hdrb[4] << 0x18 | (uint)hdrb[5] << 0x10 | (uint)hdrb[6] << 0x08 | (uint)hdrb[7];
                    string ctype = null;
                    colortype = hdrb[0x09];
                    switch (colortype)
                    {
                        case 0x00:
                            ctype = "a greyscale image in which each pixel contains a single white sample";
                            break;
                        case 0x02:
                            ctype = "a truecolor image in which each pixel contans a red, green, and blue sample";
                            break;
                        case 0x03:
                            ctype = "an indexed-color image in which each pixel is a pointer to a palette entry";
                            break;
                        case 0x04:
                            ctype = "a greyscale image with an alpha channel in which each pixel contains a white, and alpha sample";
                            break;
                        case 0x06:
                            ctype = "a truecolor image with an alpha channel in which each pixel contains a red, green, blue, and alpha sample";
                            break;
                    }

                    InfoNode nin = new InfoNode("Header information", "info",
                        InfoType.None, null,
                        //InfoType.Generic,
                            //new GenericInfo(
                            //    "Header Information",
                            //    String.Format("The image has a width of {0} and a height of {1}.\r\nIt has a bitdepth of {2} bits per sample.\r\nIt is {3}.\r\nThe image data is compressed using {4} compression algorithm.\r\nA filter method of '{5}' is specified.\r\nThe image {6} interlaced.",
                            //        xdim,
                            //        ydim,
                            //        hdrb[0x08],
                            //        ctype,
                            //        hdrb[0x0A] == 0 ? "the DEFLATE" : "an unknown",
                            //        hdrb[0x0B],
                            //        hdrb[0x0C] == 0 ? "is not" : "is")
                            //),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4);
                    Bridge.AppendNode(nin, n);

                    Bridge.AppendNode(new InfoNode("Image width", "int", InfoType.Generic, new GenericInfo("Image Width", xdim.ToString() + "px"), DataType.Critical, n.DataStart + 8, n.DataStart + 11), nin);
                    Bridge.AppendNode(new InfoNode("Image height", "int", InfoType.Generic, new GenericInfo("Image Height", ydim.ToString() + "px"), DataType.Critical, n.DataStart + 12, n.DataStart + 15), nin);
                    Bridge.AppendNode(new InfoNode("Bit depth", "byte", InfoType.Generic, new GenericInfo("Bit Depth", hdrb[0x08].ToString() + " bits per sample"), DataType.Critical, n.DataStart + 16, n.DataStart + 16), nin);
                    Bridge.AppendNode(new InfoNode("Color type", "byte", InfoType.Generic, new GenericInfo("Color Type", "This is " + ctype), DataType.Critical, n.DataStart + 17, n.DataStart + 17), nin);
                    Bridge.AppendNode(new InfoNode("Compression method", "byte", InfoType.Generic, new GenericInfo("Compression Method", String.Format("This image uses {0} compression algorithm.", hdrb[0x0A] == 0 ? "the DEFLATE" : "an unknown")), DataType.Critical, n.DataStart + 18, n.DataStart + 18), nin);
                    Bridge.AppendNode(new InfoNode("Filter type", "byte", InfoType.Generic, new GenericInfo("Filter Type", hdrb[0x0B].ToString()), DataType.Critical, n.DataStart + 19, n.DataStart + 19), nin);
                    Bridge.AppendNode(new InfoNode("Interlacing method", "byte", InfoType.Generic, new GenericInfo("Interlacing Method", String.Format("This image is{0} interlaced.", hdrb[0x0C] == 0 ? " not" : "")), DataType.Critical, n.DataStart + 20, n.DataStart + 20), nin);
                }
                #endregion
                #region tEXt
                else if (n.Text == "tEXt")
                {
                    stream.Seek(datastart, SeekOrigin.Begin);
                    //this should never realistically be needed
                    byte[] tb = new byte[0x100];
                    int remaining = datalength, read = 0;
                    bool istext = false;
                    StringBuilder key = new StringBuilder();
                    StringBuilder text = new StringBuilder();
                    while (remaining > 0)
                    {
                        read = stream.Read(tb, 0, remaining > 0x100 ? 0x100 : remaining);
                        remaining -= read;

                        if (istext)
                            text.Append(Encoding.ASCII.GetString(tb, 0, read));
                        else
                        {
                            for (int i = 0; i < read; i++)
                                if (tb[i] == 0x00)
                                {
                                    istext = true;
                                    if(i > 0)
                                        key.Append(Encoding.ASCII.GetString(tb, 0, i));
                                    if (i < read - 1)
                                        text.Append(Encoding.ASCII.GetString(tb, i + 1, read - i - 1));
                                }
                            if (istext)
                                continue;
                            key.Append(Encoding.ASCII.GetString(tb, 0, read));
                        }
                    }

                    Bridge.AppendNode(
                        new InfoNode("Standard text information", "info",
                            InfoType.Generic,
                            new GenericInfo(key.ToString(), text.ToString()),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }
                #endregion
                #region tIME
                else if (n.Text == "tIME")
                {
                    if (datalength != 0x07)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error", "error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse time information. tIME chunk must have a length of 0x07."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] tb = new byte[0x07];
                    stream.Read(tb, 0, 0x07);

                    //ushort year = (ushort);
                    DateTime time = new DateTime((tb[0] << 0x08 | tb[1]), tb[2], tb[3], tb[4], tb[5], tb[6], DateTimeKind.Utc);
                    //DateTime ltime = time.ToLocalTime(); //Thanks a lot imagemagick for completely ignoring the PNG standard and using the local time instead of UTC

                    Bridge.AppendNode(
                        new InfoNode("Last modification time information", "info",
                            InfoType.Generic,
                            new GenericInfo(
                                "Last Modification Time",
                                String.Format("The last modification time of this image has been specified as {0} UTC.\r\n\r\nFormatted (F): {1}",
                                    time.ToString("yyyy-MM-dd h:mm:sstt"),
                                    time.ToString("F"))
                            ),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }
                #endregion
                #region gAMA
                else if (n.Text == "gAMA")
                {
                    if (datalength != 0x04)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error", "error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse gamma information. gAMA chunk must have a length of 0x01."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] gamab = new byte[0x04];
                    stream.Read(gamab, 0, 0x04);

                    uint ydim = (uint)gamab[0] << 0x18 | (uint)gamab[1] << 0x10 | (uint)gamab[2] << 0x08 | (uint)gamab[3];

                    float gamma = ydim / 100000f;

                    Bridge.AppendNode(
                        new InfoNode("Gamma information", "info",
                            InfoType.Generic,
                            new GenericInfo("Gamma Information", String.Format("This chunk specifies the desired image gamma at '{0}'.", gamma)),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }
                #endregion
                #region PLTE
                else if (n.Text == "PLTE")
                {
                    if (datalength > 0x300 || datalength % 3 != 0)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error", "error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse palette information. PLTE chunk must have a length less than 0x300 an divisible by three."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] paletteb = new byte[datalength];
                    stream.Read(paletteb, 0, datalength);

                    List<ListViewItem> entries = new List<ListViewItem>();
                    int index = 0;
                    for (int i = 0; i < datalength; index++)
                    {
                        int r = paletteb[i++];
                        int g = paletteb[i++];
                        int b = paletteb[i++];

                        System.Windows.Forms.ListViewItem lvi = new System.Windows.Forms.ListViewItem(index.ToString()) { BackColor = Color.FromArgb(r, g, b) };
                        lvi.SubItems.Add(r.ToString());
                        lvi.SubItems.Add(g.ToString());
                        lvi.SubItems.Add(b.ToString());
                        lvi.ForeColor = r * 0.299 + g * 0.587 + b * 0.114 <= 186 ? Color.White : Color.Black;
                        entries.Add(lvi);
                    }

                    Bridge.AppendNode(
                        new InfoNode("Color palette", "info",
                            InfoType.Table,
                            new TableInfo(View.Details, new string[] { "Index", "Red", "Green", "Blue" }, entries.ToArray()) { ResizeStyle = ColumnHeaderAutoResizeStyle.HeaderSize },
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }
                #endregion
                #region cHRM
                else if (n.Text == "cHRM")
                {
                    if (datalength != 0x20)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error", "error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse chromatic information. cHRM chunk must have a length of 0x20."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] chrmb = new byte[0x20];
                    stream.Read(chrmb, 0, 0x20);

                    uint
                    uwx = (uint)chrmb[0x00] << 0x18 | (uint)chrmb[0x01] << 0x10 | (uint)chrmb[0x02] << 0x08 | (uint)chrmb[0x03],
                    uwy = (uint)chrmb[0x04] << 0x18 | (uint)chrmb[0x05] << 0x10 | (uint)chrmb[0x06] << 0x08 | (uint)chrmb[0x07],
                    urx = (uint)chrmb[0x08] << 0x18 | (uint)chrmb[0x09] << 0x10 | (uint)chrmb[0x0A] << 0x08 | (uint)chrmb[0x0B],
                    ury = (uint)chrmb[0x0C] << 0x18 | (uint)chrmb[0x0D] << 0x10 | (uint)chrmb[0x0E] << 0x08 | (uint)chrmb[0x0F],
                    ugx = (uint)chrmb[0x10] << 0x18 | (uint)chrmb[0x11] << 0x10 | (uint)chrmb[0x12] << 0x08 | (uint)chrmb[0x13],
                    ugy = (uint)chrmb[0x14] << 0x18 | (uint)chrmb[0x15] << 0x10 | (uint)chrmb[0x16] << 0x08 | (uint)chrmb[0x17],
                    ubx = (uint)chrmb[0x18] << 0x18 | (uint)chrmb[0x19] << 0x10 | (uint)chrmb[0x1A] << 0x08 | (uint)chrmb[0x1B],
                    uby = (uint)chrmb[0x1C] << 0x18 | (uint)chrmb[0x1D] << 0x10 | (uint)chrmb[0x1E] << 0x08 | (uint)chrmb[0x1F];

                    float
                    wx = (uwx > 0 ? uwx / 100000f : 0),
                    wy = (uwy > 0 ? uwy / 100000f : 0),
                    rx = (urx > 0 ? urx / 100000f : 0),
                    ry = (ury > 0 ? ury / 100000f : 0),
                    gx = (ugx > 0 ? ugx / 100000f : 0),
                    gy = (ugy > 0 ? ugy / 100000f : 0),
                    bx = (ubx > 0 ? ubx / 100000f : 0),
                    by = (uby > 0 ? uby / 100000f : 0);

                    InfoNode cn = new InfoNode("Chromatic information", "info",
                            InfoType.Table,
                            new TableInfo(View.Details, new string[] { "Key", "Value" }, new ListViewItem[] {
                                new ListViewItem(new string[] { "White X", wx.ToString() }),
                                new ListViewItem(new string[] { "White Y", wy.ToString() }),
                                new ListViewItem(new string[] { "Red X", rx.ToString() }),
                                new ListViewItem(new string[] { "Red Y", ry.ToString() }),
                                new ListViewItem(new string[] { "Green X", gx.ToString() }),
                                new ListViewItem(new string[] { "Green Y", gy.ToString() }),
                                new ListViewItem(new string[] { "Blue X", bx.ToString() }),
                                new ListViewItem(new string[] { "Blue Y", by.ToString() })
                            }),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4);

                    Bridge.AppendNode(
                        cn,
                        n);

                    Bridge.AppendNode(new InfoNode("White X", "int", InfoType.None, null, DataType.Critical, n.DataStart + 0x08, n.DataStart + 0x0B), cn);
                    Bridge.AppendNode(new InfoNode("White Y", "int", InfoType.None, null, DataType.Critical, n.DataStart + 0x0C, n.DataStart + 0x0F), cn);
                    Bridge.AppendNode(new InfoNode("Red X", "int", InfoType.None, null, DataType.Critical, n.DataStart + 0x10, n.DataStart + 0x13), cn);
                    Bridge.AppendNode(new InfoNode("Red Y", "int", InfoType.None, null, DataType.Critical, n.DataStart + 0x14, n.DataStart + 0x17), cn);
                    Bridge.AppendNode(new InfoNode("Green X", "int", InfoType.None, null, DataType.Critical, n.DataStart + 0x18, n.DataStart + 0x1B), cn);
                    Bridge.AppendNode(new InfoNode("Green Y", "int", InfoType.None, null, DataType.Critical, n.DataStart + 0x1C, n.DataStart + 0x1F), cn);
                    Bridge.AppendNode(new InfoNode("Blue X", "int", InfoType.None, null, DataType.Critical, n.DataStart + 0x20, n.DataStart + 0x23), cn);
                    Bridge.AppendNode(new InfoNode("Blue Y", "int", InfoType.None, null, DataType.Critical, n.DataStart + 0x24, n.DataStart + 0x27), cn);
                }
                #endregion
                #region sBIT
                else if (n.Text == "sBIT")
                {
                    if (datalength < 0x01 || datalength > 0x04)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error", "error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse significant bits information. sBIT chunk must have a length between 0x01 and 0x04."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] sbitb = new byte[datalength];
                    stream.Read(sbitb, 0, datalength);

                    InfoNode sn = new InfoNode("Significant bits information", "info",
                            InfoType.None,
                            null,
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4);

                    Bridge.AppendNode(
                        sn,
                        n);

                    if (datalength < 3)
                    {
                        Bridge.AppendNode(new InfoNode("Significant greyscale bits", "byte", InfoType.Generic, new GenericInfo("Significant Greyscale Bits", sbitb[0].ToString() + " bits"), DataType.Critical, n.DataStart + 8, n.DataStart + 8), sn);
                        if (datalength == 2)
                            Bridge.AppendNode(new InfoNode("Significant alpha bits", "byte", InfoType.Generic, new GenericInfo("Significant Alpha Bits", sbitb[1].ToString() + " bits"), DataType.Critical, n.DataStart + 9, n.DataStart + 9), sn);
                    }
                    else
                    {
                        Bridge.AppendNode(new InfoNode("Significant red bits", "byte", InfoType.Generic, new GenericInfo("Significant Red Bits", sbitb[0].ToString() + " bits"), DataType.Critical, n.DataStart + 8, n.DataStart + 8), sn);
                        Bridge.AppendNode(new InfoNode("Significant green bits", "byte", InfoType.Generic, new GenericInfo("Significant Green Bits", sbitb[1].ToString() + " bits"), DataType.Critical, n.DataStart + 9, n.DataStart + 9), sn);
                        Bridge.AppendNode(new InfoNode("Significant blue bits", "byte", InfoType.Generic, new GenericInfo("Significant Blue Bits", sbitb[2].ToString() + " bits"), DataType.Critical, n.DataStart + 10, n.DataStart + 10), sn);
                        if (datalength == 4)
                            Bridge.AppendNode(new InfoNode("Significant alpha bits", "byte", InfoType.Generic, new GenericInfo("Significant Alpha Bits", sbitb[3].ToString() + " bits"), DataType.Critical, n.DataStart + 11, n.DataStart + 11), sn);
                    }
                }
                #endregion
                else if (n.Text == "IDAT")
                {
                    Bridge.AppendNode(new InfoNode("Image Data", "binary", InfoType.None, null, DataType.Critical, n.DataStart + 8, n.DataEnd - 4), n);
                }

                /* *
                 * Chunks that absolutely require IHDR to have already been read.
                 * 
                 * The specification states that the IHDR must always be the first chunk,
                 * but we're a bit liberal with that; we're just parsing the image file,
                 * we dont actually need to display it thus we don't need it to adhere to
                 * the PNG specification we just need it to be coherent enough to read to EOF.
                 * */
                else if (colortype == 0xFF)
                {
                    Bridge.AppendNode(new InfoNode("Unknown Data", "unknown", InfoType.None, null, DataType.Critical, n.DataStart + 8, n.DataEnd - 4), n);
                    Checksum(n);
                    continue;
                }
                #region tRNS
                else if (n.Text == "tRNS")
                {
                    byte[] trnsb;
                    if (colortype == 0x03) //Indexed color
                    {
                        stream.Seek(datastart, SeekOrigin.Begin);
                        trnsb = new byte[datalength];
                        stream.Read(trnsb, 0, datalength);

                        List<ListViewItem> entries = new List<ListViewItem>();
                        for (int i = 0; i < datalength; i++)
                            entries.Add(new ListViewItem(new string[] { i.ToString(), trnsb[i].ToString() }));

                        Bridge.AppendNode(
                            new InfoNode("Transparency information (Paletted)", "info",
                                InfoType.Table,
                                new TableInfo(View.Details, new string[] { "Index", "Opacity" }, entries.ToArray()) { ResizeStyle = ColumnHeaderAutoResizeStyle.HeaderSize },
                                DataType.Critical,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }

                    int trnslen = colortype == 0x00 ? 2 : (colortype == 0x02 ? 6 : -1);

                    if (trnslen < 1)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error", "error",
                                InfoType.Generic,
                                new GenericInfo("Error", String.Format("Could not parse transparency information. The color type specified in the IHDR chunk is not supported.", trnslen, colortype)),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }
                    if (datalength != trnslen)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error", "error",
                                InfoType.Generic,
                                new GenericInfo("Error", String.Format("Could not parse transparency information. tRNS chunk must have a length of 0x{0:X2} with color type 0x{1:X2}.", trnslen, colortype)),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        Checksum(n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    trnsb = new byte[trnslen];
                    stream.Read(trnsb, 0, trnslen);

                    string body = "";
                    if (trnslen == 2)
                    {
                        uint grey = (uint)trnsb[0] << 0x08 | (uint)trnsb[1];
                        body = String.Format("transparent grey color as '{0}'", grey);
                    }
                    else if (trnslen == 6)
                    {
                        uint red = (uint)trnsb[0] << 0x08 | (uint)trnsb[1];
                        uint green = (uint)trnsb[2] << 0x08 | (uint)trnsb[3];
                        uint blue = (uint)trnsb[4] << 0x08 | (uint)trnsb[5];
                        body = String.Format("transparent RGB color as '{0}, {1}, {2}'", red, green, blue);
                    }
                    //uint ydim = (uint)gamab[0] << 0x18 | (uint)gamab[1] << 0x10 | (uint)gamab[2] << 0x08 | (uint)gamab[3];

                    Bridge.AppendNode(
                        new InfoNode("Transparency information", "info",
                            InfoType.Generic,
                            new GenericInfo("Transparency Information", String.Format("This chunk specifies the {0}.", body)),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }
                #endregion
                else
                {
                    Bridge.AppendNode(new InfoNode("Unknown Data", "unknown", InfoType.None, null, DataType.Critical, n.DataStart + 8, n.DataEnd - 4), n);
                }

                Checksum(n);
            }

            return true;
        }

        void Checksum(InfoNode n)
        {
            Bridge.AppendNode(new InfoNode("Chunk checksum", "int", InfoType.None, null, DataType.Critical, n.DataEnd - 3, n.DataEnd), n);
        }

        public void Write(RootInfoNode root, Stream sout)
        {
            Bridge.SimpleWrite(root, sout);
        }
    }
}
