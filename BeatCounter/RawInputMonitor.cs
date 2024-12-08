using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BeatCounter
{
    internal class RawInputMonitor
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterRawInputDevices(
            RAWINPUTDEVICE[] pRawInputDevices,
            uint uiNumDevices,
            uint cbSize);

        [DllImport("user32.dll")]
        private static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader);
        private const uint RID_INPUT = 0x10000003;

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }
        private const uint RIM_TYPEMOUSE = 0;
        private const uint RIM_TYPEKEYBOARD = 1;
        private const uint RIM_TYPEHID = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWHID
        {
            public uint dwSizeHid;
            public uint dwCount;
            public IntPtr bRawData;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUT
        {
            public RAWINPUTHEADER Header;
            public RAWHID Hid;
        }

        private void RegisterGamepad(IntPtr hwnd)
        {
            RAWINPUTDEVICE[] devices = new RAWINPUTDEVICE[1];
            devices[0].usUsagePage = 0x01;   // Generic Desktop Controls
            devices[0].usUsage = 0x04;       // Joystick
            devices[0].dwFlags = 0x00000100; // RIDEV_INPUTSINKフラグを指定
            devices[0].hwndTarget = hwnd;

            if (!RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new ApplicationException($"Raw Inputデバイスの登録に失敗しました。エラーコード：{errorCode}");
            }
        }

        private byte[] ReadHidData(ref Message m)
        {
            uint rawInputSize = 0;
            uint rawInputHeaderSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();

            if (GetRawInputData(
                m.LParam,
                RID_INPUT,
                IntPtr.Zero,
                ref rawInputSize,
                rawInputHeaderSize) != 0)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new ApplicationException($"GetRawInputDataサイズ取得失敗。エラーコード：{errorCode}");
            }

            RAWINPUT rawInput;
            byte[] rawHidRawData;

            IntPtr rawInputPointer = IntPtr.Zero;
            try
            {
                rawInputPointer = Marshal.AllocHGlobal((int)rawInputSize);

                if (GetRawInputData(
                    m.LParam,
                    RID_INPUT,
                    rawInputPointer,
                    ref rawInputSize,
                    rawInputHeaderSize) != rawInputSize)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new ApplicationException($"GetRawInputDataデータ取得失敗。エラーコード：{errorCode}");
                }

                rawInput = Marshal.PtrToStructure<RAWINPUT>(rawInputPointer);

                if (rawInput.Header.dwType != RIM_TYPEHID)
                {
                    return null;
                }

                var rawInputData = new byte[rawInputSize];
                Marshal.Copy(rawInputPointer, rawInputData, 0, rawInputData.Length);

                uint dataSize = rawInput.Hid.dwSizeHid * rawInput.Hid.dwCount;
                if (dataSize != 6)
                {
                    return null;
                }

                rawHidRawData = new byte[dataSize];
                int rawInputOffset = (int)rawInputSize - rawHidRawData.Length;
                Buffer.BlockCopy(rawInputData, rawInputOffset, rawHidRawData, 0, rawHidRawData.Length);

                return rawHidRawData;
            }
            finally
            {
                Marshal.FreeHGlobal(rawInputPointer);
            }
        }

        public RawInputMonitor(IntPtr hwnd)
        {
            RegisterGamepad(hwnd);
        }

        public byte[] OnMessage(ref Message m)
        {
            const int WM_INPUT = 0x00FF;

            if (m.Msg != WM_INPUT)
            {
                return null;
            }

            return ReadHidData(ref m);
        }
    }
}
