using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using Yukarinette;

namespace exPlugin
{
    // プラグインごとの動作
    public class exManager
    {
        //VOICEROIDをコントロールするのに必要
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        //対応するVOICEROIDを列挙 
        string[] voiceroidNames = {
                                          "結月ゆかり",
                                          "民安ともえ",
                                          "東北ずん子",
                                          "京町セイカ",
                                          "東北きりたん"
                                      };

        //VOICEROIDハンドル保持用変数
        AutomationElement rootHandle = null;

        //停止ボタンのハンドル保持用変数
        IntPtr stpControl = IntPtr.Zero;

        //音声保存ボタンのハンドル保持用変数
        AutomationElement btnSaveWave = null;

        public void Create(string pluginName)
        {
            // 音声認識開始時の初期化とか

            pluginName = pluginName + " : ";

            //VOICEROIDのハンドル取得まで最長15秒待つ
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (rootHandle == null)
            {
                Thread.Sleep(100);
                HandleGet();
                if (15000L <= stopwatch.ElapsedMilliseconds)
                {
                    //VOICEROID.exeが見つからなかった場合
                    YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "VOICEROID＋ " + voiceroidNames[ConfigData.Index] + " が起動していません。");
                    //YukarinetteLogger.Instance.Error("voiceroid process wait error.");
                    stopwatch.Stop();
                    throw new TimeoutException(pluginName + "Voiceroid process has been waiting 15 seconds, start-up was not completed.");
                }
            }
            stopwatch.Stop();


            //ハンドルが登録されていたら
            if (rootHandle != null)
            {
                //停止ボタンと音声再生ボタンのハンドルを取得
                try
                {
                    AutomationElement btnStop = rootHandle.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "btnStop", PropertyConditionFlags.IgnoreCase));
                    stpControl = (IntPtr)btnStop.Current.NativeWindowHandle;
                    btnSaveWave = rootHandle.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "btnSaveWave", PropertyConditionFlags.IgnoreCase));
                    YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "正常起動しました。");
                }
                catch
                {
                    //ハンドルが取得できなかった場合、ウィンドウが見つかっていない
                    YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "VOICEROID＋ " + voiceroidNames[ConfigData.Index] + "のコントロールを取得できませんでした。");
                    return;
                }
            }

        }

        public void Dispose()
        {
            // 音声認識終了時のリソース破棄とか

            //VOICEROIDハンドル破棄
            rootHandle = null;
            //停止ボタンのハンドル破棄
            stpControl = IntPtr.Zero;
            //音声保存ボタンのハンドル破棄
            btnSaveWave = null;
        }

        public void Speech(string text)
        {
            // 音声認識時のメイン処理
            // textに認識した本文が入っている

            //VOICEROIDの各種ハンドルを取得できていたら実行
            if (stpControl != IntPtr.Zero && btnSaveWave != null)
            {
                //キーワードと発話内容の比較チェック
                int index = 0;
                //csvDataは2次元Listなので、foreachで全ての要素を1次元にして比較する
                foreach (List<string> list in ConfigManager.csvData)
                {
                    //もしlistとtextが一致したら離脱（キーワードと喋った内容が一致したら）
                    if (list[0] == text)
                    {
                        //音声保存ボタンがFalseになるまでループ（最長1秒）
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        while (true)
                        {
                            //音声保存ボタンがEnabledじゃない（False）ならVOICEROIDを停止   //WAVEパスが空だとバグるので回避
                            if (!btnSaveWave.Current.IsEnabled && !string.IsNullOrWhiteSpace(ConfigManager.csvData[index][1]))
                            {
                                //VOICEROIDを停止
                                SendMessage(stpControl, 245u, IntPtr.Zero, IntPtr.Zero);
                                //WAVE再生
                                PlaySound(ConfigManager.csvData[index][1]);
                                stopwatch.Stop();
                                //whileを離脱
                                break;
                            }

                            if (1000L <= stopwatch.ElapsedMilliseconds)
                            {
                                stopwatch.Stop();
                                break;
                            }
                            //音声保存ボタンがFalseでないなら0.05秒後に再度Try
                            Thread.Sleep(50);
                        }

                        //foreach離脱
                        break;
                    }
                    else
                    {
                        //listの末端まで行くかlistとtextが一致するまでループ
                        index++;
                        //list[1]はファイルパスなので検査の対象外とする処理
                        continue;
                    }
                }
            }
        }



        //WAVEファイルを再生する関数
        private void PlaySound(string WAVEPath)
        {
            //デバッグ用文章
            //YukarinetteConsoleMessage.Instance.WriteMessage(WAVEPath);

            //WAVEファイルの準備
            SoundPlayer player = new SoundPlayer(@WAVEPath);

            //WAVEファイルが存在するかチェック
            try
            {
                //WAVEファイルのロード
                player.LoadAsync();
            }
            catch (Exception ex)
            {
                //WAVEファイルが見つからなかったらエラー文章を出力
                YukarinetteConsoleMessage.Instance.WriteMessage(ex.Message);
            }

            //player.Play();   //再生終了前に次のファイルを再生する（ぶつ切りされる）
            player.PlaySync();   //再生終了後に次のファイルを再生する（読み上げも止まる）
        }



        //VOICEROIDのプロセス発見からハンドル取得までやる関数
        private void HandleGet()
        {
            //"VOICEROID"と名の付くプロセス一覧を取得
            Process[] apps = Process.GetProcessesByName("VOICEROID");

            //"VOICEROID"プロセスが見つかった場合
            if (!(apps.Length < 1))
            {
                //複数VOICEROID.exeが見つかった場合に備えて、正しいVOICEROIDの判別
                foreach (Process app in apps)
                {
                    //1個ずつハンドル取得して確認していく
                    try
                    {
                        rootHandle = AutomationElement.FromHandle(app.MainWindowHandle);
                    }
                    catch
                    {
                        rootHandle = null;
                        return;
                    }

                    foreach (string voiceroidName in voiceroidNames)
                    {
                        //現在ハンドルを取得しているVOICEROIDと目的のVOICEROIDが一致しているか確認
                        //一致していなかったらハンドルを初期化してもう1回
                        if (!(rootHandle.Current.Name.StartsWith("VOICEROID＋ " + voiceroidNames[ConfigData.Index])))
                        {
                            rootHandle = null;
                        }
                    }
                }
            }
        }




    }
}