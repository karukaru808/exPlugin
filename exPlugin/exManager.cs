using System;
using System.Collections.Generic;
using System.Media;
using Yukarinette;

namespace exPlugin
{
    // プラグインごとの動作
    public class exManager
    {
        public void Create()
        {
            // 音声認識開始時の初期化とか
        }

        public void Dispose()
        {
            // 音声認識終了時のリソース破棄とか
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
                    //止めてから再生（いらない？）
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

        //VOICEROIDを停止する
        public virtual void Dispose()
        {
            YukarinetteLogger.Instance.Debug("start.");
            if (this.mMonitoringTask != null)
            {
                this.mMonitoring = false;
                this.mMonitoringTask.Wait();
                this.mMonitoringTask = null;
            }
            if (this.mVoiceroidProcess != null)
            {
                try
                {
                    this.mVoiceroidProcess.Kill();
                }
                catch (Exception message)
                {
                    YukarinetteLogger.Instance.Error(message);
                }
                this.mVoiceroidProcess.Dispose();
                this.mVoiceroidProcess = null;
            }
            YukarinetteLogger.Instance.Info("dispose ok.");
            YukarinetteLogger.Instance.Debug("end.");
        }
    }
}