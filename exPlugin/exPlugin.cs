using Yukarinette;

namespace exPlugin
{
    //こっちは主に呼び出し側（？）
    public class exPlugin : IYukarinetteInterface
    {
        ConfigManager configManager;

        exManager exManager;

        public override string Name
        {
            get
            {
                // プラグイン名
                return "exPlugin";
            }
        }

        public override void Loaded()
        {
            // 起動時実行
            configManager = new ConfigManager();
            configManager.LoadConfig(Name);
            configManager.CheckCSV(Name);

            //自分自身の実行ファイルのパスを取得する
            //string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //実行ファイルがあるフォルダパスを取得する
            //string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //YukarinetteConsoleMessage.Instance.WriteMessage(appPath);

            exManager = new exManager();
        }

        public override void Closed()
        {
            // 終了時実行
            configManager.Save(Name);
        }

        public override void Setting()
        {
            // 設定ボタン押下時実行
            OptionWindow.Show(configManager, Name);
        }

        public override void SpeechRecognitionStart()
        {
            // 音声認識開始時実行
            //音声認識スタートボタン押したときに呼び出される
            configManager.LoadCSV(Name);
            exManager.Create();
            //YukarinetteConsoleMessage.Instance.WriteMessage("1");
        }

        public override void SpeechRecognitionStop()
        {
            // 音声認識終了時実行
            //音声認識終了ボタン押したときに呼び出される
            exManager.Dispose();
            //YukarinetteConsoleMessage.Instance.WriteMessage("2");
        }

        public override void Speech(string text)
        {
            // 音声認識時実行
            //何か喋った時に呼び出される
            exManager.Speech(text);
            //YukarinetteConsoleMessage.Instance.WriteMessage("3");
        }

    }
}
