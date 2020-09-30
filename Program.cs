using System;
using System.Windows.Forms;
using savetray.Properties;
using System.Diagnostics;

namespace savetray
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Context());
        }

        public class Context : ApplicationContext
        {
            readonly NotifyIcon trayIcon;

            public Context()
            {
                trayIcon = new NotifyIcon()
                {
                    ContextMenu = new ContextMenu(),
                    Icon = Resources.AppIcon,
                    Text = Environment.UserName,
                    Visible = true
                };

                trayIcon.DoubleClick +=
                    (sender, e) => Dispatch("notepad", @"Resources\settings.txt");
            }

            void Dispatch(string file, string args)
            {
                try
                {
                    using (Process proc = new Process())
                    {
                        proc.StartInfo.FileName = file;
                        proc.StartInfo.Arguments = args;
                        proc.Start();
                    }
                }
                catch { }
            }
        }
    }
}
