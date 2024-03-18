using RestSharp.Authenticators;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IniParser.Model;
using IniParser;
using System.IO;
using System.Windows;

namespace Rs3Tracker {
    internal class DualPC_Connection {
        public class requestSource {
            public string Key { get; set; }
            public string Modifier { get; set; }
        }
        public static async void POST(string key, string modifier) {
//            string ip = string.Empty;
//            string port = string.Empty;
//            if (File.Exists("ServerConfiguration.ini")) {
//                var parser = new FileIniDataParser();
//                IniData data = parser.ReadFile("ServerConfiguration.ini");
//                ip = data["Server"]["IP"];
//                port = data["Server"]["Port"];
//            } else {
//                MessageBox.Show("YOU NEED TO SETUP THE SERVER STUFF!");
//                return;
//            }
//#if DEBUG
//            ip = "localhost";
//            port = "8086";
//#endif

//            var options = new RestClientOptions("http://"+ip+":"+port+"?update");
//            var client = new RestClient(options);
            
//            var request = new RestRequest();
//            request.AddBody(new requestSource() { Key = key, Modifier = modifier });
//            // The cancellation token comes from the caller. You can still make a call without it.
//            var response = client.PostAsync(request);


        }
    }
}
