﻿using System;
using System.ComponentModel;
using System.IO;
using System.Timers;

namespace SimpleLogViewer
{
    public class LogFileMonitorLineEventArgs : EventArgs
    {
        public string Line { get; set; }
    }

    public class LogFileMonitor
    {
        private readonly string path;
        private readonly string delimiter;
        private readonly Timer timer;
        private string buffer;
        private long size;
        private bool monitoring;

        public EventHandler<LogFileMonitorLineEventArgs> OnLineAddition;

        public double Interval
        {
            get { return this.timer.Interval; }
            set { this.timer.Interval = value; }
        }

        public LogFileMonitor(string path, ISynchronizeInvoke synchronizingObject = null, string delimiter = "\n", double interval = 200)
        {
            this.path = path;
            this.delimiter = delimiter;

            this.timer = new Timer
            {
                AutoReset = true,
                Interval = interval,
                SynchronizingObject = synchronizingObject
            };
            this.timer.Elapsed += this.Check;
        }

        public void Start()
        {
            this.size = new FileInfo(this.path).Length;

            this.timer.Start();
        }

        public void Stop()
        {
            this.timer.Stop();
        }

        private bool StartMonitoring()
        {
            lock (this.timer)
            {
                if (this.monitoring) return true;

                this.monitoring = true;
                return false;
            }
        }

        private void Check(object s, ElapsedEventArgs e)
        {
            if (!this.StartMonitoring()) return;

            var newSize = new FileInfo(this.path).Length;

            if (this.size >= newSize) return;

            using (var stream = File.Open(this.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(stream))
            {
                sr.BaseStream.Seek(this.size, SeekOrigin.Begin);

                var data = this.buffer + sr.ReadToEnd();

                if (!data.EndsWith(this.delimiter))
                {
                    if (data.IndexOf(this.delimiter, StringComparison.Ordinal) == -1)
                    {
                        this.buffer += data;

                        data = string.Empty;
                    }
                    else
                    {
                        var pos = data.LastIndexOf(this.delimiter, StringComparison.Ordinal) + this.delimiter.Length;

                        this.buffer = data.Substring(pos);

                        data = data.Substring(0, pos);
                    }
                }

                var lines = data.Split(new[] { this.delimiter }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    this.OnLineAddition?.Invoke(this, new LogFileMonitorLineEventArgs { Line = line });
                }
            }

            this.size = newSize;

            lock (this.timer) this.monitoring = false;
        }
    }
}
