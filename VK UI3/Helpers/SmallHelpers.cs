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




        public static void AddImagesToGrid(Grid grid, List<string> photos, DispatcherQueue dispatcherQueue)
        {
            dispatcherQueue.TryEnqueue(async () =>
            {
                grid.Children.Clear();
            });


         
            for (int i = 0; i < photos.Count; i++)
            {
                int b = i;
                dispatcherQueue.TryEnqueue(async () =>
                {

            
                    Image image = new Image
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Stretch = Stretch.UniformToFill
                    };

                    var animationsChangeImage = new AnimationsChangeImage(image, dispatcherQueue);
                    grid.Children.Add(image);

                    int col = b % 2;
                    int row = b / 2;
                    int colspan = (photos.Count == 1) ? 2 : 1;
                    int rowspan = (photos.Count == 1 || (photos.Count == 2 && b < 2) || (photos.Count == 3 &&   b == 0)) ? 2 : 1;
                    if (b == 2 && photos.Count == 3) 
                        col = 2;

             


                    Grid.SetColumnSpan(image, colspan);
                    Grid.SetRowSpan(image, rowspan);
                    Grid.SetColumn(image, col);
                    Grid.SetRow(image, row);
                    animationsChangeImage.ChangeImageWithAnimation(photos[b]);
                });
                
            }
        }

        public static void AddImagesToGrid(Grid grid, string photo, DispatcherQueue dispatcherQueue)
        {
            grid.Children.Clear();
            Image image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Fill
            };

            Grid.SetColumnSpan(image, 2);
            Grid.SetRowSpan(image, 2);

            var animationsChangeImage = new AnimationsChangeImage(image, dispatcherQueue);
            grid.Children.Add(image);
            animationsChangeImage.ChangeImageWithAnimation(photo);
        }

    
    }
}
