using System.Drawing;
using System.Windows.Forms;

namespace BeatCounter
{
    internal class TurntableCounter
    {
        private PictureBox mPicBox = null;
        private Label mLabelCounter = null;
        private Label mLabelTotalCounter = null;

        private Bitmap mBitmap = null;
        private Color mBgColor = SystemColors.Control;
        private Color mCircleColor = Color.Black;
        private int mCircleWidth = 3;
        private Brush STATE_ON = Brushes.Red;
        private Brush STATE_OFF = Brushes.White;

        private uint mTurntableCounter = 0;
        private uint mTotalTurntableCounter = 0;

        private byte mLastXAxis = 0;
        private int mLastDirection = 0;
        private int mState = 0;
        private uint mStableCount = 0;
        private uint mStableStopThreshold = 80;

        public uint TurntableStableStopThreshold
        {
            get
            {
                return mStableStopThreshold;
            }
            set
            {
                mStableStopThreshold = value;
            }
        }

        public uint TotalCounter
        {
            get
            {
                return mTotalTurntableCounter;
            }
            set
            {
                mTotalTurntableCounter = value;
                UpdateCounter();
            }
        }

        public uint Counter
        {
            get { return mTurntableCounter; }
        }

        public TurntableCounter(PictureBox pic, Label cnt, Label tcnt)
        {
            mPicBox = pic;
            mLabelCounter = cnt;
            mLabelTotalCounter = tcnt;

            mBitmap = new Bitmap(pic.Width, pic.Height);
            UpdateBitmap();
            UpdateCounter();
        }

        public void UpdateState(byte xaxis)
        {
            // 状態変化がない場合
            if (mLastXAxis == xaxis)
            {
                mStableCount++;
                if (mStableCount >= mStableStopThreshold)
                {
                    // 回転→停止
                    if (mState != 0)
                    {
                        mState = 0;
                        UpdateBitmap();
                    }
                    mStableCount = 0;
                }
                return;
            }

            // 状態に変化がある場合
            mStableCount = 0;

            // 差分から回転方向を判定（0:反時計回り, 1:時計回り）
            int diff = (xaxis - mLastXAxis + (byte.MaxValue + 1)) % (byte.MaxValue + 1);
            int direction = (diff > 0 && diff <= 127) ? 0 : 1;

            if ((mState == 0)
                || (direction != mLastDirection))
            {
                // 停止→回転
                if (mState == 0)
                {
                    mState = 1;
                    UpdateBitmap();
                }
                
                // 停止→回転 または 回転方向変化
                mTurntableCounter++;
                mTotalTurntableCounter++;
                UpdateCounter();
            }

            mLastXAxis = xaxis;
            mLastDirection = direction;
        }

        private void UpdateBitmap()
        {
            int circle_size = (int)mPicBox.Width - (mCircleWidth * 2);
            int small_circle_size = circle_size / 2;

            using (Graphics g = Graphics.FromImage(mBitmap))
            {
                g.Clear(mBgColor);

                // 大きい円
                g.FillEllipse(
                    ((mState != 0) ? STATE_ON : STATE_OFF),
                    mCircleWidth, mCircleWidth, circle_size, circle_size);

                // 円の縁
                Pen pen = new Pen(mCircleColor, mCircleWidth);
                g.DrawEllipse(
                    pen, mCircleWidth, mCircleWidth, circle_size, circle_size);
                pen.Dispose();

                // 中心の小さい円
                g.FillEllipse(
                    Brushes.Black,
                    mCircleWidth + small_circle_size / 2,
                    mCircleWidth + small_circle_size / 2,
                    small_circle_size,
                    small_circle_size);

            }
            mPicBox.Image = mBitmap;
            mPicBox.Invalidate();
        }

        private void UpdateCounter()
        {
            mLabelCounter.Text = mTurntableCounter.ToString();
            mLabelTotalCounter.Text = mTotalTurntableCounter.ToString();
        }
    }
}
