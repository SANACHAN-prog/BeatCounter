using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BeatCounter
{
    internal class KeyCounter
    {
        private Color STATE_ON = Color.Red;
        private Color STATE_OFF = Color.White;

        private Button mBtnKey = null;
        private Label mLabelCounter = null;
        private Label mLabelTotalCounter = null;

        private int mState = 0;
        private uint mKeyCnt = 0;
        private uint mTotalKeyCnt = 0;

        private List<uint> mReleaseTimes = new List<uint>();
        private Stopwatch mStopwatch = new Stopwatch();
        private int mHistoryNum = 5;
        private int mMaxReleaseTimeMS = 200;

        public int HistoryNum
        {
            get
            {
                return mHistoryNum;
            }
            set
            {
                mHistoryNum = value;
            }
        }

        public int MaxReleaseTimeMS
        {
            get
            {
                return mMaxReleaseTimeMS;
            }
            set
            {
                mMaxReleaseTimeMS = value;
            }
        }

        public uint TotalCounter
        {
            get
            {
                return mTotalKeyCnt;
            }
            set
            {
                mTotalKeyCnt = value;
                UpdateCounter();
            }
        }

        public uint Counter
        {
            get
            {
                return mKeyCnt;
            }
        }

        public uint AveRelease
        {
            get
            {
                if (mReleaseTimes.Count == 0)
                {
                    return 0;
                }

                uint total = 0;
                for (int i = 0; i < mReleaseTimes.Count; i++)
                {
                    total += mReleaseTimes[i];
                }
                return total / (uint)mReleaseTimes.Count;
            }
        }

        public KeyCounter(Button btn, Label cnt, Label tcnt)
        {
            mBtnKey = btn;
            mLabelCounter = cnt;
            mLabelTotalCounter = tcnt;
            UpdateCounter();
        }

        public void UpdateState(int state)
        {
            if (mState == state)
            {
                return;
            }

            if (state != 0)
            {
                mBtnKey.BackColor = STATE_ON;
                mKeyCnt++;
                mTotalKeyCnt++;
                UpdateCounter();

                if (mStopwatch.IsRunning)
                {
                    mStopwatch.Stop();
                }
                mStopwatch.Reset();
                mStopwatch.Start();
            }
            else
            {
                mStopwatch.Stop();
                uint elapsedMs = (uint)(mStopwatch.Elapsed.TotalMilliseconds);
                if (elapsedMs > 0 && elapsedMs <= mMaxReleaseTimeMS)
                {
                    mReleaseTimes.Add(elapsedMs);
                    if (mReleaseTimes.Count > mHistoryNum)
                    {
                        mReleaseTimes.RemoveAt(0);
                    }
                }
                mBtnKey.BackColor = STATE_OFF;
            }

            mState = state;
        }

        private void UpdateCounter()
        {
            mLabelCounter.Text = mKeyCnt.ToString();
            mLabelTotalCounter.Text = mTotalKeyCnt.ToString();
        }
    }
}
