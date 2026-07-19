using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        // Храним делегат, чтобы GC его не собрал
        private static readonly WndProcDelegate _wndProcDelegate = WndProc;
        private static IntPtr _windowClassAtom = IntPtr.Zero;

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

            // === ИНФОРМАЦИЯ О МЕСТЕ ОШИБКИ ===
            sb.AppendLine("--- МЕСТО ВОЗНИКНОВЕНИЯ ОШИБКИ ---");

            try
            {
                var stackTrace = new StackTrace(exception, true);
                var frames = stackTrace.GetFrames();

                if (frames != null && frames.Length > 0)
                {
                    var firstFrame = frames[0];
                    var method = firstFrame.GetMethod();

                    if (method != null)
                    {
                        var declaringType = method.DeclaringType;
                        if (declaringType != null)
                        {
                            sb.AppendLine($"Класс:           {declaringType.FullName}");
                            sb.AppendLine($"Пространство имен: {declaringType.Namespace ?? "Без пространства имен"}");
                            sb.AppendLine($"Сборка:          {declaringType.Assembly.GetName().Name}");
                        }
                        else
                        {
                            sb.AppendLine("Класс:           Неизвестный класс");
                        }

                        sb.AppendLine($"Метод:           {method.Name}");

                        var parameters = method.GetParameters();
                        if (parameters.Length > 0)
                        {
                            var paramList = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                            sb.AppendLine($"Параметры:       {paramList}");
                        }
                        else
                        {
                            sb.AppendLine($"Параметры:       (без параметров)");
                        }

                        if (method is System.Reflection.MethodInfo methodInfo)
                        {
                            sb.AppendLine($"Возвращаемый тип: {methodInfo.ReturnType.Name}");
                        }

                        if (method.IsPublic) sb.AppendLine("Модификатор:     public");
                        else if (method.IsPrivate) sb.AppendLine("Модификатор:     private");
                        else if (method.IsFamily) sb.AppendLine("Модификатор:     protected");
                        else if (method.IsAssembly) sb.AppendLine("Модификатор:     internal");

                        sb.AppendLine($"Статический:     {method.IsStatic}");

                        var fileName = firstFrame.GetFileName();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            sb.AppendLine($"Файл:            {System.IO.Path.GetFileName(fileName)}");
                            sb.AppendLine($"Номер строки:    {firstFrame.GetFileLineNumber()}");
                            sb.AppendLine($"Номер колонки:   {firstFrame.GetFileColumnNumber()}");
                        }
                        else
                        {
                            sb.AppendLine("(Информация о файле отсутствует)");
                        }

                        sb.AppendLine($"IL смещение:     {firstFrame.GetILOffset()}");
                        sb.AppendLine($"Native смещение: {firstFrame.GetNativeOffset()}");
                    }
                }
                else
                {
                    sb.AppendLine("(Не удалось получить информацию о стеке вызовов)");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Ошибка при получении информации о стеке: {ex.Message}");
            }

            // === ИНФОРМАЦИЯ ОБ ИСКЛЮЧЕНИИ ===
            sb.AppendLine();
            sb.AppendLine("--- ИНФОРМАЦИЯ ОБ ИСКЛЮЧЕНИИ ---");
            sb.AppendLine($"Тип исключения:  {exception.GetType().FullName}");
            sb.AppendLine($"Сообщение:       {exception.Message}");
            sb.AppendLine($"Source:          {exception.Source ?? "Не указан"}");
            sb.AppendLine($"TargetSite:      {exception.TargetSite?.ToString() ?? "Не указан"}");
            sb.AppendLine($"HResult:         0x{exception.HResult:X8} ({(uint)exception.HResult})");
            sb.AppendLine($"Data:            {(exception.Data != null && exception.Data.Count > 0 ? exception.Data.Count.ToString() : "Нет данных")}");

            if (exception.Data != null && exception.Data.Count > 0)
            {
                sb.AppendLine("--- Данные исключения ---");
                foreach (System.Collections.DictionaryEntry entry in exception.Data)
                {
                    sb.AppendLine($"  {entry.Key}: {entry.Value}");
                }
            }

            // === ВЛОЖЕННЫЕ ИСКЛЮЧЕНИЯ ===
            if (exception.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("--- ВЛОЖЕННОЕ ИСКЛЮЧЕНИЕ (InnerException) ---");
                sb.AppendLine($"Тип:             {exception.InnerException.GetType().FullName}");
                sb.AppendLine($"Сообщение:       {exception.InnerException.Message}");
                sb.AppendLine($"Source:          {exception.InnerException.Source ?? "Не указан"}");
                sb.AppendLine($"TargetSite:      {exception.InnerException.TargetSite?.ToString() ?? "Не указан"}");
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(exception.InnerException.StackTrace ?? "Стек вызовов отсутствует");

                var inner = exception.InnerException.InnerException;
                int depth = 2;
                while (inner != null && depth <= 5)
                {
                    sb.AppendLine();
                    sb.AppendLine($"--- ВЛОЖЕННОЕ ИСКЛЮЧЕНИЕ (уровень {depth}) ---");
                    sb.AppendLine($"Тип:             {inner.GetType().FullName}");
                    sb.AppendLine($"Сообщение:       {inner.Message}");
                    sb.AppendLine($"Source:          {inner.Source ?? "Не указан"}");
                    sb.AppendLine($"TargetSite:      {inner.TargetSite?.ToString() ?? "Не указан"}");
                    sb.AppendLine("Stack Trace:");
                    sb.AppendLine(inner.StackTrace ?? "Стек вызовов отсутствует");
                    inner = inner.InnerException;
                    depth++;
                }
            }

            sb.AppendLine();
            sb.AppendLine("--- ИНФОРМАЦИЯ О ПОЛЬЗОВАТЕЛЕ ---");
            try
            {
                var setting = SettingsTable.GetSetting("UserUniqID");
                if (setting != null)
                    sb.AppendLine($"UserUniqID:         {setting.settingValue}");
                else
                    sb.AppendLine("UserUniqID:         Не установлен");

                var accounts = AccountsDB.GetAllAccounts();
                sb.AppendLine($"Количество аккаунтов: {accounts.Count}");

                if (accounts.Count > 0)
                {
                    var activeAccount = AccountsDB.GetActiveAccount();
                    if (activeAccount != null)
                    {
                        // Только хеш, без токена
                        sb.AppendLine($"Активный аккаунт (хеш): {activeAccount.GetHash()}");
                    }
                    else
                    {
                        sb.AppendLine("Активный аккаунт:   Не найден");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"(Не удалось получить информацию о пользователе: {ex.Message})");

            }


            return sb.ToString();
        }

        /// <summary>
        /// Показывает нативное диалоговое окно с полным отчётом об ошибке.
        /// Запускается в отдельном STA-потоке, не блокируя UI приложения.
        /// </summary>
        /// <summary>
        /// Показывает диалог ошибки и ожидает его закрытия.
        /// </summary>
        public static void ShowErrorDialogAndWait(string report)
        {
            var resetEvent = new ManualResetEventSlim(false);

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
                        try
                        {
                            MessageBoxW(IntPtr.Zero,
                                $"Ошибка при показе диалога ошибки:\n{ex.Message}\n\nОригинальный отчет:\n{report}",
                                "VK M — Критическая ошибка",
                                0x00000010 | 0x00001000);
                        }
                        catch { }
                    }
                    finally
                    {
                        resetEvent.Set();
                    }
                });

                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = false; // Важно: не фоновый поток
                thread.Start();

                // Ожидаем закрытия диалога
                resetEvent.Wait();
            }
            catch
            {
                // Если не удалось создать поток, показываем MessageBox
                MessageBoxW(IntPtr.Zero, report, "VK M — Критическая ошибка", 0x00000010 | 0x00001000);
                resetEvent.Set();
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
            catch { }
        }

        private static void ShowNativeErrorDialog(string report)
        {
            var hInstance = GetModuleHandle(null);

            // Регистрируем класс окна (один раз)
            if (_windowClassAtom == IntPtr.Zero)
            {
                var wc = new WNDCLASSEXW
                {
                    cbSize = Marshal.SizeOf<WNDCLASSEXW>(),
                    style = 0,
                    lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
                    hInstance = hInstance,
                    hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW),
                    hbrBackground = (IntPtr)(COLOR_WINDOW + 1),
                    lpszClassName = "VKErrorDialogClass"
                };

                _windowClassAtom = new IntPtr(RegisterClassExW(ref wc));
                if (_windowClassAtom == IntPtr.Zero)
                {
                    MessageBoxW(IntPtr.Zero, report, "VK M — Критическая ошибка", 0x00000010 | 0x00001000);
                    return;
                }
            }

            // Увеличиваем размер окна
            var hwnd = CreateWindowExW(
                0,
                (string)null,
                "VK M — Критическая ошибка",
                (uint)(WindowStyles.WS_OVERLAPPEDWINDOW | WindowStyles.WS_VISIBLE | WindowStyles.WS_THICKFRAME),
                CW_USEDEFAULT, CW_USEDEFAULT, 1000, 800, // Увеличенный размер
                IntPtr.Zero,
                IntPtr.Zero,
                hInstance,
                IntPtr.Zero
            );

            if (hwnd == IntPtr.Zero)
            {
                var error = Marshal.GetLastWin32Error();
                MessageBoxW(IntPtr.Zero,
                    $"Не удалось создать окно. Ошибка: {error}\n\nОтчёт:\n{report}",
                    "VK M — Критическая ошибка",
                    0x00000010 | 0x00001000);
                return;
            }

            // Сохраняем отчёт в USERDATA
            var gcHandle = GCHandle.Alloc(report, GCHandleType.Normal);
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, GCHandle.ToIntPtr(gcHandle));

            lock (_lock)
            {
                _currentDialogHwnd = hwnd;
            }

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

        // Добавьте в секцию констант:
        private const int WS_EX_CLIENTEDGE = 0x00000200;
        private const int WM_TIMER = 0x0113;
        private const int EM_SETSEL = 0x00B1;

        // Добавьте в секцию P/Invoke:
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetTimer(IntPtr hWnd, int nIDEvent, uint uElapse, IntPtr lpTimerFunc);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool KillTimer(IntPtr hWnd, int uIDEvent);



        private static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            const int WM_CREATE = 0x0001;
            const int WM_DESTROY = 0x0002;
            const int WM_SIZE = 0x0005;
            const int WM_COMMAND = 0x0111;
            const int WM_GETMINMAXINFO = 0x0024;
            const int WM_CLOSE = 0x0010;
            const int WM_VSCROLL = 0x0115;
            const int WM_MOUSEWHEEL = 0x020A;

            const int IDC_TEXT_EDIT = 102;
            const int IDC_COPY_BUTTON = 100;
            const int IDC_CLOSE_BUTTON = 101;
            const int IDC_SAVE_BUTTON = 103;

            switch (msg)
            {
                case WM_CREATE:
                    {
                        var gcHandlePtr = GetWindowLongPtrW(hWnd, GWLP_USERDATA);
                        if (gcHandlePtr == IntPtr.Zero)
                            return 0;

                        var gcHandle = GCHandle.FromIntPtr(gcHandlePtr);
                        var report = gcHandle.Target as string ?? string.Empty;

                        // Создаём текстовое поле с прокруткой и возможностью выделения
                        var hEdit = CreateWindowExW(
                            WS_EX_CLIENTEDGE, // Добавляем 3D рамку
                            "EDIT",
                            report,
                            (uint)(EditStyles.ES_MULTILINE |
                                   EditStyles.ES_READONLY |
                                   EditStyles.ES_AUTOVSCROLL |
                                   EditStyles.ES_AUTOHSCROLL |
                                   EditStyles.ES_LEFT |
                                   EditStyles.WS_VISIBLE |
                                   EditStyles.WS_CHILD |
                                   EditStyles.WS_VSCROLL |
                                   EditStyles.WS_HSCROLL |
                                   EditStyles.WS_BORDER),
                            10, 10, 860, 570,
                            hWnd, (IntPtr)IDC_TEXT_EDIT, IntPtr.Zero, IntPtr.Zero
                        );

                        // Устанавливаем моноширинный шрифт для лучшей читаемости
                        var hFont = CreateFontW(
                            -14, 0, 0, 0, 0, 0, 0, 0,
                            DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS,
                            DEFAULT_QUALITY, FIXED_PITCH | FF_MODERN, "Consolas"
                        );
                        if (hFont == IntPtr.Zero)
                        {
                            hFont = CreateFontW(
                                -14, 0, 0, 0, 0, 0, 0, 0,
                                DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS,
                                DEFAULT_QUALITY, FIXED_PITCH | FF_MODERN, "Courier New"
                            );
                        }
                        if (hFont != IntPtr.Zero)
                        {
                            SendMessageW(hEdit, WM_SETFONT, hFont, 1);
                        }

                        // Устанавливаем начальную позицию курсора в начало
                        SendMessageW(hEdit, 0x00B1, 0, 0); // EM_SETSEL

                        // Кнопка "Копировать в буфер обмена"
                        CreateWindowExW(
                            0, "BUTTON", "📋 Копировать в буфер обмена",
                            (uint)(ButtonStyles.BS_PUSHBUTTON | ButtonStyles.WS_VISIBLE | ButtonStyles.WS_CHILD | ButtonStyles.WS_TABSTOP),
                            10, 600, 180, 35,
                            hWnd, (IntPtr)IDC_COPY_BUTTON, IntPtr.Zero, IntPtr.Zero
                        );

                        // Кнопка "Сохранить в файл"
                        CreateWindowExW(
                            0, "BUTTON", "💾 Сохранить в файл",
                            (uint)(ButtonStyles.BS_PUSHBUTTON | ButtonStyles.WS_VISIBLE | ButtonStyles.WS_CHILD | ButtonStyles.WS_TABSTOP),
                            200, 600, 180, 35,
                            hWnd, (IntPtr)IDC_SAVE_BUTTON, IntPtr.Zero, IntPtr.Zero
                        );

                        // Кнопка "Закрыть"
                        CreateWindowExW(
                            0, "BUTTON", "❌ Закрыть",
                            (uint)(ButtonStyles.BS_PUSHBUTTON | ButtonStyles.WS_VISIBLE | ButtonStyles.WS_CHILD | ButtonStyles.WS_TABSTOP),
                            700, 600, 160, 35,
                            hWnd, (IntPtr)IDC_CLOSE_BUTTON, IntPtr.Zero, IntPtr.Zero
                        );

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

                        if (width > 0 && height > 0)
                        {
                            // Изменяем размер текстового поля
                            var hEdit = GetDlgItem(hWnd, IDC_TEXT_EDIT);
                            if (hEdit != IntPtr.Zero)
                            {
                                SetWindowPos(hEdit, IntPtr.Zero, 10, 10,
                                    width - 20, height - 105,
                                    SWP_NOZORDER);
                            }

                            // Перемещаем кнопки
                            var hCopyBtn = GetDlgItem(hWnd, IDC_COPY_BUTTON);
                            if (hCopyBtn != IntPtr.Zero)
                            {
                                SetWindowPos(hCopyBtn, IntPtr.Zero, 10, height - 80,
                                    180, 35, SWP_NOZORDER);
                            }

                            var hSaveBtn = GetDlgItem(hWnd, IDC_SAVE_BUTTON);
                            if (hSaveBtn != IntPtr.Zero)
                            {
                                SetWindowPos(hSaveBtn, IntPtr.Zero, 200, height - 80,
                                    180, 35, SWP_NOZORDER);
                            }

                            var hCloseBtn = GetDlgItem(hWnd, IDC_CLOSE_BUTTON);
                            if (hCloseBtn != IntPtr.Zero)
                            {
                                SetWindowPos(hCloseBtn, IntPtr.Zero, width - 170, height - 80,
                                    160, 35, SWP_NOZORDER);
                            }
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
                                    if (textLength > 0)
                                    {
                                        var sb = new StringBuilder(textLength + 1);
                                        GetWindowTextW(hEdit, sb, sb.Capacity);

                                        if (OpenClipboard(hWnd))
                                        {
                                            EmptyClipboard();
                                            var hGlobal = Marshal.StringToHGlobalUni(sb.ToString());
                                            SetClipboardData(CF_UNICODETEXT, hGlobal);
                                            CloseClipboard();

                                            // Визуальное подтверждение (меняем текст кнопки)
                                            SetWindowTextW(GetDlgItem(hWnd, IDC_COPY_BUTTON), "✅ Скопировано!");

                                            // Возвращаем текст через 2 секунды
                                            var timerId = 1001;
                                            SetTimer(hWnd, timerId, 2000, IntPtr.Zero);
                                        }
                                    }
                                    break;
                                }

                            case IDC_SAVE_BUTTON:
                                {
                                    var hEdit = GetDlgItem(hWnd, IDC_TEXT_EDIT);
                                    var textLength = GetWindowTextLengthW(hEdit);
                                    if (textLength > 0)
                                    {
                                        var sb = new StringBuilder(textLength + 1);
                                        GetWindowTextW(hEdit, sb, sb.Capacity);

                                        try
                                        {
                                            // Сохраняем в файл в папке Documents
                                            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                            var errorFolder = Path.Combine(documentsPath, "VK_UI3_Errors");
                                            Directory.CreateDirectory(errorFolder);

                                            var fileName = $"error_report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                                            var filePath = Path.Combine(errorFolder, fileName);

                                            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                                            // Визуальное подтверждение
                                            SetWindowTextW(GetDlgItem(hWnd, IDC_SAVE_BUTTON), "✅ Сохранено!");

                                            var timerId = 1002;
                                            SetTimer(hWnd, timerId, 2000, IntPtr.Zero);

                                            // Открываем папку с файлом
                                            Process.Start("explorer.exe", $"/select, \"{filePath}\"");
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBoxW(hWnd,
                                                $"Не удалось сохранить файл:\n{ex.Message}",
                                                "Ошибка сохранения",
                                                0x00000010);
                                        }
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

                case WM_TIMER:
                    {
                        var timerId = (int)wParam;
                        switch (timerId)
                        {
                            case 1001: // Возвращаем текст кнопки "Копировать"
                                SetWindowTextW(GetDlgItem(hWnd, IDC_COPY_BUTTON), "📋 Копировать в буфер обмена");
                                KillTimer(hWnd, timerId);
                                break;
                            case 1002: // Возвращаем текст кнопки "Сохранить"
                                SetWindowTextW(GetDlgItem(hWnd, IDC_SAVE_BUTTON), "💾 Сохранить в файл");
                                KillTimer(hWnd, timerId);
                                break;
                        }
                        return 0;
                    }

                case WM_GETMINMAXINFO:
                    {
                        var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                        mmi.ptMinTrackSize.x = 600;
                        mmi.ptMinTrackSize.y = 450;
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

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
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
        [return: MarshalAs(UnmanagedType.Bool)]
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