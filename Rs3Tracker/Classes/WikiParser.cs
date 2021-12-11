﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rs3Tracker.Classes {
    public class WikiParser {
        public string getHTMLCode(string endpoint) {
            string url = "http://runescape.wiki/w/";
            string pageHTML = "";
            using (WebClient web = new WebClient()) {
                pageHTML = web.DownloadString(url + endpoint);

            }
            return pageHTML;
        }

        public string SaveImage(string name) {
            string finalName = name.Replace(" ", "_");
            if (name.Contains("Destroy")) {
                finalName = name.Replace(" ", "_") + "_(ability)";
            }

            string url = "http://runescape.wiki/images/" + finalName + ".png";
            using (WebClient client = new WebClient()) {
                try {
                    client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                } catch (Exception ex) {
                    try {
                        finalName = name.Replace(" ", "_") + "_(Ability)";
                        url = "http://runescape.wiki/images/" + finalName + ".png";
                        client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                    } catch (Exception ex2) {
                        try {
                            destroy:
                            finalName = name.Replace(" ", "_") + "_(ability)";
                            url = "http://runescape.wiki/images/" + finalName + ".png";
                            client.DownloadFile(new Uri(url), @".\Images\" + name.Replace(" ", "_") + ".png");
                        } catch(Exception ex3) {

                        }
                    }
                }

            }
            return name.Replace(" ", "_");
        }
    }
}