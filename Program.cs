using System;
using System.Windows.Forms;
using savetray.Properties;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

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
            readonly List<MenuItem> menu = new List<MenuItem>();
            readonly string settings = @"Resources\settings.txt";

            public Context()
            {
                trayIcon = new NotifyIcon()
                {
                    Icon = Resources.AppIcon,
                    Text = Environment.UserName,
                    Visible = true
                };

                foreach (string line in File.ReadAllLines(settings))
                {
                    string[] terms = line.Split(',');

                    if (terms.Length < 3) continue;

                    string label = terms[0];
                    string path = terms[1];
                    string args = terms[2];

                    menu.Add(new MenuItem(label, (sender, e) => Dispatch(path, args)));
                }

                trayIcon.ContextMenu = new ContextMenu(menu.ToArray());
                trayIcon.DoubleClick += (sender, e) => Dispatch("notepad", settings);
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
