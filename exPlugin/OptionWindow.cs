using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace exPlugin
{
    public partial class OptionWindow
    {
        private OptionWindow()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;

            // 現在の設定を設定欄に反映
            CSVPathTextBox.Text = ConfigData.csvPath;
            VOICELOIDSelected.SelectedIndex = ConfigData.Index;
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
            ConfigData.csvPath = CSVPathTextBox.Text;
            ConfigData.Index = VOICELOIDSelected.SelectedIndex;

            manager.Save();
        }

        //OKボタンクリック時動作
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
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
        
    }
}