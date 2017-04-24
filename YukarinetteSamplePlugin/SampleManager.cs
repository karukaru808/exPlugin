using System;
using Yukarinette;
using YukarinetteSamplePlugin;

namespace YukarinetteSamplePlugin
{
    // プラグインごとの動作
    public class SampleManager
    {
        static string WAVEPath;

        public void Create(string sampleSetting)
        {
            // 音声認識開始時の初期化とか
            //sampleSettingにあるWAVEファイルのPathを取得する
            WAVEPath = sampleSetting;
        }

        public void Dispose()
        {
            // 音声認識終了時のリソース破棄とか
        }

        public void Speech(string text)
        {
            // 音声認識時のメイン処理
            // textに認識した本文が入っている

            //ここからWAVE Plugin

            ////////////////////////////////////////////////////////////////////////////
            //本来はここで登録したキーワードと認識した文章が合致しているかチェックする//
            ////////////////////////////////////////////////////////////////////////////

            //デバッグ用文章
            YukarinetteConsoleMessage.Instance.WriteMessage(WAVEPath);

            //止めてから再生（いらない？）
            //StopSound();
            PlaySound();
        }

        //以下WAVEファイルを再生するためのプログラム
        //初期化（？）
        private System.Media.SoundPlayer player = null;

        //再生用
        private void PlaySound()
        {
            //WAVEファイルの準備
            player = new System.Media.SoundPlayer(@WAVEPath);

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
        private void StopSound()
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }
        }

    }
}