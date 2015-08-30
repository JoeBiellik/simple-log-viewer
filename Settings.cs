using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace SimpleLogViewer
{
    [Serializable]
    public class Settings
    {
        private const string SETTINGS_FILE = @"settings.json";

        private static Settings instance;

        public bool FlashTaskbar { get; set; }
        public bool AutoScroll { get; set; }
        public bool WordWrap { get; set; }
        public Rectangle WindowPosition { get; set; }
        public bool WindowMaximized { get; set; }

        public static string Path => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? "", SETTINGS_FILE);

        private Settings()
        {
            this.FlashTaskbar = true;
            this.AutoScroll = true;
            this.WordWrap = true;
            this.WindowPosition = Rectangle.Empty;
            this.WindowMaximized = false;
        }

        public static Settings Instance
        {
            get
            {
                if (instance != null) return instance;

                try
                {
                    instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path));

                    if (instance == null) throw new NullReferenceException("Deserialization result is null");
                }
                catch (Exception)
                {
                    instance = new Settings();
                    Save();
                }

                return instance;
            }
        }

        public static void Save()
        {
            File.WriteAllText(Path, JsonConvert.SerializeObject(Instance, Formatting.Indented));
        }
    }
}
