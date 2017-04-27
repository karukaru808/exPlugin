using System.Collections.Generic;
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

        public static string fileName
        {
            get;
            private set;
        }

        public static List<List<string>> csvData = new List<List<string>>();
        /*
        public static List<List<string>> csvData
        {
            get ;
            set ;
        }
        */

        public ConfigManager()
        {
            configData = null;

            fileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            var confpath = Path.Combine(YukarinetteCommon.AppSettingFolder, "plugins");

            configPath = Path.Combine(confpath, fileName + ".config");
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

        // CSVファイルが無かったら作成
        public void CheckCSV(string pluginName)
        {
            if (!File.Exists(ConfigData.csvPath))
            {
                using (FileStream fs = File.Create(ConfigData.csvPath))
                {
                    // UTF-8でエンコードして書き込むためのStreamWriterを作成
                    using (StreamWriter streamWrite = new StreamWriter(fs, Encoding.GetEncoding("UTF-8")))
                    {
                        //ヘッダを書き込む
                        streamWrite.WriteLine("キーワード,ファイルパス（フルパス）\r\n");
                    }

                    // ファイルストリームを閉じて、変更を確定させる
                    // 呼ばなくても using を抜けた時点で Dispose メソッドが呼び出される
                    fs.Close();
                }
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + " のCSVファイルが見つかりませんでした。新規作成します。");
                return;
            }
        }

        // CSVファイル読み込み
        public void LoadCSV(string pluginName)
        {
            //CSVファイル読み込みにトライ
            try
            {
                using (var fileStream = new FileStream(ConfigData.csvPath, FileMode.Open))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.GetEncoding("UTF-8")))
                    {
                        streamReader.ReadLine();   //最初の一行分(表のヘッダ部分)を読み飛ばす
                        while (!streamReader.EndOfStream)
                        {
                            List<string> addData = new List<string>();
                            string line = streamReader.ReadLine();   //一行ずつ読み込む
                            string[] splitData = line.Split(',');   //','区切りで分割したものを配列に追加
                            for (int i = 0; i < splitData.Length; i++)
                            {
                                addData.Add(splitData[i]);   //追加用のList<string>の作成
                            }
                            //addData.Add("\n");
                            csvData.Add(addData);   //List<List<string>>のList<string>部分の追加
                        }
                    }
                }
            }
            catch   //失敗したら新しくファイルを作る
            {
                using (FileStream fs = File.Create(ConfigData.csvPath))
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