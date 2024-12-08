using System.Windows.Forms;

namespace BeatCounter
{
    internal class TotalCounter
    {
        private Label mLabelToday = null;
        private Label mLabelTotal = null;

        private KeyCounter[] mKeyCounters = null;
        private TurntableCounter mTurntableCounter = null;

        private byte mLastTurnState = 0;
        private byte mLastKeyState = 0;

        public TotalCounter(Label today, Label total, TurntableCounter tbl, KeyCounter[] keys)
        {
            mLabelToday = today;
            mLabelTotal = total;
            mKeyCounters = keys;
            mTurntableCounter = tbl;
            UpdateTotalCounter();
        }

        private void UpdateTotalCounter()
        {
            uint today = 0;
            uint total = 0;

            for (int i = 0; i < mKeyCounters.Length; i++)
            {
                today += mKeyCounters[i].Counter;
                total += mKeyCounters[i].TotalCounter;
            }
            today += mTurntableCounter.Counter;
            total += mTurntableCounter.TotalCounter;

            mLabelToday.Text = today.ToString();
            mLabelTotal.Text = total.ToString();
        }

        public void UpdateState(byte turn, byte key)
        {
            if ((mLastTurnState != turn)
                || (mLastKeyState != key))
            {
                UpdateTotalCounter();
            }

            mLastTurnState = turn;
            mLastKeyState = key;
        }
    }
}
