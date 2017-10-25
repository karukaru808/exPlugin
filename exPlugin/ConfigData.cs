using System;
using System.IO;
using System.Reflection;
using Yukarinette;

namespace exPlugin
{
    public class ConfigData
    {
        // 設定項目
        public static string version;
        public static string csvPath;
        public static int Index;

        public string PluginVersion
        {
            get
            {
                return version;
            }

            set
            {
                version = value;
            }
        }

        public string CsvPath
        {
            get
            {
                return csvPath;
            }

            set
            {
                csvPath = value;
            }
        }

        public int VOICEROIDIndex
        {
            get
            {
                return Index;
            }

            set
            {
                Index = value;
            }
        }

        public ConfigData()
        {
            // 設定項目初期化

            //プラグインのバージョン情報
            version = (Assembly.GetExecutingAssembly().GetName().Version).ToString();

            //設定ファイルの位置情報
            csvPath = Path.Combine(Path.Combine(YukarinetteCommon.AppSettingFolder, "Plugins"), ConfigManager.fileName + ".csv");

            //使用VOICEROID情報
            Index = 0;
        }
        

    }
}