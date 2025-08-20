using System;
using Windows.ApplicationModel;

namespace VK_UI3.Helpers
{
    public class Packaged
    {

        public static bool IsPackaged()
        {
            // Проверяем наличие идентификатора пакета
            try
            {
                var package = Package.Current;
                return package != null && !string.IsNullOrEmpty(package.Id.Name);
            }
            catch (InvalidOperationException)
            {
                // Приложение не упаковано
                return false;
            }
        }

    }
}
