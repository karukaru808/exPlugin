using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using Yukarinette;

namespace exPlugin.Controller
{
    class V1Controller : exManager
    {
        //VOICEROIDをコントロールするのに必要
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        //停止（旧VOICEROID用）
        public IntPtr btnStp;

        //音声保存ボタンのハンドル保持用変数
        public AutomationElement btnSW;

        //初期動作
        protected override void ControllerCreate(int hWnd)
        {
            YukarinetteLogger.Instance.Info("ControllerCreate : V1");
        }

        //ボタンハンドルを取得する関数
        protected override void BtnHandleGet(AutomationElement root)
        {
            YukarinetteLogger.Instance.Info("BtnHandleGet : V1");
            //旧型VOICEROID向けハンドル取得
            AutomationElement aeStp = root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "btnStop", PropertyConditionFlags.IgnoreCase));
            btnStp = (IntPtr)aeStp.Current.NativeWindowHandle;
            btnSW = root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "btnSaveWave", PropertyConditionFlags.IgnoreCase));
        }
        //VOICEROIDの停止ボタンを押す関数
        protected override bool StopAction()
        {
            try
            {
                SendMessage(btnStp, 245u, IntPtr.Zero, IntPtr.Zero);
            }
            catch(Exception ex)
            {
                YukarinetteLogger.Instance.Error(ex.Message);
                YukarinetteLogger.Instance.Error(voiceroidNames[ConfigData.vIndex] + " の停止　失敗");
                YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + ex.Message);
                YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + voiceroidNames[ConfigData.vIndex] + "を停止できませんでした。");
                return false;
            }

            return true;
        }

        //音声保存ボタンの状態をチェックする関数
        protected override bool BtnSWCheck()
        {
            return btnSW.Current.IsEnabled;
        }

        //"VOICEROID"と名の付くプロセス一覧を取得
        protected override Process[] GetProcess()
        {
            return Process.GetProcessesByName("VOICEROID");
        }

        //リソース破棄
        protected override void Clear()
        {
            //停止ボタンのハンドル破棄
            btnStp = IntPtr.Zero;

            //音声保存ボタンのハンドル破棄
            btnSW = null;
        }


    }
}
