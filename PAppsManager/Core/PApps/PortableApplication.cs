using System;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PAppsManager.Core.PApps.Commands;

namespace PAppsManager.Core.PApps
{
    internal class PortableApplication
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">URL of the applicatin.</param>
        /// <param name="webClient">A function that returns the JSON string downloaded from a given URL.</param>
        /// <returns></returns>
        public static PortableApplication LoadFromUrl(string url, Func<string, string> webClient)
        {
            // Change protocol (if necessary).
            url = Regex.Replace(url, @"^papp://", "http://");

            string json = webClient(url);

            var application = JsonConvert.DeserializeObject<PortableApplication>(json, new DependencyJsonConverter(webClient));
            application.Url = url;

            return application;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new DependencyJsonConverter(null));
        }

        /// <summary>
        /// Unique URL identifying this application.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Program name.
        /// Should be identical if it's the same program (but not a must).
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Human version to display. Could be anything.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Note: Used to detect upgrades.
        /// </summary>
        [JsonProperty("release_date")]
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// URLs of other applications this application depends on in order to be installed and/or work.
        /// </summary>
        [JsonProperty("dependencies")]
        public PortableApplication[] Dependencies { get; set; }

        /// <summary>
        /// Operations to perform to retrieve and set up the portable application.
        /// This excludes any portabilization but only includes how to get the portable binaries.
        /// </summary>
        [JsonProperty("install_commands")]
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
            if (Dependencies == null)
                throw new JsonException("Portable application dependencies are not defined.");
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

        internal class DependencyJsonConverter : JsonConverter
        {
            private readonly Func<string, string> _webClient;

            public DependencyJsonConverter(Func<string, string> webClient)
            {
                _webClient = webClient;
            }

            #region Overrides of JsonConverter

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(PortableApplication[]);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var urls = serializer.Deserialize<string[]>(reader);
                if (urls == null)
                    return null;

                return urls.Select(url => LoadFromUrl(url, _webClient)).ToArray();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var applications = (PortableApplication[]) value;
                var applicatinsUrls = applications.Select(application => application.Url).ToArray();
                serializer.Serialize(writer, applicatinsUrls);
            }

            #endregion
        }
    }
}