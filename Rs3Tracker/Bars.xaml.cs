using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rs3Tracker {
    /// <summary>
    /// Interaction logic for Bars.xaml
    /// </summary>
    public partial class Bars : Window {
        private List<BarClass> bars = new List<BarClass>();

        public Bars() {
            InitializeComponent();
            if (File.Exists(".\\Bars.json")) {
                bars = JsonConvert.DeserializeObject<List<BarClass>>(File.ReadAllText(".\\Bars.json"));
                var keybinds = bars.OrderBy(i => i.name).ToList();
                foreach (var key in keybinds) {
                    dgSettings.Items.Add(key);
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(txtBarName.Text)) {
                //bars.Add(new BarClass() {
                //    name = txtBarName.Text
                //});
                dgSettings.Items.Add(new BarClass() { name = txtBarName.Text });
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            string json = "";
            List<object> lists = new List<object>();
            foreach (var item in dgSettings.Items) {
                lists.Add(item);
                json = JsonConvert.SerializeObject(lists, Formatting.Indented);
            }

            if (File.Exists(".\\Bars.json"))
                File.Delete(".\\Bars.json");

            var stream = File.Create(".\\Bars.json");
            stream.Close();
            File.WriteAllText(".\\Bars.json", json);
        }

        private void dgSettings_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) {
            e.Cancel = true;
        }
    }
}
