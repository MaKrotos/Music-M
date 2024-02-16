using Microsoft.AppCenter.Analytics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
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
using VK_UI3.Helpers.Animations;
using VK_UI3.Services;
using VK_UI3.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class RecommsPlaylist : UserControl
    {
        public RecommsPlaylist()
        {
            this.InitializeComponent();

            animationsChangeIcon = new AnimationsChangeIcon(PlayPauseIcon);
            animationsChangeImage = new AnimationsChangeImage(Cover, this.DispatcherQueue);

            DataContextChanged += RecommsPlaylist_DataContextChanged;
          
            this.Loading += RecommsPlaylist_Loading;
        }

       
        private void RecommsPlaylist_Loading(FrameworkElement sender, object args)
        {
           
        }
        AnimationsChangeIcon animationsChangeIcon;
        AnimationsChangeImage animationsChangeImage;
     

        private void RecommsPlaylist_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            var Data = DataContext;


            animationsChangeIcon.ChangeSymbolIconWithAnimation(Symbol.Play);
            animationsChangeImage.ChangeImageWithAnimation((DataContext as Playlist).Cover);
            Subtitle.Text = (DataContext as Playlist).Subtitle;
            Title.Text = (DataContext as Playlist).Title;
        }
        bool entered;
        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            entered = true;

           // Symbol symbol = dataTrack.PlayThis ? Symbol.Pause : Symbol.Play;
           /*
            if (GridPlayIcon.Opacity != 0)
            {
                changeIconPlayBTN.ChangeSymbolIconWithAnimation(symbol);
            }
            else
            {
                PlayBTN.Symbol = symbol;
            }
           */

            FadeOutAnimationGridPlayIcon.Pause();
            FadeInAnimationGridPlayIcon.Begin();
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FadeInAnimationGridPlayIcon.Pause();
            FadeOutAnimationGridPlayIcon.Begin();
      
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

          //  MainView.OpenSection(artist.Id, SectionType.Artist);

          //  var notificationService = StaticService.Container.GetRequiredService<NavigationService>();

          //  notificationService.OpenExternalPage(new PlaylistView(Playlist));
        }
    }
}
