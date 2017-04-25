using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Yukarinette;

namespace exPlugin
{
    public class ConfigManager
	{
		public ConfigData configData
		{
			get;
			protected set;
		}

		string configPath
		{
			get;
			set;
		}

        string csvData
        {
            get;
            set;
        }

        string csvPath
        {
            get;
            set;
        }

        public ConfigManager()
		{
			configData = null;

			var fileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
			var path = Path.Combine(YukarinetteCommon.AppSettingFolder, "plugins");

			configPath = Path.Combine(path, fileName + ".config");
            csvPath = Path.Combine(path, fileName + ".csv");
        }

		// 設定ファイル読み込み
		public void LoadConfig(string pluginName)
		{
			if (!File.Exists(configPath))
			{
				CreateNewSetting();

				return;
			}

            //設定ファイル読み込みにトライ
			try
			{
				using (var fileStream = new FileStream(configPath, FileMode.Open))
				{
					using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
					{
						var xmlSerializer = new XmlSerializer(typeof(ConfigData));

						configData = (ConfigData)xmlSerializer.Deserialize(streamReader);
					}
				}
			}
			catch   //失敗したら新しくファイルを作る
			{
				CreateNewSetting();

				YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + " の設定ファイルが読み取れませんでした。初期値で動作します。");
			}			
		}

        // CSVファイル読み込み      まだcsvファイルの作成を行っていない
        public void LoadCSV(string pluginName)
        {
            if (!File.Exists(configPath))
            {
                CreateNewSetting();

                return;
            }

            //設定ファイル読み込みにトライ
            try
            {
                using (var fileStream = new FileStream(configPath, FileMode.Open))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        var xmlSerializer = new XmlSerializer(typeof(ConfigData));   //ここxmlのままになってる

                        configData = (ConfigData)xmlSerializer.Deserialize(streamReader);
                    }
                }
            }
            catch   //失敗したら新しくファイルを作る
            {
                CreateNewSetting();

                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + " のCSVファイルが読み取れませんでした。初期値で動作します。");
            }
        }

        // 設定ファイル保存
        public void Save(string pluginName)
		{
			if (configData == null)
			{
				return;
			}

			try
			{
				var settingDirectory = Path.GetDirectoryName(configPath);

				if (!Directory.Exists(settingDirectory))
				{
					Directory.CreateDirectory(settingDirectory);
				}

				using (var fileStream = new FileStream(configPath, FileMode.Create))
				{
					using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
					{
						new XmlSerializer(typeof(ConfigData)).Serialize(streamWriter, configData);
					}
				}
			}
			catch
			{
				YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + " の設定ファイルの保存に失敗しました。");
			}
		}

		public void CreateNewSetting()
		{
			configData = new ConfigData();
		}
	}
}