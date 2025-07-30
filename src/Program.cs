using System;
using System.Windows.Forms;
using RushHourGame.Forms;

namespace RushHourGame
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
