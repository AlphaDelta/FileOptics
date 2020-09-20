using System;
using System.Collections.Generic;
using System.Text;

namespace FileOptics.Interface
{
    public enum InfoType
    {
        Generic, //GenericInfo
        Binary, //byte[]
        BinaryMainFocus, //byte[], will take over main HexBox
        Text, //string
        Image, //Image
        ImageFile, //string
        ImageStream, //Stream
        PositionInHexBox, //int
        Table, //TableInfo
        List, //ListInfo
        Delegate, //object[] { Action<InfoNode>, object[] }
        Panel, //Panel
        None
    }
}
