using System;
using System.Collections.Generic;
using System.Text;

namespace FileOptics.Interface
{
    public class ModuleAttrib : Attribute
    {
        public string Name;
        public int MinFileSize;
        public byte[] Magic;
        public ModuleAttrib(string Name, int MinFileSize = 0, byte[] Magic = null)
        {
            this.Name = Name;
            this.MinFileSize = MinFileSize;
            this.Magic = Magic;
        }
    }
}
