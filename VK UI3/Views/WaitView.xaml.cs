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
    public sealed partial class WaitView : Microsoft.UI.Xaml.Controls.Page
    {
        public WaitView()
        {
            this.InitializeComponent();

   
            this.Loading += WaitView_Loading;

            MainWindow.onRefreshClicked_clear();
            MainWindow.onRefreshClicked += MainWindow_onRefreshClicked;
        }

        private void WaitView_Loading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
        {
            LoadAsync();

        }

        private void MainWindow_onRefreshClicked(object sender, EventArgs e)
        {
            frameSection.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());
            LoadAsync();
        }

        public SectionType sectionType;
        public string SectionID;
        public Section section;
        internal IVKGetAudio iVKGetAudio;
        internal OpenedPlayList openedPlayList;

        public AudioPlaylist Playlist { get; internal set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            frameSection.Navigated += FrameSection_Navigated; 
                frameSection.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());




                var waitView = e.Parameter as WaitView;
                if (waitView == null) return;


                this.section = waitView.section;
                this.sectionType = waitView.sectionType;
                this.SectionID = waitView.SectionID;
                this.Playlist = waitView.Playlist;
                this.iVKGetAudio = waitView.iVKGetAudio;
                this.openedPlayList = waitView.openedPlayList;
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

        public async Task LoadAsync()
        {

            try
            {
                await (sectionType switch
                {
                    SectionType.None => loadSection(this.SectionID),
                    SectionType.Artist => LoadArtistSection(this.SectionID),
                    SectionType.Search => LoadSearchSection(this.SectionID),
                    SectionType.PlayList => LoadPlayList(),
                    SectionType.UserPlayListList => UserPlayListList(),
                    SectionType.MyListAudio => LoadMyAudioList(),
                    _ => throw new ArgumentOutOfRangeException()
                }); ; ;
            }
            finally
            {
                //  ContentState = ContentState.Loaded;
            }
        }

        private async Task UserPlayListList()
        {
            var id = long.Parse(SectionID);
            var list = await VK.api.Audio.GetPlaylistsAsync(id, 100);
            UserPlayList userPlayList = new UserPlayList();
            userPlayList.VKaudioPlaylists = list;
            userPlayList.UserId = id;
            userPlayList.offset = 100;
            userPlayList.LoadedAll = (list.Count != 100);
            userPlayList.openedPlayList = openedPlayList;


            frameSection.Navigate(typeof(UserPlayList), userPlayList, new DrillInNavigationTransitionInfo());
        }

        private async Task LoadPlayList()
        {
           if (iVKGetAudio == null)
                iVKGetAudio = new PlayListVK (this.Playlist, this.DispatcherQueue);

            if (iVKGetAudio.listAudio.Count != 0) {
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    frameSection.Navigate(typeof(PlayListPage), iVKGetAudio, new DrillInNavigationTransitionInfo());
                });
                }
            else
            {
                EventHandler handler = null;
                handler = (sender, e) =>
                {
                    this.DispatcherQueue.TryEnqueue(async () =>
                    {
                        frameSection.Navigate(typeof(PlayListPage), iVKGetAudio, new DrillInNavigationTransitionInfo());
                        // Отсоединить обработчик событий после выполнения Navigate
                        iVKGetAudio.onListUpdate -= handler;
                    });
                };
                iVKGetAudio.onListUpdate += handler;

            }

        }

        private async Task LoadMyAudioList()
        {
            var userAudio = new UserAudio(AccountsDB.activeAccount.id, this.DispatcherQueue);
            EventHandler handler = null;

            handler = (sender, e) => {
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    frameSection.Navigate(typeof(PlayListPage), userAudio, new DrillInNavigationTransitionInfo());
                    // Отсоединить обработчик событий после выполнения Navigate
                    userAudio.onListUpdate -= handler;
                });
            };

            userAudio.onListUpdate += handler;
        }

        private async Task loadSection(string sectionID, bool showTitle = false)
        {
        
            var sectin = await VK.vkService.GetSectionAsync(sectionID);
            this.section = sectin.Section;


            frameSection.Navigate(typeof(SectionView), section, new DrillInNavigationTransitionInfo());
        }



    }
}
