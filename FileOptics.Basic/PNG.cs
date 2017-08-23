using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        System.Windows.Forms.Panel pInfoPalette = null;
        System.Windows.Forms.ListView lvInfoPalette = null;
        public bool Read(RootInfoNode root, Stream stream)
        {
            root.Info = root.FilePath;
            root.IType = InfoType.ImageFile;

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
                    new InfoNode(name,
                        InfoType.Generic,
                        new GenericInfo(name + " Chunk", sb.ToString()),
                        dtype,
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

            byte colortype = 0xFF;
            foreach (InfoNode n in root.Nodes)
            {
                int datastart = (int)(n.DataStart + 8);
                int datalength = (int)((n.DataEnd - 4) - datastart + 1);

                if (datalength < 1) continue;

                if (n.Text == "IHDR")
                {
                    if (datalength != 0x0D)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse header information. IHDR chunk must have a length of 0x0D (13 decimal)."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
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

                    Bridge.AppendNode(
                        new InfoNode("Header information",
                            InfoType.Generic,
                            new GenericInfo(
                                "Header Information",
                                String.Format("The image has a width of {0} and a height of {1}.\r\nIt has a bitdepth of {2} bits per sample.\r\nIt is {3}.\r\nThe image data is compressed using {4} compression algorithm.\r\nA filter method of '{5}' is specified.\r\nThe image {6} interlaced.",
                                    xdim,
                                    ydim,
                                    hdrb[0x08],
                                    ctype,
                                    hdrb[0x0A] == 0 ? "the DEFLATE" : "an unknown",
                                    hdrb[0x0B],
                                    hdrb[0x0C] == 0 ? "is not" : "is")
                            ),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }
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
                        new InfoNode("Standard text information",
                            InfoType.Generic,
                            new GenericInfo(key.ToString(), text.ToString()),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }
                else if (n.Text == "tIME")
                {
                    if (datalength != 0x07)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse time information. tIME chunk must have a length of 0x07."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] tb = new byte[0x07];
                    stream.Read(tb, 0, 0x07);

                    //ushort year = (ushort);
                    DateTime time = new DateTime((tb[0] << 0x08 | tb[1]), tb[2], tb[3], tb[4], tb[5], tb[6], DateTimeKind.Utc);
                    //DateTime ltime = time.ToLocalTime(); //Thanks a lot imagemagick for completely ignoring the PNG standard and using the local time instead of UTC

                    Bridge.AppendNode(
                        new InfoNode("Last modification time information",
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
                else if (n.Text == "gAMA")
                {
                    if (datalength != 0x04)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse gamma information. gAMA chunk must have a length of 0x01."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] gamab = new byte[0x04];
                    stream.Read(gamab, 0, 0x04);

                    uint ydim = (uint)gamab[0] << 0x18 | (uint)gamab[1] << 0x10 | (uint)gamab[2] << 0x08 | (uint)gamab[3];

                    float gamma = ydim / 100000f;

                    Bridge.AppendNode(
                        new InfoNode("Gamma information",
                            InfoType.Generic,
                            new GenericInfo("Gamma Information", String.Format("This chunk specifies the desired image gamma at '{0}'.", gamma)),
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }
                else if (n.Text == "PLTE")
                {
                    if (datalength > 0x300 || datalength % 3 != 0)
                    {
                        Bridge.AppendNode(
                            new InfoNode("Error",
                                InfoType.Generic,
                                new GenericInfo("Error", "Could not parse palette information. PLTE chunk must have a length less than 0x300 an divisible by three."),
                                DataType.Error,
                                n.DataStart + 8, n.DataEnd - 4),
                            n);
                        continue;
                    }

                    stream.Seek(datastart, SeekOrigin.Begin);
                    byte[] paletteb = new byte[datalength];
                    stream.Read(paletteb, 0, datalength);

                    List<Color> entries = new List<Color>();
                    for (int i = 0; i < datalength; )
                        entries.Add(Color.FromArgb(paletteb[i++], paletteb[i++], paletteb[i++]));

                    //StringBuilder sb = new StringBuilder();
                    //foreach (Color c in entries)
                    //    sb.Append(String.Format("{0}, {1}, {2}", c.R, c.G, c.B));

                    //Bridge.AppendNode(
                    //    new InfoNode("Color palette",
                    //        InfoType.Generic,
                    //        new GenericInfo("Color Palette", sb.ToString()),
                    //        DataType.Critical,
                    //        n.DataStart + 8, n.DataEnd - 4),
                    //    n);

                    Bridge.AppendNode(
                        new InfoNode("Color palette",
                            InfoType.Delegate,
                            new object[] {
                                (Action<object[]>) delegate(object[] data) {
                                    if(pInfoPalette == null) {
                                        pInfoPalette = new System.Windows.Forms.Panel();

                                        lvInfoPalette = new System.Windows.Forms.ListView();
                                        lvInfoPalette.Dock = System.Windows.Forms.DockStyle.Fill;
                                        lvInfoPalette.View = System.Windows.Forms.View.Details;
                                        lvInfoPalette.FullRowSelect = true;
                                        lvInfoPalette.Columns.Add("Index");
                                        lvInfoPalette.Columns.Add("Red");
                                        lvInfoPalette.Columns.Add("Green");
                                        lvInfoPalette.Columns.Add("Blue");
                                        lvInfoPalette.AutoResizeColumns(System.Windows.Forms.ColumnHeaderAutoResizeStyle.HeaderSize);

                                        pInfoPalette.Controls.Add(lvInfoPalette);
                                    }

                                    lvInfoPalette.Items.Clear();

                                    Color[] colors = (Color[])data[0];

                                    for(int i = 0; i < colors.Length; i++) {
                                        System.Windows.Forms.ListViewItem lvi = new System.Windows.Forms.ListViewItem(i.ToString()) { BackColor = colors[i] };
                                        lvi.SubItems.Add(colors[i].R.ToString());
                                        lvi.SubItems.Add(colors[i].G.ToString());
                                        lvi.SubItems.Add(colors[i].B.ToString());
                                        lvi.ForeColor = colors[i].R * 0.299 + colors[i].G * 0.587 + colors[i].B * 0.114 <= 186 ? Color.White : Color.Black;
                                        lvInfoPalette.Items.Add(lvi);
                                    }

                                    Bridge.ShowInfo(InfoType.Panel, pInfoPalette);
                                },
                                new object[1] { entries.ToArray() }
                            },
                            DataType.Critical,
                            n.DataStart + 8, n.DataEnd - 4),
                        n);
                }

                /* *
                 * Chunks that absolutely require IHDR to have already been read.
                 * 
                 * The specification states that the IHDR must always be the first chunk,
                 * but we're a bit liberal with that; we're just parsing the image file,
                 * we dont actually need to display it thus we don't need it to adhere to
                 * the PNG specification we just need it to be coherent enough to read to EOF.
                 * */
            }

            return true;
        }

        public void Write(RootInfoNode root, Stream sout)
        {
            Bridge.SimpleWrite(root, sout);
        }
    }
}
