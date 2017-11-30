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
        exManager exmanager;

        private OptionWindow()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;

            // 現在の設定を設定欄に反映
            exmanager = new exManager();
            exmanager.getWasapiOutputDevices();
            setWasapiOutputDevices();
            OutputSelected.SelectedIndex = ConfigData.oIndex;
            VOICELOIDSelected.SelectedIndex = ConfigData.vIndex;
            CSVPathTextBox.Text = ConfigData.csvPath;
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
            exmanager.OutputDevice = (MMDevice)OutputSelected.SelectedValue;
            
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
        //WASAPI版
        // コンボボックスのset関数
        private void setWasapiOutputDevices()
        {
            //Descriptionをコンボボックスに表示させる設定
            OutputSelected.DisplayMemberPath = "Description";
            
            //選択したDeviceのデータを渡す設定
            OutputSelected.SelectedValuePath = "Device";

            //上記データのバインディング
            OutputSelected.ItemsSource = exmanager.ComboItems;
            
            //上記コンボボックスへのデータバインディングについてはややこしいので下記参照
            //http://heppoen.seesaa.net/article/430970064.html
            //http://blog.hiros-dot.net/?p=5759
            //https://code.msdn.microsoft.com/XAMLVBC-ComboBox-1e1f8339
        }

    }
}