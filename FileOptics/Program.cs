using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FileOptics
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Main m = new Main();
            CBridge b = new CBridge(m);
            FileOptics.Interface.Bridge.Base = b;
            Application.Run(m);
        }
    }
}
