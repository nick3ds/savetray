using System;
using System.Windows.Forms;
using savetray.Properties;

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
            }
        }
    }
}
