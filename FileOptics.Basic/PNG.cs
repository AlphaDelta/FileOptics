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

            return true;
        }

        public void Write(RootInfoNode root, Stream sout)
        {
            Bridge.SimpleWrite(root, sout);
        }
    }
}
