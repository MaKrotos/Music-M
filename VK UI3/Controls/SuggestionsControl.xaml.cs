using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Models;
using MusicX.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using Windows.Media.Playlists;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class SuggestionsControl : UserControl
    {
        Helpers.Animations.AnimationsChangeImage AnimationsChangeImage;
        public SuggestionsControl()
        {
            this.InitializeComponent();
          

            this.Unloaded += PlaylistControl_Unloaded;


            DataContextChanged += SuggestionsControl_DataContextChanged;


        }
        public bool setCOlorTheme { get; set; } = false;
        MusicX.Core.Models.Suggestion suggestions = null;
        private void SuggestionsControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (DataContext is MusicX.Core.Models.Suggestion suggestion)
            {
                this.suggestions = suggestion;

                this.MainText.Text = suggestion.Title;
                if (!string.IsNullOrEmpty(suggestions.Subtitle))
                {
                    this.Desc.Text = suggestion.Subtitle;
                    this.Desc.Visibility = Visibility.Visible;
                    this.MainText.MaxHeight = 20;
                }
                else
                {
                    this.MainText.MaxHeight = 40;
                    this.Desc.Visibility = Visibility.Collapsed;
                }
                ToolT.Content = $"{suggestions.Title}{(!string.IsNullOrEmpty(suggestions.Subtitle) ? $" ({suggestions.Subtitle})" : string.Empty)}";



                //ToolTipService.SetToolTip(this, toolTip);
                // bool setCOlorTheme = false;
                //  if (link.Title is "Недавнee" or "Плейлисты" or "Альбомы" or "Артисты и кураторы")
                //      setCOlorTheme = true;


            }
        }

      

        private void PlaylistControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= PlaylistControl_Unloaded;
        }





        private async void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

            try
            {
                var result = await VK.vkService.GetAudioSearchAsync(suggestions.Title, suggestions.Context);
                MainView.OpenSection(result.Catalog.DefaultSection, SectionView.SectionType.Search);
                MainView.mainView.SearchSetText(suggestions.Title);
            }
            catch (Exception ex)
            {

            }
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

          


            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
           // showLinkAnimation.Pause();
           // HideLinkAnimation.Begin();
            ShowAnimation.Pause();
            HideAnimation.Begin();
        }






    }
}
