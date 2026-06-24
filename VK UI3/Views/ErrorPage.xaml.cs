using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace VK_UI3.Views
{
    public sealed partial class ErrorPage : Page
    {
        public event EventHandler RetryRequested;

        public ErrorPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Проверяем, является ли параметр исключением
            if (e.Parameter is Exception ex)
            {
                SetError(ex);
            }
            else if (e.Parameter is string errorMessage)
            {
                // Для обратной совместимости, если передается строка
                SetError(errorMessage);
            }
        }

        // Метод для установки сообщения об ошибке из исключения
        public void SetError(Exception ex)
        {
            string errorText = $"❌ Сообщение: {ex.Message}\n\n";

            // 📌 ПОЛНЫЙ стек вызовов (включая асинхронный)
            errorText += $"📚 Полный стек вызовов:\n{GetFullStackTrace(ex)}\n\n";

            // Добавляем информацию об источнике
            errorText += $"📌 Источник: {ex.Source ?? "Не указан"}\n";

            // Добавляем внутреннее исключение, если есть
            if (ex.InnerException != null)
            {
                errorText += $"\n🔍 Внутреннее исключение:\n" +
                            $"Сообщение: {ex.InnerException.Message}\n" +
                            $"Стек: {GetFullStackTrace(ex.InnerException)}";
            }

            // Добавляем тип исключения
            errorText += $"\n\n📋 Тип: {ex.GetType().FullName}";

            // Добавляем данные исключения (если есть)
            if (ex.Data.Count > 0)
            {
                errorText += $"\n\n📊 Дополнительные данные:";
                foreach (System.Collections.DictionaryEntry entry in ex.Data)
                {
                    errorText += $"\n  {entry.Key}: {entry.Value}";
                }
            }

            ErrorDetailsText.Text = errorText;
        }

        // Метод для получения ПОЛНОГО стека вызовов
        private string GetFullStackTrace(Exception ex)
        {
            // 1. Используем обычный StackTrace
            string stackTrace = ex.StackTrace ?? "Стек вызовов недоступен";

            // 2. Пытаемся получить более полный стек через ToString()
            string fullTrace = ex.ToString();

            // 3. Если это AggregateException, добавляем все внутренние исключения
            if (ex is AggregateException aggEx)
            {
                fullTrace += "\n\n📦 Вложенные исключения (AggregateException):";
                foreach (var inner in aggEx.InnerExceptions)
                {
                    fullTrace += $"\n---\n{GetFullStackTrace(inner)}";
                }
            }

            return fullTrace;
        }

        // Перегрузка для строки (оставляем для совместимости)
        public void SetError(string errorMessage)
        {
            ErrorDetailsText.Text = errorMessage;
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            // Вызываем событие, чтобы главное окно могло обработать повторную попытку
            RetryRequested?.Invoke(this, EventArgs.Empty);

            // Также вызываем ваш статический метод, если он существует
            MainWindow.onRefreshClickedvoid();
        }

        private async void CopyErrorButton_Click(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(ErrorDetailsText.Text);
            Clipboard.SetContent(dataPackage);

            CopyErrorButton.Content = "✅ Скопировано!";
            await System.Threading.Tasks.Task.Delay(2000);
            CopyErrorButton.Content = "📋 Копировать ошибку";
        }
    }
}