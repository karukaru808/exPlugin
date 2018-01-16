using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using Yukarinette;

namespace exPlugin
{

    // プラグインごとの動作
    public abstract class exManager
    {
        //対応するVOICEROIDを列挙 
        public static string[] voiceroidNames = {
                                          "VOICEROID2",
                                          "VOICEROID＋ 結月ゆかり",
                                          "VOICEROID＋ 民安ともえ",
                                          "VOICEROID＋ 東北ずん子",
                                          "VOICEROID＋ 琴葉茜",
                                          "VOICEROID＋ 琴葉葵",
                                          "VOICEROID＋ 京町セイカ",
                                          "VOICEROID＋ 東北きりたん",
                                          "VOICEROID＋ 鷹の爪 吉田くん",
                                          "VOICEROID＋ 水奈瀬コウ",
                                          "VOICEROID＋ 月読アイ",
                                          "VOICEROID＋ 月読ショウタ"
                                      };

        //VOICEROIDウィンドウのハンドル保持用変数
        public AutomationElement root = null;

        //VOICEROIDの探索続行判定用変数
        private bool taskFlag;

        //VOICEROIDの探索タスク定義用変数
        private Task task = null;

        //初期動作
        public void Create()
        {
            //YukarinetteLogger.Instance.Info("exManager Create!");

            // 音声認識開始時の初期化とか

            //VOICEROIDの探索開始
            taskFlag = true;

            //タスク内容記述
            task = Task.Run(delegate
            {
                //VOICEROIDのハンドル取得まで最長10秒待つ
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //rootがnullならループする
                while (root == null)
                {
                    //10秒タイムアウト判定
                    if (10000L <= stopwatch.ElapsedMilliseconds)
                    {
                        //VOICEROID.exeが見つからなかった場合
                        YukarinetteLogger.Instance.Error("10秒待機　タイムアウト　VOICEROID: " + voiceroidNames[ConfigData.vIndex]);
                        YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + voiceroidNames[ConfigData.vIndex] + " が起動していません。");

                        stopwatch.Stop();
                        Dispose();
                        return;
                    }

                    //0.5秒待機
                    Thread.Sleep(500);

                    //rootHandleの取得
                    root = RootHandleGet(voiceroidNames[ConfigData.vIndex]);

                    //rootに値が入った時にコントローラを起動&ボタンのハンドル取得
                    if (root != null)
                    {
                        //コントローラを取得
                        bool flag = Controller();
                        
                        //コントローラの結果がtrue & taskFlagがtrue
                        //コントローラの取得に成功して、途中でDisposeされていない場合
                        if (flag && taskFlag)
                        {
                            YukarinetteLogger.Instance.Info("プラグイン起動　成功　VOICEROID: " + root.Current.Name);
                            YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + "正常起動しました。　VOICEROID: " + root.Current.Name);
                        }
                        //コントローラの取得に失敗して、途中でDisposeされていない場合
                        else if (!flag && taskFlag)
                        {
                            //YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + ex.Message);
                            //ハンドルが取得できなかった場合、ウィンドウが見つかっていない
                            YukarinetteLogger.Instance.Error(voiceroidNames[ConfigData.vIndex] + " のコントロール取得　失敗");
                            YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + voiceroidNames[ConfigData.vIndex] + "のコントロールを取得できませんでした。");

                            stopwatch.Stop();
                            Dispose();
                            return;
                        }
                        else
                        {
                            //YukarinetteConsoleMessage.Instance.WriteMessage("Task Kill !!!");
                        }

                        //タスクを破棄
                        stopwatch.Stop();
                        task = null;
                    }
                }

                stopwatch.Stop();
                return;
            });
        }

        //リソース破棄
        public void Dispose()
        {
            //タスクを破棄
            taskFlag = false;
            task = null;

            //VOICEROIDハンドル破棄
            root = null;

            //子クラスで初期化
            Clear();
        }


        public void Speech(string text)
        {
            // 音声認識時のメイン処理
            // textに認識した本文が入っている
            // ゆかりねっとVer0.3.2 より textの最後に「。」が挿入されるようになった
            //YukarinetteConsoleMessage.Instance.WriteMessage(text + " , " + text.Remove(text.Length - 1));

            //csvDataは2次元Listなので、foreachで全ての要素を1次元にして比較する
            //listの末端まで行くかlistとtextが一致するまでループ
            foreach (List<string> list in ConfigManager.csvData)
            {
                //もしlistとtextが一致したら離脱（キーワードと喋った内容が一致したら）
                // 末尾に「。」があるキーワードと一致 || 末尾に「。」がないキーワードと一致　のいずれかでTrueになったら
                if (list[0] == text || list[0] == text.Remove(text.Length - 1))
                {
                    //音声保存ボタンがFalseになるまでループ（最長1秒）
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    //1秒間回す
                    while (1000L > stopwatch.ElapsedMilliseconds)
                    {
                        //音声保存ボタンが not Enabled == true ならVOICEROIDを停止
                        if (BtnCheck())
                        {
                            //YukarinetteConsoleMessage.Instance.WriteMessage("Check OK");

                            //VOICEROIDを停止
                            //bool flag = StopAction();

                            //VOICEROIDを停止できたら
                            if (StopAction())
                            {
                                //WAVE再生
                                PlaySound(list[1]);
                                stopwatch.Stop();
                                //関数を離脱
                                return;
                            }
                        }

                        //0.01秒待機
                        Thread.Sleep(10);
                    }

                    //while を抜けた後は音声再生に失敗した場合のみ
                    YukarinetteLogger.Instance.Error("音声再生　タイムアウト");
                    YukarinetteConsoleMessage.Instance.WriteMessage("音声再生　タイムアウト");
                    stopwatch.Stop();
                    //関数を離脱
                    return;
                }
            }

        }

        //音声ファイルを再生する関数
        private void PlaySound(string Path)
        {
            //デバッグ用文章
            //YukarinetteConsoleMessage.Instance.WriteMessage(Path);

            //音声プレイヤー変数
            IWavePlayer wavePlayer = null;

            //オーディオファイルリーダー保持用変数
            AudioFileReader reader = null;

            //音声再生をTry
            try
            {
                var endPoints = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                MMDevice OutputDevice = endPoints[ConfigData.oIndex];

                //デバイス、共有モード、イベントコールバック無効、レイテンシ=100ms でプレイヤーを設定
                wavePlayer = (IWavePlayer)new WasapiOut(OutputDevice, AudioClientShareMode.Shared, false, 100);

                //音声ファイルのロード
                //対応フォーマット：.wav, .mp3, .aiff, MFCreateSourceReaderFromURL()で読めるもの（動画ファイルも可？）
                reader = new AudioFileReader(@Path);

                //sampleChannelにreaderをセット
                var sampleChannel = new SampleChannel(reader, true);

                /*
                //ここでボリューム等（？）の設定が可能？
                //サンプルの AudioPlaybackPanel.cs を参照
                sampleChannel.PreVolumeMeter+= OnPreVolumeMeter;
                setVolumeDelegate = vol => sampleChannel.Volume = vol;
                */

                //sampleChannelのボリュームをセット
                var postVolumeMeter = new MeteringSampleProvider(sampleChannel);

                /*
                //ここでボリューム等（？）の設定が可能？
                //サンプルの AudioPlaybackPanel.cs を参照
                postVolumeMeter.StreamVolume += OnPostVolumeMeter;
                */

                //型の明確化（変換？）
                ISampleProvider sampleProvider = postVolumeMeter;

                //AudioFileReader から sampleProvider まで1つのtryにまとめられているため、エラー処理はこれらを別のtryにすることが推奨される（？）
                //別のtryにしておけばエラーの特定楽だよね

                //プレイヤーに設定をセット
                wavePlayer.Init(sampleProvider);

                //再生
                YukarinetteLogger.Instance.Info("音声再生　開始");
                wavePlayer.Play();

                while (wavePlayer.PlaybackState.ToString() == "Playing")
                {
                    //0.05秒ごとに再生終了チェック
                    Thread.Sleep(50);
                    //YukarinetteConsoleMessage.Instance.WriteMessage(wavePlayer.PlaybackState.ToString());
                }

                //リソース破棄
                wavePlayer.Stop();
                wavePlayer.Dispose();
                wavePlayer = null;

                reader.Dispose();
                reader = null;

                YukarinetteLogger.Instance.Info("音声再生　終了");
            }
            catch (Exception ex)
            {
                //WAVEファイルが見つからなかったらエラー文章を出力
                //例 : 指定された場所にサウンド ファイルが存在することを確認してください。　Path: C:\Users\karu\Music\VOICELOID+\東北きりたん exVOICE\あいさつ_基本語\asd.wav
                YukarinetteLogger.Instance.Error(ex.Message + "　Path: " + Path);
                YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + ex.Message);
            }
        }

        //VOICEROIDのプロセス発見からハンドル取得までやる関数
        public AutomationElement RootHandleGet(string voiceroidName)
        {
            //YukarinetteLogger.Instance.Info("RootHandleGet Start.");

            //VOICEROIDハンドル保持用変数
            AutomationElement Handle = null;

            //プロセス一覧保持用変数
            Process[] apps = GetProcess();

            //"VOICEROID"プロセスが見つかった場合
            if (!(apps.Length < 1))
            {
                //複数VOICEROID.exeが見つかった場合に備えて、正しいVOICEROIDの判別
                foreach (Process app in apps)
                {
                    //1個ずつハンドル取得して確認していく
                    try
                    {
                        Handle = AutomationElement.FromHandle(app.MainWindowHandle);
                    }
                    catch
                    {
                        Handle = null;
                        continue;
                    }

                    //現在ハンドルを取得しているVOICEROIDと目的のVOICEROIDが一致しているか確認
                    //一致していなかったらハンドルを初期化してもう1回
                    if (!(Handle.Current.Name.StartsWith(voiceroidName)))
                    {
                        Handle = null;
                        continue;
                    }
                    else
                    {
                        YukarinetteLogger.Instance.Info(Handle.Current.Name + " のハンドル取得　成功");

                        return Handle;
                    }
                }
            }

            //YukarinetteLogger.Instance.Info("Missing RootHandleGet NULL.");
            return null;
        }

        private bool Controller()
        {

            //音声保存ボタンがFalseになるまでループ（最長1秒）
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //1秒間回す
            while (1000L > stopwatch.ElapsedMilliseconds && taskFlag)
            {
                try
                {
                    //コントローラを起動
                    ControllerCreate(root.Current.NativeWindowHandle);

                    //ボタンのハンドル取得
                    BtnHandleGet(root);

                    //取得成功
                    return true;
                }
                catch (Exception ex)
                {
                    /*
                    YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + ex.Message);
                    //ハンドルが取得できなかった場合、ウィンドウが見つかっていない
                    YukarinetteLogger.Instance.Error(voiceroidNames[ConfigData.vIndex] + " のコントロール取得　失敗");
                    YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + voiceroidNames[ConfigData.vIndex] + "のコントロールを取得できませんでした。");

                    stopwatch.Stop();
                    Dispose();
                    */
                }

                //0.1秒待機
                Thread.Sleep(100);
            }

            //取得失敗
            stopwatch.Stop();
            return false;
        }

        //デバッグ用
        public void BeforeSpeech()
        {
            BtnCheck();
        }

        //オーバーライドする関数たち
        protected abstract void Clear();
        protected abstract Process[] GetProcess();
        protected abstract void ControllerCreate(int hWnd);
        protected abstract void BtnHandleGet(AutomationElement root);
        protected abstract bool StopAction();
        protected abstract bool BtnCheck();
    }
}