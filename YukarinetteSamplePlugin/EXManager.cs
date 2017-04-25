using System;
using System.Media;
using Yukarinette;

namespace exPlugin
{
    // プラグインごとの動作
    public class exManager
    {
        static string csvPath;

        public void Create(string PathString)
        {
            // 音声認識開始時の初期化とか
            //CSVファイルのPathを取得する
            csvPath = PathString;
        }

        public void Dispose()
        {
            // 音声認識終了時のリソース破棄とか
        }

        public void Speech(string text)
        {
            // 音声認識時のメイン処理
            // textに認識した本文が入っている

            //ここから exPlugin

            ////////////////////////////////////////////////////////////////////////////
            //本来はここで登録したキーワードと認識した文章が合致しているかチェックする//
            ////////////////////////////////////////////////////////////////////////////

            //デバッグ用文章
            YukarinetteConsoleMessage.Instance.WriteMessage(csvPath);
            YukarinetteConsoleMessage.Instance.WriteMessage(text);

            //止めてから再生（いらない？）
            //StopSound();
            PlaySound();
        }

        //以下WAVEファイルを再生するためのプログラム
        //再生用
        private void PlaySound()
        {
            //WAVEファイルの準備
            SoundPlayer player = new SoundPlayer(@csvPath);

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

    }
}