using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace PAppsManager.Core.PApps.Commands
{
    /// <summary>
    /// Download a file.
    /// </summary>
    public class DownloadCommand : Command
    {
        public string Url { get; set; }

        public string FileName { get; set; }

        public string Hash { get; set; }

        #region Command Members

        public override string Validate()
        {
            // Validate the FileName
            var valid = ValidateFileName(FileName);
            if (valid != null)
                return valid;

            // Validate the URL
            if (string.IsNullOrWhiteSpace(Url))
                return "URL not provided";

            // Validate the Hash
            try
            {
                GetHashValidatorFor(Hash);
            }
            catch (Exception e)
            {
                return "Invalid hash: " + e.Message;
            }

            return null;
        }

        public override void Execute()
        {
            using (var webClient = new WebClient())
            {
                string targetFilePath = Path.Combine(InstallTargerDirectory, FileName);
                webClient.DownloadFile(Url, targetFilePath);

                Func<Stream, bool> hashValid = GetHashValidatorFor(Hash);
                using (var stream = new FileStream(targetFilePath, FileMode.Open))
                    if (!hashValid(stream))
                        throw new CommandException("The downloaded file hash doesn't match the expected hash. Please check your firewall and anti-virus rules and try again later.");
            }
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Download {0} as {1}", Url, FileName);
        }

        private Func<Stream, bool> GetHashValidatorFor(string hash)
        {
            // Not hash provded?
            if (string.IsNullOrWhiteSpace(Hash))
                return stream => true;

            // Parse the hash string.
            const string pattern = @"^(\w+):([0-9a-fA-F]+)$";
            Match match = Regex.Match(hash, pattern);
            if (!match.Success)
                throw new CommandException("Invalid hash, expecting something matching the regex: " + pattern);
            string hashAlgorithm = match.Groups[1].Value;
            byte[] expectedHash = StringToByteArray(match.Groups[2].Value);

            // Return the hashing function.
            switch (hashAlgorithm)
            {
                case "md5":
                    if (expectedHash.Length != 20)
                        throw new CommandException("Invalid hash, should be 20 bytes long for SHA-1.");

                    return s =>
                               {
                                   using (var md5 = new MD5CryptoServiceProvider())
                                       return md5.ComputeHash(s).SequenceEqual(expectedHash);
                               };

                case "sha1":
                    if (expectedHash.Length != 20)
                        throw new CommandException("Invalid hash, should be 20 bytes long for SHA-1.");

                    return s =>
                               {
                                   using (var sha1 = new SHA1Managed())
                                   {
                                       return sha1.ComputeHash(s).SequenceEqual(expectedHash);
                                   }
                               };

                default:
                    throw new CommandException("Unkown hash algorithm (" + hashAlgorithm + "), expected 'md5' or 'sha1'.");
            }
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}