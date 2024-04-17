using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using System;
using System.Collections.Generic;
using VK_UI3.Helpers.Animations;
using Microsoft.UI.Dispatching;

namespace VK_UI3.Helpers
{
    class SmallHelpers
    {
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
