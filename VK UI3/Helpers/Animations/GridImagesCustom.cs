using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_UI3.Helpers.Animations
{
    class GridImagesCustom : Grid
    {
        List<string> photos = null; 
        public void AddImagesToGrid(List<string> photos)
        {
            if (this.photos == photos) return;
            this.photos = photos;

            this.DispatcherQueue.TryEnqueue(async () =>
            {
                this.Children.Clear();
            });
            for (int i = 0; i < photos.Count; i++)
            {
                int b = i;
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    Image image = new Image
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Stretch = Stretch.UniformToFill
                    };
                    var animationsChangeImage = new AnimationsChangeImage(image, this.DispatcherQueue);
                    this.Children.Add(image);
                    int col = b % 2;
                    int row = b / 2;
                    int colspan = (photos.Count == 1) ? 2 : 1;
                    int rowspan = (photos.Count == 1 || (photos.Count == 2 && b < 2) || (photos.Count == 3 && b == 0)) ? 2 : 1;
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
        string photo = null;
        public void AddImagesToGrid(string photo)
        {
            if (this.photo == photo) return;
            this.Children.Clear();
            Image image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Fill
            };

            Grid.SetColumnSpan(image, 2);
            Grid.SetRowSpan(image, 2);

            var animationsChangeImage = new AnimationsChangeImage(image, this.DispatcherQueue);
            this.Children.Add(image);
            animationsChangeImage.ChangeImageWithAnimation(photo);
        }




    }




}
