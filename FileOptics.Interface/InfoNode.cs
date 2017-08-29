using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FileOptics.Interface
{
    public class InfoNode : TreeNode
    {
        public InfoType IType;
        public DataType DType;
        public object Info;
        public long DataStart, DataEnd;
        public System.Drawing.Color HighlightColor = System.Drawing.Color.FromArgb(0x33, 0x99, 0xFF);
        public bool MarkedForDeletion = false;
        public List<object> SecondaryInfo = new List<object>();

        public InfoNode(string Title, InfoType IType, object Info, DataType DType, long DataStart, long DataEnd)
        {
            this.IType = IType;
            this.Info = Info;
            this.Text = Title;
            this.DType = DType;
            this.DataStart = DataStart;
            this.DataEnd = DataEnd;
        }
        public InfoNode(string Title, int ImageIndex, InfoType IType, object Info, DataType DType, long DataStart, long DataEnd)
        {
            this.IType = IType;
            this.Info = Info;
            this.Text = Title;
            this.ImageIndex = this.SelectedImageIndex = ImageIndex;
            this.DType = DType;
            this.DataStart = DataStart;
            this.DataEnd = DataEnd;
        }
        public InfoNode(string Title, string ImageKey, InfoType IType, object Info, DataType DType, long DataStart, long DataEnd)
        {
            this.IType = IType;
            this.Info = Info;
            this.Text = Title;
            this.ImageKey = this.SelectedImageKey = ImageKey;
            this.DType = DType;
            this.DataStart = DataStart;
            this.DataEnd = DataEnd;
        }
    }
}
