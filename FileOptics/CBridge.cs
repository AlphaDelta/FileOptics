using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
