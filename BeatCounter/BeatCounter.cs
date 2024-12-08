using System;
using System.Windows.Forms;

namespace BeatCounter
{
    public partial class BeatCounter : Form
    {
        private IniFile mIniFile = null;

        // ターンテーブル
        private TurntableCounter mTurntableCounter = null;

        // 7 腱板
        private int iButtonNum = 7;
        private KeyCounter[] mKeyCounter = null;

        // 合計表示
        private TotalCounter mTotalCounter = null;

        // 平均表示
        private AverageCalculator mAverageCalculator = null;
        private Timer mAveTimer = null;

        // USB-HID イベント処理
        private RawInputMonitor mRawInputMonitor = null;
        private byte mLastBtnState = 0;

        public BeatCounter()
        {
            InitializeComponent();

            mIniFile = new IniFile("./counter.ini");

            InitializeTurntable();
            InitializeButtons();
            InitializeTotal();
            InitializeAverage();

            mRawInputMonitor = new RawInputMonitor(this.Handle);

            Application.ApplicationExit += new EventHandler(Application_Exit);
        }

        private void InitializeTurntable()
        {
            mTurntableCounter = new TurntableCounter(pictureBox1, label15, label16);

            try
            {
                // Config
                mTurntableCounter.TurntableStableStopThreshold
                    = mIniFile.GetValue("Config", "TURNTBL_STOP_STABLE_THRESHOLD",
                                        mTurntableCounter.TurntableStableStopThreshold);

                // Counter
                mTurntableCounter.TotalCounter
                    = mIniFile.GetValue("Counter", "TURNTBL", (uint)0);
            }
            catch
            {
                // ini ファイルが無い場合はエラー
            }
        }

        private void UpdateTurntable(byte xaxis)
        {
            mTurntableCounter.UpdateState(xaxis);
        }

        private void InitializeButtons()
        {
            Button[] buttons = { button1, button2, button3, button4, button5, button6, button7 };
            Label[] labels1 = { label1, label2, label3, label4, label5, label6, label7 };
            Label[] labels2 = { label8, label9, label10, label11, label12, label13, label14 };

            mKeyCounter = new KeyCounter[iButtonNum];
            for (int i = 0; i < iButtonNum; i++)
            {
                mKeyCounter[i] = new KeyCounter(buttons[i], labels1[i], labels2[i]);
            }

            try
            {
                for (int i = 0; i < iButtonNum; i++)
                {
                    // Config
                    mKeyCounter[i].HistoryNum
                        = mIniFile.GetValue("Config", "KEY_RELEASE_HISTORY_NUM", mKeyCounter[i].HistoryNum);
                    mKeyCounter[i].MaxReleaseTimeMS
                        = mIniFile.GetValue("Config", "KEY_RELEASE_MAX_MS", mKeyCounter[i].MaxReleaseTimeMS);

                    // Counter
                    mKeyCounter[i].TotalCounter
                        = mIniFile.GetValue("Counter", $"KEY{i + 1}", (uint)0);
                }
            }
            catch
            {
                // ini ファイルが無い場合はエラー
            }
        }

        private void UpdateButton(byte btn)
        {
            if (btn == mLastBtnState)
            {
                return;
            }

            byte diff = (byte)(mLastBtnState ^ btn);

            for (int i = 0; i < iButtonNum; i++)
            {
                if ((diff & (1 << i)) != 0)
                {
                    mKeyCounter[i].UpdateState(((btn & (1 << i)) != 0) ? 1 : 0);
                }
            }

            mLastBtnState = btn;
        }

        private void InitializeTotal()
        {
            mTotalCounter = new TotalCounter(label19, label20, mTurntableCounter, mKeyCounter);
        }

        private void UpdateTotal(byte turn, byte key)
        {
            mTotalCounter.UpdateState(turn, key);
        }

        private void InitializeAverage()
        {
            mAverageCalculator = new AverageCalculator(label23, label28, label26, label32, mKeyCounter);

            try
            {
                mAverageCalculator.HistoryNum
                    = mIniFile.GetValue("Config", "KEYSTROKE_HISTORY_NUM", mAverageCalculator.HistoryNum);
                mAverageCalculator.ResetMaxKeystrokeSec
                    = mIniFile.GetValue("Config", "KEYSTROKE_MAX_RESET_SEC", mAverageCalculator.ResetMaxKeystrokeSec);
            }
            catch
            {
                // ini ファイルが無い場合はエラー
            }

            mAveTimer = new Timer();
            mAveTimer.Tick += new EventHandler(AverageTimer_Fired);
            mAveTimer.Interval = 1000;
            mAveTimer.Start();
        }

        private void AverageTimer_Fired(object sender, EventArgs e)
        {
            mAverageCalculator.UpdateAverage();
        }

        public void Application_Exit(object sender, EventArgs e)
        {
            Application.ApplicationExit -= Application_Exit;

            // Config
            mIniFile["Config", "TURNTBL_STOP_STABLE_THRESHOLD"]
                = mTurntableCounter.TurntableStableStopThreshold.ToString();
            mIniFile["Config", "KEY_RELEASE_HISTORY_NUM"]
                = mKeyCounter[0].HistoryNum.ToString();
            mIniFile["Config", "KEY_RELEASE_MAX_MS"]
                = mKeyCounter[0].MaxReleaseTimeMS.ToString();
            mIniFile["Config", "KEYSTROKE_HISTORY_NUM"]
                = mAverageCalculator.HistoryNum.ToString();
            mIniFile["Config", "KEYSTROKE_MAX_RESET_SEC"]
                = mAverageCalculator.ResetMaxKeystrokeSec.ToString();

            // Counter
            for (int i = 0; i < iButtonNum; i++)
            {
                mIniFile["Counter", $"KEY{i + 1}"]
                    = mKeyCounter[i].TotalCounter.ToString();
            }
            mIniFile["Counter", "TURNTBL"]
                = mTurntableCounter.TotalCounter.ToString();
            mIniFile = null;
        }

        protected override void WndProc(ref Message m)
        {
            if (mRawInputMonitor != null)
            {
                byte[] data = mRawInputMonitor.OnMessage(ref m);
                if (data != null)
                {
                    UpdateTurntable(data[1]);
                    UpdateButton(data[3]);
                    UpdateTotal(data[1], data[3]);
                    data = null;
                }
            }
            base.WndProc(ref m);
        }
    }
}
