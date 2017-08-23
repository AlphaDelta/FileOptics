using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        void _AppendNode(InfoNode i, TreeNode super)
        {
            if (i.DType == DataType.Error)
                i.ForeColor = Color.Red;

            super.Nodes.Add(i);
        }

        public void AppendNode(InfoNode i, InfoNode super)
        {
            _AppendNode(i, super);
        }
        public void AppendNode(InfoNode i, RootInfoNode super)
        {
            _AppendNode(i, super);
        }

        Stream imgstream = null;
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
                    if (imgstream != null)
                        imgstream.Dispose();
                    break;
                case InfoType.ImageStream:
                    Stream snew = ((Stream)info);
                    main.imgInfo.Image = System.Drawing.Image.FromStream(snew);
                    if (imgstream != null)
                        imgstream.Dispose();
                    break;
                //case InfoType.None:
                //    return;
                case InfoType.Delegate:
                    if (info.GetType() == typeof(Action))
                    {
                        ((Action)info)();
                    }
                    else if (info.GetType() == typeof(object[]))
                    {
                        ((Action<object[]>)((object[])info)[0])(((object[])((object[])info)[1]));
                    }
                    return;
                case InfoType.Panel:
                    Panel newpanel = (Panel)info;
                    bool found = false;
                    foreach (Panel pn in main.infopanels)
                    {
                        if (pn != newpanel)
                        {
                            pn.Visible = false;
                            continue;
                        }

                        pn.Visible = true;
                        found = true;
                        break;
                    }
                    if (!found)
                    {
                        newpanel.Visible = true;
                        newpanel.Dock = DockStyle.Fill;
                        main.splitContainer2.Panel1.Controls.Add(newpanel);
                    }
                    return;
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
            ShowPanel(p);
        }

        public void ShowPanel(Panel p)
        {
            foreach (Panel pn in main.infopanels)
                pn.Visible = (pn == p);
        }

        public IModule FindModule(string fullname)
        {
            for (int i = 0; i < Root.ModuleAttribs.Count; i++)
                if (Root.ModuleAttribs[i].Name == fullname)
                    return Root.Modules[i];
            return null;
        }
    }
}
