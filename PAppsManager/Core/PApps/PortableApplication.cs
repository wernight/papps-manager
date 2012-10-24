using System;
using System.Net;
using JetBrains.Annotations;
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

                application.Validate();

                return application;
            }
        }

        /// <summary>
        /// Unique URL identifying this application.
        /// </summary>
        [NotNull]
        public string Url { get; set; }

        /// <summary>
        /// Program name.
        /// Should be identical if it's the same program (but not a must).
        /// </summary>
        [NotNull]
        public string Name { get; set; }

        /// <summary>
        /// Human version to display. Could be anything.
        /// </summary>
        [NotNull]
        public string Version { get; set; }

        /// <summary>
        /// Note: Used to detect upgrades.
        /// </summary>
        [JsonProperty("release_date")]
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// Operations to perform to retrieve and set up the portable application.
        /// This excludes any portabilization but only includes how to get the portable binaries.
        /// </summary>
        [NotNull,JsonProperty("install_commands")]
        public CommandList InstallCommands { get; set; }

        /// <summary>
        /// Check all required fields are present.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new JsonException("Portable application name is not defined.");
            if (string.IsNullOrWhiteSpace(Version))
                throw new JsonException("Portable application version is not defined.");
            if (ReleaseDate == DateTime.MinValue)
                throw new JsonException("Portable application release date is not defined.");
            if (InstallCommands == null || InstallCommands.Count == 0)
                throw new JsonException("Portable application has no installation commands.");
            InstallCommands.Validate();
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Url);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
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