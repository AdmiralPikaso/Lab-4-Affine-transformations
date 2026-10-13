using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lab4
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Шрифт по умолчанию как в .NET Framework, чтобы раскладка формы не поменялась
            Application.SetDefaultFont(new Font("Microsoft Sans Serif", 8.25f));
            Application.Run(new MainForm());
        }
    }
}
