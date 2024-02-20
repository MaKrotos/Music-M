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
using Windows.Media.Playlists;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    public sealed partial class WaitView : Page
    {
        public WaitView()
        {
            this.InitializeComponent();

            this.Loaded += WaitView_Loaded; ;
        }

        private void WaitView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            LoadAsync();
        }

        public SectionType sectionType;
        public string SectionID;
        public Section section;
        internal IVKGetAudio iVKGetAudio;

        public MusicX.Core.Models.Playlist Playlist { get; internal set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            frameSection.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());



            var waitView = e.Parameter as WaitView;
            if (waitView == null) return;


            this.section = waitView.section;
            this.sectionType = waitView.sectionType;
            this.SectionID = waitView.SectionID;
            this.Playlist = waitView.Playlist;
            this.iVKGetAudio = waitView.iVKGetAudio;

          
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
                    //&& nowOpenSearchSug
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
                    SectionType.MyListAudio => LoadMyAudioList(),
                    _ => throw new ArgumentOutOfRangeException()
                }); ; ;
            }
            finally
            {
                //  ContentState = ContentState.Loaded;
            }
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
