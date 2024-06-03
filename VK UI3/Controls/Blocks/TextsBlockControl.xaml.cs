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
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Services;
using System.Diagnostics;
using System.Drawing;
using VK_UI3.Services;
using VK_UI3.VKs;
using VK_UI3.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class TextsBlockControl : UserControl
    {
        public TextsBlockControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OwnerCell_DataContextChanged;

        }

        Block block = null;
        private void OwnerCell_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;

            block = (Block)DataContext;


            Title.Text = block.Texts[0].Value;

        }


    }
}
