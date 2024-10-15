using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Controls
{
    public sealed partial class ChoosenControl : Grid
    {

        public enum BlockSizing
        {
            FullSize,
            FilledSize

        }


        public ChoosenControl()
        {
            this.InitializeComponent();
            this.Loaded += ChoosenControl_Loaded; ;
            this.Loading += ChoosenControl_Loading; ;
            this.Unloaded += ChoosenControl_Unloaded;
            this.SizeChanged += ChoosenControl_SizeChanged;



        }

        private void ChoosenControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeBlocks();
        }

        public new UIElementCollection Buttons { get { return innexGridCh.Children; } }
        private void ChoosenControl_Loaded(object sender, RoutedEventArgs e)
        {

            resizeBlocks();

        }



        private void ChoosenControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ChoosenControl_Unloaded;
            this.Loading -= ChoosenControl_Loading;
            this.Loaded -= ChoosenControl_Loaded; ;
        }



        private BlockSizing _sizeBlock = BlockSizing.FilledSize;
        public BlockSizing sizeBlock
        {
            get { return _sizeBlock; }
            set
            {
                _sizeBlock = value;
                resizeBlocks();
            }
        }

        private void resizeBlocks()
        {

            foreach (var item in Buttons)
            {
                if (item is Control)
                {
                    if (sizeBlock == BlockSizing.FullSize)
                    {
                        (item as Control).Width = Math.Min(innexGridCh.Width, innexGridCh.MaxWidth) / Buttons.Count();
                        innexGridCh.Width = double.NaN;
                    }
                    else
                    {
                        (item as Control).Width = Double.NaN;
                    }
                }
            }
            myCanvas.Width = (Buttons[0] as Control).ActualWidth;
            myCanvas.Height = this.ActualHeight;
        }


        private void ChoosenControl_Loading(FrameworkElement sender, object args)
        {
            var i = 0;


            foreach (var item in Buttons)
            {


                if (item is Control control)
                {
                    var columnDef = new ColumnDefinition();
                    columnDef.Width = new GridLength(1, GridUnitType.Star);
                    innexGridCh.ColumnDefinitions.Add(columnDef);

                    control.CornerRadius = new CornerRadius(0);



                    // Set Grid.Row property
                    control.HorizontalAlignment = HorizontalAlignment.Stretch;
                    control.VerticalAlignment = VerticalAlignment.Stretch;
                    SetRow(control, 0);
                    SetColumn(control, i++);

                    control.Tapped += Control_Tapped;

                }

            }


        }

        private int _choosen = 0;
        public int choosen
        {
            get { return _choosen; }
            set
            {
                _choosen = value;
                var width = (Buttons[_choosen] as Control).ActualWidth;
                var height = (Buttons[_choosen] as Control).ActualHeight;

                var button = Buttons[_choosen] as Control;
                var parentElement = button.Parent as UIElement;

                // Получаем позицию кнопки относительно родительского элемента
                GeneralTransform transform = button.TransformToVisual(parentElement);
                Point position = transform.TransformPoint(new Point(0, 0));

                var x = position.X;
                var y = position.Y;



                AnimateCanvasMove(x, y, 200);
                AnimateCanvasResize(width, height, 200);
            }
        }

        private void Control_Tapped(object sender, TappedRoutedEventArgs e)
        {
            choosen = this.Buttons.IndexOf(sender as UIElement);
        }

        private Brush _colorPanel = (Brush)Application.Current.Resources["AccentAAFillColorSecondaryBrush"];


        public void SetCanvasBackground(Brush background)
        {
            myCanvas.Background = background;
        }

        private Storyboard moveStoryboard;
        private Storyboard resizeStoryboard;

        public void AnimateCanvasMove(double toX, double toY, double duration)
        {
            // Stop ongoing animation
            moveStoryboard?.Pause();

            var cubicEase = new CubicEase
            {
                EasingMode = EasingMode.EaseOut // Можно также использовать EaseIn или EaseInOut
            };

            var animationX = new DoubleAnimation
            {
                To = toX,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                EasingFunction = cubicEase
            };



            var animationY = new DoubleAnimation
            {
                To = toY,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                EasingFunction = cubicEase
            };

            moveStoryboard = new Storyboard();
            moveStoryboard.Children.Add(animationX);
            moveStoryboard.Children.Add(animationY);

            Storyboard.SetTarget(animationX, myCanvas);
            Storyboard.SetTargetProperty(animationX, "(Canvas.Left)");

            Storyboard.SetTarget(animationY, myCanvas);
            Storyboard.SetTargetProperty(animationY, "(Canvas.Top)");

            moveStoryboard.Begin();
        }

        public void AnimateCanvasResize(double toWidth, double toHeight, double duration)
        {
            // Stop ongoing animation
            resizeStoryboard?.Pause();

            var widthAnimation = new DoubleAnimation
            {
                To = toWidth,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            };
            var heightAnimation = new DoubleAnimation
            {
                To = toHeight,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            };

            resizeStoryboard = new Storyboard();
            resizeStoryboard.Children.Add(widthAnimation);
            resizeStoryboard.Children.Add(heightAnimation);

            Storyboard.SetTarget(widthAnimation, myCanvas);
            Storyboard.SetTargetProperty(widthAnimation, "Width");

            Storyboard.SetTarget(heightAnimation, myCanvas);
            Storyboard.SetTargetProperty(heightAnimation, "Height");

            resizeStoryboard.Begin();
        }




    }
}
