using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using SimpleLogViewer.Properties;

namespace SimpleLogViewer
{
    public partial class About : Form
    {
        public About()
        {
            this.InitializeComponent();
        }

        private void About_Load(object sender, EventArgs e)
        {
            this.labelVersion.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void linkLabelWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/JoeBiellik/simple-log-viewer");
        }
    }
}
