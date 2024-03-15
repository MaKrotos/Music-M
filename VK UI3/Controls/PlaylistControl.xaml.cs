using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using MusicX.Core.Models.Genius;
using MusicX.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Services;
using VK_UI3.Views;
using VK_UI3.Views.ModalsPages;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.Ecosystem;
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
            titleAnim = new AnimationsChangeText(Title, this.DispatcherQueue);
            descrAnim = new AnimationsChangeText(Subtitle, this.DispatcherQueue);


            DataContextChanged += RecommsPlaylist_DataContextChanged;

            this.Loading += RecommsPlaylist_Loading;
        }
        AnimationsChangeText titleAnim { get; set; }
        AnimationsChangeText descrAnim { get; set; }


        


        public static readonly DependencyProperty PlaylistItemsProperty =
         DependencyProperty.Register(
            "_PlaylistItems",
             typeof(ObservableRangeCollection<AudioPlaylist>),
             typeof(PlaylistControl),
             new PropertyMetadata(default(ObservableRangeCollection<AudioPlaylist>)));

        public DependencyProperty PlaylistItems => PlaylistItemsProperty;
        public ObservableRangeCollection<AudioPlaylist> _PlaylistItems
        {
            get { return (ObservableRangeCollection<AudioPlaylist>)GetValue(PlaylistItemsProperty); }
            set { SetValue(PlaylistItemsProperty, value); }
        }

     

        public void AddRemove_Click(object sender, RoutedEventArgs e)
        {


            try
            {
 
                VK.vkService.AddPlaylistAsync(_PlayList.Id, _PlayList.OwnerId, _PlayList.AccessKey);

              //  snackbarService.Show("Плейлист добавлен", "Плейлист теперь находится в Вашей библиотеке", ControlAppearance.Success);
            }
            catch (Exception ex)
            {
             

            }

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
            _PlayList = (DataContext as AudioPlaylist);
            FadeOutAnimationGrid.Completed += FadeOutAnimationGrid_Completed;
            update();
        }

        private void FadeOutAnimationGrid_Completed(object sender, object e)
        {
            update();
        }

        public void update() {


        

            AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");

            if (_PlayList.Description != null)
            {
                descrAnim.ChangeTextWithAnimation(_PlayList.Description);
            }
            else
            if (_PlayList.MainArtists != null && _PlayList.MainArtists.Count != 0)
                descrAnim.ChangeTextWithAnimation(_PlayList.MainArtists[0].Name);
               
            titleAnim.ChangeTextWithAnimation(_PlayList.Title);
         


            GridThumbs.Children.Clear();

            if (_PlayList.Photo != null)
            {
                AddImageToGrid(_PlayList.Cover, Microsoft.UI.Xaml.Media.Stretch.Fill);
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
            if (_PlayList.Permissions.Edit)
            {
                editAlbum.Visibility = Visibility.Visible;
            }
            else
            {
                editAlbum.Visibility = Visibility.Collapsed;
            }

            if (_PlayList.Permissions.Delete)
            {
                DeleteAlbum.Visibility = Visibility.Visible;
            }
            else
            {
                DeleteAlbum.Visibility = Visibility.Collapsed;
            }
            if (_PlayList.Permissions.Follow)
            {
                AddRemove.Visibility = Visibility.Visible;
            }
            else
            {
                AddRemove.Visibility = Visibility.Collapsed;
            }



            bool isUserPlaylist = _PlayList.OwnerId == AccountsDB.activeAccount.id && _PlayList.Original == null;
            {


            }

            FadeInAnimationGrid.Begin();
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

        private void editAlbum_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;


            var a = new CreatePlayList(_PlayList);
            
            dialog.Content = a;
            dialog.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            a.cancelPressed += (s, e) =>
            {
                if (s != null && e is AudioPlaylist)
                {
                    _PlayList = s as AudioPlaylist;

                    
                    FadeOutAnimationGrid.Begin();
                }

                try
                {
                    dialog.Hide();
                    dialog = null;
                }
                catch
                {

                }
            };

            dialog.ShowAsync();
        }

        private async void DeleteAlbum_Click(object sender, RoutedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                await VK.api.Audio.DeletePlaylistAsync(_PlayList.OwnerId, _PlayList.Id);
                var index = _PlaylistItems.IndexOf(_PlayList);
                if (index != -1)
                    _PlaylistItems.RemoveAt(index);
            });
        }
    }
}
