using System;
using System.Collections.Generic;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            cmbMode.SelectedIndex = 0;
        }

        private void btnStartTracker_Click(object sender, RoutedEventArgs e) {
            Display display = new Display(cmbMode.Text.ToLower());
            display.Show();
        }
        private void btnAbilityConfig_Click(object sender, RoutedEventArgs e) {
            AbilitySettings display = new AbilitySettings();
            display.Show();
        }
        private void btnSettings_Click(object sender, RoutedEventArgs e) {
            Settings settings = new Settings();
            settings.Show();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            Environment.Exit(1);
        }
    }
}
