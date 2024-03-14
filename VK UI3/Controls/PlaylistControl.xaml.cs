using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using VK_UI3.Controllers;
using VK_UI3.DB;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using Windows.Media.Playlists;
using Image = Microsoft.UI.Xaml.Controls.Image;
using Playlist = MusicX.Core.Models.Playlist;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class PlaylistControl : UserControl
    {
        public PlaylistControl()
        {
            this.InitializeComponent();

            AnimationsChangeFontIcon = new AnimationsChangeFontIcon(PlayPause, this.DispatcherQueue);
          

            DataContextChanged += RecommsPlaylist_DataContextChanged;

            this.Loading += RecommsPlaylist_Loading;
        }


        public void AddRemove_Click(object sender, RoutedEventArgs e)
        {

         
        }

        private void RecommsPlaylist_Loading(FrameworkElement sender, object args)
        {

        }
        AnimationsChangeFontIcon AnimationsChangeFontIcon;
        AnimationsChangeImage animationsChangeImage;

        AudioPlaylist _PlayList { get; set; }
        private void RecommsPlaylist_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            var Data = DataContext;


            AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
         
            if ((DataContext as AudioPlaylist).MainArtists != null && (DataContext as AudioPlaylist).MainArtists.Count != 0)
            Subtitle.Text = (DataContext as AudioPlaylist).MainArtists[0].Name;
            Title.Text = (DataContext as AudioPlaylist).Title;
            _PlayList = (DataContext as AudioPlaylist);

            GridThumbs.Children.Clear();

            if (_PlayList.Photo != null)
            {
                AddImageToGrid((DataContext as AudioPlaylist).Cover, Microsoft.UI.Xaml.Media.Stretch.Fill);
            }
            else if (_PlayList.Thumbs != null)
            {
                int count = _PlayList.Thumbs.Count;
                int index = 0;

                foreach (var item in _PlayList.Thumbs)
                {
                    string photo = item.Photo600 ?? item.Photo1200 ?? item.Photo300 ?? item.Photo34 ?? item.Photo270 ?? item.Photo135 ?? item.Photo68;
                    AddImageToGrid(photo, Microsoft.UI.Xaml.Media.Stretch.UniformToFill, count, index);
                    index++;
                }
            }

        }

        void AddImageToGrid(string photo, Microsoft.UI.Xaml.Media.Stretch stretch, int count = 1, int index = 0)
        {
            Image image = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = stretch
            };

            animationsChangeImage = new AnimationsChangeImage(image, this.DispatcherQueue);
            GridThumbs.Children.Add(image);

            int col = index % 2;
            int row = index / 2;
            int colspan = (count == 1 || (count == 2 && index == 0) || (count == 3 && index == 0)) ? 2 : 1;
            int rowspan = (count == 1 || (count == 2 && index < 2) || (count == 3 && index == 0)) ? 2 : 1;

            Grid.SetColumnSpan(image, colspan);
            Grid.SetRowSpan(image, rowspan);
            Grid.SetColumn(image, col);
            Grid.SetRow(image, row);

            animationsChangeImage.ChangeImageWithAnimation(photo);
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
                    // Отсоединить обработчик событий после выполнения Navigate
                    iVKGetAudio.onListUpdate -= handler;
                });
            };
            iVKGetAudio.onListUpdate += handler;
        }
    }
}
