﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class DefaultControl : UserControl
    {
        public DefaultControl()
        {

            this.InitializeComponent();
            this.DataContextChanged += DefaultControl_DataContextChanged;
            this.Unloaded += DefaultControl_Unloaded;
        }

        private void DefaultControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged -= DefaultControl_DataContextChanged;
            this.Unloaded -= DefaultControl_Unloaded;
        }

        private void DefaultControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var data = this.DataContext;

            if (DataContext is not Block block)
                return;

            string key = (string.IsNullOrEmpty(block.Layout?.Name) ? block.DataType : $"{block.DataType}_{block.Layout.Name}");
            block_not.Text += " (" + key + ") ";
            // Теперь вы можете использовать данные
        }
    }
}

