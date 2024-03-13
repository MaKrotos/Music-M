using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Shared.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Converters;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Services;
using VK_UI3.Views;
using VK_UI3.VKs;
using VkNet.Model;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
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
            this.Loading += TrackControl_Loading;
            if (changeImage == null)
                changeImage = new AnimationsChangeImage(ImageThumb, DispatcherQueue);
                changeIconPlayBTN = new AnimationsChangeIcon(PlayBTN);
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
                

                if (!addedHandler)
                {
                    dataTrack.iVKGetAudio.AudioPlayedChangeEvent += UserAudio_AudioPlayedChangeEvent;
                    addedHandler = true;
                }   

                if (track.OwnerId == AccountsDB.activeAccount.id)
                {
                    AddRemove.Text = "Удалить";
                    AddRemove.Icon = new SymbolIcon(Symbol.Remove);
                }
                else
                {
                    AddRemove.Text = "Добавть";
                    AddRemove.Icon = new SymbolIcon(Symbol.Add);
                }

                Title.Text = track.Title;
                Subtitle.Text = track.Subtitle;
                Artists.Text = track.Artist;
                audio = track;


                if (track.Album != null && track.Album.Thumb != null)
                {
                    photouri = track.Album.Thumb.Photo270 ??
                    track.Album.Thumb.Photo300 ??
                    track.Album.Thumb.Photo600 ??
                    track.Album.Thumb.Photo34 ??
                    null;
                    if (photouri == null || photouri == "")
                    {
                        ImageThumbGrid.Opacity = 0;
                    }
                    else
                    {
                        ImageThumbGrid.Opacity = 1;

                        changeImage.ChangeImageWithAnimation(
                            photouri
                         );
                    }
                }
                else
                {
                    ImageThumbGrid.Opacity = 0;
                }





                if ((track.MainArtists == null) || (!track.MainArtists.Any()))
                {
                    Artists.Text = track.Artist;
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
                        foreach (var artist in track.MainArtists)
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
                object timeString = converter.Convert(track.Duration, null, null, null);
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
                disText.Text = "Не нравиться";
            }
            else
            {
                 IconData = myResourceDictionary["FilledDislike"] as string;
                disText.Text = "Убрать дизлайк";
            }
         
         
        }

        public string _iconData  = "m12.4829 18.2961c-.7988.8372-2.0916.3869-2.4309-.5904-.27995-.8063-.6436-1.7718-.99794-2.4827-1.05964-2.1259-1.67823-3.3355-3.38432-4.849-.22637-.2008-.51811-.3626-.84069-.49013-1.12914-.44632-2.19096-1.61609-1.91324-3.0047l.35304-1.76517c.1857-.92855.88009-1.67247 1.79366-1.92162l5.59969-1.52721c2.5456-.694232 5.1395.94051 5.6115 3.53646l.6839 3.7617c.3348 1.84147-1.0799 3.53667-2.9516 3.53667h-.8835l.0103.0522c.0801.4082.1765.9703.241 1.5829.0642.6103.0983 1.2844.048 1.9126-.0493.6163-.1839 1.2491-.5042 1.7296-.1095.1643-.2721.3484-.4347.5188z";

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

        VkNet.Model.Attachments.Audio audio = new();
        AnimationsChangeImage changeImage = null;
        private void TrackControl_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        public void Title_PointerPressed(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void Title_PointerExited(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
        }

        public void Title_PointerEntered(object sender, RoutedEventArgs e)
        {
            // Ваш код здесь
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
            catch (Exception ex) { 
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
                if (audio.MainArtists == null || audio.MainArtists.Count() == 0)
                {
                    MainView.OpenSection(audio.Artist, SectionType.Search);
                }
                else
                {
                    MainView.OpenSection(audio.MainArtists.First().Id, SectionType.Artist);
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

        public void CopyLink(object sender, RoutedEventArgs e) { 
            var dataPackage = new DataPackage();
            string audioLink = $"https://vk.com/audio{audio.OwnerId}_{audio.Id}";
            dataPackage.SetText(audioLink);
            Clipboard.SetContent(dataPackage); 
        }



        public void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void AddRemove_Click(object sender, RoutedEventArgs e)
        {

            var vkService = VK.vkService;

            if (audio.OwnerId == AccountsDB.activeAccount.id)
            {
                 vkService.AudioDeleteAsync((long)audio.Id, (long) audio.OwnerId);
            }
            else
            {
                 vkService.AudioAddAsync((long) audio.Id, (long) audio.OwnerId);       
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
    }
}

