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

using Rs3Tracker.Classes;
using Newtonsoft;
using Newtonsoft.Json;

using static Rs3Tracker.Settings;
using HtmlAgilityPack;
using System.Diagnostics;

namespace Rs3Tracker {
    /// <summary>
    /// Interaction logic for AbilitySettings.xaml
    /// </summary>
    public partial class AbilitySettings : Window {
        private List<Ability> abilities = new List<Ability>();
        List<Ability> abils = new List<Ability>();
        public AbilitySettings() {
            InitializeComponent();
            if (!Directory.Exists(".\\Images"))
                Directory.CreateDirectory(".\\Images");
            if (!Directory.Exists(".\\PersonalImages"))
                Directory.CreateDirectory(".\\PersonalImages");
            if (File.Exists(".\\mongoAbilities.json")) {
                abilities = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"));
                if (abilities != null) {
                    var keybinds = abilities.OrderBy(i => i.name).ToList();
                    foreach (var key in keybinds) {
                        dgSettings.Items.Add(key);
                    }
                }
            }

            LoadCombo();
        }

        private void LoadCombo() {
            Images.Items.Clear();
            var Abils = Directory.GetFiles(".\\Images", "*.*").Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".jpg")).ToList();

            foreach (var name in Abils) {
                ComboBoxItem ComboBoxItem = new ComboBoxItem();
                ComboBoxItem.Content = name.Split('\\')[2].Split('.')[0];
                ComboBoxItem.Tag = ".\\Images";
                Images.Items.Add(ComboBoxItem);
            }
            Abils = Directory.GetFiles(".\\PersonalImages", "*.*").Where(s => s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".jpg")).ToList();

            foreach (var name in Abils) {
                ComboBoxItem ComboBoxItem = new ComboBoxItem();
                ComboBoxItem.Content = name.Split('\\')[2].Split('.')[0];
                ComboBoxItem.Tag = ".\\PersonalImages";
                Images.Items.Add(ComboBoxItem);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            string json = "";
            List<object> lists = new List<object>();
            foreach (var item in dgSettings.Items) {
                lists.Add(item);
                json = JsonConvert.SerializeObject(lists, Formatting.Indented);
            }
            if (File.Exists(".\\mongoAbilities.json"))
                File.Delete(".\\mongoAbilities.json");

            var stream = File.Create(".\\mongoAbilities.json");
            stream.Close();
            File.WriteAllText(".\\mongoAbilities.json", json);

            abilities = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(txtAbilName.Text)) {
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

            if (Images.SelectedValue != null) {
                ability.img = ((ComboBoxItem)Images.SelectedValue).Tag.ToString() + "\\" + ((ComboBoxItem)Images.SelectedValue).Content.ToString() + ".png";

            }

            var Exists = abilities.Where(p => p.name == ability.name).Select(p => p).FirstOrDefault();

            if (Exists == null) {
                abilities.Add(ability);
                dgSettings.Items.Clear();
                foreach (var abil in abilities) {
                    dgSettings.Items.Add(abil);
                }
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
                try {
                    Bitmap bitmap = new Bitmap(((ComboBoxItem)Images.SelectedValue).Tag.ToString() + "\\" + ((ComboBoxItem)Images.SelectedValue).Content.ToString() + ".png");
                    //Bitmap Image;
                    ImageSource imageSource;
                    imageSource = ImageSourceFromBitmap(bitmap);
                    imgAbil.Source = imageSource;
                    txtAbilName.Text = ((ComboBoxItem)Images.SelectedValue).Content.ToString().Replace("_", " ");
                }catch(Exception ex) {
                    MessageBox.Show("Image not supported... Verify if you download a jpg");
                }
            } else {
                imgAbil.Source = null;
            }
        }

        private void reloadCombo_Click(object sender, RoutedEventArgs e) {
            LoadCombo();
        }

        private void dgSettings_BeginningEdit(object sender, DataGridBeginningEditEventArgs e) {
            e.Cancel = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            for (int i = 0; i < dgSettings.SelectedItems.Count; i++) {
                dgSettings.Items.Remove(dgSettings.SelectedItems[i]);
                i--;
            }
        }
        //Deprecated
        private void CSVAbilParser() {
            var lines = File.ReadAllLines(".\\Abilities.csv");
            List<Ability> abils = new List<Ability>();
            foreach (var line in lines) {
                Ability ability = new Ability();
                ability.name = line.Split(',')[0];
                ability.cooldown = Convert.ToDouble(line.Split(',')[1]);
                ability.img = ".\\Images\\" + line.Split(',')[0].Replace(' ', '_') + ".png";
                abils.Add(ability);
            }
            if (File.Exists(".\\mongoAbilities.json"))
                File.Delete(".\\mongoAbilities.json");

            var stream = File.Create(".\\mongoAbilities.json");
            stream.Close();
            File.WriteAllText(".\\mongoAbilities.json", JsonConvert.SerializeObject(abils, Formatting.Indented));
            LoadCombo();
            var keybinds = abils.OrderBy(i => i.name).ToList();
            foreach (var key in keybinds) {
                dgSettings.Items.Add(key);
            }
        }

        public async void Wiki() {

        }


        /// <summary>Maps the section headings of the ability page onto the prefixes used by the tracker.</summary>
        private static readonly Dictionary<string, string> AbilitySections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "Melee", "Melee_" },
            { "Ranged", "Range_" },
            { "Magic", "Mage_" },
            { "Necromancy", "Necromancy_" },
            { "Defence", "Defense_" },
            { "Defense", "Defense_" },
            { "Constitution", "Constitution_" },
        };

        /// <summary>One entry read off the wiki, before its icon has been downloaded.</summary>
        private class ScrapedAbility {
            public string Prefix;
            public string Name;
            public double Cooldown;
            public string ImageUrl;
        }

        public void GetAbils() {
            WikiParser wikiParser = new WikiParser();
            wikiParser.ClearFailures();

            List<ScrapedAbility> scraped;
            try {
                scraped = ScrapeWiki(wikiParser);
            } catch (Exception ex) {
                MessageBox.Show("Could not read the wiki:\r\n" + ex.Message);
                return;
            }
            if (scraped.Count == 0) {
                MessageBox.Show("The wiki returned no abilities. Nothing was imported.");
                return;
            }

            abils = new List<Ability>();
            Parallel.ForEach(scraped, new ParallelOptions { MaxDegreeOfParallelism = 8 },
                entry => SetAbility(wikiParser, entry.Name, entry.Prefix, entry.Cooldown, entry.ImageUrl));

            var preImport = File.Exists(".\\mongoAbilities.json")
                ? JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"))
                : null;
            if (preImport != null) {
                for (int i = 0; i < preImport.Count(); i++) {
                    if (preImport[i].name.Contains("_Import")) {
                        preImport.RemoveAt(i);
                        i--;
                    }
                }

                preImport.AddRange(abils);
            } else {
                preImport = abils;
            }
            var stream = File.Create(".\\mongoAbilities.json");
            stream.Close();
            File.WriteAllText(".\\mongoAbilities.json", JsonConvert.SerializeObject(preImport, Formatting.Indented));

            LoadCombo();
            var abilsOrder = abils.OrderBy(i => i.name).ToList();
            dgSettings.Items.Clear();
            foreach (var ab in abilsOrder) {
                dgSettings.Items.Add(ab);
            }

            var failures = wikiParser.Failures;
            if (failures.Count > 0) {
                MessageBox.Show(abils.Count + " ABILITIES IMPORTED\r\n\r\n" + failures.Count
                    + " could not be downloaded and were skipped:\r\n"
                    + string.Join(", ", failures.Take(20))
                    + (failures.Count > 20 ? ", ..." : "")
                    + "\r\n\r\nRUN THE IMPORT AGAIN TO RETRY THEM");
            } else {
                MessageBox.Show(abils.Count + " ABILITIES IMPORTED");
            }
        }

        /// <summary>
        /// Reads every wiki page the tracker imports from. Tables are located by their headers rather
        /// than by their class attribute, because the wiki rewrites those regularly.
        /// </summary>
        private List<ScrapedAbility> ScrapeWiki(WikiParser wikiParser) {
            var scraped = new List<ScrapedAbility>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Abilities: one table per combat style, the style comes from the section it sits under.
            var doc = wikiParser.getPage("Abilities");
            if (doc != null) {
                foreach (var table in WikiTables.FindTables(doc, "Ability")) {
                    string prefix;
                    if (!AbilitySections.TryGetValue(WikiTables.SectionOf(table), out prefix))
                        continue;
                    bool firstRow = true;
                    foreach (var row in WikiTables.ParseRows(table, "Ability", "Cooldown")) {
                        // The first necromancy row is the auto attack the tracker tracks separately.
                        string name = prefix.Equals("Necromancy_") && firstRow ? row.Name + "_Auto" : row.Name;
                        firstRow = false;
                        Collect(scraped, seen, prefix, name, row.Cooldown, row.ImageUrl);
                    }
                }
            }
            if (scraped.Count == 0)
                throw new Exception("The ability tables could not be found on " + "https://runescape.wiki/w/Abilities");

            // Ancient curses: only the forms table, the curses themselves come with the prayers below.
            doc = wikiParser.getPage("Ancient_Curses");
            if (doc != null) {
                foreach (var table in WikiTables.FindTables(doc, "Prayer")) {
                    if (WikiTables.Column(WikiTables.GetColumns(table), "Effects") == null)
                        continue;
                    foreach (var row in WikiTables.ParseRows(table, "Prayer", null))
                        Collect(scraped, seen, "Curses_", row.Name, 0, row.ImageUrl);
                }
            }

            // Prayers, both the standard book and the curses.
            doc = wikiParser.getPage("Prayer");
            if (doc != null) {
                foreach (var table in WikiTables.FindTables(doc, "Prayer"))
                    foreach (var row in WikiTables.ParseRows(table, "Prayer", null))
                        Collect(scraped, seen, "Prayer_", row.Name, 0, row.ImageUrl);
            }

            // Spellbooks. None of these tables carry a cooldown column.
            foreach (string page in new[] { "Standard_spells", "Ancient_Magicks", "Lunar_spells" }) {
                doc = wikiParser.getPage(page);
                if (doc == null)
                    continue;
                foreach (var table in WikiTables.FindTables(doc, "Spell"))
                    foreach (var row in WikiTables.ParseRows(table, "Spell", "Cooldown"))
                        Collect(scraped, seen, "Spells_", row.Name, row.Cooldown, row.ImageUrl);
            }

            // Incantations matter for their cooldown, which only lives on each incantation's own page.
            doc = wikiParser.getPage("Incantations");
            if (doc != null) {
                foreach (var table in WikiTables.FindTables(doc, "Spell")) {
                    foreach (var row in WikiTables.ParseRows(table, "Spell", "Cooldown")) {
                        double cooldown = row.Cooldown > 0 ? row.Cooldown : wikiParser.getCooldownFromPage(row.Link);
                        Collect(scraped, seen, "Spells_", row.Name, cooldown, row.ImageUrl);
                    }
                }
            }

            return scraped;
        }

        private static void Collect(List<ScrapedAbility> scraped, HashSet<string> seen, string prefix, string name, double cooldown, string imageUrl) {
            if (string.IsNullOrWhiteSpace(name) || !seen.Add(prefix + name))
                return;
            scraped.Add(new ScrapedAbility { Prefix = prefix, Name = name, Cooldown = cooldown, ImageUrl = imageUrl });
        }

        private void SetAbility(WikiParser wikiParser, string name, string table = "", double cooldown = 0, string imgURL = "") {
            try {
                string fileName;
                if (string.IsNullOrEmpty(imgURL)) {
                    fileName = table.Equals("Spells_") ? wikiParser.SaveImage(name + "_icon") : wikiParser.SaveImage(name);
                } else {
                    fileName = wikiParser.SaveImageFROMURL(name, imgURL);
                }
                if (string.IsNullOrEmpty(fileName))
                    return;

                Ability ability = new Ability();
                ability.name = table + name + "_Import";
                ability.cooldown = cooldown;
                ability.img = ".\\Images\\" + fileName + ".png";
                lock (abils) {
                    abils.Add(ability);
                }
            } catch (Exception) {

            }
        }

        private async void Import_Click(object sender, RoutedEventArgs e) {
            var x = MessageBox.Show("This is going to replace all the abilities! are you sure you want to continue?", "", MessageBoxButton.YesNo);
            if (MessageBoxResult.Yes == x) {
                Mouse.OverrideCursor = Cursors.Wait;
                //CSVAbilParser();
                GetAbils();
            }
            Mouse.OverrideCursor = null;
        }
    }
}
