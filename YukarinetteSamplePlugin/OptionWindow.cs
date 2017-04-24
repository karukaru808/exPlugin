using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using Yukarinette;

namespace YukarinetteSamplePlugin
{
	public partial class OptionWindow
	{
		private OptionWindow(ConfigManager manager)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

            // 現在の設定を設定欄に反映
            WAVEPathTextBox.Text = manager.Data.SampleSetting;
		}

		// 設定画面表示時実行
		public static void Show(ConfigManager manager, string pluginName)
		{
			var optionWindow = new OptionWindow(manager);

			if (optionWindow.ShowDialog().Value)
			{
				optionWindow.Save(manager, pluginName);
			}
		}

		// 設定画面閉止時実行
		private void Save(ConfigManager manager, string pluginName)
		{
			// 設定欄の内容を設定に保存
			manager.Data.SampleSetting = WAVEPathTextBox.Text;

			manager.Save(pluginName);
		}

        //OKボタンクリック時動作
		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = new bool?(true);
		}

        //参照ボタンクリック時動作
        private void WAVEPathButton_Click(object sender, RoutedEventArgs e)
		{
			YukarinetteLogger.Instance.Debug("start.");
			string initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			if (0 < this.WAVEPathTextBox.Text.Length && File.Exists(this.WAVEPathTextBox.Text))
			{
				initialDirectory = Path.GetDirectoryName(this.WAVEPathTextBox.Text);
			}
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				FileName = "",
				InitialDirectory = initialDirectory,
				Filter = "WAVE (*.wav)|*.wav",
				Title = "WAVE ファイルを指定してください。"
			};
			YukarinetteLogger.Instance.Info("dialog open.");
			if (openFileDialog.ShowDialog().Value)
			{
				YukarinetteLogger.Instance.Info("dialog ok.");
				this.WAVEPathTextBox.Text = openFileDialog.FileName;
			}
			YukarinetteLogger.Instance.Debug("end.");
		}
	}
}