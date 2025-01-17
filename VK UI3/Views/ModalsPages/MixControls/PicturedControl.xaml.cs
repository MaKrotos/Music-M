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
using VK_UI3.Controls.Blocks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages.MixControls
{
    public sealed partial class PicturedControl : UserControl
    {
   
        public PicturedControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += SImpleStack_DataContextChanged;
            this.Unloaded += PicturedControl_Unloaded;


        }

       
        private void PicturedControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged -= SImpleStack_DataContextChanged;
            this.Unloaded -= PicturedControl_Unloaded;
        }

        MixCategory mixCategory;

        private void SImpleStack_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is MixCategory mixcat)
            {
                if (mixCategory == mixcat) return;
                mixOptions.Clear();
                TitleText.Text = mixcat.Title;

                mixCategory = mixcat;
                foreach (var opt in mixcat.Options)
                {
                    mixOptions.Add(opt);

                }
                                
               
            }
        }
        ObservableCollection<MixOption> mixOptions = new ObservableCollection<MixOption>();

       
    }

  


}
