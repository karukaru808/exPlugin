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

        public string csvPath
        {
            get;
            set;
        }

        public ConfigManager()
        {
            configData = null;

            var fileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var confpath = Path.Combine(YukarinetteCommon.AppSettingFolder, "plugins");
            var dllpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            exPlugin ex = new exPlugin();

            configPath = Path.Combine(confpath, fileName + ".config");
            csvPath = Path.Combine(dllpath, ex.Name + ".csv");
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

        // CSVファイル読み込み
        public void LoadCSV(string pluginName)
        {
            if (!File.Exists(csvPath))
            {
                using (FileStream fs = File.Create(csvPath))
                {
                    // ファイルストリームを閉じて、変更を確定させる
                    // 呼ばなくても using を抜けた時点で Dispose メソッドが呼び出される
                    fs.Close();
                }
                return;
            }

            //設定ファイル読み込みにトライ
            try
            {
                using (var fileStream = new FileStream(csvPath, FileMode.Open))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        while (streamReader.EndOfStream == false)
                        {
                            //1行ずつ読み込んで区切り文字で分離
                            string line = streamReader.ReadLine();
                            string[] fields = line.Split(',');

                            //その内容を表示
                            for (int i = 0; i < fields.Length; i++)
                            {
                                YukarinetteConsoleMessage.Instance.WriteMessage(fields[i]);
                            }

                        }
                    }
                }
            }
            catch   //失敗したら新しくファイルを作る
            {
                using (FileStream fs = File.Create(csvPath))
                {
                    // ファイルストリームを閉じて、変更を確定させる
                    // 呼ばなくても using を抜けた時点で Dispose メソッドが呼び出される
                    fs.Close();
                }

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