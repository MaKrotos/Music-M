using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Models;
using MusicX.Core.Services;
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
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class LinksController : UserControl
    {
        Helpers.Animations.AnimationsChangeImage AnimationsChangeImage;
        public LinksController()
        {
            this.InitializeComponent();
            AnimationsChangeImage = new AnimationsChangeImage(imageLink, this.DispatcherQueue);
          

            this.Unloaded += PlaylistControl_Unloaded;


            DataContextChanged += LinksController_DataContextChanged;


        }
        public bool setCOlorTheme { get; set; } = false;
        MusicX.Core.Models.Link link = null;
        private void LinksController_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (DataContext is MusicX.Core.Models.Link linked)
            {
                if (link == linked) return;
                link = linked;
                imageLink.Source = null;

           
    
                MainText.Text = link.Title;
                ToolTip toolTip = new ToolTip();
                toolTip.Content = $"{link.Title}";
                if (link.Subtitle == null || link.Subtitle == "") {
                    SecondText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SecondText.Visibility = Visibility.Visible;
                    SecondText.Text = link.Subtitle;
                    toolTip.Content += $"\n{link.Subtitle}";
                }


      
                ToolTipService.SetToolTip(this, toolTip);
              // bool setCOlorTheme = false;
              //  if (link.Title is "Недавнee" or "Плейлисты" or "Альбомы" or "Артисты и кураторы")
              //      setCOlorTheme = true;

                if (link.Meta.ContentType is "group" or "user" or "chat")
                {
                    if (link.Image is not (null or { Count: 0 }))
                    {
                        imageLink.Visibility = Visibility.Visible;
                        AnimationsChangeImage.ChangeImageWithAnimation(link.Image[1].Url, setColorTheme: setCOlorTheme);
                    }
                    else
                    {
                        imageLink.Visibility = Visibility.Collapsed;
                    }

                }
                else
                {
                    if (link.Image != null)
                    {
                        imageLink.Visibility = Visibility.Visible;
                        AnimationsChangeImage.ChangeImageWithAnimation(link.Image[0].Url, setColorTheme: setCOlorTheme);
                    }
                    else
                    {
                        imageLink.Visibility = Visibility.Collapsed;
                    
                    }

                }
               
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

                switch (link.Meta.ContentType)
                {
                    case "" or null or "audio_recent" or "audio_followings":
                    {
                        var match = IsPodcstURL(link.Url);

                        if (match)
                        {
                            //var podcasts = await vkService.GetPodcastsAsync(Link.Url);
                            //await navigationService.OpenSection(podcasts.Catalog.DefaultSection, true);

                            return;
                        }

                        match = IsMiniAppUrl(link.Url);

                        if (match)
                        {
                            //var appId = match.Groups["app"].Value;
                            //var miniAppResponse = await vkService.GetMiniApp(appId, link.Url);

                           // navigationService.OpenExternalPage(new MiniAppView(new MiniAppViewModel(appId, miniAppResponse.Object.WebviewUrl)));
                            return;
                        }

                        // ураа костыль для "Настроить рекомендации"
                        var matchRecoms = IsConfugureRecomsUrl(link.Url);

                        if (matchRecoms)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = link.Url,
                                UseShellExecute = true
                            });

                            return;
                        }

                        var music = await VK.vkService.GetAudioCatalogAsync(link.Url);
                        MainView.OpenSection(music.Catalog.DefaultSection);

                        return;
                    }
                    case "artist":
                    {
                        var url = new Uri(link.Url);

                        MainView.OpenSection(url.Segments.Last(), SectionType.Artist);
                        break;
                    }
                    
                   // case "group" or "user" or "chat" when CustomSectionsService.CustomLinkRegex().IsMatch(link.Id):
                   // {
                   //     MainView.OpenSection(link.Id);
                   //     return;
                   // }

                    case "group" or "user" or "chat":
                    {
                        var match = IsUserProfileRegex(link.Url);
                        if (match)
                        {
                            var music = await VK.vkService.GetAudioCatalogAsync(link.Url);

                            MainView.OpenSection(music.Catalog.DefaultSection);

                            return;
                        }


                        Process.Start(new ProcessStartInfo
                        {
                            FileName = link.Url,
                            UseShellExecute = true
                        });
                        break;
                    }
                    case "curator":
                    {
                        var curator = await VK.vkService.GetAudioCuratorAsync(link.Meta.TrackCode, link.Url);

                        MainView.OpenSection(curator.Catalog.DefaultSection);
                        break;
                    }
                    case "audio_playlists" or "audio_albums":
                    {
                        var match = IsPlaylistUrl(link.Url);
                      

                        if (IsAlbumUrl(link.Url) || IsPlaylistUrl(link.Url))
                        {
                            var splited = link.Url.ToString().Split("/");
                            splited = splited.Last().Split("_");



                            MainView.OpenPlayList(
                                long.Parse(splited[1]),
                                long.Parse(splited[0]),
                                splited[2]
                                );

                            return;
                        }

                        var catalog = await VK.vkService.GetAudioCatalogAsync(link.Url);

                        MainView.OpenSection(catalog.Catalog.DefaultSection);
                        break;
                    }
                    case "custom":
                    {
                        MainView.OpenSection(link.Meta.TrackCode);
                        break;
                    }
                }


               

                if (link.Url.Equals("https://vk.com/audio?catalog=my_audios"))
                {
                    MainView.OpenMyPage(SectionType.MyListAudio);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = link.Url,
                    UseShellExecute = true
                });

            }
            catch (Exception ex)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = link.Url,
                        UseShellExecute = true
                    });
                }
                catch {  }
            }
        }


        static bool IsAlbumUrl(string url)
        {
            return Regex.IsMatch(url, @"https://vk\.com/music/(?:album|playlist)/(?<id>[\-\w]+)$");
        }

        static bool IsPodcstURL(string url)
        {
            return Regex.IsMatch(url, @"https://vk\.com/podcasts\?category=[0-9]+$");
        }

        static bool IsUserProfileRegex(string url)
        {
            return Regex.IsMatch(url, @"https://vk\.com/audios\-?[0-9]+$");
        }

        static bool IsMiniAppUrl(string url)
        {
            return Regex.IsMatch(url, @"https://vk\.com/(?<app>app\-?[0-9]+)(?>[\?\#].+)?$");
        }

        static bool IsPlaylistUrl(string url)
        {
            return Regex.IsMatch(url, @"https://vk.com/music/playlist/\d+_\d+_[a-z0-9]+$");
        }
        static bool IsConfugureRecomsUrl(string url)
        {
            return Regex.IsMatch(url, @"https://vk\.com/audio\?popup=recoms_onboarding+$");
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

            if (link.Meta.ContentType is "group" or "user" or "chat")
            {
                if (!Regex.IsMatch(link.Id, CustomSectionsService.CustomLinkRegex) &&
                    !Regex.IsMatch(link.Url, "https://vk.com/audios[0-9]+$"))
                {
                    HideLinkAnimation.Pause();
                    showLinkAnimation.Begin();
                }
            }


            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            showLinkAnimation.Pause();
            HideLinkAnimation.Begin();
            ShowAnimation.Pause();
            HideAnimation.Begin();
        }






    }
}
