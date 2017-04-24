using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Yukarinette;

namespace YukarinetteSamplePlugin
{
	public class ConfigManager
	{
		public ConfigData Data
		{
			get;
			protected set;
		}

		string settingPath
		{
			get;
			set;
		}

		public ConfigManager()
		{
			Data = null;

			var fileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
			var path = Path.Combine(YukarinetteCommon.AppSettingFolder, "plugins");

			settingPath = Path.Combine(path, fileName + ".config");
		}

		// 設定ファイル読み込み
		public void Load(string pluginName)
		{
			if (!File.Exists(settingPath))
			{
				CreateNewSetting();

				return;
			}

			try
			{
				using (var fileStream = new FileStream(settingPath, FileMode.Open))
				{
					using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
					{
						var xmlSerializer = new XmlSerializer(typeof(ConfigData));

						Data = (ConfigData)xmlSerializer.Deserialize(streamReader);
					}
				}
			}
			catch
			{
				CreateNewSetting();

				YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + " の設定ファイルが読み取れませんでした。初期値で動作します。");
			}			
		}

		// 設定ファイル保存
		public void Save(string pluginName)
		{
			if (Data == null)
			{
				return;
			}

			try
			{
				var settingDirectory = Path.GetDirectoryName(settingPath);

				if (!Directory.Exists(settingDirectory))
				{
					Directory.CreateDirectory(settingDirectory);
				}

				using (var fileStream = new FileStream(settingPath, FileMode.Create))
				{
					using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
					{
						new XmlSerializer(typeof(ConfigData)).Serialize(streamWriter, Data);
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
			Data = new ConfigData();
		}
	}
}