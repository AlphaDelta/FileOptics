using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileOptics
{
    public partial class Main : Form
    {
        public List<Panel> infopanels = new List<Panel>();
        public Main()
        {
            InitializeComponent();

            imglTree.Images.Add("null", Properties.Resources.tree_null);
            imglTree.Images.Add("file", Properties.Resources.tree_file);
            imglTree.Images.Add("image", Properties.Resources.tree_img);
            imglTree.Images.Add("info", Properties.Resources.tree_info);
            imglTree.Images.Add("error", Properties.Resources.tree_error);
            imglTree.Images.Add("binary", Properties.Resources.tree_binary);
            imglTree.Images.Add("block", Properties.Resources.tree_block_blue); //Regular data block
            imglTree.Images.Add("block-orange", Properties.Resources.tree_block_orange); //Data block that could not be accounted for
            imglTree.Images.Add("block-red", Properties.Resources.tree_block_red); //Data block that has been marked for deletion
            imglTree.Images.Add("block-purple", Properties.Resources.tree_block_purple); //Most significant data block(s), ie. image data
            imglTree.Images.Add("block-trueblue", Properties.Resources.tree_block_trueblue);
            imglTree.Images.Add("int", Properties.Resources.tree_int);
            imglTree.Images.Add("intptr", Properties.Resources.tree_intptr);
            imglTree.Images.Add("byte", Properties.Resources.tree_byte);
            imglTree.Images.Add("str", Properties.Resources.tree_str);
            imglTree.Images.Add("unknown", Properties.Resources.tree_unknown);

            imgInfo.Resize += delegate (object sender, EventArgs e)
            {
                if (imgInfo.Image == null) return;

                if (imgInfo.Width > imgInfo.Image.Width &&
                    imgInfo.Height > imgInfo.Image.Height)
                    imgInfo.SizeMode = PictureBoxSizeMode.CenterImage;
                else
                    imgInfo.SizeMode = PictureBoxSizeMode.Zoom;
            };

            //tree.KeyDown += delegate
            //{
            //    RootInfoNode rin = new RootInfoNode("Generic root", "FileOptics.exe", null);
            //    Bridge.AddRootNode(rin);
            //    Bridge.AppendNode(new InfoNode("PE", InfoType.None, null, DataType.Useless, 0x00, 0x53) { HighlightColor = Color.Red }, rin);
            //};

            foreach (Control c in splitContainer2.Panel1.Controls)
                if (c.Name.StartsWith("pInfo"))
                {
                    infopanels.Add((Panel)c);
                    c.Visible = false;
                    c.Dock = DockStyle.Fill;
                }

            this.Shown += delegate
            {
                this.Invoke((Action)delegate
                {
                    using (Startup start = new Startup())
                    {
                        start.ShowDialog();
                        if (start.Canceled)
                            this.Close();
                    }
                });
            };

            this.AllowDrop = true;
            this.DragOver += delegate (object sender, DragEventArgs e)
            { e.Effect = (e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None); };
            this.DragDrop += delegate (object sender, DragEventArgs e)
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length != 1)
                {
                    MessageBox.Show("Please only drop one file at a time!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Task.Run(() =>
                {
                    LoadFile(files[0]);
                });
            };
        }

        private void LoadFile(string file)
        {
            List<int> valid = new List<int>();
            byte[] magicbuffer = new byte[0x10];
            int read = 0;

            FileStream fs = null;
            string fname = Path.GetFileName(file);
            try
            {
                try
                {
                    fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (IOException ex)
                {
                    if (!ex.Message.Contains("used by another process"))
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (MessageBox.Show("The file could not be write-locked, would you like to make a temporary copy and read that instead?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != System.Windows.Forms.DialogResult.Yes)
                        return;

                    byte[] hash = NayukiSHA256.Calculate(Encoding.ASCII.GetBytes(file));
                    StringBuilder sb = new StringBuilder(hash.Length * 2);
                    foreach (byte b in hash)
                        sb.Append(b.ToString("X2"));

                    string newf = Root.LocalAppData + sb.ToString() + ".temp";
                    File.Copy(file, newf);
                    file = newf;

                    fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                }

                if (fs.Length < 1) return;
                if ((read = fs.Read(magicbuffer, 0, magicbuffer.Length)) < 1) return;

                for (int i = 0; i < Root.ModuleAttribs.Count; i++)
                {
                    if (fs.Length < Root.ModuleAttribs[i].MinFileSize)
                        continue;

                    if (Root.ModuleAttribs[i].Magic != null && Root.ModuleAttribs[i].Magic.Length > 0 && Root.ModuleAttribs[i].Magic.Length <= magicbuffer.Length)
                    {
                        int j = 0;
                        for (; j < Root.ModuleAttribs[i].Magic.Length; j++)
                            if (Root.ModuleAttribs[i].Magic[j] != magicbuffer[j])
                                break;

                        if (j < Root.ModuleAttribs[i].Magic.Length) continue;
                    }

                    fs.Seek(0, SeekOrigin.Begin);
                    if (Root.Modules[i].CanRead(fs))
                        valid.Add(i);
                }

                this.Invoke((Action)delegate
                {
                    if (valid.Count < 1)
                        MessageBox.Show("No modules that were able to read the specified file could be found.", "Unsupported Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else if (valid.Count > 1)
                        MessageBox.Show("More than one applicable module.", "NYI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        RootInfoNode root = new RootInfoNode(fname, file, Root.Modules[valid[0]]) { ImageKey = "file", SelectedImageKey = "file" };
                        Root.Modules[valid[0]].Read(root, fs);
                        Bridge.AddRootNode(root);
                    }
                });
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }

        string fileloaded = null;
        Random rnd = new Random();
        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            hexBox1.Highlights.Clear();
            if (e.Node.Level == 0)
            {
                if (!(e.Node is RootInfoNode)) return;

                RootInfoNode rin = (RootInfoNode)e.Node;
                if (!File.Exists(rin.FilePath)) throw new FileNotFoundException();

                hexBox1.ReadFile(rin.FilePath);
                fileloaded = rin.FilePath;

                double h = rnd.NextDouble();
                int r, g, b;
                foreach (InfoNode inode in e.Node.Nodes)
                {
                    HSV.ToRGB(h * 360, 1.00d, 1d, out r, out g, out b);
                    hexBox1.Highlights.Add(new HexBoxLib.Highlight((int)inode.DataStart, (int)inode.DataEnd, Color.FromArgb(r, g, b)));

                    //do
                    //{
                    h += Root.GOLDEN_RATIO;
                    h %= 1;
                    //} while (h > 0.15 && h < 0.525); 
                }

                //hexBox1.ScrollTo(0);
                hexBox1.Invalidate();

                Bridge.ShowInfo(rin.IType, rin.Info);
            }
            else
            {
                if (!(e.Node is InfoNode)) return;
                InfoNode inode = (InfoNode)e.Node;

                TreeNode root = e.Node;
                do
                {
                    root = root.Parent;
                } while (root.Level > 0);

                if (!(root is RootInfoNode)) return;

                RootInfoNode rin = (RootInfoNode)root;
                if (fileloaded != rin.FilePath)
                {
                    if (!File.Exists(rin.FilePath)) throw new FileNotFoundException();

                    hexBox1.ReadFile(rin.FilePath);
                    fileloaded = rin.FilePath;
                }

                Color ghost = Color.FromArgb(0xDD, 0xDD, 0xDD);
                foreach (InfoNode irn in inode.Parent.Nodes)
                    hexBox1.Highlights.Add(new HexBoxLib.Highlight((int)irn.DataStart, (int)irn.DataEnd, ghost));

                hexBox1.Highlights.Add(new HexBoxLib.Highlight((int)inode.DataStart, (int)inode.DataEnd, inode.HighlightColor));
                hexBox1.ScrollTo((int)(inode.DataStart >> 4));
                hexBox1.Invalidate();

                //foreach (Panel p in infopanels)
                //    p.Visible = false;

                //switch (inode.IType)
                //{
                //    case InfoType.Generic:
                //        lblInfoGenericTitle.Text = ((GenericInfo)inode.Info).Title;
                //        txtInfoGenericBody.Text = ((GenericInfo)inode.Info).Body;
                //        pInfoGeneric.Visible = true;
                //        break;
                //}

                Bridge.ShowInfo(inode.IType, inode.Info);
            }
        }

        private void menuFile_Click(object sender, EventArgs e)
        {
            LoadFile(@"./testfile");
        }
    }
}
