using DevWinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using VK_UI3.Models;
using VK_UI3.Services;
using VK_UI3.Services.Player;
using Windows.System;

namespace VK_UI3.Views.Settings
{
    /// <summary>
    /// Контрол для настройки одной горячей клавиши с использованием DevWinUI Shortcut.
    /// </summary>
    public sealed partial class HotkeySettingControlDevWinUI : UserControl, INotifyPropertyChanged
    {
        #region Fields

        private PlayerAction _action;
        private HotkeyBinding _binding;

        #endregion

        #region Properties

        public string ActionDisplayName { get; private set; }
        public string KeyCombinationDisplay { get; private set; }

        public bool IsBindingEnabled
        {
            get => _binding?.IsEnabled ?? true;
            set
            {
                if (_binding != null && _binding.IsEnabled != value)
                {
                    _binding.IsEnabled = value;
                    HotkeyService.Instance?.UpdateBinding(_action, _binding.KeyCode, _binding.Modifiers, value);
                    UpdateUI();
                }
            }
        }

        #endregion

        #region Constructor

        public HotkeySettingControlDevWinUI()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Настраиваем KeyNameProvider для русских названий клавиш
            MainShortcut.KeyNameProvider = key =>
            {
                return key switch
                {
                    VirtualKey.Space => "Пробел",
                    VirtualKey.Left => "←",
                    VirtualKey.Up => "↑",
                    VirtualKey.Right => "→",
                    VirtualKey.Down => "↓",
                    VirtualKey.Insert => "Insert",
                    VirtualKey.Delete => "Delete",
                    VirtualKey.Home => "Home",
                    VirtualKey.End => "End",
                    VirtualKey.PageDown => "Page Down",
                    VirtualKey.PageUp => "Page Up",
                    VirtualKey.Print => "Print Screen",
                    VirtualKey.Pause => "Pause",
                    VirtualKey.NumberKeyLock => "Num Lock",
                    VirtualKey.CapitalLock => "Caps Lock",
                    VirtualKey.LeftWindows => "Win",
                    VirtualKey.RightWindows => "Win",
                    VirtualKey.LeftControl => "Ctrl",
                    VirtualKey.RightControl => "Ctrl",
                    VirtualKey.LeftShift => "Shift",
                    VirtualKey.RightShift => "Shift",
                    VirtualKey.LeftMenu => "Alt",
                    VirtualKey.RightMenu => "Alt",
                    VirtualKey.F1 => "F1",
                    VirtualKey.F2 => "F2",
                    VirtualKey.F3 => "F3",
                    VirtualKey.F4 => "F4",
                    VirtualKey.F5 => "F5",
                    VirtualKey.F6 => "F6",
                    VirtualKey.F7 => "F7",
                    VirtualKey.F8 => "F8",
                    VirtualKey.F9 => "F9",
                    VirtualKey.F10 => "F10",
                    VirtualKey.F11 => "F11",
                    VirtualKey.F12 => "F12",
                    >= VirtualKey.Number0 and <= VirtualKey.Number9 => ((char)('0' + (key - VirtualKey.Number0))).ToString(),
                    >= VirtualKey.NumberPad0 and <= VirtualKey.NumberPad9 => "Num " + (key - VirtualKey.NumberPad0),
                    >= VirtualKey.A and <= VirtualKey.Z => ((char)('A' + (key - VirtualKey.A))).ToString(),
                    _ => key.ToString()
                };
            };
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (HotkeyService.Instance != null)
            {
                HotkeyService.Instance.BindingsChanged -= OnBindingsChanged;
            }
        }

        #endregion

        #region Initialization

        public void Initialize(PlayerAction action)
        {
            _action = action;
            _binding = HotkeyService.Instance?.GetBinding(action) ?? HotkeyBinding.GetDefault(action);

            ActionDisplayName = _binding.DisplayActionName;
            MainShortcut.Title = $"{ActionDisplayName}";
            UpdateUI();

            if (HotkeyService.Instance != null)
            {
                HotkeyService.Instance.BindingsChanged += OnBindingsChanged;
            }
        }

        #endregion

        #region Shortcut Event Handlers

        private void OnMainShortcutPrimaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
        {
            // Обновляем превью и закрываем диалог, чтобы Shortcut обработал введённые клавиши
            MainShortcut.UpdatePreviewKeys();
            MainShortcut.CloseContentDialog();

            // Получаем выбранные клавиши
            var keys = MainShortcut.Keys;
            if (keys == null || keys.Count == 0)
            {
                Debug.WriteLine("[HotkeySettingControlDevWinUI] No keys selected, restoring current binding");
                UpdateUI();
                return;
            }

            // Конвертируем KeyVisualInfo в HotkeyBinding
            var keyVisualInfos = keys.Cast<KeyVisualInfo>().ToList();

            VirtualKeyModifiers modifiers = VirtualKeyModifiers.None;
            VirtualKey mainKey = VirtualKey.None;

            foreach (var kvi in keyVisualInfos)
            {
                var vk = kvi.Key;

                // Определяем модификаторы
                if (vk == VirtualKey.Control || vk == VirtualKey.LeftControl || vk == VirtualKey.RightControl)
                    modifiers |= VirtualKeyModifiers.Control;
                else if (vk == VirtualKey.Shift || vk == VirtualKey.LeftShift || vk == VirtualKey.RightShift)
                    modifiers |= VirtualKeyModifiers.Shift;
                else if (vk == VirtualKey.Menu || vk == VirtualKey.LeftMenu || vk == VirtualKey.RightMenu)
                    modifiers |= VirtualKeyModifiers.Menu;
                else if (vk == VirtualKey.LeftWindows || vk == VirtualKey.RightWindows)
                    modifiers |= VirtualKeyModifiers.Windows;
                else
                    mainKey = (VirtualKey)vk;
            }

            if (mainKey == VirtualKey.None)
            {
                Debug.WriteLine("[HotkeySettingControlDevWinUI] No main key selected");
                return;
            }

            int keyCode = (int)mainKey;

            // Проверяем, не занята ли комбинация
            if (HotkeyService.Instance != null)
            {
                var existingAction = HotkeyService.Instance.GetActionByCombination(keyCode, modifiers);
                if (existingAction.HasValue && existingAction.Value != _action)
                {
                    var existingBinding = HotkeyService.Instance.GetBinding(existingAction.Value);
                    var actionName = existingBinding?.DisplayActionName ?? existingAction.Value.ToString();

                    MainShortcut.ErrorTitle = $"Эта комбинация уже используется для \"{actionName}\"";
                    MainShortcut.IsError = true;
                    return;
                }
            }

            // Сохраняем новую комбинацию
            HotkeyService.Instance?.UpdateBinding(_action, keyCode, modifiers, true);

            if (_binding != null)
            {
                _binding.KeyCode = keyCode;
                _binding.Modifiers = modifiers;
                _binding.IsEnabled = true;
            }

            UpdateUI();
            Debug.WriteLine($"[HotkeySettingControlDevWinUI] Saved new binding for {_action}: {KeyCombinationDisplay}");
        }

        private void OnMainShortcutSecondaryButtonClick(object sender, ContentDialogButtonClickEventArgs e)
        {
            // Отмена — ничего не делаем, просто закрываем
            Debug.WriteLine("[HotkeySettingControlDevWinUI] Secondary button clicked (cancel)");
        }

        private void OnMainShortcutCloseButtonClick(object sender, ContentDialogButtonClickEventArgs e)
        {
            // Закрытие диалога — ничего не делаем
            Debug.WriteLine("[HotkeySettingControlDevWinUI] Close button clicked");
        }

        #endregion

        #region UI Event Handlers

        private void EnableCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_binding != null)
            {
                _binding.IsEnabled = true;
                HotkeyService.Instance?.UpdateBinding(_action, _binding.KeyCode, _binding.Modifiers, true);
                UpdateUI();
            }
        }

        private void EnableCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_binding != null)
            {
                _binding.IsEnabled = false;
                HotkeyService.Instance?.UpdateBinding(_action, _binding.KeyCode, _binding.Modifiers, false);
                UpdateUI();
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var defaultBinding = HotkeyBinding.GetDefault(_action);
            HotkeyService.Instance?.UpdateBinding(_action, defaultBinding.KeyCode, defaultBinding.Modifiers, defaultBinding.IsEnabled);

            if (_binding != null)
            {
                _binding.KeyCode = defaultBinding.KeyCode;
                _binding.Modifiers = defaultBinding.Modifiers;
                _binding.IsEnabled = defaultBinding.IsEnabled;
            }
            UpdateUI();
        }

        #endregion

        #region Private Methods

        private void UpdateUI()
        {
            if (_binding == null) return;

            KeyCombinationDisplay = _binding.DisplayKeyCombination;
            EnableCheckBox.IsChecked = _binding.IsEnabled;

            // Обновляем отображение в Shortcut
            UpdateShortcutKeys();

            // Обновляем видимость кнопки сброса
            var defaultBinding = HotkeyBinding.GetDefault(_action);
            ResetButton.Visibility = (_binding.KeyCode != defaultBinding.KeyCode || _binding.Modifiers != defaultBinding.Modifiers)
                ? Visibility.Visible
                : Visibility.Collapsed;

            OnPropertyChanged(nameof(ActionDisplayName));
            OnPropertyChanged(nameof(KeyCombinationDisplay));
            OnPropertyChanged(nameof(IsBindingEnabled));
        }

        /// <summary>
        /// Обновляет отображение текущей комбинации в Shortcut.
        /// </summary>
        private void UpdateShortcutKeys()
        {
            if (_binding == null || _binding.KeyCode == 0)
            {
                MainShortcut.Keys = new List<object>();
                return;
            }

            var keys = new List<object>();

            // Добавляем модификаторы
            if (_binding.Modifiers.HasFlag(VirtualKeyModifiers.Control))
                keys.Add("Ctrl");
            if (_binding.Modifiers.HasFlag(VirtualKeyModifiers.Shift))
                keys.Add("Shift");
            if (_binding.Modifiers.HasFlag(VirtualKeyModifiers.Menu))
                keys.Add("Alt");
            if (_binding.Modifiers.HasFlag(VirtualKeyModifiers.Windows))
                keys.Add("Win");

            // Добавляем основную клавишу (по имени из HotkeyBinding)
            var keyName = HotkeyBinding.GetKeyDisplayName(_binding.KeyCode);
            keys.Add(keyName);

            MainShortcut.Keys = keys;
        }

        private void OnBindingsChanged(object sender, EventArgs e)
        {
            _binding = HotkeyService.Instance?.GetBinding(_action) ?? _binding;
            UpdateUI();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
