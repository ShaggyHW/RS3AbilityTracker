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

        public class ComboboxItem {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() {
                return Text;
            }
        }

        List<ComboboxItem> comboboxItems = new List<ComboboxItem>();
        public Settings() {
            InitializeComponent();
            KeyboardHook.KeyDownEvent += KeyDown;
            if (File.Exists(".\\mongoAbilities.json")) {
                abilities = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"));
                abilities = abilities.OrderBy(i => i.name).ToList();
                if (abilities != null)
                    foreach (var abil in abilities) {
                        ComboboxItem comboboxItem = new ComboboxItem();
                        comboboxItem.Text = abil.name;
                        cmbSource.Items.Add(comboboxItem);
                    }
            }

            if (File.Exists(".\\keybinds.json")) {
                keybindingList = JsonConvert.DeserializeObject<List<KeybindClass>>(File.ReadAllText(".\\keybinds.json"));
                var keybinds = keybindingList.OrderBy(i => i.bar.name).ToList();
                if (keybinds != null)
                    foreach (var key in keybinds)
                        dgSettings.Items.Add(key);
            }

            if (File.Exists(".\\barkeybinds.json")) {
                keybindingBarList = JsonConvert.DeserializeObject<List<BarKeybindClass>>(File.ReadAllText(".\\barkeybinds.json"));
                foreach (var barkey in keybindingBarList)
                    dgSettingsBars.Items.Add(barkey);
            }


            if (File.Exists(".\\Bars.json")) {
                var bars = JsonConvert.DeserializeObject<List<BarClass>>(File.ReadAllText(".\\Bars.json"));
                cmbBar.Items.Add(new ComboboxItem() { Text = "ALL" });
                if (bars != null)
                    foreach (var bar in bars) {
                        ComboboxItem comboboxItem = new ComboboxItem();
                        comboboxItem.Text = bar.name;
                        cmbBar.Items.Add(comboboxItem);

                        cmbBarKeybind.Items.Add(comboboxItem);
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

            SelectedKey.Content = content + e.Key.ToString();
            SelectedBarKey.Content = content + e.Key.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {

            string json = "";
            List<object> lists = new List<object>();
            foreach (var item in dgSettings.Items) {
                lists.Add(item);
                json = JsonConvert.SerializeObject(lists, Formatting.Indented);
            }

            //if (dgSettings.ItemsSource != null) {
            //    var lists = (List<KeybindClass>)dgSettings.ItemsSource;
            //    foreach (var item in lists) {
            //        var updateAbility = abilities.Where(a => a.name == item.ability.name).Select(a => a).FirstOrDefault();
            //        item.ability = updateAbility;
            //    }
            //    json = JsonConvert.SerializeObject(lists, Formatting.Indented);
            //}

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
            var abil = abilities.Where(a => a.name == cmbSource.Text).Select(a => a).FirstOrDefault();
            if (keySplit.Length == 2) {
                //var barkeybind = keybindingBarList.Where(kb => kb.key.Equals(keySplit[1]) && kb.modifier.Equals(keySplit[0])).Select(kb => kb).FirstOrDefault();
                //if (barkeybind != null) {
                //    MessageBox.Show("Keybind is already set a Keybind to switch BARS!");
                //    return;
                //}


                keybindClass.modifier = keySplit[0];
                keybindClass.key = keySplit[1];
                keybindClass.ability = abil;
                keybindClass.bar = new BarClass() { name = cmbBar.Text };
            } else {
                //var barkeybind = keybindingBarList.Where(kb => kb.key.Equals(keySplit[0]) && kb.modifier.Equals("")).Select(kb => kb).FirstOrDefault();
                //if (barkeybind != null) {
                //    MessageBox.Show("Keybind is already set a Keybind to switch BARS!");
                //    return;
                //}
                keybindClass.modifier = "";
                keybindClass.key = keySplit[0];
                keybindClass.ability = abil;
                keybindClass.bar = new BarClass() { name = cmbBar.Text };
            }
            if (keybindingList == null)
                keybindingList = new List<KeybindClass>();

            //keybindingList.Add(keybindClass);
            //dgSettings.ItemsSource = null;
            dgSettings.Items.Add(keybindClass);

            SelectedKey.Content = "Selected Key";
        }

        private void cmbSource_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.Focus();
        }

        private void btnAddBarKey_Click(object sender, RoutedEventArgs e) {
            if (SelectedKey.Content.ToString().Equals("Selected Key")) {
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
                //var barkeybind = keybindingBarList.Where(kb => kb.key.Equals(keySplit[1]) && kb.modifier.Equals(keySplit[0])).Select(kb => kb).FirstOrDefault();
                //if (barkeybind != null) {
                //    MessageBox.Show("Keybind is already set a Keybind to switch BARS!");
                //    return;
                //}

                //var keybind = keybindingList.Where(kb => kb.key.Equals(keySplit[1]) && kb.modifier.Equals(keySplit[0])).Select(kb => kb).FirstOrDefault();
                //if (keybind != null) {
                //    MessageBox.Show("Keybind is already set a Keybind for an ability!");
                //    return;
                //}

                barKeybindClass.modifier = keySplit[0];
                barKeybindClass.key = keySplit[1];
                barKeybindClass.name = cmbBarKeybind.Text;

            } else {
                //var barkeybind = keybindingBarList.Where(kb => kb.key.Equals(keySplit[0]) && kb.modifier.Equals("")).Select(kb => kb).FirstOrDefault();
                //if (barkeybind != null) {
                //    MessageBox.Show("Keybind is already set a Keybind to switch BARS!");
                //    return;
                //}

                //var keybind = keybindingList.Where(kb => kb.key.Equals(keySplit[0]) && kb.modifier.Equals("")).Select(kb => kb).FirstOrDefault();
                //if (keybind != null) {
                //    MessageBox.Show("Keybind is already set a Keybind for an ability!");
                //    return;
                //}

                barKeybindClass.modifier = "";
                barKeybindClass.key = keySplit[0];
                barKeybindClass.name = cmbBarKeybind.Text;

            }
            //if (keybindingBarList == null)
            //    keybindingBarList = new List<BarKeybindClass>();

            //keybindingBarList.Add(barKeybindClass);
            //dgSettingsBars.ItemsSource = null;
            dgSettingsBars.Items.Add(barKeybindClass);
            SelectedBarKey.Content = "Selected Key";


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
    }
}
