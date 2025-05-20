using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace VK_UI3
{
    public class TrayIconManager : IDisposable
    {
        private const uint NIM_ADD = 0x00000000;
        private const uint NIM_DELETE = 0x00000002;
        private const uint NIF_MESSAGE = 0x00000001;
        private const uint NIF_ICON = 0x00000002;
        private const uint NIF_TIP = 0x00000004;
        private const uint WM_APP = 0x8000;
        private const uint WM_TRAYICON = WM_APP + 1;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONUP = 0x0205;

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, [In] ref NOTIFYICONDATA lpData);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NOTIFYICONDATA
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uID;
            public uint uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
        }

        private readonly IntPtr _windowHandle;
        private readonly MainWindow _window;
        private readonly uint _iconId = 1;
        private bool _iconAdded;

        private delegate IntPtr SubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData);
        private SubclassProc _subclassProc;

        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, UIntPtr uIdSubclass, IntPtr dwRefData);

        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern bool RemoveWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, UIntPtr uIdSubclass);

        [DllImport("comctl32.dll", SetLastError = true)]
        private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        private bool _disposed = false;

        public TrayIconManager(IntPtr windowHandle, MainWindow window)
        {
            _windowHandle = windowHandle;
            _window = window;
            _subclassProc = new SubclassProc(WindowSubclassProc);

            if (!SetWindowSubclass(_windowHandle, _subclassProc, UIntPtr.Zero, IntPtr.Zero))
            {
                throw new Exception("Не удалось установить подкласс окна.");
            }

            AddTrayIcon();
        }

        public void ShowTrayIconAgain()
        {
            if (_iconAdded)
                return;

            AddTrayIcon();
        }


        public void HideTrayIcon()
        {
            RemoveTrayIcon();
        }

        private IntPtr _trayIconHandle = IntPtr.Zero;

        private void AddTrayIcon()
        {
            if (_trayIconHandle != IntPtr.Zero)
                return;

            var nid = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                hWnd = _windowHandle,
                uID = _iconId,
                uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
                uCallbackMessage = WM_TRAYICON,
                szTip = "VK M"
            };

            string iconPath = Path.Combine(AppContext.BaseDirectory, "icon.ico");
            using (var icon = new Icon(iconPath))
            {
                _trayIconHandle = icon.Handle; // Сохраняем дескриптор
                nid.hIcon = _trayIconHandle;
                _iconAdded = Shell_NotifyIcon(NIM_ADD, ref nid);
            }
        }

        private void RemoveTrayIcon()
        {
            if (!_iconAdded)
                return;

            var nid = new NOTIFYICONDATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(NOTIFYICONDATA)),
                hWnd = _windowHandle,
                uID = _iconId
            };

            Shell_NotifyIcon(NIM_DELETE, ref nid);
            if (_trayIconHandle != IntPtr.Zero)
            {
                DestroyIcon(_trayIconHandle); // Освобождаем дескриптор
                _trayIconHandle = IntPtr.Zero;
            }
            _iconAdded = false;
        }

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private IntPtr WindowSubclassProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData)
        {
            try
            {
                if (!IsWindow(hWnd))
                {
                    Debug.WriteLine("Ошибка: Окно больше не существует.");
                    return IntPtr.Zero;
                }

                Debug.WriteLine($"WindowSubclassProc: msg={msg}, wParam={wParam}, lParam={lParam}");

                if (msg == WM_TRAYICON)
                {
                    int eventCode = lParam.ToInt32();
                    Debug.WriteLine($"WM_TRAYICON received: eventCode={eventCode}");

                    if (eventCode == WM_LBUTTONUP)
                    {
                        _window?.DispatcherQueue?.TryEnqueue(() =>
                        {
                            if (_window != null && IsWindow(_windowHandle))
                            {
                                _window.ShowWindowAgain();
                            }
                            else
                            {
                                Debug.WriteLine("Ошибка: Окно недоступно.");
                            }
                        });
                    }
                    else if (eventCode == WM_RBUTTONUP)
                    {
                        ShowContextMenu();
                    }
                }

                if (!IsWindow(hWnd))
                {
                    Debug.WriteLine("Ошибка: Окно уничтожено во время обработки.");
                    return IntPtr.Zero;
                }

                return DefSubclassProc(hWnd, msg, wParam, lParam);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в WindowSubclassProc: {ex.Message}");
                return IntPtr.Zero;
            }
        }


        private const uint TPM_LEFTALIGN = 0x0000;
        private const uint TPM_RETURNCMD = 0x0100;

        private void ShowContextMenu()
        {
            IntPtr hMenu = CreatePopupMenu();
            try
            {
                AppendMenu(hMenu, 0, 1, "Закрыть");

                POINT cursorPos;
                GetCursorPos(out cursorPos);
                SetForegroundWindow(_windowHandle);
                int command = TrackPopupMenu(hMenu, TPM_LEFTALIGN | TPM_RETURNCMD, cursorPos.X, cursorPos.Y, 0, _windowHandle, IntPtr.Zero);

                if (command == 1)
                {
                    _window.justClose = true;
                    _window.DispatcherQueue.TryEnqueue(() => _window.Close());
                }
            }
            finally
            {
                DestroyMenu(hMenu); // Гарантированное освобождение
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll")]
        private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Освобождаем управляемые ресурсы
                    RemoveTrayIcon();
                }

                // Освобождаем неуправляемые ресурсы
                if (_trayIconHandle != IntPtr.Zero)
                {
                    DestroyIcon(_trayIconHandle);
                    _trayIconHandle = IntPtr.Zero;
                }

                RemoveWindowSubclass(_windowHandle, _subclassProc, UIntPtr.Zero);
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~TrayIconManager()
        {
            Dispose(false);
        }
    }
}
