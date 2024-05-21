using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

using System.Text;
using System.Threading.Tasks;
using VK_UI3.VKs.IVK;
using Windows.Foundation;

namespace VK_UI3.Views.Controls
{
    internal class CustomGridView : GridView
    {

        public CustomGridView()
        {
            this.Loaded += CustomGridView_Loaded;
        }

        private void CustomGridView_Loaded(object sender, RoutedEventArgs e)
        {

            scrollViewer = FindScrollViewer(this);
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged; ;
        }

        // Функция для прокрутки вправо
        public void ScrollRight()
        {
            scrollViewer.ChangeView(scrollViewer.HorizontalOffset + scrollViewer.ActualWidth, null, null);
        }

        // Функция для прокрутки влево
        public void ScrollLeft()
        {
            scrollViewer.ChangeView(scrollViewer.HorizontalOffset - scrollViewer.ActualWidth, null, null);
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollViewer.VerticalScrollMode == ScrollMode.Enabled)
            {
                var isAtBottom = scrollViewer.VerticalOffset > scrollViewer.ScrollableHeight - 50;
                if (isAtBottom)
                {
                    if (loadMore != null)
                        loadMore.Invoke(this, EventArgs.Empty);
                }
            }
            if (scrollViewer.HorizontalScrollMode == ScrollMode.Enabled)
            {
                var isAtBottom = scrollViewer.HorizontalOffset  > scrollViewer.ScrollableWidth -50;
                if (isAtBottom)
                {
                    if (loadMore != null)
                        loadMore.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool CheckIfAllContentIsVisible()
        {
            

            if (scrollViewer.HorizontalScrollMode == ScrollMode.Enabled)
            {
             
                if (scrollViewer.ViewportWidth > scrollViewer.ExtentWidth-50)
                {
                    return true;
                }
            }
            if (scrollViewer.VerticalScrollMode == ScrollMode.Enabled)
            {
                if (scrollViewer.ViewportHeight > scrollViewer.ExtentHeight - 50)
                {
                    return true;
                }
            }






            return false;
        }

        public EventHandler loadMore;
        ScrollViewer scrollViewer;
        public static ScrollViewer FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer sv)
                return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                var svChild = child as ScrollViewer;
                if (svChild != null)
                    return svChild;

                var svFound = FindScrollViewer(child);
                if (svFound != null)
                    return svFound;
            }

            return null;
        }


     
 

    }
}