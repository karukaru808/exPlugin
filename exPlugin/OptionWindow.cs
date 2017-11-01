using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Yukarinette;

namespace exPlugin
{
    public partial class OptionWindow
    {
        private OptionWindow()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;

            // 現在の設定を設定欄に反映
            //getOutputDevices();
            getWasapiOutputDevices();
            OutputSelected.SelectedIndex = ConfigData.oIndex;
            VOICELOIDSelected.SelectedIndex = ConfigData.vIndex;
            CSVPathTextBox.Text = ConfigData.csvPath;

            //設定からデバイスのデータを渡す
            var exm = new exManager(); 
            exm.OutputDevice = (MMDevice)OutputSelected.SelectedValue;
        }

        // 設定画面表示時実行
        public static void Show(ConfigManager manager)
        {
            var optionWindow = new OptionWindow();

            if (optionWindow.ShowDialog().Value)
            {
                optionWindow.Save(manager);
            }
        }

        // 設定画面閉止時実行
        private void Save(ConfigManager manager)
        {
            // 設定欄の内容を設定に保存
            ConfigData.version = FileVersionInfo.GetVersionInfo((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).FileVersion;
            ConfigData.oIndex = OutputSelected.SelectedIndex;
            ConfigData.vIndex = VOICELOIDSelected.SelectedIndex;
            ConfigData.csvPath = CSVPathTextBox.Text;
            manager.Save();
        }

        //OKボタンクリック時動作
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            //選択したデバイスを渡す
            var exm = new exManager();
            exm.OutputDevice = (MMDevice)OutputSelected.SelectedValue;
            
            DialogResult = new bool?(true);
        }

        //参照ボタンクリック時動作
        private void CSVPathButton_Click(object sender, RoutedEventArgs e)
        {
            string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (0 < this.CSVPathTextBox.Text.Length && File.Exists(this.CSVPathTextBox.Text))
            {
                initialDirectory = Path.GetDirectoryName(this.CSVPathTextBox.Text);
            }
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                FileName = "",
                InitialDirectory = initialDirectory,
                Filter = "CSV (*.csv)|*.csv",
                Title = "CSVファイル を指定してください。"
            };
            if (openFileDialog.ShowDialog().Value)
            {
                CSVPathTextBox.Text = openFileDialog.FileName;
            }
        }

        //音声出力先取得関数
        //WaveOut版
        private void getWaveOutputDevices()
        {
            for (int id = 0; id < WaveOut.DeviceCount; id++)
            {
                WaveOutCapabilities capabilities = WaveOut.GetCapabilities(id);
                OutputSelected.Items.Add(String.Format("{0}:{1}", id, capabilities.ProductName));
                //Yukarinette.YukarinetteConsoleMessage.Instance.WriteMessage(capabilities.ProductName);
            }
        }

        //WASAPI版
        //コンボボックス用クラス
        class WasapiDeviceComboItem
        {
            public string Description { get; set; }
            public MMDevice Device { get; set; }
        }

        // コンボボックス用の初期化関数
        private void getWasapiOutputDevices()
        {
            //var enumerator = new MMDeviceEnumerator();
            var endPoints = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            var comboItems = new List<WasapiDeviceComboItem>();
            foreach (var endPoint in endPoints)
            {
                var comboItem = new WasapiDeviceComboItem();
                
                //表示セット
                comboItem.Description = string.Format("{0}", endPoint.FriendlyName);

                //データセット
                comboItem.Device = endPoint;
                
                //リストに追加
                comboItems.Add(comboItem);
            }

            //Descriptionをコンボボックスに表示させる設定
            OutputSelected.DisplayMemberPath = "Description";
            
            //選択したDeviceのデータを渡す設定
            OutputSelected.SelectedValuePath = "Device";
            
            //上記データのバインディング
            OutputSelected.ItemsSource = comboItems;
            
            //上記コンボボックスへのデータバインディングについてはややこしいので下記参照
            //http://heppoen.seesaa.net/article/430970064.html
            //http://blog.hiros-dot.net/?p=5759
            //https://code.msdn.microsoft.com/XAMLVBC-ComboBox-1e1f8339
        }

    }
}