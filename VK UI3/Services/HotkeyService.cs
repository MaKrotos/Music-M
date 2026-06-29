using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using VK_UI3.DB;
using VK_UI3.Models;
using VK_UI3.Services.Player;
using Windows.System;

namespace VK_UI3.Services
{
    /// <summary>
    /// Сервис управления глобальными горячими клавишами для управления воспроизведением.
    /// Использует Win32 RegisterHotKey для глобального перехвата нажатий.
    /// </summary>
    public class HotkeyService : IDisposable
    {
        #region Win32 Constants

        private const int WM_HOTKEY = 0x0312;

        // Модификаторы для RegisterHotKey
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_NOREPEAT = 0x4000;

        #endregion

        #region Win32 Imports

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion

        #region Fields

        private readonly IntPtr _hwnd;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;
        private readonly Dictionary<PlayerAction, HotkeyBinding> _bindings = new();
        private readonly Dictionary<int, PlayerAction> _hotkeyIdToAction = new();
        private bool _isInitialized = false;
        private bool _disposed = false;
        private int _nextHotkeyId = 1;

        // Событие для уведомления UI об изменениях
        public event EventHandler BindingsChanged;

        // Синглтон
        public static HotkeyService Instance { get; private set; }

        #endregion

        #region Constructor & Initialization

        private HotkeyService(IntPtr hwnd, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
        {
            _hwnd = hwnd;
            _dispatcherQueue = dispatcherQueue;
        }

        /// <summary>
        /// Инициализирует сервис горячих клавиш.
        /// </summary>
        public static void Initialize(IntPtr hwnd, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
        {
            if (Instance != null)
            {
                Instance.Dispose();
            }
            Instance = new HotkeyService(hwnd, dispatcherQueue);
            Instance.LoadBindings();
            Instance.RegisterAllHotkeys();
            Instance._isInitialized = true;
            Debug.WriteLine("[HotkeyService] Initialized successfully");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Загружает привязки из БД.
        /// </summary>
        public void LoadBindings()
        {
            _bindings.Clear();
            _hotkeyIdToAction.Clear();

            foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
            {
                var setting = SettingsTable.GetSetting($"Hotkey_{action}");
                HotkeyBinding binding;

                if (setting != null && !string.IsNullOrEmpty(setting.settingValue))
                {
                    binding = HotkeyBinding.Deserialize(action, setting.settingValue);
                }
                else
                {
                    binding = HotkeyBinding.GetDefault(action);
                }

                binding.HotkeyId = _nextHotkeyId++;
                _bindings[action] = binding;
                _hotkeyIdToAction[binding.HotkeyId] = action;
            }

            Debug.WriteLine("[HotkeyService] Loaded bindings from DB");
        }

        /// <summary>
        /// Сохраняет все привязки в БД.
        /// </summary>
        public void SaveBindings()
        {
            foreach (var kvp in _bindings)
            {
                SettingsTable.SetSetting($"Hotkey_{kvp.Key}", kvp.Value.Serialize());
            }
            Debug.WriteLine("[HotkeyService] Saved bindings to DB");
        }

        /// <summary>
        /// Сохраняет одну привязку в БД.
        /// </summary>
        public void SaveBinding(PlayerAction action)
        {
            if (_bindings.TryGetValue(action, out var binding))
            {
                SettingsTable.SetSetting($"Hotkey_{action}", binding.Serialize());
            }
        }

        /// <summary>
        /// Регистрирует все активные горячие клавиши в системе.
        /// </summary>
        public void RegisterAllHotkeys()
        {
            UnregisterAllHotkeys();

            foreach (var kvp in _bindings)
            {
                if (kvp.Value.IsEnabled && kvp.Value.KeyCode != 0)
                {
                    RegisterSingleHotkey(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Обновляет привязку для указанного действия и перерегистрирует горячую клавишу.
        /// </summary>
        public void UpdateBinding(PlayerAction action, int keyCode, VirtualKeyModifiers modifiers, bool isEnabled)
        {
            if (_bindings.TryGetValue(action, out var binding))
            {
                // Удаляем старую регистрацию
                if (binding.HotkeyId > 0)
                {
                    UnregisterHotKey(_hwnd, binding.HotkeyId);
                }

                binding.KeyCode = keyCode;
                binding.Modifiers = modifiers;
                binding.IsEnabled = isEnabled;

                // Регистрируем новую
                if (isEnabled && keyCode != 0)
                {
                    RegisterSingleHotkey(binding);
                }

                // Сохраняем в БД
                SaveBinding(action);
                BindingsChanged?.Invoke(this, EventArgs.Empty);

                Debug.WriteLine($"[HotkeyService] Updated binding for {action}: {binding.DisplayKeyCombination}");
            }
        }

        /// <summary>
        /// Сбрасывает все привязки к настройкам по умолчанию.
        /// </summary>
        public void ResetToDefaults()
        {
            UnregisterAllHotkeys();
            _bindings.Clear();
            _hotkeyIdToAction.Clear();
            _nextHotkeyId = 1;

            foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
            {
                var binding = HotkeyBinding.GetDefault(action);
                binding.HotkeyId = _nextHotkeyId++;
                _bindings[action] = binding;
                _hotkeyIdToAction[binding.HotkeyId] = action;
            }

            RegisterAllHotkeys();
            SaveBindings();
            BindingsChanged?.Invoke(this, EventArgs.Empty);

            Debug.WriteLine("[HotkeyService] Reset all bindings to defaults");
        }

        /// <summary>
        /// Возвращает привязку для указанного действия.
        /// </summary>
        public HotkeyBinding GetBinding(PlayerAction action)
        {
            return _bindings.TryGetValue(action, out var binding) ? binding : null;
        }

        /// <summary>
        /// Возвращает все привязки.
        /// </summary>
        public IEnumerable<HotkeyBinding> GetAllBindings()
        {
            return _bindings.Values.OrderBy(b => (int)b.Action);
        }

        /// <summary>
        /// Обрабатывает нажатие горячей клавиши по ID (вызывается из WndProc для WM_HOTKEY).
        /// </summary>
        public void HandleHotkeyPressed(int hotkeyId)
        {
            if (_hotkeyIdToAction.TryGetValue(hotkeyId, out var action))
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    MediaPlayerService.ExecuteAction(action);
                    Debug.WriteLine($"[HotkeyService] Executed action: {action}");
                });
            }
        }

        /// <summary>
        /// Обрабатывает WM_APPCOMMAND (медиа-клавиши с клавиатуры).
        /// </summary>
        public void HandleAppCommand(int cmd)
        {
            PlayerAction? action = null;

            switch (cmd)
            {
                case 8:  // APPCOMMAND_VOLUME_MUTE
                    action = PlayerAction.Mute;
                    break;
                case 9:  // APPCOMMAND_VOLUME_DOWN
                    action = PlayerAction.VolumeDown;
                    break;
                case 10: // APPCOMMAND_VOLUME_UP
                    action = PlayerAction.VolumeUp;
                    break;
                case 11: // APPCOMMAND_MEDIA_NEXTTRACK
                    action = PlayerAction.NextTrack;
                    break;
                case 12: // APPCOMMAND_MEDIA_PREVIOUSTRACK
                    action = PlayerAction.PreviousTrack;
                    break;
                case 13: // APPCOMMAND_MEDIA_STOP
                    action = PlayerAction.Stop;
                    break;
                case 46: // APPCOMMAND_MEDIA_PLAY_PAUSE
                case 47: // APPCOMMAND_MEDIA_PLAY
                case 48: // APPCOMMAND_MEDIA_PAUSE
                    action = PlayerAction.PlayPause;
                    break;
            }

            if (action.HasValue)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    MediaPlayerService.ExecuteAction(action.Value);
                    Debug.WriteLine($"[HotkeyService] Executed app command: {cmd} -> {action.Value}");
                });
            }
        }

        /// <summary>
        /// Проверяет, не занята ли уже указанная комбинация клавиш другим действием.
        /// </summary>
        public bool IsCombinationTaken(int keyCode, VirtualKeyModifiers modifiers, PlayerAction excludeAction)
        {
            return _bindings.Values.Any(b =>
                b.Action != excludeAction &&
                b.KeyCode == keyCode &&
                b.Modifiers == modifiers &&
                b.IsEnabled);
        }

        /// <summary>
        /// Возвращает действие, которое уже использует данную комбинацию, или null.
        /// </summary>
        public PlayerAction? GetActionByCombination(int keyCode, VirtualKeyModifiers modifiers)
        {
            var binding = _bindings.Values.FirstOrDefault(b =>
                b.KeyCode == keyCode &&
                b.Modifiers == modifiers &&
                b.IsEnabled);

            return binding?.Action;
        }

        #endregion

        #region Private Methods

        private void RegisterSingleHotkey(HotkeyBinding binding)
        {
            if (binding.KeyCode == 0 || _hwnd == IntPtr.Zero)
                return;

            // Если это стандартная медиа-клавиша без модификаторов, 
            // мы все равно можем попробовать зарегистрировать её как глобальный хоткей,
            // если пользователь хочет, чтобы она работала всегда.
            // Однако, чтобы избежать конфликтов с системным поведением WM_APPCOMMAND,
            // мы оставим этот фильтр только для тех, кто НЕ хочет переопределять их.
            // Но в текущей реализации мы просто пропускаем их.
            // Уберем это ограничение, чтобы RegisterHotKey мог перехватить их глобально.
            /* 
            if (binding.Modifiers == VirtualKeyModifiers.None && 
                binding.KeyCode >= 0xB0 && binding.KeyCode <= 0xB6)
            {
                return;
            }
            */

            uint modifiers = ConvertModifiers(binding.Modifiers);
            uint vk = (uint)binding.KeyCode;

            if (RegisterHotKey(_hwnd, binding.HotkeyId, modifiers, vk))
            {
                Debug.WriteLine($"[HotkeyService] Registered hotkey: {binding.DisplayKeyCombination} (ID={binding.HotkeyId})");
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                Debug.WriteLine($"[HotkeyService] Failed to register hotkey: {binding.DisplayKeyCombination} (Error={error})");
            }
        }

        private void UnregisterAllHotkeys()
        {
            foreach (var kvp in _bindings)
            {
                if (kvp.Value.HotkeyId > 0)
                {
                    UnregisterHotKey(_hwnd, kvp.Value.HotkeyId);
                }
            }
        }

        private static uint ConvertModifiers(VirtualKeyModifiers modifiers)
        {
            uint result = 0;
            if (modifiers.HasFlag(VirtualKeyModifiers.Control))
                result |= MOD_CONTROL;
            if (modifiers.HasFlag(VirtualKeyModifiers.Shift))
                result |= MOD_SHIFT;
            if (modifiers.HasFlag(VirtualKeyModifiers.Menu))
                result |= MOD_ALT;
            if (modifiers.HasFlag(VirtualKeyModifiers.Windows))
                result |= MOD_WIN;

            // Добавляем MOD_NOREPEAT чтобы не получать повторные сообщения при удержании
            result |= MOD_NOREPEAT;

            return result;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (!_disposed)
            {
                UnregisterAllHotkeys();
                _bindings.Clear();
                _hotkeyIdToAction.Clear();
                _disposed = true;

                if (Instance == this)
                    Instance = null;

                Debug.WriteLine("[HotkeyService] Disposed");
            }
        }

        #endregion
    }
}