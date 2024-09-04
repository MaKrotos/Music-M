using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Exceptions.Boom;
using MusicX.Core.Models;
using MusicX.Core.Models.Boom;
using MusicX.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using VK_UI3.Services;
using VK_UI3.Views.LoginWindow;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class MixPageParametres
    {
      public  List<MusicX.Core.Models.Boom.Artist> artists;
      public  List<MusicX.Core.Models.Boom.Tag> Tags;
    }
    public sealed partial class MixPage : Page
    {
        public MixPage()
        {
            this.InitializeComponent();
            this.Loading += MixPage_Loading;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {



            var mixPageParametres = e.Parameter as MixPageParametres;
            if (mixPageParametres == null) return;
            this.mixPageParametres = mixPageParametres;

            Artists.ReplaceRange(mixPageParametres.artists);
            Tags.ReplaceRange(mixPageParametres.Tags);
        }
        MixPageParametres mixPageParametres;

        private void MixPage_Loading(FrameworkElement sender, object args)
        {
            //LoadMixesAsync();
         
        }

        public ObservableRangeCollection<MusicX.Core.Models.Boom.Artist> Artists { get; set; } = new();
        public ObservableRangeCollection<Tag> Tags { get; set; } = new();
        bool IsLoaded = false;
        private async Task LoadMixesAsync()
        {
            try
            {
                

                var artists = await VKs.VK.boomService.GetArtistsAsync();
                var tags = await VKs.VK.boomService.GetTagsAsync();

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    Artists.ReplaceRange(artists);
                    Tags.ReplaceRange(tags);
                });
                IsLoaded = true;

            }
            catch (UnauthorizedException)
            {


                await VKs.VK.BoomUpdateToken();

                await LoadMixesAsync();
            }
            catch (Exception ex)
            {
                
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
