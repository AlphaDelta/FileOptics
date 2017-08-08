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

        public bool Read(RootInfoNode root, Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Write(RootInfoNode root, Stream sout)
        {
            throw new NotImplementedException();
        }
    }
}
