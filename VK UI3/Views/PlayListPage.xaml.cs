using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.DB;
using VK_UI3.DownloadTrack;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views.ModalsPages;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using Windows.Storage.Pickers;


namespace VK_UI3.Views
{
    public record PlaylistData(long PlaylistId, long OwnerId, string AccessKey);
    public sealed partial class PlayListPage : Microsoft.UI.Xaml.Controls.Page
    {
        public PlayListPage()
        {
            this.InitializeComponent();
            this.Loaded += PlayListPage_Loaded;
            this.Unloaded += PlayListPage_Unloaded;
            MainText = new AnimationsChangeText(textBlock1, this.DispatcherQueue);
            descriptionText = new AnimationsChangeText(DescriptionText, this.DispatcherQueue);
        }

        private void PlayListPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PlayListPage_Loaded;
            this.Unloaded -= PlayListPage_Unloaded;
            if (scrollViewer != null) scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
            if (vkGetAudio != null)
            {
                vkGetAudio.onPhotoUpdated.RemoveHandler(VkGetAudio_onPhotoUpdated);
                vkGetAudio.onListUpdate-=(VkGetAudio_onListUpdate);
                vkGetAudio.onNameUpdated.RemoveHandler(VkGetAudio_onNameUpdated);
                vkGetAudio.onCountUpDated.RemoveHandler(VkGetAudio_onCountUpDated);
                vkGetAudio.onInfoUpdated.RemoveHandler(VkGetAudio_onInfoUpdated);
            }
        }

        public IVKGetAudio vkGetAudio = null;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            vkGetAudio = e.Parameter as IVKGetAudio;
            if (vkGetAudio == null)
            {
                vkGetAudio = new UserAudio(AccountsDB.activeAccount.id, this.DispatcherQueue);
            }
            vkGetAudio.onPhotoUpdated.AddHandler(VkGetAudio_onPhotoUpdated);
            vkGetAudio.onListUpdate += (VkGetAudio_onListUpdate);
            vkGetAudio.onNameUpdated.AddHandler(VkGetAudio_onNameUpdated);
            vkGetAudio.onCountUpDated.AddHandler(VkGetAudio_onCountUpDated);
            vkGetAudio.onInfoUpdated.AddHandler(VkGetAudio_onInfoUpdated);

            updateUI(true);
            animationsChangeText = new(textAdd, this.DispatcherQueue);
            animationsChangeicon = new(iconAdd);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (vkGetAudio != null)
            {
                vkGetAudio.onPhotoUpdated.RemoveHandler(VkGetAudio_onPhotoUpdated);
                vkGetAudio.onListUpdate -=(VkGetAudio_onListUpdate);
                vkGetAudio.onNameUpdated.RemoveHandler(VkGetAudio_onNameUpdated);
                vkGetAudio.onCountUpDated.RemoveHandler(VkGetAudio_onCountUpDated);
                vkGetAudio.onInfoUpdated.RemoveHandler(VkGetAudio_onInfoUpdated);
            }
        }



        AnimationsChangeText MainText;
        AnimationsChangeText descriptionText;
        Helpers.Animations.AnimationsChangeText animationsChangeText;
        Helpers.Animations.AnimationsChangeIcon animationsChangeicon;

        private void updateUI(bool load = false)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
               

                SmallHelpers.AddImagesToGrid(GridThumbs, vkGetAudio.getPhotosList(), DispatcherQueue);
                if (vkGetAudio is (PlayListVK))
                {
                    var playlist = (vkGetAudio as PlayListVK).playlist;
                    if (!playlist.Permissions.Edit)
                    {
                        stackPanel.Items.Remove(EditPlaylist);
                    }

                    if (playlist.Follower != null)
                    {
                        _ = Task.Run(
                                 async () =>
                                 {
                                     (this.vkGetAudio as PlayListVK).playlist = VK.api.Audio.GetPlaylistById(playlist.Follower.OwnerId, playlist.Follower.PlaylistId);
                                 });
                    }

                    if (!playlist.Permissions.Follow && (playlist.OwnerId != AccountsDB.activeAccount.id || (playlist.Original == null)))
                        stackPanel.Items.Remove(AddPlaylist);
                    else
                    {
                        if (playlist.IsFollowing && !playlist.Permissions.Edit)
                        {
                            if (load)
                            {
                                iconAdd.Symbol = Symbol.Delete;
                                textAdd.Text = "Удалить";
                            }
                            else
                            {

                                animationsChangeText.ChangeTextWithAnimation("Удалить");
                                animationsChangeicon.ChangeSymbolIconWithAnimation(Symbol.Delete);
                            }
                        }
                        else
                        {
                            if (load)
                            {
                                iconAdd.Symbol = Symbol.Add;
                                textAdd.Text = "Добавить к себе";
                            }
                            else 
                            {
                             
                                animationsChangeText.ChangeTextWithAnimation("Добавить к себе");
                                animationsChangeicon.ChangeSymbolIconWithAnimation(Symbol.Add);
                            }
                        }
                    }

                    if (load)
                        DescriptionText.Text = playlist.Description;

                    else
                        descriptionText.ChangeTextWithAnimation(playlist.Description);


                    if (playlist.Description == "" || playlist.Description == null)
                        DescriptionText.Visibility = Visibility.Collapsed;

                    FollowersText.Visibility = Visibility.Visible;
                    FollowersText.Text = $"{playlist.Followers} подписчиков";

                    if (playlist.Genres.Count == 0)
                    {
                        Genres.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        string GenresText = string.Join(", ", playlist.Genres.Select(g => g.Name));
                        Genres.Text = $"Жанры: {GenresText}";
                        Genres.Visibility = Visibility.Visible;
                    }

                    string dateInDesiredFormat = playlist.CreateTime.ToString("d MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);
                    string updateTimeF = playlist.UpdateTime.ToString("d MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);

                    if (playlist.UpdateTime != DateTime.MinValue && playlist.CreateTime.Date != playlist.UpdateTime.Date)
                    {
                        Year.Text = $"Создан: {dateInDesiredFormat}, обновлён {updateTimeF}";
                    }
                    else
                    {
                        Year.Text = $"Создан: {dateInDesiredFormat}";
                    }
                    Year.Visibility = Visibility.Visible;

                    Plays.Text = $"Прослушиваний : {playlist.Plays}";
                    Plays.Visibility = Visibility.Visible;

                    CountTrText.Text = $"Треков: {playlist.Count}";

                }
                else
                {
                    stackPanel.Items.Remove(AddPlaylist);
                    stackPanel.Items.Remove(EditPlaylist);

                   
                    MainText.ChangeTextWithAnimation(vkGetAudio.name);
                    CountTrText.Text = $"Треков: {vkGetAudio.countTracks}";

                }
             
                MainText.ChangeTextWithAnimation(vkGetAudio.name);

                CountTrText.Visibility = Visibility.Visible;
                

            });

        }

        private void VkGetAudio_onInfoUpdated(object sender, EventArgs e)
        {
            updateUI();
        }

        private void VkGetAudio_onCountUpDated(object sender, EventArgs e)
        {
            updateUI();
        }

        private void VkGetAudio_onNameUpdated(object sender, EventArgs e)
        {
            updateUI();
        }

        private void VkGetAudio_onListUpdate(object sender, EventArgs e)
        {
            blockLoad = false;
            if (vkGetAudio.itsAll)
                LoadingIndicator.Visibility = Visibility.Collapsed;
        }

    
        private void VkGetAudio_onPhotoUpdated(object sender, EventArgs e)
        {
            SmallHelpers.AddImagesToGrid(GridThumbs, vkGetAudio.getPhotosList(), DispatcherQueue);
        }

      
        private ScrollViewer FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer)
                return d as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var sw = FindScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (sw != null) return sw;
            }

            return null;
        }
        ScrollViewer scrollViewer;
        private void PlayListPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Находим ScrollViewer внутри ListView

            scrollViewer = FindScrollViewer(TrackListView);
            if (scrollViewer != null)
            {
                // Подписываемся на событие изменения прокрутки
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged; ;
            }
        }

        bool blockLoad = false;
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            
            var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight;
            if (isAtBottom)
            {
               
                if (vkGetAudio.itsAll)
                {
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
                if (blockLoad) return;
                blockLoad = true;
                LoadingIndicator.Visibility = Visibility.Visible;
                vkGetAudio.GetTracks();
                LoadingIndicator.IsActive = true;
            }
        }

        private async void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var playlist = (vkGetAudio as PlayListVK).playlist;

            _ = Task.Run(
                     async () =>
                     {

                         if (playlist.IsFollowing)
                         {
                             bool deleted = false;
                             if (playlist.Follower != null)
                             {
                                 deleted = await VK.vkService.DeletePlaylistAsync(playlist.Follower.PlaylistId, playlist.Follower.OwnerId);
                             }
                             else
                             {
                                 deleted = await VK.vkService.DeletePlaylistAsync(playlist.Id, playlist.OwnerId);
                             }
                             (vkGetAudio as PlayListVK).playlist = await VK.api.Audio.GetPlaylistByIdAsync(playlist.Original.OwnerId, playlist.Original.PlaylistId);
                         }
                         else
                         {
                             if (playlist.Original != null)
                             {
                                 var js = await VK.vkService.AddPlaylistAsync(playlist.Original.PlaylistId, playlist.Original.OwnerId, playlist.Original.AccessKey);
                                 (vkGetAudio as PlayListVK).playlist = await VK.api.Audio.GetPlaylistByIdAsync((long)js["owner_id"], (long)js["playlist_id"]);
                             }
                             else
                             {
                                  var js = await VK.vkService.AddPlaylistAsync(playlist.Id, playlist.OwnerId, playlist.AccessKey);
                                 (vkGetAudio as PlayListVK).playlist = await VK.api.Audio.GetPlaylistByIdAsync((long)js["owner_id"], (long)js["playlist_id"]);
                             }
                            
                         }
                         updateUI();


                     });

        }

        private void PlayPlaylist_Click(object sender, RoutedEventArgs e)
        {

            vkGetAudio.currentTrack = 0;
            AudioPlayer.PlayList(vkGetAudio);
        }

        private void EditPlaylist_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new CustomDialog();

            dialog.Transitions = new TransitionCollection
            {
                new PopupThemeTransition()
            };


            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;


            var a = new CreatePlayList((vkGetAudio as PlayListVK).playlist);

            dialog.Content = a;
            dialog.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
            a.cancelPressed+=((s, e) =>
            {
                if (s != null && s is AudioPlaylist)
                {
                    (vkGetAudio as PlayListVK).playlist = (AudioPlaylist)s;
                    updateUI();
                    vkGetAudio.name = (s as AudioPlaylist).Title;
                }
                try
                {
                    dialog.Hide();
                    dialog = null;
                }
                catch
                {

                }
            });

            dialog.ShowAsync();

        }


        private async void choosePath(object sender, RoutedEventArgs e)
        {
            pickFolder();
        }


        List<MenuFlyoutItem> menuFlyoutItem = new List<MenuFlyoutItem>();
        private void DownloadPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "VKMMKZ");
            path = Path.Combine(path, "FFMPeg.exe");


            foreach (var item in menuFlyoutItem)
            {
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    folderFlyOut.Items.Remove(item);
                });
            }
            var paths = PathTable.GetAllPaths();
            if (paths.Count == 0)
            {
                pickFolder();
                return;
            }

            foreach (var item in paths)
            {

                MenuFlyoutItem newItem = new MenuFlyoutItem
                {
                    Text = item.path,
                    Icon = new FontIcon { Glyph = "\uF12B" }
                };

                newItem.Click += (s, e) =>
                {

                    _ = Task.Run(
                                   async () =>
                                   {
                                       PlayListDownload playListDownload = new PlayListDownload(vkGetAudio, item.path, this.DispatcherQueue);
                                   });
                };

                menuFlyoutItem.Add(newItem);
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    folderFlyOut.Items.Add(newItem);
                });

            }

        }

      
       

        private async void pickFolder()
        {
            try
            {
                FolderPicker folderPicker = new();
                folderPicker.FileTypeFilter.Add("*");


                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, MainWindow.hvn);

                Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    // Директория выбрана, можно продолжить работу с folder
                    PathTable.AddPath(folder.Path);


                    _ = Task.Run(
                                   async () =>
                                   {
                                       new PlayListDownload(vkGetAudio, folder.Path, this.DispatcherQueue);
                                   });
                }
                else
                {
                    // Операция была отменена пользователем
                }
            }catch (Exception ex)
            {





            }
        }
    }
}
