using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FileOptics.Interface
{
    public class Bridge
    {
        internal static IBridge _base = null;
        public static IBridge Base
        {
            set { if (_base == null) _base = value; else throw new Exception("Base has already been set"); }
        }

        public static void AddRootNode(RootInfoNode i) { _base.AddRootNode(i); }

        public static void AppendNode(InfoNode i, InfoNode super) { _base.AppendNode(i, super); }
        public static void AppendNode(InfoNode i, RootInfoNode super) { _base.AppendNode(i, super); }

        public static void ShowInfo(InfoNode i) { _base.ShowInfo(i); }
        public static void ShowPanel(InfoType t) { _base.ShowPanel(t); }

        public static void SimpleWrite(RootInfoNode i, Stream outp)
        {

        }
    }
}
