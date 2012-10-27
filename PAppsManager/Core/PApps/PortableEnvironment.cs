using System;
using System.IO;
using Newtonsoft.Json;

namespace PAppsManager.Core.PApps
{
    internal class PortableEnvironment : IDisposable
    {
        public PortableEnvironment()
        {
            Applications = new PortableApplicationCollection(this);
            Shortcuts = new ShortcutCollection(this);
        }

        public PortableApplicationCollection Applications { get; private set; }

        public ShortcutCollection Shortcuts { get; private set; }

        /// <summary>
        /// Load the environment settings from a file.
        /// 
        /// Create all the shortcuts, change the environment variables and the registry keys.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static PortableEnvironment Load(StreamReader reader)
        {
            string json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<PortableEnvironment>(json);
        }

        /// <summary>
        /// Save the current environment settings (shortcuts etc.)
        /// </summary>
        /// <param name="writer"></param>
        public void Save(StreamWriter writer)
        {
            // Save them.
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            writer.Write(json);
        }

        /// <summary>
        /// Tear down the portable environment set up and restore the original environment.
        /// </summary>
        public void CleanUp()
        {
            Shortcuts.Clear();
        }

        public void Dispose()
        {
            CleanUp();
        }
    }
}