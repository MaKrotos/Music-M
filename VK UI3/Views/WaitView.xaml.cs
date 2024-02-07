using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Services;
using VK_UI3.Views.LoginWindow;
using VK_UI3.VKs;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    
        }

        public SectionType sectionType;
        public string SectionID;
        public Section section;


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            frameSection.Navigate(typeof(waitPage), null, new DrillInNavigationTransitionInfo());



            var waitView = e.Parameter as WaitView;
            if (waitView == null) return;


            this.section = waitView.section;
            this.sectionType = waitView.sectionType;
            this.SectionID = waitView.SectionID;

            LoadAsync();

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

        public async Task LoadAsync()
        {

            try
            {
                await (sectionType switch
                {
                    SectionType.None => loadSection(this.SectionID),
                    SectionType.Artist => LoadArtistSection(this.SectionID),
                    SectionType.Search => LoadSearchSection(this.SectionID),
                    SectionType.MyListAudio => LoadMyAudioList(),
                    _ => throw new ArgumentOutOfRangeException()
                }); ;
            }
            finally
            {
                //  ContentState = ContentState.Loaded;
            }
        }

        private async Task LoadMyAudioList()
        {
            var userAudio = new UserAudio(AccountsDB.activeAccount.id, this.DispatcherQueue);
            EventHandler handler = null;

            handler = (sender, e) => {
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    frameSection.Navigate(typeof(MainMenu), userAudio, new DrillInNavigationTransitionInfo());
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
