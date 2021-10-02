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

using static Rs3Tracker.Settings;

namespace Rs3Tracker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Display display;

        public MainWindow() {
            InitializeComponent();
            btnStartTracker.Content = "Start Tracker";
            cmbMode.SelectedIndex = 0;
            if (File.Exists(".\\Bars.json")) {
                var bars = JsonConvert.DeserializeObject<List<BarClass>>(File.ReadAllText(".\\Bars.json"));
                foreach (var bar in bars) {
                    ComboboxItem comboboxItem = new ComboboxItem();
                    comboboxItem.Text = bar.name;
                    cmbMode.Items.Add(comboboxItem);
                }
            }
        }

        private void btnStartTracker_Click(object sender, RoutedEventArgs e) {
            if (display != null) {
                display.Close();
                display = null;
                btnStartTracker.Content = "Start Tracker";
           
            } else {
                btnStartTracker.Content = "Close Tracker";

                if (!File.Exists(".\\keybinds.json")) {
                    MessageBox.Show("Missing Keybinds");
                    return;
                }
                if (!File.Exists(".\\barkeybinds.json")) {
                    MessageBox.Show("Missing Bar Keybinds");
                    return;
                }
                if (string.IsNullOrEmpty(cmbMode.Text))
                    return;

                display = new Display(cmbMode.Text.ToLower(), TrackCD.IsChecked.Value, onTop.IsChecked.Value);
                display.Show();
            }
        }
        private void btnAbilityConfig_Click(object sender, RoutedEventArgs e) {
            AbilitySettings display = new AbilitySettings();
            display.ShowDialog();
        }
        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            Environment.Exit(1);
        }

        private void btnBars_Click(object sender, RoutedEventArgs e) {
            Bars bars = new Bars();
            bars.ShowDialog();
            if (File.Exists(".\\Bars.json")) {
                cmbMode.Items.Clear();
                var bars2 = JsonConvert.DeserializeObject<List<BarClass>>(File.ReadAllText(".\\Bars.json"));
                foreach (var bar in bars2) {
                    ComboboxItem comboboxItem = new ComboboxItem();
                    comboboxItem.Text = bar.name;
                    cmbMode.Items.Add(comboboxItem);
                }
            }
        }


    }
}
