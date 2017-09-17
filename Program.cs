using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
namespace SunAnalysis
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
            Application.Run(new 日照分析工具());
        }
    }
}
