using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using Microsoft.UI.Xaml.Data;
using System;
using System.Diagnostics;

namespace VK_UI3.Helpers
{
    public class ExtendedSlider : Slider
    {



        // РћРїСЂРµРґРµР»РёС‚Рµ SecondValue РєР°Рє СЃРІРѕР№СЃС‚РІРѕ Р·Р°РІРёСЃРёРјРѕСЃС‚Рё
        public static readonly DependencyProperty SecondValueProperty =
            DependencyProperty.Register("SecondValue", typeof(double), typeof(ExtendedSlider), new PropertyMetadata(0.0));

        public double SecondValue
        {
            get { return (double)GetValue(SecondValueProperty); }
            set
            {
                SetValue(SecondValueProperty, value);

                if (thumb != null)
                {
                    var toolTipObject = ToolTipService.GetToolTip(thumb);
                    if (toolTipObject != null)
                    {

                        var toolTip = ToolTipService.GetToolTip(thumb) as ToolTip;

                        toolTip.Content = ThumbToolTipValueConverter.Convert(SecondValue, null, null, null);
                        ToolTipService.SetToolTip(thumb, toolTip);
                    }
                    thumb.Margin = new Thickness((SecondValue /Minimum - Maximum), 0, 0, 0);
                }
               
            }
        }
        Microsoft.UI.Xaml.Controls.Primitives.Thumb thumb;
        FrameworkElement track;
        private FrameworkElement trackRectangle;
        private FrameworkElement trackRectangleVert;
        public ExtendedSlider()
        {
            this.Loaded += (s, e) =>
            {
                // РќР°Р№РґРёС‚Рµ Thumb РІ С€Р°Р±Р»РѕРЅРµ СЃР»Р°Р№РґРµСЂР°
                var oldThumb = FindChild<Microsoft.UI.Xaml.Controls.Primitives.Thumb>(this);
                var newThumb = new Microsoft.UI.Xaml.Controls.Primitives.Thumb();
                thumb = FindChild<Microsoft.UI.Xaml.Controls.Primitives.Thumb>(this);
                // РљРѕРїРёСЂРѕРІР°С‚СЊ РІСЃРµ РґРѕСЃС‚СѓРїРЅС‹Рµ СЃРІРѕР№СЃС‚РІР°
              

                var toolTip = ToolTipService.GetToolTip(thumb) as ToolTip;
                toolTip.Content = ThumbToolTipValueConverter.Convert(SecondValue, null, null ,null);
                ToolTipService.SetToolTip(thumb, toolTip);

              
            };
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
            // РћР±РЅРѕРІРёС‚Рµ SecondValue РїСЂРё РёР·РјРµРЅРµРЅРёРё Р·РЅР°С‡РµРЅРёСЏ СЃР»Р°Р№РґРµСЂР°

        }

    }


}

