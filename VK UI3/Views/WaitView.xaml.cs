using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Views.LoginWindow;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using Windows.Media.Playlists;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{

    public class WaitParameters {
        public SectionType sectionType;
        public string SectionID;
        public Section section;
        public IVKGetAudio iVKGetAudio;
        public OpenedPlayList openedPlayList;
        public AudioPlaylist Playlist;
    }

    public sealed partial class WaitView : Microsoft.UI.Xaml.Controls.Page, IDisposable
    {
        public WaitView()
        {
            this.InitializeComponent();

   
            this.Loading += WaitView_Loading;
            this.Unloaded += WaitView_Unloaded;

          
        }
        WaitParameters waitParameters;
        private void WaitView_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            MainWindow.onRefreshClicked -= MainWindow_onRefreshClicked;
            this.handlerContainer.Handler = null;
            this.Loading -= WaitView_Loading;
            this.Unloaded -= WaitView_Unloaded;
        }

        private void WaitView_Loading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
        {
            LoadAsync();
            MainWindow.onRefreshClicked += MainWindow_onRefreshClicked;
        }

        private void MainWindow_onRefreshClicked(object sender, EventArgs e)
        {
            frameSection.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());
            LoadAsync();
        }




        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            frameSection.Navigated += FrameSection_Navigated; 
            frameSection.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());

                var waitView = e.Parameter as WaitParameters;
                if (waitView == null) return;
                waitParameters = waitView;


        }

        private void FrameSection_Navigated(object sender, NavigationEventArgs e)
        {

            frameSection.BackStack.Clear();
        }

        private void FrameSection_Navigating(object sender, NavigatingCancelEventArgs e)
        {
           
        }

        private async Task LoadArtistSection(string artistId)
        {
            try
            {
                var artist = await VK.vkService.GetAudioArtistAsync(artistId);
                loadSection(artist.Catalog.DefaultSection);
            }
            catch (Exception ex)
            {
                AppCenterHelper.SendCrash(ex);

            }
        }

        private async Task LoadSearchSection(string query)
        {
            try
            {
                if (query == null 
                    ) return;

                var res = await VK.vkService.GetAudioSearchAsync(query);

                if (query == null)
                {

                    try
                    {
                        res.Catalog.Sections[0].Blocks[1].Suggestions = res.Suggestions;
                       // loadBlocks(res.Catalog.Sections[0].Blocks);
                     //   nowOpenSearchSug = true;
                    }
                    catch (Exception ex)
                    {
                        AppCenterHelper.SendCrash(ex);
                        // frameSection.Navigate(typeof(SectionView), section, new DrillInNavigationTransitionInfo());
                        // loadBlocks(res.Catalog.Sections[0].Blocks);
                    }
                    frameSection.Navigate(typeof(SectionView), res.Catalog.Sections[0], new DrillInNavigationTransitionInfo());
                    return;
                }

               // nowOpenSearchSug = false;
                
                loadSection(res.Catalog.DefaultSection);
            }
            catch (Exception ex)
            {
                AppCenterHelper.SendCrash(ex);


            }
        }

        public class HandlerContainer
        {
            public EventHandler Handler { get; set; }
        }
        HandlerContainer handlerContainer = new HandlerContainer();
        public async Task LoadAsync()
        {
            
            
            try
            {
                await (waitParameters.sectionType switch
                {
                    SectionType.None => loadSection(waitParameters.SectionID),
                    SectionType.Artist => LoadArtistSection(waitParameters.SectionID),
                    SectionType.Search => LoadSearchSection(waitParameters.SectionID),
                    SectionType.PlayList => LoadPlayList(handlerContainer),
                    SectionType.UserPlayListList => UserPlayListList(),
                    SectionType.MyListAudio => LoadMyAudioList(handlerContainer),
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            finally
            {
               
            }
        }


        private async Task UserPlayListList()
        {
            var id = long.Parse(waitParameters.SectionID);
            var list = await VK.api.Audio.GetPlaylistsAsync(id, 100);
            UserPlayListParameters userPlayListParameters = new UserPlayListParameters();
            userPlayListParameters.VKaudioPlaylists = list;
            userPlayListParameters.UserId = id;
            userPlayListParameters.offset = 100;
            userPlayListParameters.LoadedAll = (list.Count != 100);
            userPlayListParameters.openedPlayList = waitParameters.openedPlayList;
            frameSection.Navigate(typeof(UserPlayList), userPlayListParameters, new DrillInNavigationTransitionInfo());
        }

        private async Task LoadPlayList(HandlerContainer handlerContainer)
        {
     
            if (waitParameters.iVKGetAudio == null)
                waitParameters.iVKGetAudio = new PlayListVK(this.waitParameters.Playlist, this.DispatcherQueue);

            if (waitParameters.iVKGetAudio.listAudio.Count != 0) frameSection.Navigate(typeof(PlayListPage), waitParameters.iVKGetAudio, new DrillInNavigationTransitionInfo());
            else
            {

                handlerContainer.Handler = (sender, e) =>
                    {
                    //    iVKGetAudio.onListUpdate.RemoveHandler(handlerContainer.Handler);
                        this.DispatcherQueue.TryEnqueue(async () =>
                        {
                            handlerContainer.Handler = null; // Освободить ссылку на обработчик
                            frameSection.Navigate(typeof(PlayListPage), waitParameters.iVKGetAudio, new DrillInNavigationTransitionInfo());
                        });
                        waitParameters.iVKGetAudio.onListUpdate.RemoveHandler(handlerContainer.Handler);
                    };
                waitParameters.iVKGetAudio.onListUpdate.AddHandler(handlerContainer.Handler);
            }
        }

        private async Task LoadMyAudioList(HandlerContainer handlerContainer)
        {
            waitParameters.iVKGetAudio = new UserAudio(AccountsDB.activeAccount.id, this.DispatcherQueue);

            handlerContainer.Handler = (sender, e) => {
            //    iVKGetAudio.onListUpdate.RemoveHandler(handlerContainer.Handler);
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                   
                    handlerContainer.Handler = null; // Освободить ссылку на обработчик
                    frameSection.Navigate(typeof(PlayListPage), waitParameters.iVKGetAudio, new DrillInNavigationTransitionInfo());
                });
                waitParameters.iVKGetAudio.onListUpdate.RemoveHandler(handlerContainer.Handler);
            };

            waitParameters.iVKGetAudio.onListUpdate.AddHandler(handlerContainer.Handler);
        }

        private async Task loadSection(string sectionID, bool showTitle = false)
        {
            var sectin = await VK.vkService.GetSectionAsync(sectionID);
            this.waitParameters.section = sectin.Section;
            frameSection.Navigate(typeof(SectionView), waitParameters.section, new DrillInNavigationTransitionInfo());
        }

        public void Dispose()
        {
           MainWindow.onRefreshClicked -= MainWindow_onRefreshClicked;
           this.handlerContainer.Handler = null;
        }
    }
}
