using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HtmlAgilityPack;

namespace Rs3Tracker.Classes {
    public class WikiParser {

        /// <summary>
        /// runescape.wiki blocks generic browser user agents with a 403 challenge page. Their policy
        /// asks automated clients to identify themselves and to leave a way of being contacted, so
        /// send the tool name and its repository instead of pretending to be Chrome.
        /// </summary>
        public const string UserAgent = "RS3AbilityTracker/1.0 (+https://github.com/ShaggyHW/RS3AbilityTracker)";

        private const string Wiki = "https://runescape.wiki";
        private const string ImagesFolder = @".\Images\";
        private const int Attempts = 3;

        private readonly List<string> failures = new List<string>();
        private readonly object failuresLock = new object();

        static WikiParser() {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            ServicePointManager.DefaultConnectionLimit = 16;
        }

        /// <summary>Names of every image that could not be downloaded since the last <see cref="ClearFailures"/>.</summary>
        public List<string> Failures {
            get { lock (failuresLock) { return failures.ToList(); } }
        }

        public void ClearFailures() {
            lock (failuresLock) { failures.Clear(); }
        }

        private void RecordFailure(string what) {
            lock (failuresLock) {
                if (!failures.Contains(what))
                    failures.Add(what);
            }
        }

        private WebClient CreateClient() {
            var web = new WebClient();
            web.Encoding = Encoding.UTF8;
            web.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);
            web.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9");
            return web;
        }

        /// <summary>Downloads a wiki article. Throws when the wiki cannot be reached at all.</summary>
        public string getHTMLCode(string endpoint) {
            string url = Wiki + "/w/" + endpoint;
            Exception last = null;
            for (int attempt = 0; attempt < Attempts; attempt++) {
                try {
                    using (var web = CreateClient()) {
                        return web.DownloadString(url);
                    }
                } catch (Exception ex) {
                    last = ex;
                    Thread.Sleep(500 * (attempt + 1));
                }
            }
            throw new WebException("Could not download " + url, last);
        }

        /// <summary>Loads a wiki article into a parsed document, or null when it could not be downloaded.</summary>
        public HtmlDocument getPage(string endpoint) {
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(getHTMLCode(endpoint));
                return doc;
            } catch (Exception) {
                RecordFailure("page " + endpoint);
                return null;
            }
        }

        /// <summary>
        /// The cooldown of a single spell / ability, read off the infobox of its own article.
        /// Used for the tables that no longer carry a cooldown column.
        /// </summary>
        public double getCooldownFromPage(string link) {
            if (string.IsNullOrEmpty(link))
                return 0;
            var doc = getPage(link.StartsWith("/w/") ? link.Substring(3) : link);
            if (doc == null)
                return 0;
            var cell = doc.DocumentNode.SelectSingleNode("//td[@data-attr-param='cooldown_disp']")
                    ?? doc.DocumentNode.SelectSingleNode("//td[@data-attr-param='cooldown']")
                    ?? doc.DocumentNode.SelectSingleNode("//tr[th[contains(normalize-space(.), 'Cooldown')]]/td");
            return cell == null ? 0 : WikiTables.ParseSeconds(cell.InnerText);
        }

        const int ERROR_SHARING_VIOLATION = 32;
        const int ERROR_LOCK_VIOLATION = 33;
        protected virtual bool IsFileLocked(string filePath) {
            try {
                FileInfo file = new FileInfo(filePath);
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None)) {
                    stream.Close();
                }
            } catch (IOException exception) {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)

                int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);

                if (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION)

                    return true;

            }

            //file is not locked
            return false;
        }

        /// <summary>The file name an ability is stored under, without extension.</summary>
        public static string FileNameFor(string name) {
            string fileName = (name ?? "").Replace(" ", "_");
            foreach (char invalid in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalid, '_');
            return fileName;
        }

        /// <summary>
        /// Downloads the icon at <paramref name="endpoint"/> (a wiki relative image url) and stores it
        /// under the ability name. Returns the stored file name, or "" when nothing could be stored.
        /// </summary>
        public string SaveImageFROMURL(string name, string endpoint) {
            string fileName = FileNameFor(name);
            if (string.IsNullOrEmpty(fileName))
                return "";
            string target = ImagesFolder + fileName + ".png";
            if (File.Exists(target))
                return fileName;
            if (IsFileLocked(target))
                return "";
            if (string.IsNullOrEmpty(endpoint))
                return SaveImage(name);

            string url = endpoint.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? endpoint : Wiki + endpoint;
            if (Download(url, target))
                return fileName;
            return SaveImage(name);
        }

        /// <summary>
        /// Downloads an icon by guessing its file name on the wiki, for the rows that carry no image.
        /// Returns the stored file name, or "" when none of the guesses exist.
        /// </summary>
        public string SaveImage(string name) {
            string fileName = FileNameFor(name);
            if (string.IsNullOrEmpty(fileName))
                return "";
            string target = ImagesFolder + fileName + ".png";
            if (File.Exists(target))
                return fileName;
            if (IsFileLocked(target))
                return "";

            foreach (string candidate in new[] { fileName, fileName + "_(ability)", fileName + "_(Ability)", fileName + "_icon" }) {
                if (Download(Wiki + "/images/" + Uri.EscapeDataString(candidate) + ".png", target))
                    return fileName;
            }
            RecordFailure(name);
            return "";
        }

        private bool Download(string url, string target) {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(target)));
                using (var client = CreateClient()) {
                    client.DownloadFile(new Uri(url), target);
                }
                return true;
            } catch (Exception) {
                // A failed DownloadFile still leaves an empty file behind, which would then be
                // mistaken for a cached icon on the next import.
                try {
                    if (File.Exists(target) && new FileInfo(target).Length == 0)
                        File.Delete(target);
                } catch (Exception) { }
                return false;
            }
        }
    }
}
