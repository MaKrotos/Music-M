using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using VK_UI3.Helpers;
using VK_UI3.Views;
using VK_UI3.VKs.IVK;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class RecomUsersControl : UserControl
    {
        public static Windows.UI.Color GetColorFromHex(string hexaColor)
        {
            //get the color as System.Drawing.Color
            var clr = (System.Drawing.Color)new ColorConverter().ConvertFromString(hexaColor);

            //convert it to Windows.UI.Color
            return Windows.UI.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
        }
        public RecomUsersControl()
        {
            this.InitializeComponent();

            DataContextChanged += RecomUsersControl_DataContextChanged;
        }
        RecommendedPlaylist _PlayList;
        private void RecomUsersControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            _PlayList = (DataContext as RecommendedPlaylist);

            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(AvatarImage, this.DispatcherQueue);


            update();
        }
        Helpers.Animations.AnimationsChangeImage animationsChangeImage;
        PlayListVK playListVK = null;
        ObservableCollection<ExtendedAudio> extendedAudios = new ObservableCollection<ExtendedAudio>();
        private void update()
        {
            playListVK = new PlayListVK(_PlayList, DispatcherQueue);
            double percentage;
            if (double.TryParse(_PlayList.Percentage.Replace(".", ","), out percentage))
            {
                PercentsTXT.Text = Math.Round(percentage * 100) + "%";
            }

            PercentsDescrTXT.Text = _PlayList.PercentageTitle;
            PlayListNameTXT.Text = _PlayList.Playlist.Title;

            var a = GetColorFromHex(_PlayList.Color); ;
            var b = a;
            b.A = 50;

            gradStart.Color = GetColorFromHex(_PlayList.Color);
            gradStop.Color = b;

            if (_PlayList.Playlist.userOwner != null)
            {
                animationsChangeImage.ChangeImageWithAnimation(_PlayList.Playlist.userOwner.Photo100);
            }
            if (_PlayList.Playlist.groupOwner != null)
            {
                animationsChangeImage.ChangeImageWithAnimation(_PlayList.Playlist.groupOwner.Photo100);
            }
            UserNameTXT.Text = _PlayList.Playlist.OwnerName;
            if (!playListVK.listAudioTrue.SequenceEqual(extendedAudios))
            {
                extendedAudios.Clear();
                foreach (var item in playListVK.listAudioTrue)
                {
                    extendedAudios.Add(item);
                }
            }

        }

        private void StackPanel_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint(sender as UIElement).Properties.IsLeftButtonPressed)
            {

                MainView.OpenPlayList(_PlayList.Playlist);
            }

        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            ShowAnimation.Pause();
            HideAnimation.Begin();
        }
    }
}
