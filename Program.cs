using System;
using System.Windows.Forms;
using savetray.Properties;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;

namespace savetray
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new Mutex(true, "savetray_mutex", out bool created);

            if (created)
                Application.Run(new Context());
        }

        public class Context : ApplicationContext
        {
            readonly NotifyIcon trayIcon;
            readonly List<MenuItem> menu = new List<MenuItem>();
            readonly string settings = @"Resources\settings.txt";
            readonly string usericon = @"Resources\favicon.ico";

            public Context()
            {
                trayIcon = new NotifyIcon()
                {
                    Text = Environment.UserName,
                    Visible = true
                };

                Dictionary<string, List<MenuItem>> items =
                    new Dictionary<string, List<MenuItem>>();

                foreach (string line in File.ReadAllLines(settings))
                {
                    string[] terms = line.Split(';');
                    string label = terms[0].Trim();

                    if (terms.Length < 2 || label?[0] == '#') continue;

                    string path = terms[1].Trim();
                    string args = terms.Length > 2 ? terms[2].Trim() : null;

                    string[] tags = label.Split('\\');
                    string cat = tags.Length > 1 ? tags[0].Trim() : "";
                    string tag = tags.Length > 1 ? tags[1].Trim() : tags[0].Trim();

                    void action(object sender, EventArgs e) => Dispatch(path, args);

                    if (tag == "$")
                    {
                        trayIcon.DoubleClick += action;
                        continue;
                    }

                    if (!items.ContainsKey(cat))
                        items.Add(cat, new List<MenuItem>());

                    items[cat].Add(new MenuItem(tag, action));
                }

                foreach (string key in items.Keys)
                {
                    if (key == "")
                        menu.AddRange(items[key]);
                    else
                        menu.Add(new MenuItem(MenuMerge.Add, 0, Shortcut.None, key,
                        null, null, null, items[key].ToArray()));
                }

                menu.Add(new MenuItem("settings..", (sender, e) =>
                {
                    trayIcon.ContextMenu.Dispose();
                    Dispatch(settings, null, true);
                    Application.Restart();
                }));

                trayIcon.ContextMenu = new ContextMenu(menu.ToArray());

                try { trayIcon.Icon = Icon.ExtractAssociatedIcon(usericon); }
                catch { trayIcon.Icon = Resources.AppIcon; }

                Application.ApplicationExit += (sender, e) => trayIcon.Visible = false;
            }

            void Dispatch(string file, string args, bool await = false)
            {
                try
                {
                    using (Process proc = new Process())
                    {
                        proc.StartInfo.FileName = file;
                        proc.StartInfo.Arguments = args;
                        proc.Start();

                        if (await) proc.WaitForExit();
                    }
                }
                catch { }
            }
        }
    }
}
