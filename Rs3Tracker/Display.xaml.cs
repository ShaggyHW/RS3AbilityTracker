using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Drawing.Imaging; //Advanced Image Functionalities
using System.IO; //File Operations
using FMUtils.KeyboardHook;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Rs3Tracker {
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class Display : Window {
        Hook KeyboardHook = new Hook("Globalaction Link");
        List<KeybindClass> keybindClasses = new List<KeybindClass>();
        List<BarKeybindClass> keybindBarClasses = new List<BarKeybindClass>();
        int imgCounter = 0;
        public string style = "";
        public List<Keypressed> ListKeypressed = new List<Keypressed>();
        public List<Keypressed> ListPreviousKeypressed = new List<Keypressed>();
        public Stopwatch stopwatch = new Stopwatch();
        public bool control = false;
        private Keypressed previousKey = new Keypressed();
        private List<Keypressed> ListPreviousKeys = new List<Keypressed>();
        private bool trackCD;
        private bool pause = false;
        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            // Make entire window and everything in it "transparent" to the Mouse
            var windowHwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(windowHwnd);

            // Make the button "visible" to the Mouse
            //var buttonHwndSource = (HwndSource)HwndSource.FromVisual(btn);
            //var buttonHwnd = buttonHwndSource.Handle;
            //WindowsServices.SetWindowExNotTransparent(buttonHwnd);
        }
        public static class WindowsServices {
            const int WS_EX_TRANSPARENT = 0x00000020;
            const int GWL_EXSTYLE = (-20);

            [DllImport("user32.dll")]
            static extern int GetWindowLong(IntPtr hwnd, int index);

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

            public static void SetWindowExTransparent(IntPtr hwnd) {
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            }

            public static void SetWindowExNotTransparent(IntPtr hwnd) {
                var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
            }
        }
        bool resize = false;
        bool serverConn = false;
        public Display(string _style, bool trackCD, bool onTop, bool resize, bool serverConnection) {
            this.resize = resize;
            this.serverConn = serverConnection;
            //this.Left = LeftPos;
            //this.Top = TopPos;
            InitializeComponent();
            if (resize) {
                AllowsTransparency = false;
                ResizeON();
            } else {
                AllowsTransparency = true;
                ResizeOFF();
            }
            //this.LeftPos = LeftPos;
            //this.TopPos = TopPos;
            //this.height = height;
            //this.width = width;
            Loaded += Display_Loaded;
            var windowHwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExNotTransparent(windowHwnd);
            KeyboardHook.KeyDownEvent += HookKeyDown;
            style = _style;
            TESTLABEL.Content = style;
            keybindClasses = JsonConvert.DeserializeObject<List<KeybindClass>>(File.ReadAllText(".\\keybinds.json"));
            keybindBarClasses = JsonConvert.DeserializeObject<List<BarKeybindClass>>(File.ReadAllText(".\\barkeybinds.json"));
            changeStyle();
            stopwatch.Start();
            previousKey.ability = new Ability();
            this.trackCD = trackCD;
            this.Topmost = onTop;
        }

        private void Display_Loaded(object sender, RoutedEventArgs e) {
            if (resize) {

                ResizeON();
            } else {

                ResizeOFF();
            }
            //this.Height = height;
            //this.Width = width;
        }

        //        private void 

        public void ResizeON() {
            this.ResizeMode = ResizeMode.CanResize;
            var windowHwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExNotTransparent(windowHwnd);
        }

        public void ResizeOFF() {
            var windowHwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(windowHwnd);
            this.ResizeMode = ResizeMode.NoResize;
        }

        public void changeStyle() {
            keybindClasses = JsonConvert.DeserializeObject<List<KeybindClass>>(File.ReadAllText(".\\keybinds.json"));
            keybindClasses = keybindClasses.Where(p => p.bar.name.ToLower() == style.ToLower() || p.bar.name.ToLower() == "all").Select(p => p).ToList();
        }

        #region imageProcessing
        private Bitmap Tint(Bitmap bmpSource, System.Drawing.Color clrScaleColor, float sngScaleDepth) {

            Bitmap bmpTemp = new Bitmap(bmpSource.Width, bmpSource.Height); //Create Temporary Bitmap To Work With

            ImageAttributes iaImageProps = new ImageAttributes(); //Contains information about how bitmap and metafile colors are manipulated during rendering. 

            ColorMatrix cmNewColors = default(ColorMatrix); //Defines a 5 x 5 matrix that contains the coordinates for the RGBAW space
            cmNewColors = new ColorMatrix(new float[][] {
                new float[] {
                    1,
                    0,
                    0,
                    0,
                    0
                },
                new float[] {
                    0,
                    1,
                    0,
                    0,
                    0
                },
                new float[] {
                    0,
                    0,
                    1,
                    0,
                    0
                },
                new float[] {
                    0,
                    0,
                    0,
                    1,
                    0
                },
                new float[] {
                    clrScaleColor.R / 255 * sngScaleDepth,
                    clrScaleColor.G / 255 * sngScaleDepth,
                    clrScaleColor.B / 255 * sngScaleDepth,
                    0,
                    1
                }
            });

            iaImageProps.SetColorMatrix(cmNewColors); //Apply Matrix
            Graphics grpGraphics = Graphics.FromImage(bmpTemp); //Create Graphics Object and Draw Bitmap Onto Graphics Object
            grpGraphics.DrawImage(bmpSource, new System.Drawing.Rectangle(0, 0, bmpSource.Width, bmpSource.Height), 0, 0, bmpSource.Width, bmpSource.Height, GraphicsUnit.Pixel, iaImageProps);
            return bmpTemp;
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
        #endregion

        private void HookKeyDown(KeyboardHookEventArgs e) {
            NLog.Logger logger = LogManager.GetLogger("");
            try {
                #region display
                if (!control) {
                    control = true;
                    Keypressed keypressed = new Keypressed();
                    keypressed.ability = new Ability();
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


                    if (serverConn) {
                        //DualPC_Connection dualPC_Connection = new DualPC_Connection();
                        //Task.Factory.StartNew(() => DualPC_Connection.POST(e.Key.ToString().ToLower(), modifier));
                    }


                    List<Ability> abilityList = (from r in keybindClasses
                                                 where r.key.ToLower() == e.Key.ToString().ToLower()
                                                 where r.modifier.ToString().ToLower() == modifier.ToLower()
                                                 select r.ability).ToList();

                    if (abilityList.Count == 0) {
                        if (keybindBarClasses != null) {
                            var listBarChange2 = keybindBarClasses.Where(p => p.key.ToLower().Equals(e.Key.ToString().ToLower()) && p.modifier.ToLower().Equals(modifier.ToLower())).Select(p => p).FirstOrDefault();
                            if (listBarChange2 != null) {
                                if (listBarChange2.name.ToLower().Equals("clear")) {
                                    displayImg1.Source = null;
                                    displayImg2.Source = null;
                                    displayImg3.Source = null;
                                    displayImg4.Source = null;
                                    displayImg5.Source = null;
                                    displayImg6.Source = null;
                                    displayImg7.Source = null;
                                    displayImg8.Source = null;
                                    displayImg9.Source = null;
                                    displayImg10.Source = null;
                                    control = false;
                                    return;
                                } else if (listBarChange2.name.ToLower().Equals("pause")) {
                                    pause = !pause;
                                }
                            }
                        }
                    }

                    if (pause) {
                        control = false;
                        return;
                    }

                    foreach (var ability in abilityList) {

                        if (ability == null)
                            continue;

                        keypressed.modifier = modifier;
                        keypressed.key = e.Key.ToString();
                        keypressed.ability.name = ability.name;
                        keypressed.ability.img = ability.img;
                        keypressed.ability.cooldown = ability.cooldown;
                        keypressed.timepressed = stopwatch.Elapsed.TotalMilliseconds;

                        for (int i = 0; i < ListPreviousKeypressed.Count; i++) {
                            var prevabil = ListPreviousKeypressed[i];
                            if ((keypressed.timepressed - prevabil.timepressed) > 1200) {
                                ListPreviousKeypressed.RemoveAt(i);
                                i--;
                            }
                        }

                        previousKey = ListPreviousKeypressed.Where(a => a.ability.img.Equals(keypressed.ability.img)).Select(a => a).FirstOrDefault();
                        if (previousKey != null) {
                            control = false;
                            return;
                        }
                        ListKeypressed.Add(keypressed);
                        previousKey = new Keypressed() {
                            timepressed = keypressed.timepressed,
                            ability = new Ability {
                                img = keypressed.ability.img,
                                name = keypressed.ability.name
                            }
                        };
                        ListPreviousKeypressed.Add(previousKey);


                       Bitmap bitmap = new Bitmap(ability.img);
                        Bitmap Image;
                        ImageSource imageSource;
                        if (trackCD) {
                            bool onCD = abilCoolDown(ListPreviousKeys, keypressed);
                            if (onCD) {
                                Image = Tint(bitmap, System.Drawing.Color.Red, 0.5f);
                                imageSource = ImageSourceFromBitmap(Image);
                            } else {
                                imageSource = ImageSourceFromBitmap(bitmap);
                                ListPreviousKeys.Add(previousKey);
                            }
                        } else {
                            imageSource = ImageSourceFromBitmap(bitmap);
                            ListPreviousKeys.Add(previousKey);
                        }

                        //Display
                        switch (imgCounter) {
                            case 0:
                                displayImg10.Source = imageSource;
                                break;
                            case 1:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            case 2:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            case 3:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            case 4:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            case 5:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            case 6:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            case 7:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            case 8:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            case 9:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                            default:
                                moveImgs(imgCounter);
                                displayImg10.Source = imageSource;
                                break;
                        }
                        if (imgCounter < 9)
                            imgCounter++;
                    }
                    if (keybindBarClasses != null) {
                        var listBarChange = keybindBarClasses.Where(p => p.key.ToLower().Equals(e.Key.ToString().ToLower()) && p.modifier.ToLower().Equals(modifier.ToLower()) && (p.bar.name.ToLower().Equals(style.ToLower()) || p.bar.name.Equals("ALL"))).Select(p => p).FirstOrDefault();
                        if (listBarChange != null) {
                            if (!listBarChange.name.ToLower().Equals("pause") && !listBarChange.name.ToLower().Equals("clear")) {
                                style = listBarChange.name;
                                TESTLABEL.Content = style;
                                changeStyle();
                            }
                        }
                    }
                    control = false;

                }
                #endregion
            } catch (Exception ex) { control = false; 
                logger.Error(ex, ex.ToString()); }
        }

        private bool abilCoolDown(List<Keypressed> prevKeys, Keypressed keypressed) {
            var prevKey = prevKeys.Where(pk => pk.ability.name == keypressed.ability.name).Select(pk => pk).FirstOrDefault();
            if (prevKey != null) {
                double abilCD = keypressed.ability.cooldown * 1000;
                if ((keypressed.timepressed - prevKey.timepressed) > abilCD) {
                    prevKeys.Remove(prevKey);
                    return false;
                }

                return true;
            } else {
                return false;
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

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                this.DragMove();
                //MainWindow.displayX = this.Top;
                //MainWindow.displayY = this.Left;
                //MainWindow.Height = this.Height;
                //MainWindow.Width = this.Width;
            }
        }
    }
}
