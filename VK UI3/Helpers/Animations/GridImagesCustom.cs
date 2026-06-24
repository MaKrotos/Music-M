using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;

namespace VK_UI3.Helpers.Animations
{
    class GridImagesCustom : Grid
    {
        private List<string> photos = null;
        private string photo = null;

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(
                nameof(Stretch),
                typeof(Stretch),
                typeof(GridImagesCustom),
                new PropertyMetadata(Stretch.UniformToFill, OnStretchChanged));

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        private static void OnStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as GridImagesCustom;
            control?.UpdateImagesStretch();
        }

        private void UpdateImagesStretch()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                foreach (var child in this.Children)
                {
                    if (child is Image image)
                    {
                        image.Stretch = Stretch;
                    }
                }
            });
        }

        public void AddImagesToGrid(List<string> photos)
        {
            if (this.photos == photos) return;
            this.photos = photos;

            DispatcherQueue.TryEnqueue(() =>
            {
                this.Children.Clear();

                for (int i = 0; i < photos.Count; i++)
                {
                    int b = i;
                    Image image = new Image
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Stretch = Stretch
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
                }
            });
        }

        public void AddImagesToGrid(string photo)
        {
            if (this.photo == photo) return;
            this.photo = photo;

            DispatcherQueue.TryEnqueue(() =>
            {
                this.Children.Clear();

                Image image = new Image
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Stretch = Stretch
                };

                Grid.SetColumnSpan(image, 2);
                Grid.SetRowSpan(image, 2);

                var animationsChangeImage = new AnimationsChangeImage(image, this.DispatcherQueue);
                this.Children.Add(image);
                animationsChangeImage.ChangeImageWithAnimation(photo);
            });
        }
    }
}