using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using MusicX.Core.Models.Genius;
using MusicX.Core.Services;
using NAudio.Gui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
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
            AudioPlayer.oniVKUpdate += AudioPlayer_oniVKUpdate;


            DataContextChanged += RecommsPlaylist_DataContextChanged;

            this.Loading += RecommsPlaylist_Loading;
        }

        private void AudioPlayer_oniVKUpdate(object sender, EventArgs e)
        {
            updatePlayState();
        }
    
        private void updatePlayState(bool prin = false, bool pause = false)
        {
            string icon = "\uE768"; // default icon

            if (prin)
            {
                icon = pause ? "\uE768" : "\uE769";
            }
            else if (isThisPlayList_Now_Play && AudioPlayer.mediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Paused)
            {
                icon = "\uE769";
            }

            if (!isThisPlayList_Now_Play)
            {
                if (!entered)
                {
                    FadeInAnimationGridPlayIcon.Pause();
                    FadeOutAnimationGridPlayIcon.Begin();
                }
            }
            else
            {
                FadeOutAnimationGridPlayIcon.Pause();
                FadeInAnimationGridPlayIcon.Begin();
            }

            AnimationsChangeFontIcon.ChangeFontIconWithAnimation(icon);
        }

        public bool isThisPlayList_Now_Play
        {
            get
            {
                if (_PlayList == null || AudioPlayer.iVKGetAudio == null)
                    return false;

                if (!(AudioPlayer.iVKGetAudio is PlayListVK))
                    return false;

                var playlist = (AudioPlayer.iVKGetAudio as PlayListVK).playlist;

                if (playlist == _PlayList)
                    return true;

                if (playlist.Id == _PlayList.Id && playlist.OwnerId == _PlayList.OwnerId && playlist.AccessKey == _PlayList.AccessKey)
                    return true;

                if (playlist.Original != null)
                {
                    if (playlist.Id == playlist.Original.PlaylistId && playlist.OwnerId == playlist.Original.OwnerId && playlist.AccessKey == playlist.Original.AccessKey)
                        return true;
                }

                return false;
            }
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

     

        public async void AddRemove_Click(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(
               async () =>
               {

                   try
                   {

                       if (!_PlayList.IsFollowing)
                       {
                           if (_PlayList.Original != null)
                           {
                               await VK.vkService.AddPlaylistAsync(_PlayList.Original.PlaylistId, _PlayList.Original.OwnerId, _PlayList.Original.AccessKey);
                           }else
                               await VK.vkService.AddPlaylistAsync(_PlayList.Id, _PlayList.OwnerId, _PlayList.AccessKey);

                           if (_PlayList.Original != null)
                           {
                               var js = await VK.vkService.AddPlaylistAsync(_PlayList.Original.PlaylistId, _PlayList.Original.OwnerId, _PlayList.Original.AccessKey);
                               _PlayList = await VK.api.Audio.GetPlaylistByIdAsync((long)js["owner_id"], (long)js["playlist_id"]);
                           }
                           else
                           {
                               var js = await VK.vkService.AddPlaylistAsync(_PlayList.Id, _PlayList.OwnerId, _PlayList.AccessKey);
                               _PlayList = await VK.api.Audio.GetPlaylistByIdAsync((long)js["owner_id"], (long)js["playlist_id"]);
                           }



                       }
                       else
                       {
                         
                           if (_PlayList.Follower != null)
                           {
                               await VK.vkService.DeletePlaylistAsync(_PlayList.Follower.PlaylistId, _PlayList.Follower.OwnerId);
                           }
                           else
                           {
                               await VK.vkService.DeletePlaylistAsync(_PlayList.Id, _PlayList.OwnerId);
                           }
                           if (_PlayList.Original != null)
                               _PlayList = await VK.api.Audio.GetPlaylistByIdAsync(_PlayList.Original.OwnerId, _PlayList.Original.PlaylistId);
                       }
                   }catch (Exception ex) 
                   { 
                   
                   
                   }
                   updateAddedBTN();
               });



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
         


      

            if (_PlayList.Cover != null)
            {
                SmallHelpers.AddImagesToGrid(GridThumbs, _PlayList.Cover, this.DispatcherQueue);
            }
            else if (_PlayList.Thumbs != null)
            {
                int count = _PlayList.Thumbs.Count;
                int index = 0;
                List<string> list = new List<string>();
                foreach (var item in _PlayList.Thumbs)
                {
                    string photo = item.Photo600 ?? item.Photo1200 ?? item.Photo300 ?? item.Photo34 ?? item.Photo270 ?? item.Photo135 ?? item.Photo68;
                    list.Add(photo);
                    index++;
                }
                SmallHelpers.AddImagesToGrid(GridThumbs, list, this.DispatcherQueue);
            }
           

            updateAddedBTN();
            updatePlayState();




            bool isUserPlaylist = _PlayList.OwnerId == AccountsDB.activeAccount.id && _PlayList.Original == null;
            {


            }

            FadeInAnimationGrid.Begin();
        }

        private void updateAddedBTN()
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                bool canEdit = _PlayList.Permissions.Edit;
                bool canDelete = _PlayList.Permissions.Delete;
          

                editAlbum.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
                DeleteAlbum.Visibility = canDelete ? Visibility.Visible : Visibility.Collapsed;


                AddRemove.Visibility = Visibility.Visible;

                if (_PlayList.Permissions.Edit)
                    AddRemove.Visibility = Visibility.Collapsed;

                if (!_PlayList.IsFollowing && !_PlayList.Permissions.Follow)
                    AddRemove.Visibility = Visibility.Collapsed;

                if (AddRemove.Visibility != Visibility.Collapsed)
                if (!_PlayList.IsFollowing && _PlayList.OwnerId != AccountsDB.activeAccount.id)
                {
                    AddRemove.Text = "Добавить к себе";
                    AddRemove.Icon = new SymbolIcon(Symbol.Add);
                }
                else
                {
                    AddRemove.Text = "Отписаться";
                    AddRemove.Icon = new SymbolIcon(Symbol.Delete);


                }
            });
        }

        bool entered;
        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            entered = true;
            FadeOutAnimationGridPlayIcon.Pause();
            FadeInAnimationGridPlayIcon.Begin();
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            entered = false;
            if (!isThisPlayList_Now_Play)
            {
                FadeInAnimationGridPlayIcon.Pause();
                FadeOutAnimationGridPlayIcon.Begin();
            }
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
          
            iVKGetAudio = new PlayListVK(_PlayList, this.DispatcherQueue);

            if (isThisPlayList_Now_Play) {
                if (AudioPlayer.mediaPlayer.PlaybackSession.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Paused)
                {
                    AudioPlayer.mediaPlayer.Pause();
                    updatePlayState(true, true);
                }
                else
                {
                    AudioPlayer.mediaPlayer.Play();
                    updatePlayState(true, false);
                }
            
            }
            else
            {
                AnimationsChangeFontIcon.ChangeFontIconWithAnimation("\uE916");
                EventHandler handler = null;
                handler = (sender, e) =>
                {
                    this.DispatcherQueue.TryEnqueue(async () =>
                    {


                        iVKGetAudio.currentTrack = 0;
                        AudioPlayer.PlayList(iVKGetAudio);

                        //                                                 Navigate
                        iVKGetAudio.onListUpdate -= handler;
                    });
                };
           
            iVKGetAudio.onListUpdate += handler;
            }
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
                if (s != null && s is AudioPlaylist)
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
                TempPlayLists.TempPlayLists.updateNextRequest = true;
                await VK.api.Audio.DeletePlaylistAsync(_PlayList.OwnerId, _PlayList.Id);
                var index = _PlaylistItems.IndexOf(_PlayList);
                if (index != -1)
                    _PlaylistItems.RemoveAt(index);
            });
        }
    }
}
