using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FileOptics
{
    public class CBridge : IBridge
    {
        Main main;
        public CBridge(Main main)
        {
            this.main = main;
        }

        public void AddRootNode(RootInfoNode i)
        {
            main.tree.Nodes.Add(i);
        }

        public void AppendNode(InfoNode i, InfoNode super)
        {
            super.Nodes.Add(i);
        }
        public void AppendNode(InfoNode i, RootInfoNode super)
        {
            super.Nodes.Add(i);
        }


        public void ShowInfo(InfoNode inode)
        {
            switch (inode.IType)
            {
                case InfoType.Generic:
                    main.lblInfoGenericTitle.Text = ((GenericInfo)inode.Info).Title;
                    main.txtInfoGenericBody.Text = ((GenericInfo)inode.Info).Body;
                    break;
            }

            ShowPanel(inode.IType);
        }

        public void ShowPanel(InfoType type)
        {
            Panel p = null;
            switch (type)
            {
                case InfoType.Generic: p = main.pInfoGeneric; break;
            }
            foreach (Panel pn in main.infopanels)
                p.Visible = (pn == p);
        }
    }
}
