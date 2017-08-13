using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileOptics.Basic
{
    /* *
     * Based on three different ISO documents, worth a sum of 385 PDF pages,
     * only about 5 of which actually explain the file format.
     * 
     * I hate JFIF and EXIF with every fiber of my being.
     * */

    [ModuleAttrib("JPEG:JFIF/EXIF Image File", 0x20, new byte[] { 0xFF, 0xD8 })]
    public class JFIF : IModule
    {
        public bool CanRead(Stream stream)
        {
            return true;
        }

        //TODO: Parse APPn and COM data
        const int JFIF_BUFFER_SIZE = 1024 * 512; //512KB
        public bool Read(RootInfoNode root, Stream stream)
        {
            root.Info = root.FilePath;
            root.IType = InfoType.ImageFile;

            byte[] buffer = new byte[JFIF_BUFFER_SIZE];
            byte[] tbb = new byte[2];
            int toread = 0;
            int read, i;
            int pos = 0;
            bool markerpending = false, EOI = false;
            byte lastmarker = 0x00;
            int lastmarkerend = 0;
            int skip = 0;
            while (!EOI && (read = stream.Read(buffer, 0, JFIF_BUFFER_SIZE)) > 0)
            {
                if (skip >= JFIF_BUFFER_SIZE)
                {
                    skip -= JFIF_BUFFER_SIZE;
                    continue;
                }

                for (i = skip, skip = 0, pos = (int)stream.Position - read; !EOI && i < read; i++, pos++)
                {
                    if (toread > 0)
                    {
                        tbb[--toread] = buffer[i];
                        //toread--;

                        if (toread > 0)
                            continue;

                        int length = (int)BitConverter.ToUInt16(tbb, 0);
                        if (length < 1) continue;
                        skip = length - 2;
                        i += skip + 1;
                        pos += skip + 1;

                        if((lastmarker & 0xF0) == 0xE0)
                            Bridge.AppendNode(new InfoNode("APP" + (lastmarker & 0x0F), InfoType.Generic, new GenericInfo("APP" + (lastmarker & 0x0F), "TODO."), ((lastmarker & 0x0F) < 2 ? DataType.Critical : DataType.Metadata), lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xDA)
                            Bridge.AppendNode(new InfoNode("SOS", InfoType.Generic, new GenericInfo("SOS", "Marks the start of image data stream."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xDB)
                            Bridge.AppendNode(new InfoNode("DQT", InfoType.Generic, new GenericInfo("DQT", "Quantization table definition."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xDC)
                            Bridge.AppendNode(new InfoNode("DNL", InfoType.Generic, new GenericInfo("DNL", "Defines number of lines."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xDD)
                            Bridge.AppendNode(new InfoNode("DRI", InfoType.Generic, new GenericInfo("DRI", "Restart interoperability definition."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xDE)
                            Bridge.AppendNode(new InfoNode("DHP", InfoType.Generic, new GenericInfo("DHP", "Hierarchical progression definition."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xDF)
                            Bridge.AppendNode(new InfoNode("EXP", InfoType.Generic, new GenericInfo("EXP", "Expand reference components."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xC0)
                            Bridge.AppendNode(new InfoNode("SOF", InfoType.Generic, new GenericInfo("SOF", "Parameter data relating to frame."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xC4)
                            Bridge.AppendNode(new InfoNode("DHT", InfoType.Generic, new GenericInfo("DHT", "Huffman table definition."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xCC)
                            Bridge.AppendNode(new InfoNode("DAC", InfoType.Generic, new GenericInfo("DAC", "Arithmetic coding condition(s) definition."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if ((lastmarker & 0xF0) == 0xC0)
                            Bridge.AppendNode(new InfoNode("SOF" + (lastmarker & 0x0F), InfoType.Generic, new GenericInfo("SOF" + (lastmarker & 0x0F), "Start Of Frame; Parameter data relating to frame."), DataType.Critical, lastmarkerend - 1, lastmarkerend += length), root);
                        else if (lastmarker == 0xFE)
                            Bridge.AppendNode(new InfoNode("COM", InfoType.Generic, new GenericInfo("COM", "Comment definition."), DataType.Ancillary, lastmarkerend - 1, lastmarkerend += length), root);
                    }
                    else if (markerpending)
                    {
                        if (lastmarkerend > 0 && buffer[i] != 0x00 && buffer[i] != 0xFF && (buffer[i] < 0xD0 || buffer[i] > 0xD7) && lastmarkerend != i - 2)
                        {
                            if (lastmarker == 0xDA)
                                Bridge.AppendNode(new InfoNode("Image data", InfoType.Generic, new GenericInfo("Image Data", "Contains the primary image data."), DataType.Critical, lastmarkerend + 1, pos - 2), root);
                            else
                                Bridge.AppendNode(new InfoNode("Unmarked data", InfoType.Generic, new GenericInfo("Unmarked Data", "This data could not be accounted for, typically useless."), DataType.Useless, lastmarkerend + 1, pos - 2), root);
                        }

                        if (buffer[i] == 0xD8)
                            Bridge.AppendNode(new InfoNode("SOI", InfoType.Generic, new GenericInfo("SOI", "Start Of Image marker."), DataType.Critical, pos - 1, pos), root);
                        else if (buffer[i] == 0x01)
                            Bridge.AppendNode(new InfoNode("TEM", InfoType.Generic, new GenericInfo("TEM", "Temporary marker."), DataType.Useless, pos - 1, pos), root);
                        else if (buffer[i] >= 0x02 && buffer[i] <= 0xBF)
                            Bridge.AppendNode(new InfoNode("RES", InfoType.Generic, new GenericInfo("RES", "Reserved marker."), DataType.Useless, pos - 1, pos), root);
                        else if (buffer[i] == 0xD9)
                        {
                            Bridge.AppendNode(new InfoNode("EOI", InfoType.Generic, new GenericInfo("EOI", "End Of Image marker."), DataType.Critical, pos - 1, pos), root);
                            EOI = true;
                        }
                        else if (
                            (buffer[i] & 0xF0) == 0xE0 ||
                            (buffer[i] & 0xF0) == 0xC0 ||
                            buffer[i] == 0xDA ||
                            buffer[i] == 0xDB ||
                            buffer[i] == 0xDC ||
                            buffer[i] == 0xDD ||
                            buffer[i] == 0xDE ||
                            buffer[i] == 0xDF ||
                            buffer[i] == 0xFE)
                        {
                            toread = 2;
                        }
                        else if (buffer[i] == 0x00 || buffer[i] == 0xFF || (buffer[i] >= 0xD0 && buffer[i] <= 0xD7))
                        {
                            markerpending = false;
                            continue;
                        }
                        else
                        {
                            Bridge.AppendNode(new InfoNode("UNK", InfoType.Generic, new GenericInfo("UNK", "Unknown JPEG marker, not defined in standard."), DataType.Useless, pos - 1, pos), root);
                        }


                        lastmarker = buffer[i];
                        lastmarkerend = pos;

                        markerpending = false;
                        continue;
                    }

                    if (buffer[i] != 0xFF) continue;
                    else markerpending = true;
                }
            }

            if (!EOI)
                return false;

            if (pos < stream.Length)
            {
                Bridge.AppendNode(
                    new InfoNode("EOF",
                        InfoType.Generic,
                        new GenericInfo("EOF", "End Of File data; serves no function or purpose."),
                        DataType.Useless,
                        pos, stream.Length - 1),
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
