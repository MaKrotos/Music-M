using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models.Mix;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages.MixControls
{
    public sealed partial class SImpleStack : UserControl
    {
   
        public SImpleStack()
        {
            this.InitializeComponent();
            this.Loaded += SImpleStack_Loaded;
            this.DataContextChanged += SImpleStack_DataContextChanged;
        
        }
        MixCategory mixCategory;

        private void SImpleStack_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is MixCategory mixcat)
            {
                if (mixCategory == mixcat) return;
                mixOptions.Clear();
                mixCategory = mixcat;
                foreach (var opt in mixcat.Options)
                {
                    mixOptions.Add(opt);

                }
                                
               
            }
        }
        ObservableCollection<MixOption> mixOptions = new ObservableCollection<MixOption>();

        private void SImpleStack_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }

    public class TypeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? DefaultTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        {
            var mixOption = item as MixCategory;
            return mixOption?.Type == "pictured_button_horizontal_group" ? StringTemplate : DefaultTemplate;
        }
    }
    public class CustomElementFactory : IElementFactory
    {
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? DefaultTemplate { get; set; }

        public UIElement GetElement(ElementFactoryGetArgs args)
        {
            var mixOption = args.Data as MixOption;
            if (mixOption?.IconUri != null)
            {
                return (UIElement)StringTemplate.LoadContent();
            }
            return (UIElement)DefaultTemplate.LoadContent();
        }

        public void RecycleElement(ElementFactoryRecycleArgs args)
        {
            // Optional: Add recycling logic here if needed
        }
    }


}
