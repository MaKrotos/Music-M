using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Views.LoginWindow;
using VK_UI3.Views.Share;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{

    public class WaitParameters
    {
        public SectionType sectionType;
        public string SectionID;
        public Section section;
        public IVKGetAudio iVKGetAudio;
        public OpenedPlayList openedPlayList;
        public AudioPlaylist Playlist;
        public Object moreParams;
        public MainView mainView;
        public string searchText;
    }

    public sealed partial class WaitView : Microsoft.UI.Xaml.Controls.Page, IDisposable
    {
        public WaitView()
        {
            this.InitializeComponent();
            this.Loaded += WaitView_Loaded;
            this.Unloaded += WaitView_Unloaded;
        }

        private void WaitView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

            LoadAsync();
            MainWindow.onRefreshClicked += MainWindow_onRefreshClicked;
        }

        WaitParameters waitParameters;

        private void WaitView_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            MainWindow.onRefreshClicked -= MainWindow_onRefreshClicked;
            this.handlerContainer.Handler = null;
            this.Loaded -= WaitView_Loaded;
            this.Unloaded -= WaitView_Unloaded;
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

            }
        }


        private async Task LoadUserCestion(string ownerID)
        {
            try
            {
                var artist = await VK.vkService.GetAudioUser(ownerID);
                loadSection(artist.Catalog.DefaultSection);
            }
            catch (Exception ex)
            {


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
                    SectionType.UserSection => LoadUserCestion(waitParameters.SectionID),

                    SectionType.Search => string.IsNullOrEmpty(waitParameters.searchText) ? 
                                            loadSection(waitParameters.SectionID) : 
                                            LoadSearchSection(waitParameters.searchText),

                    SectionType.PlayList => LoadPlayList(handlerContainer),
                    SectionType.UserPlayListList => UserPlayListList(),
                    SectionType.MyListAudio => LoadMyAudioList(handlerContainer),
                    SectionType.ConversDialogs => LoadDialogs(),
                    SectionType.LoadFriends => LoadFriends(),
            
                    _ => throw new ArgumentOutOfRangeException()
                }); ;

                if (waitParameters.sectionType == SectionType.Search)
                {
                    MainView.mainView.showSearch();
                }
                else
                {
                    MainView.mainView.hideSearch();

                }
            }
            finally
            {

            }
        }


        private async Task LoadDialogs()
        {
            _ = Task.Run(async () =>
            {
                var parameters = new GetConversationsParams();
                parameters.Count = 50;
                parameters.Offset = 0;
                parameters.Extended = true;
                parameters.Fields = new string[] {
                "photo_max_orig",
                "online",
                "can_write_private_message"
            };

                var a = (await VK.api.Messages.GetConversationsAsync(parameters));

                ConversationsListParams conversationsListParams;
                if (waitParameters.moreParams != null && waitParameters.moreParams is ConversationsListParams paramss) conversationsListParams = paramss;
                else conversationsListParams = new();
                conversationsListParams.result = a;
                if (a.Count < 50) conversationsListParams.itsAll = true;

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    frameSection.Navigate(typeof(ConversationsList), conversationsListParams, new DrillInNavigationTransitionInfo());
                });
            });
                
        }

        private async Task LoadFriends()
        {

            _ = Task.Run(async () =>
            { 
                var parameters = new VkParameters
            {
                { "count", 25 },
                { "fields", "can_see_audio,photo_50,online,online,photo_100,photo_200_orig,status,nickname,can_write_private_message"},
                { "offset", 0 },
                { "order", "name" }
            };

            var a = (await VK.api.CallAsync("friends.get", parameters)).ToVkCollectionOf<User>(
                x => parameters["fields"] != null
                      ? x
                      : new User
                      {
                          Id = x
                      }
                );
                FriendsListParametrs userPlayListParameters;
                if (waitParameters.moreParams != null && waitParameters.moreParams is FriendsListParametrs paramss) userPlayListParameters = paramss;
                else userPlayListParameters = new();
                if (a.Count < 25) userPlayListParameters.itsAll = true;
                userPlayListParameters.friends = a;
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    frameSection.Navigate(typeof(FriendsList), userPlayListParameters, new DrillInNavigationTransitionInfo());
                });
            });
        }

        private async Task UserPlayListList()
        {
            _ = Task.Run(async () =>
            {
                var id = long.Parse(waitParameters.SectionID);
                var list = await VK.api.Audio.GetPlaylistsAsync(id, 100);
                UserPlayListParameters userPlayListParameters = new UserPlayListParameters();
                userPlayListParameters.VKaudioPlaylists = list;
                userPlayListParameters.UserId = id;
                userPlayListParameters.offset = 100;
                userPlayListParameters.LoadedAll = (list.Count != 100);
                userPlayListParameters.openedPlayList = waitParameters.openedPlayList;

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    frameSection.Navigate(typeof(UserPlayList), userPlayListParameters, new DrillInNavigationTransitionInfo());
                });
            });
        }

        private async Task LoadPlayList(HandlerContainer handlerContainer)
        {
            _ = Task.Run(async () =>
            {
                if (waitParameters.iVKGetAudio == null)
                    waitParameters.iVKGetAudio = new PlayListVK(this.waitParameters.Playlist, this.DispatcherQueue);

                if (waitParameters.iVKGetAudio.listAudio.Count != 0)
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        frameSection.Navigate(typeof(PlayListPage), waitParameters.iVKGetAudio, new DrillInNavigationTransitionInfo());
                    });
                }
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
                            waitParameters.iVKGetAudio.onListUpdate -= (handlerContainer.Handler);
                        };
                    waitParameters.iVKGetAudio.onListUpdate += (handlerContainer.Handler);
                    if (!waitParameters.iVKGetAudio.getLoadedTracks)
                        waitParameters.iVKGetAudio.GetTracks();
                }
            });
        }

        private async Task LoadMyAudioList(HandlerContainer handlerContainer)
        {
            waitParameters.iVKGetAudio = new UserAudio(AccountsDB.activeAccount.id, this.DispatcherQueue);

            handlerContainer.Handler = (sender, e) =>
            {
                //    iVKGetAudio.onListUpdate.RemoveHandler(handlerContainer.Handler);
                this.DispatcherQueue.TryEnqueue(async () =>
                {

                    handlerContainer.Handler = null; // Освободить ссылку на обработчик
                    frameSection.Navigate(typeof(PlayListPage), waitParameters.iVKGetAudio, new DrillInNavigationTransitionInfo());
                });
                waitParameters.iVKGetAudio.onListUpdate -= (handlerContainer.Handler);
            };

            waitParameters.iVKGetAudio.onListUpdate += (handlerContainer.Handler);
        }

        private async Task loadSection(string sectionID, bool showTitle = false)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var sectin = await VK.vkService.GetSectionAsync(sectionID);
                    this.waitParameters.section = sectin.Section;
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        frameSection.Navigate(typeof(SectionView), waitParameters.section, new DrillInNavigationTransitionInfo());
                    });
                }
                catch (Exception ex) {
                
                }
            });
        }

        public void Dispose()
        {
            MainWindow.onRefreshClicked -= MainWindow_onRefreshClicked;
            this.handlerContainer.Handler = null;
        }
    }
}
