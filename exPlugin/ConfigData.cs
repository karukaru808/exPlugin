using System.IO;
using System.Reflection;
using Yukarinette;

namespace exPlugin
{
    public class ConfigData
    {
        // 設定項目
        public static string csvPath;
        public static int Index;

        public string Path
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
            var dllpath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            csvPath = System.IO.Path.Combine(dllpath, ConfigManager.fileName + ".csv");
            Index = 0;
        }
        

    }
}