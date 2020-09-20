using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FileOptics.Interface
{
    public interface IBridge
    {
        void AddRootNode(RootInfoNode i);

        void AppendNode(InfoNode i, TreeNode super);
        void AppendNode(InfoNode i, InfoNode super);
        void AppendNode(InfoNode i, RootInfoNode super);

        void ShowInfo(InfoType type, object info);
        void ShowPanel(InfoType type);
        void ShowPanel(System.Windows.Forms.Panel p);

        IModule FindModule(string fullname);
    }
}
