using System;
using System.Text;
using VK_UI3.Services.Player;
using Windows.System;

namespace VK_UI3.Models
{
    /// <summary>
    /// Модель привязки горячей клавиши к действию плеера.
    /// </summary>
    public class HotkeyBinding
    {
        /// <summary>
        /// Действие плеера.
        /// </summary>
        public PlayerAction Action { get; set; }

        /// <summary>
        /// Основная клавиша комбинации (Win32 виртуальный код клавиши).
        /// </summary>
        public int KeyCode { get; set; }

        /// <summary>
        /// Модификаторы (Ctrl, Shift, Alt, Menu/Windows).
        /// </summary>
        public VirtualKeyModifiers Modifiers { get; set; }

        /// <summary>
        /// Включена ли данная горячая клавиша.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Уникальный идентификатор для RegisterHotKey (генерируется автоматически).
        /// </summary>
        public int HotkeyId { get; set; }

        /// <summary>
        /// Человекочитаемое название действия.
        /// </summary>
        public string DisplayActionName => GetActionDisplayName();

        /// <summary>
        /// Человекочитаемое представление комбинации клавиш.
        /// </summary>
        public string DisplayKeyCombination => GetKeyCombinationDisplayName();

        /// <summary>
        /// Win32 виртуальный код клавиши (для совместимости с VirtualKey).
        /// </summary>
        public VirtualKey Key
        {
            get => (VirtualKey)KeyCode;
            set => KeyCode = (int)value;
        }

        /// <summary>
        /// Возвращает название действия для отображения в UI.
        /// </summary>
        private string GetActionDisplayName()
        {
            return Action switch
            {
                PlayerAction.PlayPause => "Воспроизведение / Пауза",
                PlayerAction.NextTrack => "Следующий трек",
                PlayerAction.PreviousTrack => "Предыдущий трек",
                PlayerAction.VolumeUp => "Громкость +",
                PlayerAction.VolumeDown => "Громкость -",
                PlayerAction.Mute => "Отключить звук",
                PlayerAction.Stop => "Остановить",
                _ => Action.ToString()
            };
        }

        /// <summary>
        /// Возвращает человекочитаемое представление комбинации клавиш.
        /// </summary>
        private string GetKeyCombinationDisplayName()
        {
            if (KeyCode == 0)
                return "Не назначена";

            var sb = new StringBuilder();

            if (Modifiers.HasFlag(VirtualKeyModifiers.Control))
                sb.Append("Ctrl + ");
            if (Modifiers.HasFlag(VirtualKeyModifiers.Shift))
                sb.Append("Shift + ");
            if (Modifiers.HasFlag(VirtualKeyModifiers.Menu))
                sb.Append("Alt + ");
            if (Modifiers.HasFlag(VirtualKeyModifiers.Windows))
                sb.Append("Win + ");

            sb.Append(GetKeyDisplayName(KeyCode));

            return sb.ToString();
        }

        /// <summary>
        /// Возвращает отображаемое имя клавиши по Win32 виртуальному коду.
        /// </summary>
        public static string GetKeyDisplayName(int keyCode)
        {
            return keyCode switch
            {
                0x20 => "Пробел",
                0x25 => "←",
                0x26 => "↑",
                0x27 => "→",
                0x28 => "↓",
                0x2D => "Insert",
                0x2E => "Delete",
                0x24 => "Home",
                0x23 => "End",
                0x22 => "Page Down",
                0x21 => "Page Up",
                0x2C => "Print Screen",
                0x13 => "Pause",
                0x90 => "Num Lock",
                0x14 => "Caps Lock",
                0x5B => "Win",
                // Media keys (0xB0-0xB9)
                0xB0 => "⏯ Media Play",
                0xB1 => "⏮ Media Prev",
                0xB2 => "⏭ Media Next",
                0xB3 => "⏹ Media Stop",
                0xB4 => "🔇 Media Mute",
                0xB5 => "🔊 Media Volume",
                0xB6 => "🔉 Media Volume Down",
                // Function keys
                0x70 => "F1", 0x71 => "F2", 0x72 => "F3", 0x73 => "F4",
                0x74 => "F5", 0x75 => "F6", 0x76 => "F7", 0x77 => "F8",
                0x78 => "F9", 0x79 => "F10", 0x7A => "F11", 0x7B => "F12",
                // Number keys
                >= 0x30 and <= 0x39 => ((char)('0' + (keyCode - 0x30))).ToString(),
                // Numpad
                >= 0x60 and <= 0x69 => "Num " + (keyCode - 0x60),
                // Letters A-Z
                >= 0x41 and <= 0x5A => ((char)('A' + (keyCode - 0x41))).ToString(),
                // OEM keys
                0xBA => ";", 0xBB => "=", 0xBC => ",", 0xBD => "-",
                0xBE => ".", 0xBF => "/", 0xC0 => "`",
                0xDB => "[", 0xDC => "\\", 0xDD => "]", 0xDE => "'",
                _ => $"0x{keyCode:X2}"
            };
        }

        /// <summary>
        /// Сериализует привязку в строку для хранения в БД.
        /// </summary>
        public string Serialize()
        {
            return $"{KeyCode}|{(int)Modifiers}|{(IsEnabled ? 1 : 0)}";
        }

        /// <summary>
        /// Десериализует привязку из строки БД.
        /// </summary>
        public static HotkeyBinding Deserialize(PlayerAction action, string data)
        {
            var binding = new HotkeyBinding { Action = action };

            if (string.IsNullOrEmpty(data))
                return binding;

            try
            {
                var parts = data.Split('|');
                if (parts.Length >= 3)
                {
                    binding.KeyCode = int.Parse(parts[0]);
                    binding.Modifiers = (VirtualKeyModifiers)int.Parse(parts[1]);
                    binding.IsEnabled = parts[2] == "1";
                }
            }
            catch
            {
                // В случае ошибки парсинга возвращаем настройки по умолчанию
            }

            return binding;
        }

        /// <summary>
        /// Создаёт привязку по умолчанию для указанного действия.
        /// </summary>
        public static HotkeyBinding GetDefault(PlayerAction action)
        {
            var binding = new HotkeyBinding { Action = action, IsEnabled = true };

            switch (action)
            {
                case PlayerAction.PlayPause:
                    binding.KeyCode = 0xB0; // VK_MEDIA_PLAY_PAUSE
                    break;
                case PlayerAction.NextTrack:
                    binding.KeyCode = 0xB2; // VK_MEDIA_NEXT_TRACK
                    break;
                case PlayerAction.PreviousTrack:
                    binding.KeyCode = 0xB1; // VK_MEDIA_PREV_TRACK
                    break;
                case PlayerAction.VolumeUp:
                    binding.KeyCode = 0xB5; // VK_VOLUME_UP (0xAF)
                    break;
                case PlayerAction.VolumeDown:
                    binding.KeyCode = 0xB6; // VK_VOLUME_DOWN (0xAE)
                    break;
                case PlayerAction.Mute:
                    binding.KeyCode = 0xB4; // VK_VOLUME_MUTE (0xAD)
                    break;
                case PlayerAction.Stop:
                    binding.KeyCode = 0xB3; // VK_MEDIA_STOP
                    break;
            }

            return binding;
        }
    }
}