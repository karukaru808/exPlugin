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
                                          "VOICEROID＋ 結月ゆかり",
                                          "VOICEROID＋ 民安ともえ",
                                          "VOICEROID＋ 東北ずん子",
                                          "VOICEROID＋ 京町セイカ",
                                          "VOICEROID＋ 東北きりたん"
                                      };

        //VOICEROIDハンドル保持用変数
        AutomationElement rootHandle = null;

        //停止ボタンのハンドル保持用変数
        IntPtr stpControl = IntPtr.Zero;

        //音声保存ボタンのハンドル保持用変数
        AutomationElement btnSaveWave = null;

        //プラグイン名保存用変数
        private string pluginName;

        public void Create(string pName)
        {
            // 音声認識開始時の初期化とか

            pluginName = pName + " : ";

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
                    YukarinetteLogger.Instance.Error("15秒待機　タイムアウト　VOICEROID: " + voiceroidNames[ConfigData.vIndex]);
                    YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + voiceroidNames[ConfigData.vIndex] + " が起動していません。");
                    stopwatch.Stop();
                    return;
                    //throw new TimeoutException(pluginName + "Voiceroid process has been waiting 15 seconds, start-up was not completed.");
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
                    YukarinetteLogger.Instance.Info("プラグイン起動　成功　VOICEROID: " + rootHandle.Current.Name);
                    YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + "正常起動しました。　VOICEROID: " + rootHandle.Current.Name);
                }
                catch
                {
                    //ハンドルが取得できなかった場合、ウィンドウが見つかっていない
                    YukarinetteLogger.Instance.Error(voiceroidNames[ConfigData.vIndex] + " のコントロール取得　失敗");
                    YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + voiceroidNames[ConfigData.vIndex] + "のコントロールを取得できませんでした。");
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
            // ゆかりねっとVer0.3.2 より textの最後に「。」が挿入されるようになった
            // YukarinetteConsoleMessage.Instance.WriteMessage(text.Remove(text.Length - 1));
            
            //VOICEROIDの各種ハンドルを取得できていたら実行
            if (stpControl != IntPtr.Zero && btnSaveWave != null)
            {
                //キーワードと発話内容の比較チェック
                int index = 0;
                //csvDataは2次元Listなので、foreachで全ての要素を1次元にして比較する
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
                        while(1000L > stopwatch.ElapsedMilliseconds)
                        {
                            //音声保存ボタンがEnabledじゃない（False）ならVOICEROIDを停止   //WAVEパスが空だとバグるので回避
                            //if (!btnSaveWave.Current.IsEnabled && !string.IsNullOrWhiteSpace(ConfigManager.csvData[index][1]))
                            if (!btnSaveWave.Current.IsEnabled)
                            {
                                //VOICEROIDを停止
                                SendMessage(stpControl, 245u, IntPtr.Zero, IntPtr.Zero);
                                //WAVE再生
                                PlaySound(ConfigManager.csvData[index][1]);
                                stopwatch.Stop();
                                //関数を離脱
                                return;
                            }
                        }

                        //while を抜けた後は音声再生に失敗した場合のみ
                        YukarinetteLogger.Instance.Error("音声再生　タイムアウト");
                        stopwatch.Stop();
                        //関数を離脱
                        return;
                        
                        /*
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
                                YukarinetteLogger.Instance.Info("音声再生 成功");
                                //whileを離脱
                                break;
                            }

                            if (1000L <= stopwatch.ElapsedMilliseconds)
                            {
                                YukarinetteLogger.Instance.Error("音声再生 タイムアウト");
                                stopwatch.Stop();
                                return;
                            }
                            //音声保存ボタンがFalseでないなら0.05秒後に再度Try
                            Thread.Sleep(50);
                        }
                    
                        //foreach離脱
                        return;
                        */

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
            
            //WAVEファイルが存在するかチェック
            try
            {
                //WAVEファイルの準備
                SoundPlayer player = new SoundPlayer(@WAVEPath);

                //WAVEファイルのロード
                player.LoadAsync();

                //player.Play();   //再生終了前に次のファイルを再生する（ぶつ切りされる）
                player.PlaySync();   //再生終了後に次のファイルを再生する（読み上げも止まる）
                
                YukarinetteLogger.Instance.Info("音声再生　成功");
            }
            catch (Exception ex)
            {
                //WAVEファイルが見つからなかったらエラー文章を出力
                //例 : 指定された場所にサウンド ファイルが存在することを確認してください。　Path: C:\Users\karu\Music\VOICELOID+\東北きりたん exVOICE\あいさつ_基本語\asd.wav
                YukarinetteLogger.Instance.Error(ex.Message + "　Path: " + WAVEPath);
                YukarinetteConsoleMessage.Instance.WriteMessage(pluginName + ex.Message);
            }
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

                    //現在ハンドルを取得しているVOICEROIDと目的のVOICEROIDが一致しているか確認
                    //一致していなかったらハンドルを初期化してもう1回
                    if (!(rootHandle.Current.Name.StartsWith(voiceroidNames[ConfigData.vIndex])))
                    {
                        rootHandle = null;
                    } else {
                        YukarinetteLogger.Instance.Info(rootHandle.Current.Name + " のハンドル取得　成功");
                        return;
                    }
                }
            }
        }




    }
}