using System;
using System.Windows.Forms;
using savetray.Properties;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Media;

using Timer = System.Timers.Timer;

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
            readonly string wavfiles = "alarm|beep";

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
                    List<string> terms = new List<string>(
                        line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                    string label = terms[0].Trim();

                    if (terms.Count < 2 || label?[0] == '#') continue;

                    string[] tags = label.Split(new char[] { '\\' }, 2);
                    string cat = tags.Length > 1 ? tags[0].Trim() : "";
                    string tag = tags[tags.Length - 1].Trim();

                    int.TryParse(terms[1].Trim('&', ' '), out int delay);
                    if (delay != 0) terms.RemoveAt(1);

                    string path = terms[1].Trim();
                    string args = terms.Count > 2 ? terms[2].Trim() : null;

                    EventHandler action;

                    if (delay > 0)
                    {
                        action = (object sender, EventArgs e) =>
                        {
                            Timer timer = new Timer(delay * 1000);
                            timer.Elapsed += delegate { Dispatch(path, args); };
                            timer.AutoReset = false;
                            timer.Start();
                        };
                    }
                    else 
                        action = (object sender, EventArgs e) => Dispatch(path, args);

                    if (tag == "$")
                    {
                        trayIcon.DoubleClick += action;
                        continue;
                    }

                    if (!items.ContainsKey(cat))
                        items.Add(cat, new List<MenuItem>());

                    MenuItem item = items[cat].Find(m => m.Text == tag);

                    if (item is null)
                        items[cat].Add(new MenuItem(tag, action));
                    else
                        item.Click += action;
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
                    if (wavfiles.Contains(file.ToLower()))
                    {
                        SoundPlayer player = new SoundPlayer($"Resources\\{file}.wav");
                        player.PlaySync();
                        player.Dispose();
                        return;
                    }

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
