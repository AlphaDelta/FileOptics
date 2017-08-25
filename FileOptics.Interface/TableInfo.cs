using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FileOptics.Interface
{
    public class TableInfo
    {
        public View View;
        public ListViewItem[] Items;
        public string[] Columns;
        public ColumnHeaderAutoResizeStyle ResizeStyle = ColumnHeaderAutoResizeStyle.ColumnContent;

        public TableInfo(View View, string[] Columns, ListViewItem[] Items)
        {
            this.View = View;
            this.Columns = Columns;
            this.Items = Items;
        }
    }
}
