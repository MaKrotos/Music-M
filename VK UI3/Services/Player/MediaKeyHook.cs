using System;
using System.Runtime.InteropServices;

namespace VK_UI3.Services.Player
{
    #region Media Key Hook Classes

    public class MediaKeyHook : IDisposable
    {
        private readonly IntPtr _hwnd;
        private readonly IntPtr _originalWndProc;
        private readonly WndProcDelegate _wndProc;
        private bool _disposed = false;
        private readonly int _gwlWndProc;

        public event EventHandler<MediaKeyEventArgs> MediaKeyPressed;
        public event EventHandler<VolumeKeyEventArgs> VolumeKeyPressed;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public MediaKeyHook(IntPtr hwnd, int gwlWndProc)
        {
            _hwnd = hwnd;
            _gwlWndProc = gwlWndProc;
            _wndProc = WndProc;

            // Устанавливаем свой обработчик сообщений
            _originalWndProc = SetWindowLongPtr(
                _hwnd,
                _gwlWndProc,
                Marshal.GetFunctionPointerForDelegate(_wndProc)
            );

            // Удерживаем делегат в памяти, чтобы не было сборки мусора
            GC.KeepAlive(_wndProc);
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            const int WM_APPCOMMAND = 0x0319;

            if (msg == WM_APPCOMMAND)
            {
                // Из старого кода: int cmd = HIWORD(wParam) & ~APPCOMMAND_MASK;
                // HIWORD - получить старшее слово (16-31 биты)
                const int APPCOMMAND_MASK = 0xF000;
                int cmd = (int)((wParam.ToInt64() >> 16) & 0xFFFF) & ~APPCOMMAND_MASK;

                // Для отладки
                System.Diagnostics.Debug.WriteLine($"APPCOMMAND received: cmd={cmd}");

                switch (cmd)
                {
                    case 8:  // APPCOMMAND_VOLUME_MUTE
                        VolumeKeyPressed?.Invoke(this, new VolumeKeyEventArgs(VolumeKey.VolumeMute));
                        return IntPtr.Zero;
                    case 9:  // APPCOMMAND_VOLUME_DOWN
                        VolumeKeyPressed?.Invoke(this, new VolumeKeyEventArgs(VolumeKey.VolumeDown));
                        return IntPtr.Zero;
                    case 10: // APPCOMMAND_VOLUME_UP
                        VolumeKeyPressed?.Invoke(this, new VolumeKeyEventArgs(VolumeKey.VolumeUp));
                        return IntPtr.Zero;
                    case 46: // APPCOMMAND_MEDIA_PLAY_PAUSE
                    case 47: // APPCOMMAND_MEDIA_PLAY
                    case 48: // APPCOMMAND_MEDIA_PAUSE
                        MediaKeyPressed?.Invoke(this, new MediaKeyEventArgs(MediaKey.PlayPause));
                        return IntPtr.Zero;
                    case 11: // APPCOMMAND_MEDIA_NEXTTRACK
                        MediaKeyPressed?.Invoke(this, new MediaKeyEventArgs(MediaKey.Next));
                        return IntPtr.Zero;
                    case 12: // APPCOMMAND_MEDIA_PREVIOUSTRACK
                        MediaKeyPressed?.Invoke(this, new MediaKeyEventArgs(MediaKey.Previous));
                        return IntPtr.Zero;
                    case 13: // APPCOMMAND_MEDIA_STOP
                        MediaKeyPressed?.Invoke(this, new MediaKeyEventArgs(MediaKey.Stop));
                        return IntPtr.Zero;
                }
            }

            // Вызываем оригинальный обработчик оконных сообщений
            return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Восстанавливаем оригинальный обработчик оконных сообщений
                    if (_hwnd != IntPtr.Zero && _originalWndProc != IntPtr.Zero)
                    {
                        SetWindowLongPtr(_hwnd, _gwlWndProc, _originalWndProc);
                    }
                }

                _disposed = true;
            }
        }

        ~MediaKeyHook()
        {
            Dispose(false);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }

    #endregion
}