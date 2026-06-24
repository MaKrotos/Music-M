using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VK_UI3.DB;

namespace VK_UI3.Helpers
{
    internal static class ErrorReportHelper
    {
        private static readonly object _lock = new object();
        private static bool _isDialogShowing = false;
        private static IntPtr _currentDialogHwnd = IntPtr.Zero;

        /// <summary>
        /// Собирает полный отчёт об ошибке в форматированную строку.
        /// </summary>
        public static string BuildFullReport(Exception exception, string source = "Unknown")
        {
            var sb = new StringBuilder();
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}.{assemblyVersion.Revision}";

            sb.AppendLine("========================================");
            sb.AppendLine("  КРИТИЧЕСКАЯ ОШИБКА ПРИЛОЖЕНИЯ VK M");
            sb.AppendLine("========================================");
            sb.AppendLine();
            sb.AppendLine($"Время:             {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Версия приложения: {version}");
            sb.AppendLine($"Источник:          {source}");
            sb.AppendLine();
            sb.AppendLine("--- Исключение ---");
            sb.AppendLine($"Тип:     {exception.GetType().FullName}");
            sb.AppendLine($"Сообщение: {exception.Message}");
            sb.AppendLine();
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(exception.StackTrace);

            if (exception.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("--- Inner Exception ---");
                sb.AppendLine($"Тип:     {exception.InnerException.GetType().FullName}");
                sb.AppendLine($"Сообщение: {exception.InnerException.Message}");
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(exception.InnerException.StackTrace);
            }

            sb.AppendLine();
            sb.AppendLine("--- Системная информация ---");
            sb.AppendLine($"OS:                 {RuntimeInformation.OSDescription}");
            sb.AppendLine($"OS Architecture:    {RuntimeInformation.OSArchitecture}");
            sb.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
            sb.AppendLine($"CLR Version:        {Environment.Version}");

            // Информация о пользователе (с защитой от повторных ошибок)
            try
            {
                var setting = SettingsTable.GetSetting("UserUniqID");
                if (setting != null)
                    sb.AppendLine($"UserUniqID:         {setting.settingValue}");

                var accounts = AccountsDB.GetAllAccounts();
                sb.AppendLine($"Количество аккаунтов: {accounts.Count}");

                if (accounts.Count > 0)
                {
                    var activeAccount = AccountsDB.GetActiveAccount();
                    if (activeAccount != null)
                        sb.AppendLine($"Активный аккаунт:   {activeAccount.GetHash()}");
                }
            }
            catch
            {
                sb.AppendLine("(Не удалось получить информацию о пользователе)");
            }

            sb.AppendLine();
            sb.AppendLine("========================================");
            sb.AppendLine("  Конец отчёта");
            sb.AppendLine("========================================");

            return sb.ToString();
        }

        /// <summary>
        /// Показывает нативное диалоговое окно с полным отчётом об ошибке.
        /// Запускается в отдельном STA-потоке, не блокируя UI приложения.
        /// </summary>
        public static void ShowErrorDialog(string report)
        {
            lock (_lock)
            {
                // Если диалог уже показан, обновляем его содержимое или игнорируем
                if (_isDialogShowing)
                {
                    // Можно обновить содержимое существующего окна
                    if (_currentDialogHwnd != IntPtr.Zero)
                    {
                        UpdateDialogContent(_currentDialogHwnd, report);
                    }
                    return;
                }

                _isDialogShowing = true;
            }

            try
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        ShowNativeErrorDialog(report);
                    }
                    catch (Exception ex)
                    {
                        // Если не удалось показать диалог, пробуем через MessageBox
                        try
                        {
                            MessageBoxW(IntPtr.Zero,
                                $"Ошибка при показе диалога ошибки:\n{ex.Message}\n\nОригинальный отчет:\n{report}",
                                "VK M — Критическая ошибка",
                                0x00000010 | 0x00001000);
                        }
                        catch { /* Игнорируем */ }
                    }
                    finally
                    {
                        lock (_lock)
                        {
                            _isDialogShowing = false;
                            _currentDialogHwnd = IntPtr.Zero;
                        }
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
            }
            catch
            {
                lock (_lock)
                {
                    _isDialogShowing = false;
                }
                // Если не удалось создать поток, показываем MessageBox в текущем потоке
                MessageBoxW(IntPtr.Zero, report, "VK M — Критическая ошибка", 0x00000010 | 0x00001000);
            }
        }

        private static void UpdateDialogContent(IntPtr hwnd, string newReport)
        {
            try
            {
                const int IDC_TEXT_EDIT = 102;
                var hEdit = GetDlgItem(hwnd, IDC_TEXT_EDIT);
                if (hEdit != IntPtr.Zero)
                {
                    SetWindowTextW(hEdit, newReport);
                }
            }
            catch { /* Игнорируем ошибки обновления */ }
        }

        private static void ShowNativeErrorDialog(string report)
        {
            var hInstance = GetModuleHandle(null);

            // Регистрируем класс окна
            var wc = new WNDCLASSEXW
            {
                cbSize = Marshal.SizeOf<WNDCLASSEXW>(),
                style = 0,
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate<WndProcDelegate>(WndProc),
                hInstance = hInstance,
                hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW),
                hbrBackground = (IntPtr)(COLOR_WINDOW + 1),
                lpszClassName = "VKErrorDialogClass"
            };

            var atom = RegisterClassExW(ref wc);
            if (atom == 0)
            {
                MessageBoxW(IntPtr.Zero, report, "VK M — Критическая ошибка", 0x00000010 | 0x00001000);
                return;
            }

            // Создаём окно
            var hwnd = CreateWindowExW(
                0, atom, "VK M — Критическая ошибка",
                (uint)(WindowStyles.WS_OVERLAPPEDWINDOW | WindowStyles.WS_VISIBLE),
                CW_USEDEFAULT, CW_USEDEFAULT, 800, 600,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero
            );

            if (hwnd == IntPtr.Zero)
            {
                MessageBoxW(IntPtr.Zero, report, "VK M — Критическая ошибка", 0x00000010 | 0x00001000);
                return;
            }

            // Сохраняем отчёт и хэндл окна для глобального доступа
            var gcHandle = GCHandle.Alloc(report, GCHandleType.Normal);
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, GCHandle.ToIntPtr(gcHandle));

            lock (_lock)
            {
                _currentDialogHwnd = hwnd;
            }

            // Центрируем окно на экране
            CenterWindow(hwnd);

            // Цикл сообщений
            while (GetMessageW(out var msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessageW(ref msg);
            }

            lock (_lock)
            {
                _currentDialogHwnd = IntPtr.Zero;
            }

            gcHandle.Free();
        }

        private static void CenterWindow(IntPtr hwnd)
        {
            GetWindowRect(hwnd, out var windowRect);
            var screenWidth = GetSystemMetrics(SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SM_CYSCREEN);

            var windowWidth = windowRect.right - windowRect.left;
            var windowHeight = windowRect.bottom - windowRect.top;

            var x = (screenWidth - windowWidth) / 2;
            var y = (screenHeight - windowHeight) / 2;

            SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            const int WM_CREATE = 0x0001;
            const int WM_DESTROY = 0x0002;
            const int WM_SIZE = 0x0005;
            const int WM_COMMAND = 0x0111;
            const int WM_GETMINMAXINFO = 0x0024;
            const int WM_CLOSE = 0x0010;
            const int IDC_COPY_BUTTON = 100;
            const int IDC_CLOSE_BUTTON = 101;
            const int IDC_TEXT_EDIT = 102;

            switch (msg)
            {
                case WM_CREATE:
                    {
                        var gcHandle = GCHandle.FromIntPtr(GetWindowLongPtrW(hWnd, GWLP_USERDATA));
                        var report = gcHandle.Target as string ?? string.Empty;

                        // Создаём текстовое поле
                        CreateWindowExW(
                            0, "EDIT", report,
                            (uint)(EditStyles.ES_MULTILINE | EditStyles.ES_READONLY | EditStyles.ES_AUTOVSCROLL |
                                   EditStyles.ES_AUTOHSCROLL | EditStyles.ES_LEFT |
                                   EditStyles.WS_VISIBLE | EditStyles.WS_CHILD | EditStyles.WS_VSCROLL | EditStyles.WS_HSCROLL |
                                   EditStyles.WS_BORDER),
                            10, 10, 760, 480,
                            hWnd, (IntPtr)IDC_TEXT_EDIT, IntPtr.Zero, IntPtr.Zero
                        );

                        // Кнопка "Копировать в буфер обмена"
                        CreateWindowExW(
                            0, "BUTTON", "Копировать в буфер обмена",
                            (uint)(ButtonStyles.BS_PUSHBUTTON | ButtonStyles.WS_VISIBLE | ButtonStyles.WS_CHILD | ButtonStyles.WS_TABSTOP),
                            10, 510, 180, 35,
                            hWnd, (IntPtr)IDC_COPY_BUTTON, IntPtr.Zero, IntPtr.Zero
                        );

                        // Кнопка "Закрыть"
                        CreateWindowExW(
                            0, "BUTTON", "Закрыть",
                            (uint)(ButtonStyles.BS_PUSHBUTTON | ButtonStyles.WS_VISIBLE | ButtonStyles.WS_CHILD | ButtonStyles.WS_TABSTOP),
                            610, 510, 160, 35,
                            hWnd, (IntPtr)IDC_CLOSE_BUTTON, IntPtr.Zero, IntPtr.Zero
                        );

                        // Устанавливаем моноширинный шрифт
                        var hFont = CreateFontW(
                            -13, 0, 0, 0, 0, 0, 0, 0,
                            DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS,
                            DEFAULT_QUALITY, FIXED_PITCH | FF_MODERN, "Consolas"
                        );
                        if (hFont == IntPtr.Zero)
                        {
                            hFont = CreateFontW(
                                -13, 0, 0, 0, 0, 0, 0, 0,
                                DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS,
                                DEFAULT_QUALITY, FIXED_PITCH | FF_MODERN, "Courier New"
                            );
                        }
                        if (hFont != IntPtr.Zero)
                        {
                            var hEdit = GetDlgItem(hWnd, IDC_TEXT_EDIT);
                            SendMessageW(hEdit, WM_SETFONT, hFont, 1);
                        }

                        return 0;
                    }

                case WM_CLOSE:
                    {
                        DestroyWindow(hWnd);
                        return 0;
                    }

                case WM_SIZE:
                    {
                        var width = (int)(lParam & 0xFFFF);
                        var height = (int)((lParam >> 16) & 0xFFFF);

                        var hEdit = GetDlgItem(hWnd, IDC_TEXT_EDIT);
                        if (hEdit != IntPtr.Zero)
                        {
                            SetWindowPos(hEdit, IntPtr.Zero, 10, 10,
                                width - 20, height - 90,
                                SWP_NOZORDER);
                        }

                        var hCopyBtn = GetDlgItem(hWnd, IDC_COPY_BUTTON);
                        if (hCopyBtn != IntPtr.Zero)
                        {
                            SetWindowPos(hCopyBtn, IntPtr.Zero, 10, height - 65,
                                180, 35, SWP_NOZORDER);
                        }

                        var hCloseBtn = GetDlgItem(hWnd, IDC_CLOSE_BUTTON);
                        if (hCloseBtn != IntPtr.Zero)
                        {
                            SetWindowPos(hCloseBtn, IntPtr.Zero, width - 170, height - 65,
                                160, 35, SWP_NOZORDER);
                        }

                        return 0;
                    }

                case WM_COMMAND:
                    {
                        var commandId = (int)(wParam & 0xFFFF);
                        switch (commandId)
                        {
                            case IDC_COPY_BUTTON:
                                {
                                    var hEdit = GetDlgItem(hWnd, IDC_TEXT_EDIT);
                                    var textLength = GetWindowTextLengthW(hEdit);
                                    var sb = new StringBuilder(textLength + 1);
                                    GetWindowTextW(hEdit, sb, sb.Capacity);

                                    if (OpenClipboard(hWnd))
                                    {
                                        EmptyClipboard();
                                        var hGlobal = Marshal.StringToHGlobalUni(sb.ToString());
                                        SetClipboardData(CF_UNICODETEXT, hGlobal);
                                        CloseClipboard();
                                    }
                                    break;
                                }

                            case IDC_CLOSE_BUTTON:
                                {
                                    DestroyWindow(hWnd);
                                    return 0;
                                }
                        }
                        return 0;
                    }

                case WM_GETMINMAXINFO:
                    {
                        var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                        mmi.ptMinTrackSize.x = 500;
                        mmi.ptMinTrackSize.y = 350;
                        Marshal.StructureToPtr(mmi, lParam, true);
                        return 0;
                    }

                case WM_DESTROY:
                    {
                        PostQuitMessage(0);
                        return 0;
                    }
            }

            return DefWindowProcW(hWnd, msg, wParam, lParam);
        }

        #region Win32 P/Invoke

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private const int CW_USEDEFAULT = unchecked((int)0x80000000);
        private const int COLOR_WINDOW = 5;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private const int GWLP_USERDATA = -21;
        private const int WM_SETFONT = 0x0030;
        private const uint CF_UNICODETEXT = 13;
        private const string IDC_ARROW = "IDC_ARROW";

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;

        private const byte DEFAULT_CHARSET = 1;
        private const uint OUT_DEFAULT_PRECIS = 0;
        private const uint CLIP_DEFAULT_PRECIS = 0;
        private const uint DEFAULT_QUALITY = 0;
        private const uint FIXED_PITCH = 1;
        private const uint FF_MODERN = 48;

        [Flags]
        private enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_CAPTION = 0x00C00000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_VISIBLE = 0x10000000,
            WS_CHILD = 0x40000000,
            WS_HSCROLL = 0x00100000,
            WS_VSCROLL = 0x00200000,
        }

        [Flags]
        private enum EditStyles : uint
        {
            ES_LEFT = 0x0000,
            ES_MULTILINE = 0x0004,
            ES_AUTOVSCROLL = 0x0040,
            ES_AUTOHSCROLL = 0x0080,
            ES_READONLY = 0x0800,
            WS_VISIBLE = 0x10000000,
            WS_CHILD = 0x40000000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_BORDER = 0x00800000,
        }

        [Flags]
        private enum ButtonStyles : uint
        {
            BS_PUSHBUTTON = 0x00000000,
            WS_VISIBLE = 0x10000000,
            WS_CHILD = 0x40000000,
            WS_TABSTOP = 0x00010000,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASSEXW
        {
            public int cbSize;
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ushort RegisterClassExW(ref WNDCLASSEXW lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowExW(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.U2)] ushort atom,
            string lpWindowName,
            uint dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam
        );

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowExW(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMessageW(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr DispatchMessageW(ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, string lpCursorName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextW(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLengthW(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageW(IntPtr hWnd, uint msg, IntPtr wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtrW(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetWindowTextW(IntPtr hWnd, string text);

        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFontW(
            int cHeight, int cWidth, int cEscapement, int cOrientation,
            int cWeight, uint bItalic, uint bUnderline, uint bStrikeOut,
            uint iCharSet, uint iOutPrecision, uint iClipPrecision,
            uint iQuality, uint iPitchAndFamily, string pszFaceName
        );

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

        #endregion
    }
}