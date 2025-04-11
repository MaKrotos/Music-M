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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Notification
{
    public sealed partial class NotifController : UserControl
    {
        public NotifController()
        {
            this.InitializeComponent();
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is not Notification notification)
                return;
            

            HeaderB.Text = notification.header;
            TextB.Text = notification.Message;

            if (string.IsNullOrEmpty(notification.header))
            {
                HeaderB.Visibility = Visibility.Collapsed;
            }
            else
            {
                HeaderB.Visibility = Visibility.Visible;
            }

            if (string.IsNullOrEmpty(notification.Message))
            {
                TextB.Visibility = Visibility.Collapsed;
            }
            {
                TextB.Visibility = Visibility.Visible;
            }

            BTNsGrid.Children.Clear();
            BTNsGrid.ColumnDefinitions.Clear();
            if (notification.button1 != null)
            {
                var btn = new Button();
                btn.Content = notification.button1.Text;
                if (notification.button1.BtnAction != null)
                btn.Click += (sender, e) => notification.button1.BtnAction();
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                BTNsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                Grid.SetColumn(btn, 0);
                BTNsGrid.Children.Add(btn);
            }

            if (notification.button2 != null)
            {
                var btn = new Button();
                btn.Content = notification.button2.Text;
                if (notification.button1.BtnAction != null)
                btn.Click += (sender, e) => notification.button2.BtnAction();
                btn.HorizontalAlignment = HorizontalAlignment.Stretch;
                BTNsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                Grid.SetColumn(btn, 1);
                BTNsGrid.Children.Add(btn);
            }
            /*

            if (notification.button1 != null)
            {
                BTNsGrid.add

                txtBTN1.Text = notification.button1.Text;
                BTN1.Visibility = Visibility.Visible;
            }
            else
            { 
                BTN1.Visibility = Visibility.Collapsed;
            }

            if (notification.button2 != null)
            {
                txtBTN2.Text = notification.button2.Text;
                BTN2.Visibility = Visibility.Visible;
            }
            else
            {
                BTN2.Visibility = Visibility.Collapsed;
            }
            */
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Notification notification)
            {
                return;
            }
            notification.Delete();
        }
    }
}
