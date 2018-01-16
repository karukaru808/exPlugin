// *** YukarinetteSamplePlugin ***
// MIT License
// 
// Copyright(c) 2017 midorigoke
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// 
// *** NAudio ***
// Microsoft Public License
// Copyright(c) 2008 Mark Heath
// 


using exPlugin.Controller;
using Yukarinette;

namespace exPlugin
{
    //こっちは主に呼び出し側
    public class exPlugin : IYukarinetteInterface
    {
        ConfigManager configManager;
        private exManager exManager;

        private static string pName = "exPlugin";

        public override string Name
        {
            get
            {
                // プラグイン名
                return pName;
            }
        }

        public static string ConsoleName
        {
            get
            {
                // プラグイン名
                return pName + " : ";
            }
        }

        public override void Loaded()
        {
            // 起動時実行
            configManager = new ConfigManager();
            configManager.LoadConfig();
            configManager.CheckCSV();
            configManager.LoadCSV();

            //自分自身の実行ファイルのパスを取得する
            //string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //実行ファイルがあるフォルダパスを取得する
            //string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //YukarinetteConsoleMessage.Instance.WriteMessage(appPath);
        }

        public override void Closed()
        {
            // 終了時実行
            configManager.Save();
        }

        public override void Setting()
        {
            // 設定ボタン押下時実行
            OptionWindow.Show(configManager);
        }

        public override void SpeechRecognitionStart()
        {
            //YukarinetteConsoleMessage.Instance.WriteMessage("SpeechRecognitionStart");

            if (ConfigData.vIndex != 0)
            {
                exManager = new V1Controller();
            }
            else
            {
                exManager = new V2Controller();
            }

            // 音声認識開始時実行
            //音声認識スタートボタン押したときに呼び出される
            configManager.LoadCSV();
            exManager.Create();

            //デバッグ用
            //exManager.BeforeSpeech();
        }

        public override void SpeechRecognitionStop()
        {
            // 音声認識終了時実行
            //音声認識終了ボタン押したときに呼び出される
            //YukarinetteConsoleMessage.Instance.WriteMessage("SpeechRecognitionStop");
            exManager.Dispose();
        }

        public override void BeforeSpeech(string text)
        {
            // 音声認識時実行
            //何か喋った時に呼び出される
            //Speechよりも前に処理される
            //YukarinetteConsoleMessage.Instance.WriteMessage("BeforeSpeech");
            //exManager.BeforeSpeech();
        }

        public override void Speech(string text)
        {
            // 音声認識時実行
            //何か喋った時に呼び出される
            //YukarinetteConsoleMessage.Instance.WriteMessage("Speech");
            exManager.Speech(text);
        }

        public override void AfterSpeech(string text)
        {
            // 音声認識時実行
            //何か喋った時に呼び出される
            //Speechが終わったら処理される
            //YukarinetteConsoleMessage.Instance.WriteMessage("AfterSpeech");
            //exManager.AfterSpeech();
        }

    }
}
