using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using MusicX.Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using VK_UI3.Controllers;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Services;
using VK_UI3.Views;
using VkNet.Model;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{


    public sealed partial class TrackControl : UserControl
    {

        public TrackControl()
        {
            this.InitializeComponent();

            this.DataContextChanged += TrackControl_DataContextChanged;
            Loaded += TrackControl_Loaded;
            if (changeImage == null)
                changeImage = new AnimationsChangeImage(ImageThumb, DispatcherQueue);
                changeIconPlayBTN = new AnimationsChangeIcon(PlayBTN);
        }
        ExtendedAudio dataTrack = null;
        bool addedHandler = false;
        private void TrackControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if ((DataContext as ExtendedAudio) != null)
            {

                var track = (Audio)(DataContext as ExtendedAudio).Audio;
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
                    AddRemove.Text = "�������";
                    AddRemove.Icon = new SymbolIcon(Symbol.Remove);
                }

                Title.Text = track.Title;
                Subtitle.Text = track.Subtitle;
                Artists.Text = track.Artist;
                audio = track;

                if (track.Album != null && track.Album.Thumb != null)
                    changeImage.ChangeImageWithAnimation(
                        track.Album.Thumb.Photo270 ??
                      
                        track.Album.Thumb.Photo300 ??
                        track.Album.Thumb.Photo600 ??
                        track.Album.Thumb.Photo34 ??
                        "null"
                    );


                if ((track.MainArtists == null) || (!track.MainArtists.Any()))
                {
                    Artists.Text = track.Artist;
                    // Artists.MouseEnter += Artists_MouseEnter;
                    // Artists.MouseLeave += Artists_MouseLeave;
                    // Artists.MouseLeftButtonDown += Artists_MouseLeftButtonDown;

                    MenuFlyoutItem itemToRemove = (MenuFlyoutItem)FindName("GoArtist");
                    if (itemToRemove != null)
                    {
                        flyOutm.Items.Remove(itemToRemove);
                    }
                }
                else
                {

                }
            }
        }

        Audio audio = new();
        AnimationsChangeImage changeImage = null;
        private void TrackControl_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        public void Title_PointerPressed(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }

        public void Title_PointerExited(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }

        public void Title_PointerEntered(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }

        public void RecommendedAudio_Click(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }

        public void PlayNext_Click(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }

        private void UserAudio_AudioPlayedChangeEvent(object sender, EventArgs e)
        {
            try
            {
                Symbol symbol = dataTrack.PlayThis ? Symbol.Pause : Symbol.Play;
                ChangeSymbolIcon(symbol);
                HandleAnimation(dataTrack.PlayThis);
            }
            catch (Exception ex) { }
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
                if (audio.MainArtists == null || (audio.MainArtists).Count == 0)
                {
                    MainView.OpenSection(audio.Artist, SectionType.Search);
                }
                else 
                {
                    MainView.OpenSection(audio.MainArtists[0].Id, SectionType.Artist);
                }
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
            }
        }

        private void GoArtist_Click(object sender, RoutedEventArgs e)
        {
            GoMainArtist();
        }

        public void Download_Click(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }

        public void AddToQueue_Click(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }

        public void AddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }

        public void AddRemove_Click(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
        }
        public void AddArtistIgnore_Click(object sender, RoutedEventArgs e)
        {
            // ��� ��� �����
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
            if ((DataContext as ExtendedAudio).PlayThis) return;
            FadeInAnimationGridPlayIcon.Pause();
            FadeOutAnimationGridPlayIcon.Begin();

            //FadeOutAnimation.Begin();
        }

        private void UCcontrol_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
 
            dataTrack.iVKGetAudio.currentTrack = dataTrack.NumberInList;
            AudioPlayer.PlayList(dataTrack.iVKGetAudio);
        }
    }
}