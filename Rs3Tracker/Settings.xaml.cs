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
        private List<KeybindClass> keybindingList = new List<KeybindClass>();
        private List<BarKeybindClass> keybindingBarList = new List<BarKeybindClass>();
        private List<Ability> abilities = new List<Ability>();


        List<ComboBoxItem> ComboBoxItems = new List<ComboBoxItem>();
        public Settings() {
            InitializeComponent();
            KeyboardHook.KeyDownEvent += KeyDown;
            Loaded += Settings_Loaded;
           
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e) {
            if (File.Exists(".\\mongoAbilities.json")) {
                abilities = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"));
                if (abilities == null) {
                    this.Close();
                    return;
                }
                abilities = abilities.OrderBy(i => i.name).ToList();
                if (abilities != null)
                    foreach (var abil in abilities) {
                        ComboBoxItem ComboBoxItem = new ComboBoxItem();
                        ComboBoxItem.Content = abil.name;
                        cmbSource.Items.Add(ComboBoxItem);
                    }
            }

            if (File.Exists(".\\keybinds.json")) {
                keybindingList = JsonConvert.DeserializeObject<List<KeybindClass>>(File.ReadAllText(".\\keybinds.json"));
                if (keybindingList != null) {
                    var keybinds = keybindingList.OrderBy(i => i.bar.name).ToList();
                    if (keybinds != null)
                        foreach (var key in keybinds)
                            dgSettings.Items.Add(key);
                }
            }

            if (File.Exists(".\\barkeybinds.json")) {
                keybindingBarList = JsonConvert.DeserializeObject<List<BarKeybindClass>>(File.ReadAllText(".\\barkeybinds.json"));
                if (keybindingBarList != null)
                    foreach (var barkey in keybindingBarList)
                        dgSettingsBars.Items.Add(barkey);
            }


            if (File.Exists(".\\Bars.json")) {
                var bars = JsonConvert.DeserializeObject<List<BarClass>>(File.ReadAllText(".\\Bars.json"));
                cmbBar.Items.Add(new ComboBoxItem() { Content = "ALL" });
                if (bars != null)
                    foreach (var bar in bars) {
                        ComboBoxItem ComboBoxItem = new ComboBoxItem();
                        ComboBoxItem.Content = bar.name;
                        cmbBar.Items.Add(ComboBoxItem);
                        ComboBoxItem = new ComboBoxItem();
                        ComboBoxItem.Content = bar.name;
                        cmbBarKeybind.Items.Add(ComboBoxItem);
                    }
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

            SelectedKey.Text = content + e.Key.ToString();
            SelectedBarKey.Content = content + e.Key.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {

            string json = "";
            List<object> lists = new List<object>();
            foreach (var item in dgSettings.Items) {
                lists.Add(item);
                json = JsonConvert.SerializeObject(lists, Formatting.Indented);
            }

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
            if (SelectedKey.Text.ToString().Equals("Selected Key")) {
                MessageBox.Show("please select a keybind");
                return;
            }
            KeybindClass keybindClass = new KeybindClass();
            string[] keySplit = SelectedKey.Text.ToString().Split('+');
            var abil = abilities.Where(a => a.name == cmbSource.Text).Select(a => a).FirstOrDefault();
            if (keySplit.Length == 2) {
                keybindClass.modifier = keySplit[0];
                keybindClass.key = keySplit[1];
                keybindClass.ability = abil;
                keybindClass.bar = new BarClass() { name = cmbBar.Text };
            } else {
                keybindClass.modifier = "";
                keybindClass.key = keySplit[0];
                keybindClass.ability = abil;
                keybindClass.bar = new BarClass() { name = cmbBar.Text };
            }
            if (keybindingList == null)
                keybindingList = new List<KeybindClass>();


            dgSettings.Items.Add(keybindClass);

            SelectedKey.Text = "Selected Key";
        }

        private void cmbSource_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SelectedKey.IsEnabled = true;
            SelectedKey.Focus();
            SelectedKey.IsEnabled = false;
        }

        private void btnAddBarKey_Click(object sender, RoutedEventArgs e) {
            if (SelectedKey.Text.ToString().Equals("Selected Key")) {
                MessageBox.Show("please select a keybind");
                return;
            }
            if (string.IsNullOrEmpty(cmbBarKeybind.Text.ToString())) {
                MessageBox.Show("please select a keybind");
                return;
            }

            BarKeybindClass barKeybindClass = new BarKeybindClass();
            string[] keySplit = SelectedBarKey.Content.ToString().Split('+');

            if (keySplit.Length == 2) {
                barKeybindClass.modifier = keySplit[0];
                barKeybindClass.key = keySplit[1];
                barKeybindClass.name = cmbBarKeybind.Text;
                barKeybindClass.bar = new BarClass() { name = cmbBar.Text };
            } else {
                barKeybindClass.modifier = "";
                barKeybindClass.key = keySplit[0];
                barKeybindClass.name = cmbBarKeybind.Text;
                barKeybindClass.bar = new BarClass() { name = cmbBar.Text };
            }

            dgSettingsBars.Items.Add(barKeybindClass);
            SelectedBarKey.Content = "Selected Key";
            cmbSource.Focus();
        }

        private void btnSaveBars_Click(object sender, RoutedEventArgs e) {
            string json = "";
            List<object> lists = new List<object>();
            foreach (var item in dgSettingsBars.Items) {
                lists.Add(item);
                json = JsonConvert.SerializeObject(lists, Formatting.Indented);
            }


            if (File.Exists(".\\barkeybinds.json"))
                File.Delete(".\\barkeybinds.json");

            var stream = File.Create(".\\barkeybinds.json");
            stream.Close();
            File.WriteAllText(".\\barkeybinds.json", json);
        }

        private void dgSettings_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) {
            e.Cancel = true;
        }

        private void dgSettingsBars_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) {
            e.Cancel = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            dgSettings.Items.Remove(dgSettings.SelectedItem);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            for (int i = 0; i < dgSettings.SelectedItems.Count; i++) {
                dgSettings.Items.Remove(dgSettings.SelectedItems[i]);
                i--;
            }
        }

        private void btnDeleteBar_Click(object sender, RoutedEventArgs e) {
            for (int i = 0; i < dgSettingsBars.SelectedItems.Count; i++) {
                dgSettingsBars.Items.Remove(dgSettingsBars.SelectedItems[i]);
                i--;
            }
        }
        int cmbtxtLen = 0;
        private void cmbSource_TextChanged(object sender, TextChangedEventArgs e) {

            if (cmbtxtLen != 0) {
                if (cmbtxtLen > cmbSource.Text.Length)
                    if (File.Exists(".\\mongoAbilities.json")) {
                        cmbSource.Items.Clear();
                        abilities = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"));
                        abilities = abilities.OrderBy(i => i.name).ToList();
                        if (abilities != null)
                            foreach (var abil in abilities) {
                                ComboBoxItem ComboBoxItem = new ComboBoxItem();
                                ComboBoxItem.Content = abil.name;
                                ComboBoxItem.Tag = abil.img;
                                cmbSource.Items.Add(ComboBoxItem);
                            }
                    }
            }

            if (!string.IsNullOrEmpty(cmbSource.Text)) {
                cmbtxtLen = cmbSource.Text.Length;
                for (int i = 0; i < cmbSource.Items.Count; i++) {
                    if (!((ComboBoxItem)cmbSource.Items[i]).Content.ToString().ToLower().Contains(cmbSource.Text.ToLower())) {
                        cmbSource.Items.RemoveAt(i);
                        i--;
                    }
                }
            }

        }

        private void cmbSource_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            cmbSource.IsDropDownOpen = true;
        }
    }
}
