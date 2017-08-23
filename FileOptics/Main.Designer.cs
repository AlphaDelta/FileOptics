namespace FileOptics
{
    partial class Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tree = new System.Windows.Forms.TreeView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.pInfoImage = new System.Windows.Forms.Panel();
            this.imgInfo = new System.Windows.Forms.PictureBox();
            this.pInfoGeneric = new System.Windows.Forms.Panel();
            this.txtInfoGenericBody = new System.Windows.Forms.TextBox();
            this.lblInfoGenericTitle = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.hexBox1 = new HexBoxLib.HexBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.pInfoImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgInfo)).BeginInit();
            this.pInfoGeneric.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(-1, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer1.Panel1.Controls.Add(this.tree);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1146, 416);
            this.splitContainer1.SplitterDistance = 158;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.TabStop = false;
            // 
            // tree
            // 
            this.tree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.HideSelection = false;
            this.tree.LineColor = System.Drawing.Color.Silver;
            this.tree.Location = new System.Drawing.Point(0, 4);
            this.tree.Name = "tree";
            this.tree.Size = new System.Drawing.Size(156, 406);
            this.tree.TabIndex = 0;
            this.tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_AfterSelect);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer2.Panel1.Controls.Add(this.pInfoImage);
            this.splitContainer2.Panel1.Controls.Add(this.pInfoGeneric);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.Window;
            this.splitContainer2.Panel2.Controls.Add(this.hexBox1);
            this.splitContainer2.Size = new System.Drawing.Size(984, 416);
            this.splitContainer2.SplitterDistance = 332;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.TabStop = false;
            // 
            // pInfoImage
            // 
            this.pInfoImage.Controls.Add(this.imgInfo);
            this.pInfoImage.Location = new System.Drawing.Point(74, 347);
            this.pInfoImage.Name = "pInfoImage";
            this.pInfoImage.Size = new System.Drawing.Size(64, 64);
            this.pInfoImage.TabIndex = 0;
            this.pInfoImage.Visible = false;
            // 
            // imgInfo
            // 
            this.imgInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgInfo.Location = new System.Drawing.Point(0, 0);
            this.imgInfo.Name = "imgInfo";
            this.imgInfo.Size = new System.Drawing.Size(64, 64);
            this.imgInfo.TabIndex = 0;
            this.imgInfo.TabStop = false;
            // 
            // pInfoGeneric
            // 
            this.pInfoGeneric.Controls.Add(this.txtInfoGenericBody);
            this.pInfoGeneric.Controls.Add(this.lblInfoGenericTitle);
            this.pInfoGeneric.Location = new System.Drawing.Point(4, 347);
            this.pInfoGeneric.Name = "pInfoGeneric";
            this.pInfoGeneric.Padding = new System.Windows.Forms.Padding(5);
            this.pInfoGeneric.Size = new System.Drawing.Size(64, 64);
            this.pInfoGeneric.TabIndex = 0;
            this.pInfoGeneric.Visible = false;
            // 
            // txtInfoGenericBody
            // 
            this.txtInfoGenericBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfoGenericBody.BackColor = System.Drawing.SystemColors.Window;
            this.txtInfoGenericBody.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInfoGenericBody.Location = new System.Drawing.Point(8, 30);
            this.txtInfoGenericBody.Multiline = true;
            this.txtInfoGenericBody.Name = "txtInfoGenericBody";
            this.txtInfoGenericBody.ReadOnly = true;
            this.txtInfoGenericBody.Size = new System.Drawing.Size(48, 26);
            this.txtInfoGenericBody.TabIndex = 1;
            this.txtInfoGenericBody.Text = "Body text";
            // 
            // lblInfoGenericTitle
            // 
            this.lblInfoGenericTitle.AutoSize = true;
            this.lblInfoGenericTitle.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfoGenericTitle.Location = new System.Drawing.Point(4, 8);
            this.lblInfoGenericTitle.Name = "lblInfoGenericTitle";
            this.lblInfoGenericTitle.Size = new System.Drawing.Size(47, 19);
            this.lblInfoGenericTitle.TabIndex = 0;
            this.lblInfoGenericTitle.Text = "Title";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1144, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuFile
            // 
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(37, 20);
            this.menuFile.Text = "File";
            // 
            // hexBox1
            // 
            this.hexBox1.ASCIITableColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.hexBox1.BackColor = System.Drawing.Color.White;
            this.hexBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexBox1.HeaderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(191)))));
            this.hexBox1.HexTableColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            this.hexBox1.Location = new System.Drawing.Point(0, 0);
            this.hexBox1.Name = "hexBox1";
            this.hexBox1.OffsetColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(191)))));
            this.hexBox1.Size = new System.Drawing.Size(646, 414);
            this.hexBox1.TabIndex = 0;
            this.hexBox1.Text = "hexBox1";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1144, 439);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Main";
            this.ShowIcon = false;
            this.Text = "FileOptics";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.pInfoImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgInfo)).EndInit();
            this.pInfoGeneric.ResumeLayout(false);
            this.pInfoGeneric.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem menuFile;
        internal System.Windows.Forms.SplitContainer splitContainer2;
        internal HexBoxLib.HexBox hexBox1;
        internal System.Windows.Forms.SplitContainer splitContainer1;
        internal System.Windows.Forms.TreeView tree;
        internal System.Windows.Forms.MenuStrip menuStrip1;
        internal System.Windows.Forms.Label lblInfoGenericTitle;
        internal System.Windows.Forms.TextBox txtInfoGenericBody;
        internal System.Windows.Forms.Panel pInfoGeneric;
        internal System.Windows.Forms.Panel pInfoImage;
        internal System.Windows.Forms.PictureBox imgInfo;
    }
}

