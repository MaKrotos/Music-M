using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace VK_UI3.Helpers
{
    public class ExtendedSlider : Slider
    {




    
        public ExtendedSlider()
        {
            this.Loaded += ExtendedSlider_Loaded;

            // Найдите Thumb в шаблоне слайдера
            //  var oldThumb = FindChild<Microsoft.UI.Xaml.Controls.Primitives.Thumb>(this);
            //  var newThumb = new Microsoft.UI.Xaml.Controls.Primitives.Thumb();
            //   thumb = FindChild<Microsoft.UI.Xaml.Controls.Primitives.Thumb>(this);
            // Копировать все доступные свойства


            //   var toolTip = ToolTipService.GetToolTip(thumb) as ToolTip;
            //   toolTip.Content = ThumbToolTipValueConverter.Convert(SecondValue, null, null, null);
            //   ToolTipService.SetToolTip(thumb, toolTip);



        }



        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    var childOfChild = FindChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }



        private void OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // Обновите SecondValue при изменении значения слайдера

        }



        private void ExtendedSlider_Loaded(object sender, RoutedEventArgs e)
        {
            var slider = sender as ExtendedSlider;

        }
    }
}
