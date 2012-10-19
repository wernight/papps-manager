using System;
using System.Net;
using Newtonsoft.Json;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManager.Core.PApps
{
    internal class PortableApplication
    {
        public static PortableApplication LoadFromUrl(string url)
        {
            using (var webClient = new WebClient())
            {
                string json = webClient.DownloadString(url);

                var application = JsonConvert.DeserializeObject<PortableApplication>(json);
                application.Url = url;
                return application;
            }
        }

        /// <summary>
        /// Unique URL identifying this application.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Program name.
        /// Should be identical if it's the same program (but not a must).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Human version to display. Could be anything.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Note: Used to detect upgrades.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// Operations to perform to retrieve and set up the portable application.
        /// This excludes any portabilization but only includes how to get the portable binaries.
        /// </summary>
        public CommandList InstallCommands { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PortableApplication)obj);
        }

        protected bool Equals(PortableApplication other)
        {
            return string.Equals(Url, other.Url);
        }

        public override int GetHashCode()
        {
            return (Url != null ? Url.GetHashCode() : 0);
        }
    }
}