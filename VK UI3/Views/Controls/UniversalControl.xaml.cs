using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using VK_UI3.VKs;
using System.Threading;
using VK_UI3.VKs.IVK;
using System.Collections;
using System.Net;
using VK_UI3.Controls.Blocks;
using Windows.Foundation;


namespace VK_UI3.Views.Controls
{
    public sealed partial class UniversalControl : UserControl
    {
        public bool disableLoadMode{ get { return gridV.disableLoadMode; } set { gridV.disableLoadMode = value; }  }
        public UniversalControl()
        {
            this.InitializeComponent();


            //    this.Loading += ListPlaylists_Loading;
            this.Loaded += ListPlaylists_Loaded;
            this.Unloaded += ListPlaylists_Unloaded;
            gridView = gridV;
        }
        public ScrollViewer scrollVi { get { return gridV.scrollViewer; } }
        public GridView gridView;

        public void scrollToIndex(int index)
        {
            if (index >= 0 && index < gridView.Items.Count)
            {
                this.DispatcherQueue.TryEnqueue(() => {
                    var container = gridView.ContainerFromIndex(index) as GridViewItem;
                    if (container != null)
                    {
                        var transform = container.TransformToVisual(this);
                        var position = transform.TransformPoint(new Point(0, 0));
                        double itemLeft = position.X;
                        double itemRight = itemLeft + container.ActualWidth;
                        double viewportWidth = scrollVi.ViewportWidth;

                        if (itemLeft < 0 || itemRight > viewportWidth)
                        {
                            double newOffset = itemLeft < 0 ? scrollVi.HorizontalOffset + itemLeft - 40 : scrollVi.HorizontalOffset + itemRight - viewportWidth;
                            scrollVi.ChangeView(newOffset, null, null);
                        }
                    
                    }
                });
            }
        }


        public void scrollToIndexVertical(int index)
        {
            if (index >= 0 && index < gridView.Items.Count)
            {
                this.DispatcherQueue.TryEnqueue(() => {
                    var container = gridView.ContainerFromIndex(index) as GridViewItem;
                    if (container != null)
                    {
                        var transform = container.TransformToVisual(this);
                        var position = transform.TransformPoint(new Point(0, 0));
                        double itemTop = position.Y;
                        double itemBottom = itemTop + container.ActualHeight;
                        double viewportHeight = scrollVi.ViewportHeight;

                        if (itemTop < 0 || itemBottom > viewportHeight)
                        {
                            double newOffset = itemTop < 0 ? scrollVi.VerticalOffset + itemTop - 40 : scrollVi.VerticalOffset + itemBottom - viewportHeight;
                            scrollVi.ChangeView(null, newOffset, null);
                        }
                    }
                });
            }
        }



        public Action loadMore { get; set; } = nul;

        private static void nul()
        {

        }


        public IEnumerable ItemsSource
        {
            get;
            set;
        }


        public ItemsPanelTemplate ItemsPanelTemplate
        {
            get;
            set;
        }
        public DataTemplate ItemTemplate
        {
            get;
            set;
        }


        public double incrementalLoadingThreshold
        {
            get;
            set;
        } = 0.5;
        public bool itsAll { get; set; }


        private void ListPlaylists_Loaded(object sender, RoutedEventArgs e)
        {

            gridV.ItemsPanel = ItemsPanelTemplate;
            gridV.ItemTemplate = ItemTemplate;
            if (gridV.CheckIfAllContentIsVisible() && !disableLoadMode)
                loadMore?.Invoke();
            gridV.loadMore += load;
            gridV.LeftChange += LeftChange;
            gridV.RightChange += RightChange;
        }

        private void ListPlaylists_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ListPlaylists_Unloaded;
 
            gridV.loadMore -= load;
            gridV.LeftChange -= LeftChange;
            gridV.RightChange -= RightChange;
        }

        private void load(object sender, EventArgs e)
        {

            if (loadMore != null && !disableLoadMode)
            loadMore?.Invoke();
        }

        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public bool CheckIfAllContentIsVisible()
        {
            
            return gridV.CheckIfAllContentIsVisible();
        }

        Block localBlock;

        /*
        private void ListPlaylists_Loading(FrameworkElement sender, object args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;
                playlists.Clear();
                localBlock = block;

                if (block.Meta != null && block.Meta.anchor == "vibes")

                {
                    gridV.ItemTemplate = this.Resources["compact"] as DataTemplate;
                }
                else
                {
                    gridV.ItemTemplate = this.Resources["default"] as DataTemplate;
                }

                var pl = (DataContext as Block).Playlists;


                foreach (var item in pl)
                {
                //    playlists.Add(item);
                }
              
            }
            catch (Exception ex)
            {



            }
        }
        */


        private void LeftChange(object sender, EventArgs e)
        {
            LeftCh();
        }
        private void RightChange(object sender, EventArgs e)
        {
            RightCh();
        }
     
        private void RightCh()
        {
            if (gridV.showRight)
            {
                RightGrid.Visibility = Visibility.Visible;

                FadeOutAnimationRightBTN.Pause();
                FadeInAnimationRightBTN.Begin();

            }
            else
            {
                if (itsAll)
                {
                    FadeInAnimationRightBTN.Pause();
                    FadeOutAnimationRightBTN.Begin();
                }
            }
        }
        private void FadeOutAnimationRightBTN_Completed(object sender, object e)
        {
            if (!gridV.showRight || !enterpoint)
            {
                RightGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void FadeOutAnimationLeftBTN_Completed(object sender, object e)
        {
            if (!gridV.showLeft || !enterpoint)
            {
                LeftGrid.Visibility = Visibility.Collapsed;
            }

        }

        private void gridCh_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            enterpoint = true;
            var a = gridV.ShowLeftChecker;
            a = gridV.ShowRightChecker;
            LeftCh();
            RightCh();
        }
        bool enterpoint;
        private void gridCh_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            this.enterpoint = false;
            FadeInAnimationLeftBTN.Pause();
            FadeOutAnimationLeftBTN.Begin();
            FadeInAnimationRightBTN.Pause();
            FadeOutAnimationRightBTN.Begin();
        }
        private void LeftCh()
        {
            if (gridV.showLeft)
            {
                LeftGrid.Visibility = Visibility.Visible;

                FadeOutAnimationLeftBTN.Pause();
                FadeInAnimationLeftBTN.Begin();
            }
            else
            {
                FadeInAnimationLeftBTN.Pause();
                FadeOutAnimationLeftBTN.Begin();

            }
        }
     


        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
            gridV.ScrollRight();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
            gridV.ScrollLeft();
        }

    }

  
}
