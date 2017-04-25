using System.IO;

namespace exPlugin
{
    public class ConfigData
    {
        // 設定項目
        public string csvPath
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
            ConfigManager configManager = new ConfigManager();
            csvPath = configManager.csvPath;
            VOICELOIDIndex = 0;
        }
        

    }
}