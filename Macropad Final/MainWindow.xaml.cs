using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;
using NAudio.CoreAudioApi;
using System.Management;

namespace Macropad_Final
{
    public partial class MainWindow : Window
    {
        // imports
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        // hotkey variables
        const int HOTKEY1 = 0x7C; // F13 key
        const int HOTKEY2 = 0x7D; // F14 key
        const int HOTKEY3 = 0x7E; // F15 key
        const int HOTKEY4 = 0x7F; // F16 key
        const int HOTKEY5 = 0x80; // F17 key
        const int HOTKEY6 = 0x81; // F18 key
        const int HOTKEY7 = 0x82; // F19 key
        const int HOTKEY8 = 0x83; // F20 key
        const int HOTKEY9 = 0x84; // F21 key
        const int HOTKEY10 = 0x85; // F22 key
        const int HOTKEY11 = 0x86; // F23 key
        const int HOTKEY12 = 0x87; // F24 key
        public int[] Hotkeys = { HOTKEY1, HOTKEY2, HOTKEY3, HOTKEY4, HOTKEY5, HOTKEY6, HOTKEY7, HOTKEY8, HOTKEY9, HOTKEY10, HOTKEY11, HOTKEY12 };

        //variables
        public string Shortcut;
        public string[] SelectedShortcuts = new string[12];
        public string[] Shortcuts = {"^(c)", "^(v)", "^(a)", "^(z)", "^(y)", "^(x)", "^(s)", "^(p)", "^(f)", "^(b)", "^(i)", "^(u)", "^(.)", "^(,)", "%{TAB}", "%{F4}"}; // + is shift
        bool HotKeyPressed = false;
        public int NumberPressed;

        // volume watcher
        private MMDeviceEnumerator mmDeviceEnumerator;
        private MMDevice audioDevice;

        // brightness watcher
        private ManagementEventWatcher brightnessWatcher;
        public int CurrentBrightness;

        public MainWindow()
        {
            InitializeComponent();
            StartVolumeWatcher();
            StartBrightnessWatcher();

            // Put all comboboxes in an array 
            ComboBox[] comboBoxes = {
                comboBox1, comboBox2, comboBox3, comboBox4, comboBox5, comboBox6,
                comboBox7, comboBox8, comboBox9, comboBox10, comboBox11, comboBox12
            };

            // Subscribe all comboboxes to the event handler
            for (int i = 0; i < comboBoxes.Length; i++)
            {
                comboBoxes[i].Tag = i; // Store index in Tag property
                comboBoxes[i].SelectionChanged += ComboBox_SelectionChanged;
                if (comboBoxes[i].SelectedIndex == -1)
                    comboBoxes[i].SelectedIndex = 0;
            }

            Thread HotkeyThread = new Thread(HotkeyClick);
            HotkeyThread.IsBackground = true;
            HotkeyThread.Start();
        }

        //detect hotkey presses and activate the corresponding shortcut using SendInput()
        private void HotkeyClick()
        {
            while (true)
            {
                for (int i = 0; i < SelectedShortcuts.Length; i++)
                {
                    if (GetAsyncKeyState(Hotkeys[i]) < 0) //detect hotkey press
                    {
                        HotKeyPressed = !HotKeyPressed;
                        Thread.Sleep(100);
                        NumberPressed = i;
                    }
                }

                if (HotKeyPressed) //detects if a hotkey was pressed and sends input
                {
                    Dispatcher.Invoke(() =>
                    {
                        SendInput();
                    });
                    HotKeyPressed = !HotKeyPressed;
                }
            }

            void SendInput() // Sends input of the shortcut corresponding to the pressed hotkey
            {
                Shortcut = SelectedShortcuts[NumberPressed]; //changes the shortcut to the one corresponding to the pressed hotkey
                if (!string.IsNullOrEmpty(Shortcut))
                {
                    WinForms.SendKeys.SendWait(Shortcut);
                }
            }
        }

        // Single event handler for all comboboxes
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            int index = (int)comboBox.Tag;
            SelectedShortcuts[index] = Shortcuts[comboBox.SelectedIndex];
        }

        // Detects the volume of the device everytime it changes
        private void StartVolumeWatcher()
        {
            mmDeviceEnumerator = new MMDeviceEnumerator();
            audioDevice = mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            VolumeTb.Text = $"{(int)Math.Round(audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100)}%";

            audioDevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
        }

        // Sets text in VolumeTb to the volume of the device
        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            Dispatcher.Invoke(() => VolumeTb.Text = $"{(int)Math.Round(data.MasterVolume * 100)}%");
        }

        // Detects the brightness of the device everytime it changes and changes BrightnessTb.text to that value
        private void StartBrightnessWatcher()
        {
            try
            {
                // get current brightness once
                using (var searcher = new ManagementObjectSearcher("root\\wmi", "SELECT CurrentBrightness FROM WmiMonitorBrightness"))
                {
                    foreach (ManagementObject i in searcher.Get())
                    {
                        var brightness = i["CurrentBrightness"];
                        if (brightness != null)
                        {
                            var currentBrightness = Math.Min(100, Math.Max(1, Convert.ToInt32(brightness)));
                            Dispatcher.Invoke(() => BrightnessTb.Text = $"{currentBrightness}%");
                        }
                        break;
                    }
                }

                // detects the brightness everytime it changes
                brightnessWatcher = new ManagementEventWatcher("root\\wmi", "SELECT * FROM WmiMonitorBrightnessEvent");
                brightnessWatcher.EventArrived += (s, e) =>
                {
                    var brightnessL = e.NewEvent["Brightness"];
                    if (brightnessL != null)
                    {
                        var currentBrightness = Math.Min(100, Math.Max(1, Convert.ToInt32(brightnessL)));
                        Dispatcher.Invoke(() => BrightnessTb.Text = $"{currentBrightness}%");
                    }
                };
                brightnessWatcher.Start();
            }
            catch
            {
                
            }
        }

    }
}
