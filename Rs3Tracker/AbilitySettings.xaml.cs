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

using Newtonsoft;
using Newtonsoft.Json;

using static Rs3Tracker.Settings;

namespace Rs3Tracker {
    /// <summary>
    /// Interaction logic for AbilitySettings.xaml
    /// </summary>
    public partial class AbilitySettings : Window {
        private List<Ability> abilities = new List<Ability>();
        public AbilitySettings() {
            InitializeComponent();
            if (File.Exists(".\\mongoAbilities.json")) {
                abilities = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"));
                var keybinds = abilities.OrderBy(i => i.cmbtStyle).ToList();
                dgSettings.ItemsSource = keybinds;
            }

            var Abils = Directory.GetFiles(".\\Images", "*.png");
            foreach (var name in Abils) {
                ComboboxItem comboboxItem = new ComboboxItem();
                comboboxItem.Text = name.Split('\\')[2].Split('.')[0];
                Images.Items.Add(comboboxItem);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            string json = "";
            if (dgSettings.ItemsSource != null)
                json = JsonConvert.SerializeObject(dgSettings.ItemsSource, Formatting.Indented);

            if (File.Exists(".\\mongoAbilities.json"))
                File.Delete(".\\mongoAbilities.json");

            var stream = File.Create(".\\mongoAbilities.json");
            stream.Close();
            File.WriteAllText(".\\mongoAbilities.json", json);

            abilities = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(txtAbilName.Text) || string.IsNullOrEmpty(txtCooldDown.Text) || string.IsNullOrEmpty(txtCmbtStyle.Text)) {
                MessageBox.Show("Data Missing");
                return;
            }

            Ability ability = new Ability();
            ability.name = txtAbilName.Text;
            double cd = -1;
            Double.TryParse(txtCooldDown.Text, out cd);
            if (cd != -1)
                ability.cooldown = cd;
            else
                return;
            ability.cmbtStyle = txtCmbtStyle.Text;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            ability.img = path + "Images\\" + Images.SelectedValue.ToString() + ".png";

            var Exists = abilities.Where(p => p.name == ability.name).Select(p => p).FirstOrDefault();

            if (Exists == null) {
                abilities.Add(ability);
                dgSettings.ItemsSource = null;
                dgSettings.ItemsSource = abilities;
                clearData();
            } else {
                MessageBox.Show("Ability Exists!");
            }
            
        }

        private void clearData() {
            imgAbil.Source = null;
            txtAbilName.Text = "";
            txtCmbtStyle.Text = "";
            txtCooldDown.Text = "";
            Images.SelectedIndex = 0;
        }

        private void Images_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            BitmapImage bitmap;
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path + "Images\\" + Images.SelectedValue.ToString() + ".png");
            bitmap.EndInit();
            imgAbil.Source = bitmap;
        }
    }
}
