using System;
using System.Collections.Generic;
using System.Text;

namespace FileOptics.Interface
{
    public interface IBridge
    {
        void AddRootNode(RootInfoNode i);

        void AppendNode(InfoNode i, InfoNode super);
        void AppendNode(InfoNode i, RootInfoNode super);

        void ShowInfo(InfoType type, object info);
        void ShowPanel(InfoType type);

        IModule FindModule(string fullname);
    }
}
