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


        public void GetAbils() {
            WikiParser wikiParser = new WikiParser();
            string Code = wikiParser.getHTMLCode("Abilities");
            var doc = new HtmlDocument();
            doc.LoadHtml(Code);
            var tables = doc.DocumentNode.SelectNodes("//table[@class='wikitable sortable sticky-header']");
            abils = new List<Ability>();
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < tables.Count(); k++) {
        
                var table = tables[k];
                string type = "";
                switch (k) {
                    case 0:
                        type = "Melee_";
                        break;
                    case 1:
                        type = "Melee_";
                        break;
                    case 2:
                        type = "Range_";
                        break;
                    case 3:
                        type = "Mage_";
                        break;
                    case 4:
                        type = "Necromancy_";

                        break;
                    case 5:
                        type = "Defense_";

                        break;
                    case 6:
                        type = "Constitution_";
                        break;
                }
                for (int i = 1; i < table.ChildNodes.Count(); i++) {
                    for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2) {
                        string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();
                      
                        string imgURL = "";
                        try {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("srcset");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                            index = htmlrest.IndexOf("?");
                             imgURL = htmlrest.Substring(0, index);


                        }catch(Exception ex) {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("src");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                            index = htmlrest.IndexOf("?");
                             imgURL = htmlrest.Substring(0, index);
                        }
                        if(!imgURL.Contains("images") || !imgURL.Contains(".png")) {
                            imgURL = "";
                        }
                        if (type.Equals("Necromancy_") && j.Equals(2)) {
                            name = name + "_Auto";
                        }
                        string coolDown = "";
                        if (type.Equals("Necromancy_")) {
                            coolDown = table.ChildNodes[i].ChildNodes[j].ChildNodes[17].InnerText.Replace("\n", "").Trim();
                        } else {
                            coolDown = table.ChildNodes[i].ChildNodes[j].ChildNodes[15].InnerText.Replace("\n", "").Trim();
                        }
                        double CD = 0;
                        try {
                            CD = Convert.ToDouble(coolDown);
                        } catch { }
                        tasks.Add(Task.Factory.StartNew(() => SetAbility(wikiParser, name, type, CD, imgURL)));
                    }
                }
            }

            Code = wikiParser.getHTMLCode("Ancient_Curses");
            doc = new HtmlDocument();
            doc.LoadHtml(Code);
            tables = doc.DocumentNode.SelectNodes("//table[@class='wikitable sticky-header align-left-2 align-left-4']");
            foreach (var table in tables) {
                for (int i = 1; i < table.ChildNodes.Count(); i++) {
                    for (int j = 4; j < table.ChildNodes[i].ChildNodes.Count(); j += 2) {
                        string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerText.Replace("\n", "").Trim();
                        string imgURL = "";
                        try {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("srcset");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);


                        } catch (Exception ex) {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("src");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);
                        }
                 
                        if (!imgURL.Contains("images") || !imgURL.Contains(".png")) {
                            imgURL = "";
                        }
                        tasks.Add(Task.Factory.StartNew(() => SetAbility(wikiParser, name, "Curses_",0,imgURL)));
                        //abils.Add(ability);
                    }
                }
            }

            Code = wikiParser.getHTMLCode("Prayer");
            doc = new HtmlDocument();
            doc.LoadHtml(Code);
            tables = doc.DocumentNode.SelectNodes("//table[@class='wikitable align-right-3 align-right-4 align-right-5']");
            foreach (var table in tables) {
                for (int i = 1; i < table.ChildNodes.Count(); i++) {
                    for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2) {
                        //Ability ability = new Ability();
                        string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerText.Replace("\n", "").Trim();
                        if (string.IsNullOrEmpty(name)) {
                            continue;
                        }
                        string imgURL = "";
                        try {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("srcset");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);


                        } catch (Exception ex) {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("src");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);
                        }
                        if (!imgURL.Contains("images") || !imgURL.Contains(".png")) {
                            imgURL = "";
                        }
                        tasks.Add(Task.Factory.StartNew(() => SetAbility(wikiParser, name, "Prayer_",0,imgURL)));
                    }
                }
            }

            Code = wikiParser.getHTMLCode("Standard_spells");
            doc = new HtmlDocument();
            doc.LoadHtml(Code);
            tables = doc.DocumentNode.SelectNodes("//table[@class='wikitable align-right-1 align-center-2 align-left-3 align-center-4 align-left-5 align-left-6 align-left-7 align-left-8 align-left-9 align-center-10']");
            foreach (var table in tables) {
                for (int i = 1; i < table.ChildNodes.Count(); i++) {
                    for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2) {
                        //Ability ability = new Ability();
                        string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();
                        string imgURL = "";
                        try {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("srcset");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);


                        } catch (Exception ex) {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("src");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);
                        }
                        if (!imgURL.Contains("images") || !imgURL.Contains(".png")) {
                            imgURL = "";
                        }
                        tasks.Add(Task.Factory.StartNew(() => SetAbility(wikiParser, name, "Spells_", 0, imgURL)));
                    }
                }
            }


            Code = wikiParser.getHTMLCode("Ancient_Magicks");
            doc = new HtmlDocument();
            doc.LoadHtml(Code);
            tables = doc.DocumentNode.SelectNodes("//table[@class='wikitable align-right-1 align-center-2 align-left-3 align-center-4 align-left-5 align-left-6 align-left-7 align-left-8 align-left-9 align-center-10']");
            foreach (var table in tables) {
                for (int i = 1; i < table.ChildNodes.Count(); i++) {
                    for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2) {
                        //Ability ability = new Ability();
                        string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();
                        string imgURL = "";
                        try {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("srcset");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);


                        } catch (Exception ex) {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("src");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);
                        }
                        if (!imgURL.Contains("images") || !imgURL.Contains(".png")) {
                            imgURL = "";
                        }
                        tasks.Add(Task.Factory.StartNew(() => SetAbility(wikiParser, name, "Spells_", 0, imgURL)));
                    }
                }
            }

            Code = wikiParser.getHTMLCode("Lunar_spells");
            doc = new HtmlDocument();
            doc.LoadHtml(Code);
            tables = doc.DocumentNode.SelectNodes("//table[@class='wikitable align-right-1 align-center-2 align-left-3 align-center-4 align-left-5 align-left-6 align-left-7 align-left-8 align-left-9 align-center-10']");
            foreach (var table in tables) {
                for (int i = 1; i < table.ChildNodes.Count(); i++) {
                    for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2) {
                        //Ability ability = new Ability();
                        string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();
                 
                        string imgURL = "";
                        try {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("srcset");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);


                        } catch (Exception ex) {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("src");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);
                        }
                        if (!imgURL.Contains("images") || !imgURL.Contains(".png")) {
                            imgURL = "";
                        }
                        tasks.Add(Task.Factory.StartNew(() => SetAbility(wikiParser, name, "Spells_", 0, imgURL)));
                    }
                }
            }

            Code = wikiParser.getHTMLCode("Incantations");
            doc = new HtmlDocument();
            doc.LoadHtml(Code);
            tables = doc.DocumentNode.SelectNodes("//table[@class='wikitable align-right-1 align-center-2 align-left-3 align-center-4 align-left-5 align-left-6 align-center-7']");
            foreach (var table in tables) {
                for (int i = 1; i < table.ChildNodes.Count(); i++) {
                    for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2) {
                        //Ability ability = new Ability();
                        string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Replace("&#160;","").Trim();
                        string coolDown = "";

                        coolDown = table.ChildNodes[i].ChildNodes[j].ChildNodes[9].InnerText.Replace("\n", "").Replace("seconds", "").Trim();
                        bool itsMinutes = false;
                        if (coolDown.Contains("minute")) {
                            itsMinutes = true;
                            coolDown = coolDown.Replace("minute", "");
                        }
                        double CD = 0;
                        try {
                            CD = Convert.ToDouble(coolDown);
                            if (itsMinutes)
                                CD = CD * 60;
                        } catch { }
                        string imgURL = "";
                        try {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("srcset");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);


                        } catch (Exception ex) {
                            string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                            var index = imgHTML.IndexOf("src");

                            string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                            index = htmlrest.IndexOf("?");
                            imgURL = htmlrest.Substring(0, index);
                        }
                        if (!imgURL.Contains("images") || !imgURL.Contains(".png")) {
                            imgURL = "";
                        }


                        tasks.Add(Task.Factory.StartNew(() => SetAbility(wikiParser, name, "Spells_", CD,imgURL)));
                    }
                }
            }

            Task.WaitAll(tasks.ToArray());
            var preImport = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(".\\mongoAbilities.json"));
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

            MessageBox.Show("ABILITIES IMPORTED");
        }

        private void SetAbility(WikiParser wikiParser, string name, string table = "", double cooldown = 0, string imgURL = "") {
            try {
                if (imgURL.Contains("Affliction")) { 
                    
                }
                Ability ability = new Ability();
                string fileName = "";
                if (string.IsNullOrEmpty(imgURL)) {
                    if (table.Equals("Spells_"))
                        fileName = wikiParser.SaveImage(name + "_icon");
                    else
                        fileName = wikiParser.SaveImage(name);
                } else {
                    fileName = wikiParser.SaveImageFROMURL(name, imgURL);
                }
                if (string.IsNullOrEmpty(fileName))
                    return;
                //string img = table.ChildNodes[i].ChildNodes[2].ChildNodes[3].InnerText.Replace("\n", "");                            
                ability.name = table + name + "_Import";
                ability.cooldown = cooldown;
                ability.img = ".\\Images\\" + fileName + ".png";
                abils.Add(ability);
            }catch(Exception ex) {

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
