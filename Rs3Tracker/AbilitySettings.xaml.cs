using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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
               

                foreach (var ability in abilities) {
                    if (!string.IsNullOrEmpty(ability.img)) {
                        var index = ability.img.IndexOf("Images");
                        string path = ".\\" + ability.img.Substring(index);
                        ability.img = path;                       
                    }
                }
                var keybinds = abilities.OrderBy(i => i.name).ToList();
                dgSettings.ItemsSource = keybinds;
            }

            LoadCombo();
        }

        private void LoadCombo() {
            Images.Items.Clear();
            var Abils = Directory.GetFiles(".\\Images", "*.*").Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".jpg")).ToList();
            
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
            if (string.IsNullOrEmpty(txtAbilName.Text) || string.IsNullOrEmpty(txtCooldDown.Text)) {
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
            //ability.cmbtStyle = txtCmbtStyle.Text;

            if (Images.SelectedValue != null) 
                ability.img = ".\\Images\\" + Images.SelectedValue.ToString() + ".png";
            

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
            txtCooldDown.Text = "";
            Images.SelectedIndex = -1;
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceFromBitmap(Bitmap bmp) {
            var handle = bmp.GetHbitmap();
            try {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            } finally { DeleteObject(handle); }
        }
        private void Images_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Images.SelectedValue != null) {
                Bitmap bitmap = new Bitmap(".\\Images\\" + Images.SelectedValue.ToString() + ".png");
                Bitmap Image;
                ImageSource imageSource;
                imageSource = ImageSourceFromBitmap(bitmap);
                imgAbil.Source = imageSource;

                txtAbilName.Text = Images.SelectedValue.ToString().Replace("_", " ");

            } else {
                imgAbil.Source = null;
            }

        }

        private void reloadCombo_Click(object sender, RoutedEventArgs e) {
            LoadCombo();
        }
    }
}
