using System;
using System.Collections.Generic;
using System.Text;

namespace FileOptics.Interface
{
    //public class DataInterpreter
    //{
    //    public static byte[] CorrectEndianness(byte[] data, bool littleendian)
    //    {
    //        if (BitConverter.IsLittleEndian != littleendian)
    //        {
    //            byte[] tempdata = data;

    //            tempdata[0] = data[3];
    //            tempdata[1] = data[2];
    //            tempdata[2] = data[1];
    //            tempdata[3] = data[0];

    //            return tempdata;
    //        }

    //        return data;
    //    }

    //    public static Int32 ReadInt32(byte[] data, bool littleendian, int offset = 0)
    //    {
    //        byte[] temp = CorrectEndianness(data, littleendian);
    //        return BitConverter.ToInt32(data, offset);
    //    }
    //}
}
