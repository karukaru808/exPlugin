using System;
using System.Diagnostics;
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
        public static int vIndex;
        public static int oIndex;

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

        public int OutputIndex
        {
            get
            {
                return oIndex;
            }

            set
            {
                oIndex = value;
            }
        }

        public int VOICEROIDIndex
        {
            get
            {
                return vIndex;
            }

            set
            {
                vIndex = value;
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



        public ConfigData()
        {
            // 設定項目初期化

            //プラグインのバージョン情報
            //version = (Assembly.GetExecutingAssembly().GetName().Version).ToString();     //アセンブリバージョン
            version = FileVersionInfo.GetVersionInfo((new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).FileVersion;        //ファイルバージョン
            
            //音声出力先情報
            oIndex = 0;

            //使用VOICEROID情報
            vIndex = 0;

            //設定ファイルの位置情報
            csvPath = Path.Combine(Path.Combine(YukarinetteCommon.AppSettingFolder, "Plugins"), ConfigManager.fileName + ".csv");


        }
        

    }
}