using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FileOptics.Interface
{
    public class RootInfoNode : TreeNode
    {
        public string FilePath;
        public IModule Module;

        public RootInfoNode(string Title, string FilePath, IModule Module)
        {
            this.FilePath = FilePath;
            this.Text = Title;
            this.Module = Module;
        }
    }
}
