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


namespace VK_UI3.Views.Controls
{
    public sealed partial class BannerControlUnivers : UserControl
    {
        public BannerControlUnivers()
        {
            this.InitializeComponent();


            //    this.Loading += ListPlaylists_Loading;
            this.Loaded += ListPlaylists_Loaded;
            this.Unloaded += ListPlaylists_Unloaded;
        
        }
 
        public GridView gridView;


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

        public bool itsAll { get; set; }


        private void ListPlaylists_Loaded(object sender, RoutedEventArgs e)
        {

        
        }

        private void ListPlaylists_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= ListPlaylists_Unloaded;
 
          
        }

        private void load(object sender, EventArgs e)
        {

          
        }

        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public bool CheckIfAllContentIsVisible()
        {

            return false;
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
         
        }
        private void FadeOutAnimationRightBTN_Completed(object sender, object e)
        {
           
        }

        private void FadeOutAnimationLeftBTN_Completed(object sender, object e)
        {
        

        }

        private void gridCh_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
    
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

        }
     


        private void ScrollRight_Click(object sender, RoutedEventArgs e)
        {
          // gridV.ScrollRight();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
        {
         //   gridV.ScrollLeft();
        }

    }

  
}
