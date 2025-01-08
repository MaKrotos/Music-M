using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.Foundation;
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



        private void desctruct() 
        {
            AudioPlayer.onClickonTrack -= AudioPlayer_onClickonTrack;

            this.Loaded -= PlayListPage_Loaded;
            this.Unloaded -= PlayListPage_Unloaded;
            Upload.UploadTrack.addedTrack -= addedTrack;
            if (scrollViewer != null) scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
            if (vkGetAudio != null)
            {
                vkGetAudio.onPhotoUpdated.RemoveHandler(VkGetAudio_onPhotoUpdated);
                vkGetAudio.onListUpdate -= (VkGetAudio_onListUpdate);
                vkGetAudio.onNameUpdated.RemoveHandler(VkGetAudio_onNameUpdated);
                vkGetAudio.onCountUpDated.RemoveHandler(VkGetAudio_onCountUpDated);
                vkGetAudio.onInfoUpdated.RemoveHandler(VkGetAudio_onInfoUpdated);
            }
           
        }

        private void PlayListPage_Unloaded(object sender, RoutedEventArgs e)
        {
            desctruct();
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
            desctruct();
            base.OnNavigatedFrom(e);

            
        }

        AudioPlaylist audioPlaylistOriginal;

        AnimationsChangeText MainText;
        AnimationsChangeText descriptionText;
        Helpers.Animations.AnimationsChangeText animationsChangeText;
        Helpers.Animations.AnimationsChangeIcon animationsChangeicon;

        private void updateUI(bool load = false)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {

                GridThumbs.AddImagesToGrid(vkGetAudio.getPhotosList());

                switch (vkGetAudio)
                {
                    case MessagesAudio:
                        FontIconTypeVk.Glyph = "\uE8F2";
                        break;

                    case MixAudio:
                        FontIconTypeVk.Glyph = "\uE8F2";
                        break;
                    case PlayListVK:
                        FontIconTypeVk.Glyph = "\uE93C";
                        break;
                    case SectionAudio:
                        FontIconTypeVk.Glyph = "\uE8F2";
                        break;
                    case SimpleAudio:
                        FontIconTypeVk.Glyph = "\uE8F2";
                        break;
                    case UserAudio:
                        FontIconTypeVk.Glyph = "\uE77B";
                        break;

                    default:
                        break;
                }

             


                if (vkGetAudio is (PlayListVK))
                {
                    var playlist = (vkGetAudio as PlayListVK).playlist;
                    if (audioPlaylistOriginal == null)
                        audioPlaylistOriginal = playlist;

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

                    string qownerName = "";
                    bool hasOwner = false;
                    if (audioPlaylistOriginal.userOwner != null)
                    {
                        hasOwner = true;
                        ownerGrid.Visibility = Visibility.Visible;
                        var a = new Helpers.Animations.AnimationsChangeImage(ownerPictire, DispatcherQueue);
                        a.ChangeImageWithAnimation(audioPlaylistOriginal.userOwner.Photo100);
                        qownerName = audioPlaylistOriginal.userOwner.FirstName + " " + audioPlaylistOriginal.userOwner.LastName;
                    }
                    if (audioPlaylistOriginal.groupOwner != null)
                    {
                        hasOwner = true;
                        ownerGrid.Visibility = Visibility.Visible;
                        var a = new Helpers.Animations.AnimationsChangeImage(ownerPictire, DispatcherQueue);
                        a.ChangeImageWithAnimation(audioPlaylistOriginal.groupOwner.Photo100);
                        qownerName = audioPlaylistOriginal.groupOwner.Name;
                    }
                    if (!hasOwner)
                    {
                        stackPanel.Items.Remove(ownerGrid);
                    }
                    else
                    {
                        ownerName.Text = qownerName;
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
                    if (vkGetAudio.countTracks == -1) {
                        CountTrText.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        CountTrText.Text = $"Треков: {vkGetAudio.countTracks}";
                    }

                }
                if (vkGetAudio is UserAudio userAudio)
                {
                    
                    if (long.Parse(userAudio.id) == AccountsDB.activeAccount.id)
                    {
                        UploadTrack.Visibility = Visibility.Visible;
                        VK_UI3.Views.Upload.UploadTrack.addedTrack += addedTrack;
                    }

                    if (userAudio.user != null)
                    {
                        DescriptionText.Text = userAudio.user.Status;
                        if (
                                userAudio.user.Status != null &&
                                (userAudio.user.Status.Equals("") || DescriptionText.Text == null)
                            )
                                DescriptionText.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    stackPanel.Items.Remove(UploadTrack);


                }
            

                if (DescriptionText.Text is null || DescriptionText.Text == "") DescriptionText.Visibility = Visibility.Collapsed;
                MainText.ChangeTextWithAnimation(vkGetAudio.name);
                CountTrText.Visibility = Visibility.Visible;

                CheckAndHideGrid(AddInfo);
            });
        }

        public void CheckAndHideGrid(StackPanel grid)
        {
            bool allChildrenHidden = true;

            foreach (UIElement child in grid.Children)
            {
                if (child.Visibility == Visibility.Visible)
                {
                    if (child is TextBlock textblock && (textblock.Text is null || textblock.Text == ""))
                    { textblock.Visibility = Visibility.Collapsed; }
                    else
                    {
                        allChildrenHidden = false;
                        break;
                    }
                }
            }

            if (allChildrenHidden)
            {
                grid.Visibility = Visibility.Collapsed;
            }
            else
            {
                grid.Visibility = Visibility.Visible;
            }
        }


        private void addedTrack(object sender, EventArgs e)
        {
            if (sender is Audio audio)
            {
                vkGetAudio.listAudio.Insert(0, new ExtendedAudio(audio, vkGetAudio));
                vkGetAudio.updateNumbers();
            }
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
            this.DispatcherQueue.TryEnqueue(() => {
                blockLoad = false;
                if (vkGetAudio.itsAll)
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                else
                {
                    checkBottom();
                }
            });
        }


        private void VkGetAudio_onPhotoUpdated(object sender, EventArgs e)
        {
            GridThumbs.AddImagesToGrid(vkGetAudio.getPhotosList());
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
            AudioPlayer.onClickonTrack += AudioPlayer_onClickonTrack;
            scrollViewer = FindScrollViewer(TrackListView);
            if (scrollViewer != null)
            {
                // Подписываемся на событие изменения прокрутки
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged; ;
            }
            checkBottom();
        }

        private void AudioPlayer_onClickonTrack(object sender, EventArgs e)
        {
            var audio = sender as ExtendedAudio;
            var ins = vkGetAudio.listAudio.IndexOf(audio);

            if (ins < 0)
            {
                audio = vkGetAudio.listAudio.FirstOrDefault(a => a.audio.Id == audio.audio.Id && a.audio.OwnerId == audio.audio.OwnerId);
                ins = vkGetAudio.listAudio.IndexOf(audio);
            }

            if (ins >= 0 && ins < TrackListView.Items.Count)
            {
                var container = TrackListView.ContainerFromIndex(ins) as ListViewItem;
                if (container != null)
                {
                    var transform = container.TransformToVisual(TrackListView);
                    var position = transform.TransformPoint(new Point(0, 0));
                    double itemHeight = position.Y;

                    scrollViewer.ChangeView(null, scrollViewer.VerticalOffset + itemHeight - 5, null);
                }
            }
        }



        bool blockLoad = false;
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            checkBottom();
        }

        private void checkBottom()
        {
            if (vkGetAudio.itsAll)
            {
                if (LoadingIndicator.Visibility != Visibility.Collapsed)
                {
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                }
                return;
            }
            var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 100;
            if (isAtBottom)
            {
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
            a.cancelPressed += ((s, e) =>
            {
                if (s != null && s is AudioPlaylist)
                {
                    (vkGetAudio as PlayListVK).playlist = (AudioPlaylist)s;
                    updateUI();
                    vkGetAudio.name = (s as AudioPlaylist).Title;
                }
                try
                {
                    if (dialog != null)
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

        private async void UploadTrackClick(object sender, RoutedEventArgs e)
        {
            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.m_window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".wav");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                ContentDialog dialog = new CustomDialog();
                dialog.XamlRoot = this.XamlRoot;
                var a = new EditTrack(file.Path);
                dialog.Content = a;

                EventHandler handler = null;
                handler = (s, e) =>
                {
                    if (dialog != null)
                        dialog.Hide();
                    //    a.selectedPlayList -= handler; // Отписка от события
                };

                a.cancelPressed += handler;

                TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> closedHandler = null;
                closedHandler = (s, e) =>
                {
                    a.cancelPressed -= handler;
                    dialog.Closed -= closedHandler;
                    dialog = null;
                };

                dialog.Closed += closedHandler;

                dialog.ShowAsync();

                dialog.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                dialog.Resources["ContentDialogMaxWidth"] = double.PositiveInfinity;
                dialog.Resources["ContentDialogMaxHeight"] = double.PositiveInfinity;
                dialog.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                dialog.Translation = new System.Numerics.Vector3(0, 0, 0);
                dialog.Resources.Remove("BackgroundElement");

                dialog.Shadow = null;
                dialog.BorderThickness = new Thickness(0);
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
            }
            catch (Exception ex)
            {
            }
        }

        private void ownerGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        private void ownerGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ShowAnimation.Pause();
            HideAnimation.Begin();
        }

        private void ownerGrid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MainView.OpenSection((vkGetAudio as PlayListVK).playlist.OwnerId.ToString(), SectionView.SectionType.UserSection);
        }

        private void GeneratePlayList_Click(object sender, RoutedEventArgs e)
        {
            MainView.mainView.openGenerator(iVKGetAudio: this.vkGetAudio, unicID: $"generate_iVKget_{this.vkGetAudio.name}", $"genBy {this.vkGetAudio.name}");

        }
    }
}
