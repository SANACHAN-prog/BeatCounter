using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BeatCounter
{
    internal class AverageCalculator
    {
        private Label mLabelAveKeystroke = null;
        private Label mLabelMaxKeystroke = null;
        private Label mLabelAveRelease = null;
        private Label mLabelMinRelease = null;

        private KeyCounter[] mKeyCounters = null;
        private uint mLastTotalCount = 0;
        private List<int> mCountDiffs = new List<int>();
        private int mMaxDiffNum = 5;

        private int mLastMaxKeystroke = 0;
        private int mResetMaxKeystrokeCnt = 0;
        private int mResetMaxKeystrokeSec = 15;
        private int mLastMinRelease = 0;

        public int HistoryNum
        {
            get
            {
                return mMaxDiffNum;
            }
            set
            {
                mMaxDiffNum = value;
            }
        }

        public int ResetMaxKeystrokeSec
        {
            get
            {
                return mResetMaxKeystrokeSec;
            }
            set
            {
                mResetMaxKeystrokeSec = value;
            }
        }

        public AverageCalculator(Label keystrk, Label maxkey, Label release, Label minrel, KeyCounter[] keys)
        {
            mLabelAveKeystroke = keystrk;
            mLabelMaxKeystroke = maxkey;
            mLabelAveRelease = release;
            mLabelMinRelease = minrel;
            mKeyCounters = keys;
            UpdateAverage();
        }

        public void UpdateAverage()
        {
            uint total_cnt = 0;
            uint total_rel = 0;
            for (int i = 0; i < mKeyCounters.Length; i++)
            {
                total_cnt += mKeyCounters[i].Counter;
                total_rel += mKeyCounters[i].AveRelease;
            }

            // 平均打鍵数
            int diff = 0;
            int ave = 0;
            if (mLastTotalCount != 0)
            {
                diff = (int)total_cnt - (int)mLastTotalCount;
            }
            mCountDiffs.Add(diff);
            if (mCountDiffs.Count > mMaxDiffNum)
            {
                mCountDiffs.RemoveAt(0);
            }
            ave = (int)(mCountDiffs.Average() + 0.5);
            mLabelAveKeystroke.Text = ave.ToString();

            // 最大平均打鍵数
            if (ave == 0)
            {
                mResetMaxKeystrokeCnt++;
                if (mResetMaxKeystrokeCnt >= mResetMaxKeystrokeSec)
                {
                    mResetMaxKeystrokeCnt = 0;
                    mLastMaxKeystroke = -1;     // 0 で描画を更新させる
                }
            }
            else
            {
                mResetMaxKeystrokeCnt = 0;
            }
            if ((ave > mLastMaxKeystroke)
                || (mLastTotalCount == 0))
            {
                mLastMaxKeystroke = ave;
                mLabelMaxKeystroke.Text = ave.ToString();
            }

            mLastTotalCount = total_cnt;

            // 平均リリース時間
            int ave_rel = (int)total_rel / (int)mKeyCounters.Length;
            mLabelAveRelease.Text = ave_rel.ToString();

            // 最小リリース時間
            if (mLastMinRelease == 0)
            {
                // 初期化
                mLabelMinRelease.Text = mLastMinRelease.ToString();
            }

            if (((ave_rel != 0) && (mLastMinRelease == 0))
                || (ave_rel < mLastMinRelease))
            {
                mLastMinRelease = ave_rel;
                mLabelMinRelease.Text = ave_rel.ToString();
            }
        }
    }
}
