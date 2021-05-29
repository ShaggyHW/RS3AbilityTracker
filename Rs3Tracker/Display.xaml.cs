using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using FMUtils.KeyboardHook;

using Newtonsoft.Json;

namespace Rs3Tracker {
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class Display : Window {
        Hook KeyboardHook = new Hook("Globalaction Link");
        List<KeybindClass> keybindClasses = new List<KeybindClass>();
        int imgCounter, allcounter = 0;
        string style = "";
        public List<Keypressed> ListKeypressed = new List<Keypressed>();
        public Stopwatch stopwatch = new Stopwatch();
        public bool control = false;
        private Keypressed previousKey = new Keypressed();



        public Display(string _style) {
            InitializeComponent();
            KeyboardHook.KeyDownEvent += HookKeyDown;
            //  KeyboardHook.KeyUpEvent += KeyUp;
            List<KeybindClass> keybindClasses2 = JsonConvert.DeserializeObject<List<KeybindClass>>(File.ReadAllText(".\\keybinds.json"));
            style = _style;
            keybindClasses = (from r in keybindClasses2
                              where r.ability.cmbtStyle.ToLower() == style.ToLower()
                              select r).ToList();
            stopwatch.Start();
            previousKey.ability = new Ability();
        }

        private void HookKeyDown(KeyboardHookEventArgs e) {
            if (!control) {
                control = true;
                Keypressed keypressed = new Keypressed();
                keypressed.ability = new Ability();
                string path = "";
                BitmapImage bitmap = new BitmapImage();
                string modifier = "";

                if (e.Key.ToString().ToLower().Equals("none")) {
                    control = false;
                    return;
                }

                if (e.isAltPressed)
                    modifier = "ALT";
                else if (e.isCtrlPressed)
                    modifier = "CTRL";
                else if (e.isShiftPressed)
                    modifier = "SHIFT";
                else if (e.isWinPressed)
                    modifier = "WIN";

                List<Ability> img = (from r in keybindClasses
                                     where r.key.ToLower() == e.Key.ToString().ToLower()
                                     where r.modifier.ToString().ToLower() == modifier.ToLower()
                                     select r.ability).ToList();

                if (img.Count == 0) {
                    control = false;
                    return;
                }

                keypressed.modifier = modifier;
                keypressed.key = e.Key.ToString();
                keypressed.ability.img = img[0].img;
                keypressed.ability.cmbtStyle = style;
                keypressed.timepressed = stopwatch.Elapsed.TotalMilliseconds;

                if (!string.IsNullOrEmpty(previousKey.ability.img))
                    if (((keypressed.timepressed - previousKey.timepressed) < 1200) && previousKey.ability.img.Equals(keypressed.ability.img)) {
                        control = false;
                        return;
                    }

                switch (imgCounter) {
                    case 0:
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 1:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 2:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 3:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 4:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 5:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 6:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 7:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 8:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    case 9:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                    default:
                        moveImgs(imgCounter);
                        path = AppDomain.CurrentDomain.BaseDirectory;
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img[0].img);
                        bitmap.EndInit();
                        displayImg10.Source = bitmap;
                        break;
                }

                if (imgCounter < 9)
                    imgCounter++;

                allcounter++;

                ListKeypressed.Add(keypressed);

                previousKey = new Keypressed() {
                    timepressed = keypressed.timepressed,
                    ability = new Ability {
                        img= keypressed.ability.img
                    }                 
                };

                control = false;
            }
        }

        private void moveImgs(int counter) {
            switch (counter) {
                case 1:
                    displayImg9.Source = displayImg10.Source;
                    break;
                case 2:
                    displayImg8.Source = displayImg9.Source;
                    displayImg9.Source = displayImg10.Source;
                    break;
                case 3:
                    displayImg7.Source = displayImg8.Source;
                    displayImg8.Source = displayImg9.Source;
                    displayImg9.Source = displayImg10.Source;
                    break;
                case 4:
                    displayImg6.Source = displayImg7.Source;
                    displayImg7.Source = displayImg8.Source;
                    displayImg8.Source = displayImg9.Source;
                    displayImg9.Source = displayImg10.Source;
                    break;
                case 5:
                    displayImg5.Source = displayImg6.Source;
                    displayImg6.Source = displayImg7.Source;
                    displayImg7.Source = displayImg8.Source;
                    displayImg8.Source = displayImg9.Source;
                    displayImg9.Source = displayImg10.Source;
                    break;
                case 6:
                    displayImg4.Source = displayImg5.Source;
                    displayImg5.Source = displayImg6.Source;
                    displayImg6.Source = displayImg7.Source;
                    displayImg7.Source = displayImg8.Source;
                    displayImg8.Source = displayImg9.Source;
                    displayImg9.Source = displayImg10.Source;
                    break;
                case 7:
                    displayImg3.Source = displayImg4.Source;
                    displayImg4.Source = displayImg5.Source;
                    displayImg5.Source = displayImg6.Source;
                    displayImg6.Source = displayImg7.Source;
                    displayImg7.Source = displayImg8.Source;
                    displayImg8.Source = displayImg9.Source;
                    displayImg9.Source = displayImg10.Source;
                    break;
                case 8:
                    displayImg2.Source = displayImg3.Source;
                    displayImg3.Source = displayImg4.Source;
                    displayImg4.Source = displayImg5.Source;
                    displayImg5.Source = displayImg6.Source;
                    displayImg6.Source = displayImg7.Source;
                    displayImg7.Source = displayImg8.Source;
                    displayImg8.Source = displayImg9.Source;
                    displayImg9.Source = displayImg10.Source;
                    break;
                case 9:
                    displayImg1.Source = displayImg2.Source;
                    displayImg2.Source = displayImg3.Source;
                    displayImg3.Source = displayImg4.Source;
                    displayImg4.Source = displayImg5.Source;
                    displayImg5.Source = displayImg6.Source;
                    displayImg6.Source = displayImg7.Source;
                    displayImg7.Source = displayImg8.Source;
                    displayImg8.Source = displayImg9.Source;
                    displayImg9.Source = displayImg10.Source;
                    break;
            }
        }

    }
}
