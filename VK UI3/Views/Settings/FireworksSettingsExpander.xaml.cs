using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace VK_UI3.Views.Settings
{
    public sealed partial class FireworksSettingsExpander : Expander
    {
        public FireworksSettingsExpander()
        {
            InitializeComponent();
        }

        private async void TestLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            var fireworks = MainWindow.mainWindow?.Fireworks;
            if (fireworks == null) return;

            // Запоминаем текущее состояние
            bool wasRunning = false;
            try
            {
                // Используем рефлексию для проверки _isRunning, так как поле приватное
                var field = fireworks.GetType().GetField("_isRunning",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                    wasRunning = (bool)field.GetValue(fireworks);
            }
            catch { }

            // Если фейерверки не запущены, запускаем их временно
            if (!wasRunning)
            {
                // Увеличиваем количество ракет для теста
                int originalCount = fireworks.RocketCount;
                double originalInterval = 1.5;
                try
                {
                    var intervalField = fireworks.GetType().GetField("_lastSpawnTimeMs",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                }
                catch { }

                fireworks.RocketCount = 8;
                fireworks.Start();

                // Ждём несколько секунд, чтобы фейерверки поработали
                await Task.Delay(5000);

                // Восстанавливаем настройки и останавливаем
                fireworks.RocketCount = originalCount;
                fireworks.Stop();
            }
        }
    }
}