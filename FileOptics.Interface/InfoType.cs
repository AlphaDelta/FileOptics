using System;
using System.Collections.Generic;
using System.Text;

namespace FileOptics.Interface
{
    public enum InfoType
    {
        Generic, //GenericInfo
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
