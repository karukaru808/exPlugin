using Yukarinette;

namespace YukarinetteSamplePlugin
{
    //こっちは主に呼び出し側（？）
    public class SamplePlugin : IYukarinetteInterface
    {
        ConfigManager configManager;

        SampleManager sampleManager;

        public override string Name
        {
            get
            {
                // プラグイン名
                return "SamplePlugin";
            }
        }

        public override void Loaded()
        {
            // 起動時実行
            configManager = new ConfigManager();
            configManager.Load(Name);

            sampleManager = new SampleManager();
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
            sampleManager.Create(configManager.Data.SampleSetting);
        }

        public override void SpeechRecognitionStop()
        {
            // 音声認識終了時実行
            sampleManager.Dispose();
        }

        public override void Speech(string text)
        {
            // 音声認識時実行
            sampleManager.Speech(text);
        }

    }
}
