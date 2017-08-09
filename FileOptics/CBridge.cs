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


        public void ShowInfo(InfoType type, object info)
        {
            switch (type)
            {
                case InfoType.Generic:
                    main.lblInfoGenericTitle.Text = ((GenericInfo)info).Title;
                    main.txtInfoGenericBody.Text = ((GenericInfo)info).Body;
                    break;
                case InfoType.ImageFile:
                    main.imgInfo.Load((string)info);
                    break;
            }

            ShowPanel(type);
        }

        public void ShowPanel(InfoType type)
        {
            Panel p = null;
            switch (type)
            {
                case InfoType.Generic: p = main.pInfoGeneric; break;
                case InfoType.Image:
                case InfoType.ImageFile:
                case InfoType.ImageStream:
                    p = main.pInfoImage; break;
            }
            foreach (Panel pn in main.infopanels)
                pn.Visible = (pn == p);
        }
    }
}
