using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FileOptics.Interface
{
    public class RootInfoNode : TreeNode
    {
        public InfoType IType;
        public object Info;
        public string FilePath;
        public IModule Module;

        public RootInfoNode(string Title, string FilePath, IModule Module, InfoType IType = InfoType.None, object Info = null)
        {
            this.FilePath = FilePath;
            this.Text = Title;
            this.Module = Module;
            this.IType = IType;
            this.Info = Info;
        }
    }
}
