using System;
using System.Drawing;
using System.IO;

namespace DeadLock.Core.Properties
{
    public class Settings
    {
        private string languagePath = string.Empty;

        public readonly static Settings Default = new Settings();

        public int Language { get; set; } = 1;
        public Color MetroColor { get; set; } = Color.SteelBlue;
        public Size FormSize { get; set; } = new Size { Height = 0, Width = 0 };
        public bool RememberFormSize;
        public bool AutoUpdate { get; set; } = true;
        public bool ShowNotifyIcon { get; set; } = true;
        public bool StartMinimized { get; set; } = true;
        public bool ShowAdminWarning { get; set; } = true;
        public bool ViewDetails { get; set; } = true;
        public int BorderThickness { get; set; } = 3;
        public string LanguagePath
        {
            get { return languagePath; }
            set
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        languagePath = Path.GetFullPath(value);
                }
                catch { } // do nothing or notify user?
            }
        }
        public bool TakeOwnership { get; set; }
    }
}
