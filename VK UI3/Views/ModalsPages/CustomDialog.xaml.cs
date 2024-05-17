using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CustomDialog : ContentDialog
    {
        public CustomDialog()
        {
            this.InitializeComponent();
         
        }

        private void ContentDialog_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var a = this;

            Style defaultStyle = (Style)Application.Current.Resources["DefaultContentDialogStyle"];

            // Перебрать все установщики в стиле
            foreach (SetterBase setterBase in defaultStyle.Setters)
            {
                if (setterBase is Setter setter)
                {
                    // Вывести свойство и значение установщика
                    var prop = setter.Property.ToString();
                    Console.WriteLine($"Property: {prop}, Value: {setter.Value}");
                   
                }
            }
            var l = defaultStyle.Setters.Count;
            for (int i = 0; i < l; i++)
            {

                defaultStyle.Setters.RemoveAt(0);
            }
            this.Style = defaultStyle;

          
        }
    }
}
