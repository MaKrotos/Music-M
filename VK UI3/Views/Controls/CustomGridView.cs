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
        public EventHandler RightChange;
        public EventHandler LeftChange;
        private void CustomGridView_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = FindScrollViewer(this);
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }



        bool _showLeft = false;
        bool _showRight = false;

        public bool showLeft
        {
            get { return _showLeft; }
            set
            {
                if (_showLeft == value) return;
                _showLeft = value;
                if (LeftChange != null)
                    LeftChange.Invoke(this, EventArgs.Empty);
            }
        }
        public bool showRight
        {
            get { return _showRight; }
            set
            {
                if (_showRight == value) return;
                _showRight = value;
                if (RightChange != null)
                    RightChange.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ShowRightChecker { get {
                if (scrollViewer == null) return false;
                var isAtRight = scrollViewer.HorizontalOffset > scrollViewer.ScrollableWidth - 10;
                showRight = !isAtRight;
                return !isAtRight;
            } }

        public bool ShowLeftChecker
        {
            get
            {
                if (scrollViewer == null) return false;
                var isAtLeft = scrollViewer.HorizontalOffset == 0;
                showLeft = !isAtLeft;
                return !isAtLeft;
            }
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
        private object _lock = new object();
        private bool lockVertical = false;
        private bool lockHorizontal = false;
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            
                if (scrollViewer.VerticalScrollMode == ScrollMode.Enabled)
                {
                    var isAtBottom = scrollViewer.VerticalOffset > scrollViewer.ScrollableHeight - 50;
                    if (isAtBottom)
                    {
                        lock (_lock)
                        {
                            if (loadMore != null)
                            {
                                if (!lockVertical)
                                {
                                    lockVertical = true;
                                    loadMore.Invoke(this, EventArgs.Empty);
                                }
                            }
                        }
                    }
                    else
                    {
                        lockVertical = false;
                    }
                }

                if (scrollViewer.HorizontalScrollMode == ScrollMode.Enabled)
                {
                    var isAtRight = scrollViewer.HorizontalOffset > scrollViewer.ScrollableWidth - 50;
                    if (isAtRight)
                    {
                        lock (_lock)
                        {
                            if (loadMore != null)
                            {
                                if (!lockHorizontal)
                                {
                                    lockHorizontal = true;
                                    loadMore.Invoke(this, EventArgs.Empty);
                                }
                            }
                        }
                  
                    }
                    else
                    {
                      
                  
                    }

                var a = ShowLeftChecker;
                a = ShowRightChecker;
                }
            
        }

  
        public bool CheckIfAllContentIsVisible()
        {
            if (scrollViewer == null) return false;

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

        public ScrollViewer _scrollViewer;

        public ScrollViewer scrollViewer { 
            get 
            { if (_scrollViewer == null)
                    _scrollViewer = FindScrollViewer(this); 
                return _scrollViewer; 
            }
            set { _scrollViewer = value; }
        }
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