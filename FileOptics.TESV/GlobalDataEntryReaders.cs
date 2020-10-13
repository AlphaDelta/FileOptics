using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileOptics.TESV
{
    public static class GlobalDataEntryReaders
    {
        public static void MiscStatsEntryType(Stream stream, TreeNode parent, ref byte[] buffer)
        {
            uint count = ESS.ReadUInt32Basic(stream, "Array count", parent, ref buffer);

            for (int i = 0; i < count; i++)
            {
                InfoNode nStat = new InfoNode(
                    "", "block-trueblue",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    stream.Position, 0);

                nStat.Text = ESS.ReadWStringBasic(stream, "Stat name", nStat, ref buffer);
                ESS.ReadUInt8Basic(stream, "Category", nStat, ref buffer);
                ESS.ReadUInt32Basic(stream, "Value", nStat, ref buffer);

                nStat.DataEnd = stream.Position - 1;
                Bridge.AppendNode(nStat, parent);
            }
        }

        public static void GlobalVariablesEntryType(Stream stream, TreeNode parent, ref byte[] buffer)
        {
            uint count = ESS.ReadVarLenBasic(stream, "Array count", (InfoNode)parent, ref buffer);

            for (int i = 0; i < count; i++)
            {
                InfoNode nStat = new InfoNode(
                    "", "block-trueblue",
                    InfoType.None,
                    null,
                    DataType.Critical,
                    stream.Position, 0);

                nStat.Text = ESS.ReadRefIDBasic(stream, "FormID", nStat, ref buffer).ToString("X6");
                ESS.ReadFloatBasic(stream, "Value", nStat, ref buffer);

                nStat.DataEnd = stream.Position - 1;
                Bridge.AppendNode(nStat, parent);
            }
        }
    }
}
