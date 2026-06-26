using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Text;
using VK_UI3.Models;
using VK_UI3.Services;
using VK_UI3.Services.Player;
using Windows.System;
using Windows.UI.Core;

namespace VK_UI3.Views.Settings
{
    /// <summary>
    /// Контрол для настройки одной горячей клавиши с возможностью записи нажатия (key recording).
    /// </summary>
    public sealed partial class HotkeySettingControl : UserControl
    {
        #region Fields

        private PlayerAction _action;
        private HotkeyBinding _binding;
        private bool _isRecording = false;

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

        public HotkeySettingControl()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Resources.TryGetValue("DefaultCheckBoxStyle", out var style) && style is Style checkBoxStyle)
            {
                EnableCheckBox.Style = checkBoxStyle;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CancelRecording();
        }

        public void Initialize(PlayerAction action)
        {
            _action = action;
            _binding = HotkeyService.Instance?.GetBinding(action) ?? HotkeyBinding.GetDefault(action);

            ActionDisplayName = _binding.DisplayActionName;
            UpdateUI();

            if (HotkeyService.Instance != null)
            {
                HotkeyService.Instance.BindingsChanged += OnBindingsChanged;
            }
        }

        #endregion

        #region UI Event Handlers

        private void KeyBindingBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_isRecording)
            {
                CancelRecording();
                return;
            }
            StartRecording();
        }

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

        #region Key Recording

        private void StartRecording()
        {
            _isRecording = true;
            KeyCombinationText.Text = "🎮 Нажмите клавишу...";
            KeyBindingBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue) { Opacity = 0.25 };
            KeyBindingBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DodgerBlue);
            RecordIcon.Visibility = Visibility.Collapsed;
            ResetButton.Visibility = Visibility.Collapsed;

            HiddenTextBox.Focus(FocusState.Programmatic);
        }

        private void CancelRecording()
        {
            if (!_isRecording) return;
            _isRecording = false;
            UpdateUI();
        }

        private void HiddenTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_isRecording)
            {
                HiddenTextBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// Форматирует комбинацию клавиш с модификаторами для отображения.
        /// </summary>
        private string FormatKeyCombination(VirtualKey key, VirtualKeyModifiers modifiers)
        {
            var sb = new StringBuilder();

            if (modifiers.HasFlag(VirtualKeyModifiers.Control))
                sb.Append("Ctrl + ");
            if (modifiers.HasFlag(VirtualKeyModifiers.Shift))
                sb.Append("Shift + ");
            if (modifiers.HasFlag(VirtualKeyModifiers.Menu))
                sb.Append("Alt + ");
            if (modifiers.HasFlag(VirtualKeyModifiers.Windows))
                sb.Append("Win + ");

            sb.Append(HotkeyBinding.GetKeyDisplayName((int)key));

            return sb.ToString();
        }

        private void HiddenTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!_isRecording) return;

            var key = e.Key;
            var modifiers = VirtualKeyModifiers.None;

            // Определяем модификаторы через InputKeyboardSource
            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
                modifiers |= VirtualKeyModifiers.Control;
            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
                modifiers |= VirtualKeyModifiers.Shift;
            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down))
                modifiers |= VirtualKeyModifiers.Menu;

            // Показываем текущую комбинацию в реальном времени (кроме клавиш-модификаторов)
            if (key != VirtualKey.Control && key != VirtualKey.Shift && key != VirtualKey.Menu &&
                key != VirtualKey.LeftWindows && key != VirtualKey.RightWindows)
            {
                KeyCombinationText.Text = "🎮 " + FormatKeyCombination(key, modifiers);
            }

            // Игнорируем только клавиши-модификаторы
            if (key == VirtualKey.Control || key == VirtualKey.Shift || key == VirtualKey.Menu ||
                key == VirtualKey.LeftWindows || key == VirtualKey.RightWindows)
            {
                e.Handled = true;
                return;
            }

            // Escape для отмены
            if (key == VirtualKey.Escape)
            {
                CancelRecording();
                e.Handled = true;
                return;
            }

            int keyCode = (int)key;

            // Требуем хотя бы один модификатор (Ctrl, Shift, Alt) для назначения
            if (modifiers == VirtualKeyModifiers.None)
            {
                KeyCombinationText.Text = "🎮 Нужен модификатор (Ctrl/Shift/Alt)...";
                e.Handled = true;
                return;
            }

            // Проверяем, не занята ли комбинация
            if (HotkeyService.Instance != null)
            {
                var existingAction = HotkeyService.Instance.GetActionByCombination(keyCode, modifiers);
                if (existingAction.HasValue && existingAction.Value != _action)
                {
                    _ = ShowCombinationTakenWarning(existingAction.Value);
                    CancelRecording();
                    e.Handled = true;
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

            CancelRecording();
            e.Handled = true;
        }

        private async System.Threading.Tasks.Task ShowCombinationTakenWarning(PlayerAction existingAction)
        {
            var existingBinding = HotkeyService.Instance?.GetBinding(existingAction);
            var actionName = existingBinding?.DisplayActionName ?? existingAction.ToString();

            var dialog = new ContentDialog
            {
                Title = "Комбинация занята",
                Content = $"Эта комбинация клавиш уже используется для действия \"{actionName}\". Пожалуйста, выберите другую комбинацию.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        #endregion

        #region Private Methods

        private void UpdateUI()
        {
            if (_binding == null) return;

            KeyCombinationDisplay = _binding.DisplayKeyCombination;
            EnableCheckBox.IsChecked = _binding.IsEnabled;

            if (Application.Current.Resources.TryGetValue("TextControlBackground", out var bgBrush) && bgBrush is Brush bg)
                KeyBindingBorder.Background = bg;
            if (Application.Current.Resources.TryGetValue("TextControlBorderBrush", out var strokeBrush) && strokeBrush is Brush stroke)
                KeyBindingBorder.BorderBrush = stroke;

            KeyCombinationText.Text = KeyCombinationDisplay;
            RecordIcon.Visibility = Visibility.Visible;

            var defaultBinding = HotkeyBinding.GetDefault(_action);
            ResetButton.Visibility = (_binding.KeyCode != defaultBinding.KeyCode || _binding.Modifiers != defaultBinding.Modifiers)
                ? Visibility.Visible
                : Visibility.Collapsed;

            OnPropertyChanged(nameof(ActionDisplayName));
            OnPropertyChanged(nameof(KeyCombinationDisplay));
            OnPropertyChanged(nameof(IsBindingEnabled));
        }

        private void OnBindingsChanged(object sender, EventArgs e)
        {
            _binding = HotkeyService.Instance?.GetBinding(_action) ?? _binding;
            UpdateUI();
        }

        #endregion

        #region Property Changed

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}