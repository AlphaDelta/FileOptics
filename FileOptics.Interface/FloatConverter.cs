using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FileOptics.Interface
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FloatConverter
    {
        [FieldOffset(0)] private float f;
        [FieldOffset(0)] private uint i;

        private static FloatConverter inst = new FloatConverter();
        public static float Convert(uint value)
        {
            inst.i = value;
            return inst.f;
        }
    }
}
