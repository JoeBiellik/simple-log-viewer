using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SimpleLogViewer
{
    public partial class Main : Form
    {
        private LogFileMonitor monitor;

        public Main()
        {
            this.InitializeComponent();

            if (Settings.Instance.WindowPosition != Rectangle.Empty && Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(Settings.Instance.WindowPosition)))
            {
                this.StartPosition = FormStartPosition.Manual;
                this.DesktopBounds = Settings.Instance.WindowPosition;
                this.WindowState = Settings.Instance.WindowMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
            }
            else
            {
                this.StartPosition = FormStartPosition.WindowsDefaultLocation;

                if (Settings.Instance.WindowPosition != Rectangle.Empty)
                {
                    this.Size = Settings.Instance.WindowPosition.Size;
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            this.flashToolStripMenuItem.Checked = Settings.Instance.FlashTaskbar;
            this.scrollToolStripMenuItem.Checked = Settings.Instance.AutoScroll;
            this.wordWrapToolStripMenuItem.Checked = Settings.Instance.WordWrap;
        }

        private void LogChanged(object sender, LogFileMonitorLineEventArgs e)
        {
            if (this.scrollToolStripMenuItem.Checked)
            {
                this.textBoxLog.AppendText(e.Line + Environment.NewLine);
            }
            else
            {
                this.textBoxLog.Text += e.Line + Environment.NewLine;
            }

            if (Settings.Instance.FlashTaskbar && !this.Focused)
            {
                FlashWindow.Flash(this, 1);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "log",
                Multiselect = false,
                ReadOnlyChecked = true,
                ShowHelp = false,
                Title = "Select the log file you want to view"
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                this.monitor?.Stop();

                this.toolStripStatusLabel.Text = "Monitoring " + dialog.FileName;

                this.monitor = new LogFileMonitor(dialog.FileName, this);
                this.monitor.OnLineAddition += this.LogChanged;
                this.monitor.Start();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.monitor?.Stop();

            Application.Exit();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.textBoxLog.Clear();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(this.textBoxLog.SelectedText);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.textBoxLog.SelectAll();
        }

        private void flashToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.FlashTaskbar = this.flashToolStripMenuItem.Checked;
            Settings.Save();
        }

        private void wordWrapToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            this.textBoxLog.WordWrap = this.wordWrapToolStripMenuItem.Checked;

            Settings.Instance.WordWrap = this.wordWrapToolStripMenuItem.Checked;
            Settings.Save();
        }

        private void scrollToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.AutoScroll = this.scrollToolStripMenuItem.Checked;
            Settings.Save();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new About()).ShowDialog();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            switch (this.WindowState)
            {
                case FormWindowState.Maximized:
                    Settings.Instance.WindowMaximized = true;
                    Settings.Instance.WindowPosition = this.RestoreBounds;
                    break;
                default:
                    Settings.Instance.WindowMaximized = false;
                    Settings.Instance.WindowPosition = this.DesktopBounds;
                    break;
            }

            Settings.Save();
        }
    }
}
