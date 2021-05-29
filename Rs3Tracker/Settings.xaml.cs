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
using System.IO;
using Newtonsoft.Json;
using System.Data;
using FMUtils.KeyboardHook;

namespace Rs3Tracker {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window {
        Hook KeyboardHook = new Hook("Globalaction Link");
        string style = "";
        private List<KeybindClass> keybindingList = new List<KeybindClass>();

        public class ComboboxItem {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() {
                return Text;
            }
        }
      
        List<ComboboxItem> comboboxItems = new List<ComboboxItem>();
        public Settings(string _style) {
            InitializeComponent();
            KeyboardHook.KeyDownEvent += KeyDown;
            var Abils = Directory.GetFiles(".\\Images","*.png");
            foreach (var name in Abils) {
                ComboboxItem comboboxItem = new ComboboxItem();
                comboboxItem.Text = name.Split('\\')[2].Split('.')[0];
                cmbSource.Items.Add(comboboxItem);
            }
            style = _style;
            if (File.Exists(".\\keybinds.json")) {
                keybindingList = JsonConvert.DeserializeObject<List<KeybindClass>>(File.ReadAllText(".\\keybinds.json"));
                var keybinds = keybindingList.OrderBy(i => i.cmtStyle).ToList();
                dgSettings.ItemsSource = keybinds;
            }
        }

        private void KeyDown(KeyboardHookEventArgs e) {
            // handle keydown event here
            // Such as by checking if e (KeyboardHookEventArgs) matches the key you're interested in
            string content = "";
            if (e.isAltPressed)
                content += "ALT+";
            if (e.isCtrlPressed)
                content += "CTRL+";
            if (e.isShiftPressed)
                content += "SHIFT+";
            if (e.isWinPressed)
                content += "WIN+";

            SelectedKey.Content = content + e.Key.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {

            string json = "";
            if (dgSettings.ItemsSource != null)
                json = JsonConvert.SerializeObject(dgSettings.ItemsSource, Formatting.Indented);

            if (File.Exists(".\\keybinds.json"))
                File.Delete(".\\keybinds.json");

            var stream = File.Create(".\\keybinds.json");
            stream.Close();
            File.WriteAllText(".\\keybinds.json", json);

        }

        private void btnAddKey_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(cmbSource.Text)) {
                MessageBox.Show("please select an ability");
                return;
            }
            if (SelectedKey.Content.ToString().Equals("Selected Key")) {
                MessageBox.Show("please select a keybind");
                return;
            }
            KeybindClass keybindClass = new KeybindClass();
            string[] keySplit = SelectedKey.Content.ToString().Split('+');
            //if (keySplit.Length == 2) {
            //    keybindClass.modifier = keySplit[0];
            //    keybindClass.key = keySplit[1];
            //    keybindClass.img = cmbSource.Text;
            //    keybindClass.cmtStyle = style;
            //    keybindClass.duplicate = chkDuplicate.IsChecked;
            //} else {
            //    keybindClass.modifier = "";
            //    keybindClass.key = keySplit[0];
            //    keybindClass.img = cmbSource.Text;
            //    keybindClass.cmtStyle = style;
            //    keybindClass.duplicate = chkDuplicate.IsChecked;
            //}
            if (keybindingList == null)
                keybindingList = new List<KeybindClass>();

            keybindingList.Add(keybindClass);
            dgSettings.ItemsSource = null;
            dgSettings.ItemsSource = keybindingList;

            SelectedKey.Content = "Selected Key";
        }
    }
}
