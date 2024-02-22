using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using System;
using VK_UI3.Controllers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.VKs.IVK;
using Windows.Media.Playlists;
using Playlist = MusicX.Core.Models.Playlist;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class RecommsPlaylist : UserControl
    {
        public RecommsPlaylist()
        {
            this.InitializeComponent();

            AnimationsChangeFontIcon = new AnimationsChangeFontIcon(PlayPause, this.DispatcherQueue);
            animationsChangeImage = new AnimationsChangeImage(Cover, this.DispatcherQueue);

            DataContextChanged += RecommsPlaylist_DataContextChanged;
          
            this.Loading += RecommsPlaylist_Loading;
        }

       
        private void RecommsPlaylist_Loading(FrameworkElement sender, object args)
        {
           
        }
        AnimationsChangeFontIcon AnimationsChangeFontIcon;
        AnimationsChangeImage animationsChangeImage;

        MusicX.Core.Models.Playlist _PlayList { get; set; }
        private void RecommsPlaylist_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            var Data = DataContext;


            AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
            animationsChangeImage.ChangeImageWithAnimation((DataContext as Playlist).Cover);
            Subtitle.Text = (DataContext as Playlist).Subtitle;
            Title.Text = (DataContext as Playlist).Title;
            _PlayList = (DataContext as Playlist);
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
            if (e.GetCurrentPoint(sender as UIElement).Properties.IsLeftButtonPressed)
            {
                if (iVKGetAudio == null)
                    MainView.OpenPlayList(_PlayList);
                else
                {
                    MainView.OpenPlayList(iVKGetAudio);
                }
            }
        }


        IVKGetAudio iVKGetAudio = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uE916");
            iVKGetAudio = new PlayListVK(_PlayList, this.DispatcherQueue);



            EventHandler handler = null;
            handler = (sender, e) => {
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    if (iVKGetAudio.listAudio.Count == 0)
                    {
                        AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
                        return;
                    }

                    iVKGetAudio.currentTrack = 0;
                    AudioPlayer.PlayList(iVKGetAudio);
                    AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uE769");
                    // ����������� ���������� ������� ����� ���������� Navigate
                    iVKGetAudio.onListUpdate -= handler;
                });
            };
            iVKGetAudio.onListUpdate += handler;
        }
    }
}