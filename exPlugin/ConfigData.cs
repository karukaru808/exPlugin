using System.IO;
using System.Reflection;
using Yukarinette;

namespace exPlugin
{
    public class ConfigData
    {
        // 設定項目
        public static string csvPath
        {
            get;
            set;
        }

        public int VOICELOIDIndex
        {
            get;
            set;
        }

        public ConfigData()
        {
            // 設定項目初期化
            var dllpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            csvPath = Path.Combine(dllpath, ConfigManager.fileName + ".csv");
            VOICELOIDIndex = 0;
        }
        

    }
}