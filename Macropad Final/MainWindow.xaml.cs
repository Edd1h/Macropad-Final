using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

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
        public string[] Shortcuts = { "+{A}", "+{B}", "+{C}", "+{D}", "+{E}", "+{F}", "+{G}", "+{H}" }; // + is shift
        bool HotKeyPressed = false;
        public int NumberPressed;

        public MainWindow()
        {
            InitializeComponent();

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
                for (int i = 0; i < 12; i++)
                {
                    if (GetAsyncKeyState(Hotkeys[i]) < 0) //detect hotkey press
                    {
                        HotKeyPressed = !HotKeyPressed;
                        Thread.Sleep(50);
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
    }
}
