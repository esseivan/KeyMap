using EsseivaN.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyMap
{
    public partial class FrmMain : Form
    {
        private const string APP_NAME = "KeyMapLogger";
        readonly SettingsManager<Dictionary<Keys, int>> settingsManager = new EsseivaN.Tools.SettingsManager<Dictionary<Keys, int>>();
        Dictionary<Keys, int> keyCount = null;

        private const string fileName = "keys.txt";
        private string exePath = string.Empty;
        private string filePath = string.Empty;

        private Timer autosave_tmr = new Timer();

        public FrmMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            exePath = Application.ExecutablePath;
            filePath = Path.Combine(Path.GetDirectoryName(exePath), fileName);
            LoadFromFile(filePath);
            KeyboardHook.KeyPressed += KeyboardHook_KeyPressed;
            notifyIcon1.MouseDoubleClick += NotifyIcon1_MouseDoubleClick;
            btnAutorun.Checked = Tools.GetStartup(APP_NAME);
            btnAutorun.CheckedChanged += BtnAutorun_CheckedChanged;
            // 5min timer
            autosave_tmr.Interval = 5 * 60 * 1000;
            autosave_tmr.Tick += Autosave_tmr_Tick;
            autosave_tmr.Enabled = true;
        }

        private void Autosave_tmr_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Autosave");
            SaveToFile(filePath);
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            // Get arguments
            string[] args = Environment.GetCommandLineArgs();

            foreach (string arg_ in args)
            {
                string arg = arg_.ToLower();
                switch (arg)
                {
                    case "-hide":
                    case "--hide":
                    case "/hide":
                        SetVisible(false);
                        break;
                    default:
                        break;
                }
            }
        }

        private void BtnAutorun_CheckedChanged(object sender, EventArgs e)
        {
            Tools.SetStartup(APP_NAME, btnAutorun.Checked, "/hide");
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SetVisible(true);
        }

        private void SetVisible(bool state)
        {
            this.Visible = state;
            this.WindowState = state ? FormWindowState.Normal : FormWindowState.Minimized;
            notifyIcon1.Visible = !state;
        }

        private void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (keyCount.ContainsKey(e.KeyCode))
                keyCount[e.KeyCode]++;
            else
                keyCount[e.KeyCode] = 1;

            lblLog.Text = e.KeyCode + " - " + keyCount[e.KeyCode];
            Console.WriteLine(lblLog.Text);
        }

        private void SaveToFile(string filename)
        {
            // Sort keys
            SortDictionnary();

            // Reset autosave timer
            autosave_tmr.Stop();
            autosave_tmr.Start();

            settingsManager.Clear();
            settingsManager.AddSetting(keyCount);
            settingsManager.Save(filename);
        }

        public void SortDictionnary()
        {
            var keyList = keyCount.ToList();
            keyList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            keyCount = keyList.ToDictionary(x => x.Key, x => x.Value);
        }

        private void LoadFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    keyCount = settingsManager.Load(filename).Values.FirstOrDefault();
                }
                catch (FileLoadException)
                {
                    Dialog.DialogConfig dc = new Dialog.DialogConfig()
                    {
                        Button1 = Dialog.ButtonType.OK,
                        Button2 = Dialog.ButtonType.Retry,
                        Input = false,
                        Icon = Dialog.DialogIcon.Error,
                        Message = "Unable to load the " + filename + " file. Exiting...",
                        Title = "Error",
                    };
                    Dialog.ShowDialogResult result = Dialog.ShowDialog(dc);
                    if (result.DialogResult == Dialog.DialogResult.Retry)
                    {
                        LoadFromFile(filename);
                    }
                    else
                    {
                        Application.Exit();
                        Application.DoEvents();
                    }
                }
            }
            else
            {
                File.Create(filePath).Dispose();
            }

            if (keyCount == null)
            {
                keyCount = new Dictionary<Keys, int>();
                SaveToFile(fileName);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save dictionary to file
            SaveToFile(filePath);
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            Process.Start(filePath);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            settingsManager.Clear();
            settingsManager.AddSetting(new Dictionary<Keys, int>());
            settingsManager.Save(filePath);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            SaveToFile(filePath);
            //}
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            LoadFromFile(filePath);
            //}
        }

        private void BtnDismiss_Click(object sender, EventArgs e)
        {
            SetVisible(false);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                SetVisible(false);
            }
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToFile(filePath);
            this.Close();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToFile(filePath);
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetVisible(true);
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            SortDictionnary();
            FrmPreview preview = new FrmPreview(keyCount);
            preview.ShowDialog();
        }
    }
}
