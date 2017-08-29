using FileOptics.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace FileOptics
{
    public partial class Startup : Form
    {
        public bool Canceled = true;
        public Startup()
        {
            InitializeComponent();
        }

        private void Startup_Load(object sender, EventArgs e)
        {
            BackgroundWorker bg = new BackgroundWorker();

            bg.DoWork += delegate
            {
                this.Invoke((Action)delegate { lbl.Text = "Checking for necessary files and folders."; });
                if (!Directory.Exists("modules")) Directory.CreateDirectory("modules");
                if (!File.Exists("modules\\trusted")) File.Create("modules\\trusted").Close();

                string lad = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\FileOptics\";
                if (!Directory.Exists(lad)) Directory.CreateDirectory(lad);
                Root.LocalAppData = lad;

                string[] tempfs = Directory.GetFiles(Root.LocalAppData, "*.temp", SearchOption.TopDirectoryOnly);
                if (tempfs.Length > 0)
                {
                    this.Invoke((Action)delegate { lbl.Text = "Cleaning temporary files."; });
                    foreach (string tempf in tempfs)
                        File.Delete(tempf);
                }

                this.Invoke((Action)delegate { lbl.Text = "Reading trusted module checksum list."; });
                using (FileStream s = File.Open("modules\\trusted", FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    byte[] buffer = new byte[32];
                    int read;
                    while ((read = s.Read(buffer, 0, buffer.Length)) == buffer.Length)
                        Root.TrustedModules.Add(buffer);
                }

                this.Invoke((Action)delegate { lbl.Text = "Finding modules."; });
                string[] files = Directory.GetFiles("modules", "*.dll", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    this.Invoke((Action)delegate { lbl.Text = String.Format("Reading '{0}'.", Path.GetFileName(file)); });
                    if (!File.Exists(file)) continue;

                    NayukiSHA256 sha256 = new NayukiSHA256();
                    sha256.AppendFile(file, true);

                    int i = 0;
                    foreach (byte[] trusted in Root.TrustedModules)
                    {
                        for (i = 0; i < 32; i++)
                            if (trusted[i] != sha256.Hash[i]) break;
                        if (i >= 32) break;
                    }
                    if (i < 32)
                    {
                        bool scon = false;
                        this.Invoke((Action)delegate
                        {
                            if (MessageBox.Show(String.Format("The module '{0}' has not yet been trusted. Would you like to add it to the trusted module list? (Note: Modules are like programs, they can be just as dangerous as running an exe. Please be responsible with trusting modules of unknown origin)\r\nChoosing 'No' will result in the module not being loaded.", Path.GetFileName(file)), "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != System.Windows.Forms.DialogResult.Yes)
                                scon = true;
                        });
                        if (scon) continue;

                        //File.WriteAllBytes("modules\\trusted", sha256.Hash);
                        using (FileStream trusted = File.Open("modules\\trusted", FileMode.Append, FileAccess.Write, FileShare.Read))
                            trusted.Write(sha256.Hash, 0, sha256.Hash.Length);
                    }

                    Assembly asm = Assembly.LoadFrom(file);
                    foreach (Type t in asm.GetTypes())
                    {
                        if (t.GetInterface("IModule") == null) continue;
                        ModuleAttrib[] mods = (ModuleAttrib[])t.GetCustomAttributes(typeof(ModuleAttrib), false);
                        if (mods.Length != 1) continue;

                        mods[0].Name += String.Format(" ({0})", t.Namespace);
                        Root.ModuleAttribs.Add(mods[0]);
                        Root.Modules.Add((IModule)Activator.CreateInstance(t));
                    }
                }
                this.Invoke((Action)delegate { lbl.Text = "Complete!"; Canceled = false; this.Close(); });
            };
            bg.RunWorkerAsync();
        }
    }
}
