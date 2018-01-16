using Accessibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using Yukarinette;

namespace exPlugin.Controller
{
    public class V2Controller : exManager
    {
        //停止（VOICEROID2用）
        public V2Controller btnStp;

        //音声保存ボタンのハンドル保持用変数
        public V2Controller btnSW;

        //先頭送りボタン？：デバッグ用
        //public V2Controller btnWC;

        //ハンドル一時保持用変数
        private IAccessible Accessible;

        //ウィンドウハンドル取得
        [DllImport("oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint dwObjectID, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] [In] [Out] ref object ppvObject);

        //子ウィンドウ抽出
        [DllImport("oleacc.dll")]
        private static extern int AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] [In] [Out] object[] rgvarChildren, out int pcObtained);

        //ボタンハンドル取得
        public V2Controller[] Children
        {
            get
            {
                int accChildCount = Accessible.accChildCount;
                object[] objects = new object[accChildCount];

                AccessibleChildren(Accessible, 0, accChildCount, objects, out accChildCount);
                
                List<V2Controller> list = new List<V2Controller>();

                foreach (object obj in objects)
                {
                    list.Add(new V2Controller((IAccessible)obj));
                }

                return list.ToArray();
            }
        }

        //再帰呼び出し用に準備
        private V2Controller(IAccessible accessible)
        {
            Accessible = accessible;
        }

        public V2Controller()
        {
        }

        //初期動作
        protected override void ControllerCreate(int hWnd)
        {
            YukarinetteLogger.Instance.Info("ControllerCreate : V2");

            IntPtr intPtr = new IntPtr(hWnd);
            Guid guid = new Guid("{618736e0-3c3d-11cf-810c-00aa00389b71}");
            object obj = null;
            AccessibleObjectFromWindow(intPtr, 0u, ref guid, ref obj);

            Accessible = (IAccessible)obj;
        }

        //VOICEROIDの停止ボタンを押す関数
        protected override bool StopAction()
        {
            try
            {
                //btnStp.Accessible.accSelect(1, 0);
                btnStp.Accessible.accDoDefaultAction(0);
            }
            catch (Exception ex)
            {
                YukarinetteLogger.Instance.Error(ex.Message);
                YukarinetteLogger.Instance.Error(voiceroidNames[ConfigData.vIndex] + " の停止　失敗");
                YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + ex.Message);
                YukarinetteConsoleMessage.Instance.WriteMessage(exPlugin.ConsoleName + voiceroidNames[ConfigData.vIndex] + "を停止できませんでした。");
                return false;
            }

            return true;
        }

        //ボタンハンドルを取得する関数
        protected override void BtnHandleGet(AutomationElement root)
        {
            YukarinetteLogger.Instance.Info("BtnHandleGet : V2");
            
            btnStp = Children[3].Children[3].Children[2];
            btnSW = Children[3].Children[3].Children[5];

            //デバッグ用
            //btnWC = Children[3].Children[3].Children[3];
        }

        //音声保存ボタンの状態をチェックする関数
        protected override bool BtnSWCheck()
        {
            object btnSWObje = btnSW.Accessible.get_accState(0);
            //object btnWCObje = btnWC.Accessible.get_accState(0);

            /*
            YukarinetteConsoleMessage.Instance.WriteMessage("btnSW : "+btnSWObje.GetType().ToString());
            YukarinetteConsoleMessage.Instance.WriteMessage("btnSW : " + btnSWObje.ToString());
            YukarinetteConsoleMessage.Instance.WriteMessage("btnWC : "+btnWCObje.GetType().ToString());
            YukarinetteConsoleMessage.Instance.WriteMessage("btnWC : " + btnWCObje.ToString());
            */

            //1はボタン押せない状態
            if ((int)btnSWObje == 1)
            {
                return false;
            }

            return true;
        }

        //"VoiceroidEditor"と名の付くプロセス一覧を取得
        protected override Process[] GetProcess()
        {
            return Process.GetProcessesByName("VoiceroidEditor");
        }

        //リソース破棄
        protected override void Clear()
        {
            //停止ボタンのハンドル破棄
            btnStp = null;

            //音声保存ボタンのハンドル破棄
            btnSW = null;
        }


    }
}