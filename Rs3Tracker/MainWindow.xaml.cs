using IniParser.Model;
using IniParser;
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
using System.Windows.Media.Media3D;

namespace Rs3Tracker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window {
        Display display = null;
        public static double displayX = 0;
        public static double displayY = 0;
        public static double DisplayHeight = 80;
        public static double DisplayWidth = 800;
        public MainWindow() {
            InitializeComponent();
            Closing += MainWindow_Closing;
            btnStartTracker.Content = "Start Tracker";
            cmbMode.SelectedIndex = 0;
            if (File.Exists(".\\Bars.json")) {
                var bars = JsonConvert.DeserializeObject<List<BarClass>>(File.ReadAllText(".\\Bars.json"));
                foreach (var bar in bars) {
                    ComboBoxItem ComboBoxItem = new ComboBoxItem();
                    ComboBoxItem.Content = bar.name;
                    cmbMode.Items.Add(ComboBoxItem);
                }
            }
            if (File.Exists("Configuration.ini")) {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile("Configuration.ini");
                displayX = Convert.ToDouble(data["UI"]["LEFT"]);
                displayY = Convert.ToDouble(data["UI"]["TOP"]);
                DisplayHeight = Convert.ToDouble(data["UI"]["HEIGHT"]);
                DisplayWidth = Convert.ToDouble(data["UI"]["WIDTH"]);
            }

            if (!File.Exists(".\\mongoAbilities.json"))
                File.Create(".\\mongoAbilities.json");
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            btnClose_Click(null, null);
        }

        private void btnStartTracker_Click(object sender, RoutedEventArgs e) {
            if (display != null) {
                displayX = display.Left; 
                displayY = display.Top;
                DisplayHeight = display.Height;
                DisplayWidth = display.Width;
                display.Close();
                display = null;
                btnStartTracker.Content = "Start Tracker";

            } else {
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

                btnStartTracker.Content = "Close Tracker";
                display = new Display(cmbMode.Text.ToLower(), TrackCD.IsChecked.Value, onTop.IsChecked.Value, CanResize.IsChecked.Value);
                display.Top = displayY;
                display.Left = displayX;
                display.Height = DisplayHeight;
                display.Width = DisplayWidth;
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
            if (display != null) {
                displayX = display.Left;
                displayY = display.Top;
                DisplayHeight = display.Height;
                DisplayWidth = display.Width;                                          
                display.Close();
            }
            var parser = new FileIniDataParser();
            IniData data = new IniData();
            data["UI"]["LEFT"] = displayX.ToString();
            data["UI"]["TOP"] = displayY.ToString();
            data["UI"]["HEIGHT"] = DisplayHeight.ToString();
            data["UI"]["WIDTH"] = DisplayWidth.ToString();
            parser.WriteFile("Configuration.ini", data);
            Environment.Exit(1);
        }

        private void btnBars_Click(object sender, RoutedEventArgs e) {
            Bars bars = new Bars();
            bars.ShowDialog();
            if (File.Exists(".\\Bars.json")) {
                cmbMode.Items.Clear();
                var bars2 = JsonConvert.DeserializeObject<List<BarClass>>(File.ReadAllText(".\\Bars.json"));
                foreach (var bar in bars2) {
                    ComboBoxItem ComboBoxItem = new ComboBoxItem();
                    ComboBoxItem.Content = bar.name;
                    cmbMode.Items.Add(ComboBoxItem);
                }
            }
        }

        private void cmbMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (display != null) {
                if (cmbMode.SelectedItem != null) {
                    display.style = ((ComboBoxItem)cmbMode.SelectedItem).Content.ToString().ToLower();
                    display.changeStyle();
                }
            }
        }

        private void CanResize_Checked(object sender, RoutedEventArgs e) {
            if (display != null) {
                display.ResizeON();
            }
        }


        private void CanResize_Unchecked(object sender, RoutedEventArgs e) {
            if (display != null) {
                display.ResizeOFF();
            }
        }
    }
}
