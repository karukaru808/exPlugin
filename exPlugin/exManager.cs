using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Timers;
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
        AutomationElement ae = null;

        //停止ボタンのハンドル保持用変数
        IntPtr stpControl = IntPtr.Zero;

        public void Create()
        {
            // 音声認識開始時の初期化とか

            //VOICEROID起動まで待つ
            time(1000);

            //VOICEROID.exeの起動チェック
            Process[] apps = Process.GetProcessesByName("VOICEROID");

            //VOICEROID.exeが見つかった場合
            if (!(apps.Length < 1))
            {
                //VOICEROID.exeのプロセス取得
                //複数VOICEROID.exeが見つかった場合に備えて、正しいVOICEROIDの判別
                foreach (Process app in apps)
                {
                    //1個ずつハンドル取得して確認していく
                    AutomationElement rootHandle = AutomationElement.FromHandle(app.MainWindowHandle);
                    foreach (string voiceroidName in voiceroidNames)
                    {
                        //現在ハンドルを取得しているVOICEROIDと目的のVOICEROIDが一致しているか確認
                        string name = rootHandle.Current.Name;
                        if (name.StartsWith("VOICEROID＋ " + voiceroidNames[ConfigData.Index]))
                        {
                            //一致していたら正式ハンドルとして登録
                            ae = rootHandle;
                        }
                    }
                }
            }
            //VOICEROID.exeが見つからなかった場合
            else
            {
                YukarinetteConsoleMessage.Instance.WriteMessage("VOICEROID＋ " + voiceroidNames[ConfigData.Index] + " が見つかりませんでした。");
                return;
            }
            
            //テキストボックス、再生ボタン、停止ボタンのGUIコントロール取得
            AutomationElement txtForm = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "txtMain", PropertyConditionFlags.IgnoreCase));
            IntPtr txtFormHandle = IntPtr.Zero;
            try
            {
                txtFormHandle = (IntPtr)txtForm.Current.NativeWindowHandle;
            }
            catch (NullReferenceException e)
            {
                //ハンドルが取得できなかった場合、ウィンドウが見つかっていない
                Console.WriteLine(voiceroidNames[ConfigData.Index] + "のウィンドウが取得できませんでした. 最小化されているか, ほかのプロセスによってブロックされています.");
                return;
            }
            

            //停止ボタンのハンドルを取得
            AutomationElement btnStop = ae.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "btnStop", PropertyConditionFlags.IgnoreCase));
            stpControl = (IntPtr)btnStop.Current.NativeWindowHandle;
        }

        public void Dispose()
        {
            // 音声認識終了時のリソース破棄とか

            //停止ボタンのハンドル変数初期化
            stpControl = IntPtr.Zero;
        }

        public void Speech(string text)
        {
            // 音声認識時のメイン処理
            // textに認識した本文が入っている

            //キーワードと発話内容の比較チェック
            int index = 0;
            //csvDataは2次元Listなので、foreachで全ての要素を1次元にして比較する
            foreach (List<string> list in ConfigManager.csvData)
            {
                //もしlistとtextが一致したら離脱（キーワードと喋った内容が一致したら）
                if (list[0] == text)
                {
                    //VOICEROIDを停止
                    if (stpControl != IntPtr.Zero)
                    {
                        //不安定だからちょっとだけ待つ
                        time(10);
                        SendMessage(stpControl, 245u, IntPtr.Zero, IntPtr.Zero);
                        YukarinetteConsoleMessage.Instance.WriteMessage("STOP");
                    }


                    //WAVEを止めてから再生（いらない？）
                    //StopSound();

                    //もし喋った内容とキーワードが一致したらWAVE再生
                    PlaySound(ConfigManager.csvData[index][1]);

                    //foreach離脱
                    break;
                }
                else
                {
                    index++;
                    continue;
                }
            }
        }

        //以下WAVEファイルを再生するためのプログラム
        //再生用
        private void PlaySound(string WAVEPath)
        {
            //デバッグ用文章
            YukarinetteConsoleMessage.Instance.WriteMessage(WAVEPath);

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

            player.Play();   //再生終了前に次のファイルを再生する（ぶつ切りされる）
            //player.PlaySync();   //再生終了後に次のファイルを再生する（読み上げも止まる）
        }

        //停止用（動作的にいらない？まあ保険で入れておこう...）
        /*
        private void StopSound()
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }
        }
        */

        static void time(int interval)
        {
            // タイマーの生成
            var timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(func);
            timer.Interval = interval;

            // タイマーを開始
            timer.Start();

            // タイマーを停止
            timer.Stop();
        }

        static void func(object sender, ElapsedEventArgs e) { }

    }
}