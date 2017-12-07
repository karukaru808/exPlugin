using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            set;
        }

        public static string configPath
        {
            get;
            set;
        }

        public static string fileName
        {
            get;
            set;
        }

        public static List<List<string>> csvData = new List<List<string>>();

        private string pluginName;

        public ConfigManager(string pName)
        {
            pluginName = pName + " : ";

            configData = null;

            fileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            configPath = Path.Combine(Path.Combine(YukarinetteCommon.AppSettingFolder, "Plugins"), fileName + ".config");
        }

        // 設定ファイル読み込み
        public void LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                //Configファイルが存在しないなら新規作成
                CreateNewSetting();
                return;
            }

            // 設定ファイル読み込みにトライ
            try
            {
                using (var fileStream = new FileStream(configPath, FileMode.Open))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        //データの整理
                        var xmlSerializer = new XmlSerializer(typeof(ConfigData));
                        
                        //データのコピー
                        configData = (ConfigData)xmlSerializer.Deserialize(streamReader);
                    }

                    //filestreamをクローズして解放する
                    fileStream.Close();
                }
            }
            catch   // 失敗したら新しくファイルを作る
            {
                CreateNewSetting();
                YukarinetteLogger.Instance.Error("設定ファイル　読み取り不可");
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "設定ファイルが読み取れませんでした。初期値で動作します。");
            }

            //YukarinetteConsoleMessage.Instance.WriteMessage(configData.PluginVersion);
            //YukarinetteConsoleMessage.Instance.WriteMessage(FileVersionInfo.GetVersionInfo((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).FileVersion);

            //もし設定ファイルのバージョンとプラグインのバージョンが不一致ならば
            if (configData.PluginVersion != FileVersionInfo.GetVersionInfo((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).FileVersion.ToString())
            {
                //新しい設定ファイルを作成する
                YukarinetteLogger.Instance.Error("設定ファイル　バージョン相違　FileVersion: " + configData.PluginVersion + ", PluginVersion: " + FileVersionInfo.GetVersionInfo((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).FileVersion);
                CreateNewSetting();                
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "設定ファイルのバージョンが違います。初期値で動作します。");
            }
        }

        // CSVファイルが無かったら作成
        public void CheckCSV()
        {            
            if (!File.Exists(ConfigData.csvPath))
            {
                try
                {
                    CreateNewCSV();
                    YukarinetteLogger.Instance.Error("CSVファイル　未検出");
                    YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "CSVファイルが見つかりませんでした。新規作成します。");
                }
                catch
                {
                    YukarinetteLogger.Instance.Error("CSVファイル　未検出　新規作成失敗");
                    YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "CSVファイルが見つかりませんでした。また新規作成に失敗しました。読書権限があるか確認してください。");
                }
            }
        }

        // CSVファイル読み込み
        public void LoadCSV()
        {
            //csvDataqを初期化
            csvData = new List<List<string>>();

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

                            //foreach で書いた
                            string[] splitDatas = line.Split(',');   //','区切りで分割したものを配列に追加
                            foreach (string splitData in splitDatas)
                            {
                                addData.Add(splitData);     //追加用のList<string>の作成
                            }
                            csvData.Add(addData);       //List<List<string>>のList<string>部分の追加
                        }
                    }
                }
            }
            catch   //失敗したら新しくファイルを作る
            {
                CreateNewCSV();
                YukarinetteLogger.Instance.Error("CSVファイル　読み取り不可");
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "CSVファイルが読み取れませんでした。初期値で動作します。");
            }
        }

        // 設定ファイル保存
        public void Save()
        {
            //そもそもデータがnullなら弾き返す
            if (configData == null)
            {
                YukarinetteLogger.Instance.Error("設定ファイル　データNULL");
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "Error!　設定ファイルに書き込むデータが存在しません。");
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "Error!　このメッセージが出た場合は Twitter まで連絡ください。");
                return;
            }

            try
            {
                //設定フォルダのパスを取得　デフォ：%AppData%\Local\Yukarinette\plugins
                var settingDirectory = Path.GetDirectoryName(configPath);

                //設定フォルダが存在しないなら
                if (!Directory.Exists(settingDirectory))
                {
                    //設定フォルダの新規作成
                    Directory.CreateDirectory(settingDirectory);
                }

                //ファイルストリームの取得
                using (var filestream = new FileStream(configPath, FileMode.Create))
                {
                    //filestreamに対してUTF-8で書き込み準備
                    using (var streamWriter = new StreamWriter(filestream, Encoding.UTF8))
                    {
                        //データ書き込み
                        new XmlSerializer(typeof(ConfigData)).Serialize(streamWriter, configData);
                    }

                    //filestreamをクローズして解放する
                    filestream.Close();
                }
            }
            catch
            {
                YukarinetteLogger.Instance.Error("設定ファイル　保存失敗");
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "設定ファイルの保存に失敗しました。");
            }
        }

        //Configファイル新規作成用関数
        public void CreateNewSetting()
        {
            //Configファイルに書き込む内容の初期化
            configData = new ConfigData();

            //保存
            Save();
        }

        //CSVファイル新規作成用関数
        public void CreateNewCSV()
        {
            using (FileStream filestream = File.Create(ConfigData.csvPath))
            {
                // UTF-8でエンコードして書き込むためのStreamWriterを作成
                using (StreamWriter streamWrite = new StreamWriter(filestream, Encoding.GetEncoding("UTF-8")))
                {
                    //ヘッダを書き込む
                    streamWrite.WriteLine("キーワード,ファイルパス（フルパス）\r\n,");
                }

                // ファイルストリームを閉じて、変更を確定させる
                filestream.Close();
            }
        }
    }
}