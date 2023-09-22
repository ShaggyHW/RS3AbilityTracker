using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rs3Tracker.Classes {
    public class WikiParser {
        public string getHTMLCode(string endpoint) {
            string url = "http://runescape.wiki/w/";
            string pageHTML = "";
            using (WebClient web = new WebClient()) {
                web.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
                pageHTML = web.DownloadString(url + endpoint);
            }
            return pageHTML;
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

        public string SaveImageFROMURL(string name, string endpoint) {
            string finalName = name.Replace(" ", "_");
            //if (name.Contains("Destroy")) {
            //    finalName = name.Replace(" ", "_") + "_(ability)";
            //}
            if (File.Exists(@".\Images\" + name.Replace(" ", "_") + ".png")) {
                return name.Replace(" ", "_");
            }
            if (IsFileLocked(@".\Images\" + name.Replace(" ", "_") + ".png")) {
                return "";
            }
            string url = "https://runescape.wiki" + endpoint;
            using (WebClient client = new WebClient()) {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                try {
                    client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                    client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                } catch (Exception ex) {
                    try {
                        finalName = name.Replace(" ", "_") + "_(Ability)";
                        url = "https://runescape.wiki/images/" + finalName + ".png";
                        client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                        client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                    } catch (Exception ex2) {
                        try {

                            finalName = name.Replace(" ", "_") + "_(ability)";
                            url = "https://runescape.wiki/images/" + finalName + ".png";
                            client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                            client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                        } catch (Exception ex3) {
                            MessageBox.Show(endpoint);
                        }
                    }
                }

            }
            return name.Replace(" ", "_");
        }

        public string SaveImage(string name) {
            string finalName = name.Replace(" ", "_");
            if (name.Contains("Destroy")) {
                finalName = name.Replace(" ", "_") + "_(ability)";
            }
            if (IsFileLocked(@".\Images\" + name.Replace(" ", "_") + ".png")) {
                return "";
            }
            string url = "https://runescape.wiki/images/" + name + ".png";
            using (WebClient client = new WebClient()) {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                try {
                    client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                    client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                } catch (Exception ex) {
                    try {
                        finalName = name.Replace(" ", "_") + "_(Ability)";
                        url = "https://runescape.wiki/images/" + finalName + ".png";
                        client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                        client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                    } catch (Exception ex2) {
                        try {

                            finalName = name.Replace(" ", "_") + "_(ability)";
                            url = "https://runescape.wiki/images/" + finalName + ".png";
                            client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                            client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                        } catch (Exception ex3) {
                            MessageBox.Show(name);
                        }
                    }
                }

            }
            return name.Replace(" ", "_");
        }
    }
}
