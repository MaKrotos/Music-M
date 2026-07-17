using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Automation;

namespace VK_UI3.Views.Settings
{
    public sealed class BackDropSetting : ComboBox
    {
        private bool _isLoading = true;
        private bool _isApplying = false;

        public BackDropSetting()
        {
            this.Loaded += BackDropSetting_Loaded1;
        }

        private void BackDropSetting_Loaded1(object sender, RoutedEventArgs e)
        {
            this.Items.Add("null");
            this.Items.Add("Mica (Base)");
            this.Items.Add("Mica (BaseAlt)");
            this.Items.Add("Акрил (Default)");
            this.Items.Add("Прозрачный");

            this.SelectedIndex = 3;
            this.SelectionChanged += BackDropSetting_SelectionChanged;
            this.Loaded += BackDropSetting_Loaded;

            if (Application.Current.Resources.TryGetValue("DefaultComboBoxStyle", out object style))
                this.Style = style as Style;

            AutomationProperties.SetName(this, "Стиль фона");
            AutomationProperties.SetHelpText(this, "Выбирает стиль фонового оформления окна");
        }

        private void BackDropSetting_Loaded(object sender, RoutedEventArgs e)
        {
            _isLoading = true;
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    var set = SettingsTable.GetSetting("backDrop");
                    if (set != null)
                    {
                        this.SelectedIndex = set.settingValue switch
                        {
                            "null" => 0,
                            "mica_base" => 1,
                            "mica_basealt" => 2,
                            "acrylic_default" => 3,
                            "transparent" => 4,
                            _ => 3
                        };
                    }
                    else
                    {
                        this.SelectedIndex = 3;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настройки фона: {ex.Message}");
                    this.SelectedIndex = 3;
                }
                finally
                {
                    _isLoading = false;
                }
            });
        }

        private async void BackDropSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Защита от повторных вызовов во время загрузки или применения
            if (!_isLoading || _isApplying) return;

            // Защита от вызова при программном изменении индекса
            if (sender == null) return;

            try
            {
                _isApplying = true;

                string key = this.SelectedIndex switch
                {
                    0 => "null",
                    1 => "mica_base",
                    2 => "mica_basealt",
                    3 => "acrylic_default",
                    4 => "transparent",
                    _ => "acrylic_default"
                };

                // Сохраняем настройку в БД
                try
                {
                    SettingsTable.SetSetting("backDrop", key);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настройки фона: {ex.Message}");
                }

                // Временно отключаем элемент, чтобы предотвратить повторные события
                this.IsEnabled = false;

                // Даем системе время завершить обработку событий ввода
                await Task.Delay(50);

                // Применяем стиль в потоке UI с защитой от ошибок
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        // Проверяем, что MainWindow существует и инициализирован
                        if (MainWindow.updateMica != null)
                        {
                            MainWindow.updateMica?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("MainWindow.updateMica не инициализирован");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка применения стиля фона: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                    finally
                    {
                        // Возвращаем активность элементу
                        this.IsEnabled = true;
                        _isApplying = false;
                    }
                });

                // Дополнительная задержка на случай, если DispatcherQueue не успел выполнить
                await Task.Delay(100);
                RefreshBackDropAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Критическая ошибка в BackDropSetting_SelectionChanged: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Восстанавливаем состояние в случае ошибки
                this.IsEnabled = true;
                _isApplying = false;
            }
        }

        // Дополнительный метод для безопасного принудительного обновления
        public async Task RefreshBackDropAsync()
        {
            if (_isApplying) return;

            try
            {
                _isApplying = true;
                this.IsEnabled = false;

                await Task.Delay(50);

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        MainWindow.updateMica?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка принудительного обновления фона: {ex.Message}");
                    }
                    finally
                    {
                        this.IsEnabled = true;
                        _isApplying = false;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Критическая ошибка в RefreshBackDropAsync: {ex.Message}");
                this.IsEnabled = true;
                _isApplying = false;
            }
        }
    }
}