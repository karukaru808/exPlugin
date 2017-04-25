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

        //ConfigManager configManager;
        public ConfigData()
        {
            // 設定項目初期化
            csvPath = "";
            //csvPath = configManager.configData.csvPath;
            VOICELOIDIndex = 0;
        }
        

    }
}