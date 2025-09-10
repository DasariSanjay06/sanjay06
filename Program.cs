using System;
using System.Windows.Forms;

namespace BarcodeDetector
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles for modern Windows appearance
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Run the main form
            Application.Run(new MainForm());
        }
    }
}