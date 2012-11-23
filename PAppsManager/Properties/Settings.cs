using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace PAppsManager.Properties
{
    /// <summary>
    /// Application settings serialized as JSON. This class simplifies saving settings next to the application.
    /// </summary>
    public class Settings
    {
        private static readonly string SettingsFile;
        private TimeSpan _updateCheckInterval;

        static Settings()
        {
            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            SettingsFile = Path.Combine(exeDirectory, "Settings.json");

            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                Default = JsonConvert.DeserializeObject<Settings>(json);
            }
            else
            {
                Default = new Settings();
            }
        }

        public Settings()
        {
            // Default settings.
            PortableApplicationsBaseDirectory = "..";
            StartMenuDirectoryName = "PApps";
            UpdateCheckInterval = new TimeSpan(7, 0, 0, 0);
        }

        public static Settings Default { get; private set; }

        public string PortableApplicationsBaseDirectory { get; set; }

        public string StartMenuDirectoryName { get; set; }

        public TimeSpan UpdateCheckInterval
        {
            get { return _updateCheckInterval; }
            set { _updateCheckInterval = value.TotalHours > 3 ? value : new TimeSpan(3, 0, 0); }
        }

        public DateTime LastUpdateCheckTime { get; set; }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this);
            using (var writer = new StreamWriter(SettingsFile))
                writer.Write(json);
        }
    }
}