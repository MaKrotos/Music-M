using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace VK_UI3.Helpers
{
    class SmallHelpers
    {
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
            StringBuilder result = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        public static ScrollViewer FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer sv)
                return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                var svChild = child as ScrollViewer;
                if (svChild != null)
                    return svChild;

                var svFound = FindScrollViewer(child);
                if (svFound != null)
                    return svFound;
            }

            return null;
        }


        public static async Task<string> GetPathDataAsync(string filePath)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(filePath));
            var svgContent = await FileIO.ReadTextAsync(file);
            var doc = XDocument.Parse(svgContent);
            XNamespace ns = "http://www.w3.org/2000/svg";
            var pathElement = doc.Descendants(ns + "path").FirstOrDefault();
            if (pathElement != null)
            {
                var pathData = pathElement.Attribute("d")?.Value;
                return pathData;
            }
            return null;
        }





    }
}
