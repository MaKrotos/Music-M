using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Converters;
using VK_UI3.DB;
using VK_UI3.DownloadTrack;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.Views.ModalsPages;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.Pickers;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{


    public sealed partial class TrackControl : UserControl, INotifyPropertyChanged
    {

        public TrackControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += TrackControl_DataContextChanged;
            Loaded += TrackControl_Loaded;
            Unloaded += TrackControl_Unloaded;
            this.Loading += TrackControl_Loading;
            if (changeImage == null)
            changeImage = new AnimationsChangeImage(ImageThumb, DispatcherQueue);
            changeIconPlayBTN = new AnimationsChangeIcon(PlayBTN);
        }

        private void TrackControl_Unloaded(object sender, RoutedEventArgs e)
        {
            addedHandler = false;
            if (dataTrack != null)
            {
                dataTrack.iVKGetAudio.AudioPlayedChangeEvent -= UserAudio_AudioPlayedChangeEvent;
            }
            this.DataContextChanged -= TrackControl_DataContextChanged;
            Loaded -= TrackControl_Loaded;
            Unloaded -= TrackControl_Unloaded;
        }

        private void TrackControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ImageThumb.Opacity = 0;
            if ((DataContext as ExtendedAudio) != null)
            {

                var track = (DataContext as ExtendedAudio).audio;
                if (track == null)
                    return;

                dataTrack = (DataContext as ExtendedAudio);

                updateUI();
            }
        }

        private void updateUI()
        {
          
                bool isOwner = dataTrack.audio.OwnerId == AccountsDB.activeAccount.id;

                if (dataTrack.audio.Release_audio_id == null && dataTrack.audio.OwnerId == AccountsDB.activeAccount.id)
                {

                    EditTrack.Visibility = Visibility.Visible;
                }
                else EditTrack.Visibility = Visibility.Collapsed;
                AddRemove.Visibility = Visibility.Visible;
                AddRemove.Text = isOwner ? "Удалить" : "Добавить";
                AddRemove.Icon = new SymbolIcon(isOwner ? Symbol.Delete : Symbol.Add);

                if (dataTrack.iVKGetAudio is PlayListVK aplaylist && aplaylist.playlist.Permissions.Edit)
                {
                    RemovePlayList.Visibility = Visibility.Visible;
                    if (isOwner)
                    {
                        AddRemove.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    RemovePlayList.Visibility = Visibility.Collapsed;
                }

                Title.Text = dataTrack.audio.Title;
                Subtitle.Text = dataTrack.audio.Subtitle;
                Artists.Text = dataTrack.audio.Artist;



                if (dataTrack.audio.Album != null && dataTrack.audio.Album.Thumb != null)
                {
                    photouri = dataTrack.audio.Album.Thumb.Photo270 ??
                    dataTrack.audio.Album.Thumb.Photo300 ??
                    dataTrack.audio.Album.Thumb.Photo600 ??
                    dataTrack.audio.Album.Thumb.Photo34 ??
                    null;
                    if (photouri == null || photouri == "")
                    {
                        ImageThumbGrid.Opacity = 0;
                    }
                    else
                    {
                        ImageThumbGrid.Opacity = 1;

                        changeImage.ChangeImageWithAnimation(
                            photouri, true
                         );
                    }
                }
                else
                {
                    ImageThumbGrid.Opacity = 0;
                }



                if ((dataTrack.audio.MainArtists == null) || (!dataTrack.audio.MainArtists.Any()))
                {
                    Artists.Text = dataTrack.audio.Artist;
                    // Artists.MouseEnter += Artists_MouseEnter;
                    // Artists.MouseLeave += Artists_MouseLeave;
                    // Artists.MouseLeftButtonDown += Artists_MouseLeftButtonDown;

                    MenuFlyoutSubItem itemToRemove = (MenuFlyoutSubItem)FindName("GoArtist");
                    if (itemToRemove != null)
                    {
                        flyOutm.Items.Remove(itemToRemove);
                    }
                }
                else
                {

                    MenuFlyoutSubItem goArtistItem = (MenuFlyoutSubItem)FindName("GoArtist");
                    if (goArtistItem != null)
                    {
                        goArtistItem.Items.Clear();
                        foreach (var artist in dataTrack.audio.MainArtists)
                        {
                            var menuItem = new MenuFlyoutItem
                            {
                                Text = artist.Name,
                                Icon = new SymbolIcon(Symbol.ContactInfo)
                            };

                            menuItem.Click += (s, e) =>
                            {


                                try
                                {
                                    MainView.OpenSection(artist.Id, SectionType.Artist);

                                }
                                catch (Exception ex)
                                {
                                    AppCenterHelper.SendCrash(ex);
                                }

                            };
                            goArtistItem.Items.Add(menuItem);
                        }
                    }


                }
                // Создание экземпляра конвертера
                SecondsToTimeConverter converter = new SecondsToTimeConverter();

                // Преобразование секунд в строку времени
                object timeString = converter.Convert(dataTrack.audio.Duration, null, null, null);
                Console.WriteLine(timeString);
                Time.Text = (string)timeString;





                if (this.dataTrack.audio.Album != null)
                {
                    FlyGoAlbum.Visibility = Visibility.Visible;
                }
                else
                {
                    FlyGoAlbum.Visibility = Visibility.Collapsed;
                }

                SetIconDislike();

                try
                {
                    Symbol symbol = dataTrack.PlayThis ? Symbol.Pause : Symbol.Play;
                    ChangeSymbolIcon(symbol);
                    HandleAnimation(dataTrack.PlayThis);
                }
                catch (Exception ex)
                {
                    AppCenterHelper.SendCrash(ex);
                }

                if (dataTrack != null && !addedHandler)
                {
                    dataTrack.iVKGetAudio.AudioPlayedChangeEvent += UserAudio_AudioPlayedChangeEvent;
                    addedHandler = true;
                }
        }

        private void SetIconDislike()
        {
            // Загрузите ResourceDictionary из файла XAML внутри пакета .appx
            ResourceDictionary myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ms-appx:///Resource/icons.xaml", UriKind.Absolute);

            // Получите доступ к ресурсу по ключу
            if (!dataTrack.audio.Dislike)
            {
                IconData = myResourceDictionary["Dislike"] as string;
                disText.Text = "Не нравится";
            }
            else
            {
                IconData = myResourceDictionary["FilledDislike"] as string;
                disText.Text = "Убрать дизлайк";
            }


        }

        public string _iconData = "m12.4829 18.2961c-.7988.8372-2.0916.3869-2.4309-.5904-.27995-.8063-.6436-1.7718-.99794-2.4827-1.05964-2.1259-1.67823-3.3355-3.38432-4.849-.22637-.2008-.51811-.3626-.84069-.49013-1.12914-.44632-2.19096-1.61609-1.91324-3.0047l.35304-1.76517c.1857-.92855.88009-1.67247 1.79366-1.92162l5.59969-1.52721c2.5456-.694232 5.1395.94051 5.6115 3.53646l.6839 3.7617c.3348 1.84147-1.0799 3.53667-2.9516 3.53667h-.8835l.0103.0522c.0801.4082.1765.9703.241 1.5829.0642.6103.0983 1.2844.048 1.9126-.0493.6163-.1839 1.2491-.5042 1.7296-.1095.1643-.2721.3484-.4347.5188z";

        public string IconData
        {
            get { return _iconData; }
            set
            {
                if (_iconData != value)
                {
                    _iconData = value;
                    OnPropertyChanged(nameof(IconData));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;




        private void TrackControl_Loading(FrameworkElement sender, object args)
        {


        }

        ExtendedAudio dataTrack = null;
        bool addedHandler = false;





        string photouri = null;

      
        AnimationsChangeImage changeImage = null;
        private void TrackControl_Loaded(object sender, RoutedEventArgs e)
        {
         
        }
        public void RecommendedAudio_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }


        public void PlayNext_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        private void UserAudio_AudioPlayedChangeEvent(object sender, EventArgs e)
        {
            try
            {
                Symbol symbol = dataTrack.PlayThis ? Symbol.Pause : Symbol.Play;
                ChangeSymbolIcon(symbol);
                HandleAnimation(dataTrack.PlayThis);
            }
            catch (Exception ex)
            {
                AppCenterHelper.SendCrash(ex);
            }
        }


        private void ChangeSymbolIcon(Symbol symbol)
        {
            if (GridPlayIcon.Opacity != 0)
            {
                changeIconPlayBTN.ChangeSymbolIconWithAnimation(symbol);
            }
            else
            {
                PlayBTN.Symbol = symbol;
            }
        }

        private void HandleAnimation(bool playThis)
        {
            if (playThis)
            {
                FadeOutAnimationGridPlayIcon.Pause();
                FadeInAnimationGridPlayIcon.Begin();
            }
            else if (!entered)
            {
                FadeInAnimationGridPlayIcon.Pause();
                FadeOutAnimationGridPlayIcon.Begin();
            }
        }

        private void GoMainArtist()
        {
            try
            {
                if (dataTrack.audio.MainArtists == null || dataTrack.audio.MainArtists.Count() == 0)
                {
                    MainView.OpenSection(dataTrack.audio.Artist, SectionType.Search);
                }
                else
                {
                    MainView.OpenSection(dataTrack.audio.MainArtists.First().Id, SectionType.Artist);
                }
            }
            catch (Exception ex)
            {


                AppCenterHelper.SendCrash(ex);
            }
        }

        private void GoArtist_Click(object sender, RoutedEventArgs e)
        {
            GoMainArtist();
        }

        public void Download_Click(object sender, RoutedEventArgs e)
        {

        }

        public void AddToQueue_Click(object sender, RoutedEventArgs e)
        {


        }

        public void CopyLink(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            string audioLink = $"https://vk.com/audio{dataTrack.audio.OwnerId}_{dataTrack.audio.Id}";
            dataPackage.SetText(audioLink);
            Clipboard.SetContent(dataPackage);
        }


        public void AddRemove_Click(object sender, RoutedEventArgs e)
        {
            var vkService = VK.vkService;

            if (dataTrack.audio.OwnerId == AccountsDB.activeAccount.id)
            {
                vkService.AudioDeleteAsync((long)dataTrack.audio.Id, (long)dataTrack.audio.OwnerId);
                dataTrack.iVKGetAudio.listAudioTrue.Remove(dataTrack);
            }
            else
            {
                vkService.AudioAddAsync((long)dataTrack.audio.Id, (long)dataTrack.audio.OwnerId);
            }
        }
        public void AddArtistIgnore_Click(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }
        AnimationsChangeIcon changeIconPlayBTN = null;
        bool entered = false;
        private void UCcontrol_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //FadeInAnimation.Begin();
            entered = true;

            Symbol symbol = dataTrack.PlayThis ? Symbol.Pause : Symbol.Play;
            if (GridPlayIcon.Opacity != 0)
            {
                changeIconPlayBTN.ChangeSymbolIconWithAnimation(symbol);
            }
            else
            {
                PlayBTN.Symbol = symbol;
            }
            FadeOutAnimationGridPlayIcon.Pause();
            FadeInAnimationGridPlayIcon.Begin();
        }

        private void UCcontrol_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            entered = false;
            var a = DataContext as ExtendedAudio;
            if (a == null) return;
            if (a.PlayThis) return;
            FadeInAnimationGridPlayIcon.Pause();
            FadeOutAnimationGridPlayIcon.Begin();
        }

        private void UCcontrol_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).Properties.IsLeftButtonPressed)
            {
                dataTrack.iVKGetAudio.currentTrack = dataTrack.NumberInList;
                AudioPlayer.PlayList(dataTrack.iVKGetAudio);
            }
        }


        bool waitDisliked = false;



        private void DislikeClick(object sender, RoutedEventArgs e)
        {
            if (waitDisliked) return;
            waitDisliked = true;
            Task.Run(
                  async () =>
                  {
                      if (dataTrack.audio.Dislike)
                      {

                          var complete = await VK.RemoveDislike((long)dataTrack.audio.Id, (long)dataTrack.audio.OwnerId);
                          if (complete)
                              this.dataTrack.audio.Dislike = !this.dataTrack.audio.Dislike;

                      }
                      else
                      {
                          var complete = await VK.AddDislike((long)dataTrack.audio.Id, (long)dataTrack.audio.OwnerId);
                          if (complete)
                              this.dataTrack.audio.Dislike = !this.dataTrack.audio.Dislike;

                      }
                      DispatcherQueue.TryEnqueue(async () =>
                      {
                          SetIconDislike();
                      });
                      waitDisliked = false;
                  });
        }

        private void GoToAlbum(object sender, RoutedEventArgs e)
        {
            MainView.OpenPlayList(
                dataTrack.audio.Album.Id,
                dataTrack.audio.Album.OwnerId,
                dataTrack.audio.Album.AccessKey);
        }
        List<MenuFlyoutItem> menuFlyoutItems = new List<MenuFlyoutItem>();
        List<MenuFlyoutItem> menuFlyoutItemsUploads = new List<MenuFlyoutItem>();
        private void UCcontrol_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
          
            Task.Run(
                   async () =>
                   {
                       var i = 0;
                       var listed = await TempPlayLists.TempPlayLists.GetPlayListAsync();
                       

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                           
                                foreach (var item in menuFlyoutItems)
                                {
                                    AddPlayList.Items.Remove(item);
                                }
                            
                            menuFlyoutItems.Clear();

                            foreach (var album in listed)
                            {
                                var menuItem = new MenuFlyoutItem
                                {
                                    Text = album.Title,
                                    Icon = new FontIcon
                                    {
                                        Glyph = "\uE142", // Замените на код глифа вашей иконки альбома

                                    }
                                };


                                menuItem.Click += (s, e) =>
                                {


                                    try
                                    {
                                        VK.api.Audio.AddToPlaylistAsync(album.OwnerId, album.Id, new List<string> { $"{dataTrack.audio.OwnerId}_{dataTrack.audio.Id}" });

                                    }
                                    catch (Exception ex)
                                    {
                                        AppCenterHelper.SendCrash(ex);
                                    }

                                };
                                menuFlyoutItems.Add(menuItem);
                                AddPlayList.Items.Insert(i++, menuItem);
                            }
                        });

                       var j = 0;
                       var paths = PathTable.GetAllPaths();


                       DispatcherQueue.TryEnqueue(async () =>
                       {

                           foreach (var item in menuFlyoutItemsUploads)
                           {
                               DownloadFlyOut.Items.Remove(item);
                           }

                           menuFlyoutItemsUploads.Clear();

                           foreach (var path in paths)
                           {
                               var menuItem = new MenuFlyoutItem
                               {
                                   Text = path.path,
                                   Icon = new FontIcon { Glyph = "\uF12B" }
                               };


                               menuItem.Click += (s, e) =>
                               {
                                   IVKGetAudio iVKGetAudio = new SimpleAudio(this.DispatcherQueue);
                                   iVKGetAudio.name = dataTrack.audio.Title;
                                   iVKGetAudio.itsAll = true;
                                   iVKGetAudio.countTracks = 1;
                                   iVKGetAudio.listAudio.Add(dataTrack);

                                   _ = Task.Run(
                                                  async () =>
                                                  {
                                                      PlayListDownload playListDownload = new PlayListDownload(iVKGetAudio, path.path, this.DispatcherQueue, true);
                                                  });
                               };
                               menuFlyoutItemsUploads.Add(menuItem);
                               DownloadFlyOut.Items.Insert(j++, menuItem);
                           }
                       });

                   });
        }

        private void CreatePlayListBTN_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new CustomDialog();

            dialog.Transitions = new TransitionCollection
                {
                    new PopupThemeTransition()
                };

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;


            var a = new CreatePlayList(dataTrack.audio);
            dialog.Content = a;
            dialog.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            a.cancelPressed+=((s, e) =>
            {
                dialog.Hide();
                dialog = null;

                if (s != null && s is AudioPlaylist)
                {
                    TempPlayLists.TempPlayLists.updateNextRequest = true;
                }
            });

            dialog.ShowAsync();

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

                    IVKGetAudio iVKGetAudio = new SimpleAudio(this.DispatcherQueue);
                    iVKGetAudio.name = dataTrack.audio.Title;
                    iVKGetAudio.itsAll = true;
                    iVKGetAudio.countTracks = 1;
                    iVKGetAudio.listAudio.Add(dataTrack);

                    _ = Task.Run(
                                   async () =>
                                   {
                                       new PlayListDownload(iVKGetAudio, folder.Path, this.DispatcherQueue, true);
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

        private void PickFolderDownload_Click(object sender, RoutedEventArgs e)
        {

            pickFolder();


        }


            private void addToPlayListMore_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new CustomDialog();

            dialog.Transitions = new TransitionCollection
                {
                    new PopupThemeTransition()
                };




            dialog.XamlRoot = this.XamlRoot;
            var a = new UserPlayList(dataTrack.audio);
            dialog.Content = a;

            EventHandler handler = null;
            handler = (s, e) =>
            {
                dialog.Hide();
               
            //    a.selectedPlayList -= handler; // Отписка от события
            };

            a.selectedPlayList += handler;

            TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> closedHandler = null;
            closedHandler = (s, e) =>
            {
                a.selectedPlayList -= handler;
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






        private async void RemovePlayList_Click(object sender, RoutedEventArgs e)
        {
            if (!(dataTrack.iVKGetAudio is PlayListVK)) return;

            // Создаем параметры для метода audio.moveToAlbum
            _ = Task.Run(
                  async () =>
                  {

                        var deleted =  await VK.deleteFromPlaylist((long)dataTrack.audio.Id, (long)dataTrack.audio.OwnerId,
                            (dataTrack.iVKGetAudio as PlayListVK).playlist.Id);
                          if (deleted)
                          this.DispatcherQueue.TryEnqueue(async () =>
                          {
                              dataTrack.iVKGetAudio.listAudio.Remove(dataTrack);
                          });
                  }
                  );

       

        }

        private void EditTrack_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new CustomDialog();
            dialog.XamlRoot = this.XamlRoot;
            var a = new EditTrack(dataTrack.audio);
            dialog.Content = a;

            EventHandler handler = null;
            handler = (s, e) =>
            {
                dialog.Hide();
                if (s is Audio ss)
                {
                    dataTrack.audio = ss;
                    updateUI();
                }
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
}