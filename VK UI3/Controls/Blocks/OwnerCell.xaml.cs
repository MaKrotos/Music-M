using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class OwnerCell : UserControl
    {
        public OwnerCell()
        {
            this.InitializeComponent();
            this.DataContextChanged += OwnerCell_DataContextChanged;
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(PhotoOwner, this.DispatcherQueue);
        }
        Helpers.Animations.AnimationsChangeImage animationsChangeImage; 
        Block block = null;
        private void OwnerCell_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            block = (Block)DataContext;

            animationsChangeImage.ChangeImageWithAnimation(block.MusicOwners[0].Images.Last().Url);
        }

        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (block.MusicOwners[0].Url != null)
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri(block.MusicOwners[0].Url));
        }
    }
}
