using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShysterWatch
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-GB");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!Directory.Exists(Properties.Settings.Default.RootFolder))
            {
                Application.Run(new Settings());
            }


            //Application.Run(new Form1());
            Application.Run(new SearchForm());

        }
    }
}
