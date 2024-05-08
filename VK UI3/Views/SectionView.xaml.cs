using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.Controls;
using VK_UI3.Controls.Blocks;
using VK_UI3.Helpers;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SectionView : Page
    {
        public Section section;


        string nextLoad = null;

        public SectionType sectionType;

        public SectionView()
        {
            this.InitializeComponent();



            this.Loaded += SectionView_Loaded;
            this.Unloaded += SectionView_Unloaded;
        }

        private void SectionView_Unloaded(object sender, RoutedEventArgs e)
        {

            this.Loaded -= SectionView_Loaded;
            this.Unloaded -= SectionView_Unloaded;
            this.scrollVIew.ViewChanged -= scrollVIew_ViewChanged;
        }

        private void SectionView_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.section != null && this.section.Blocks != null && this.section.Blocks.Count != 0)
            {
                this.nextLoad = this.section.NextFrom;
                loadBlocks(this.section.Blocks);
            }
            else
            {

                LoadAsync();
            }
        }



        ScrollViewer scrollViewer = null;

        bool loadedAll = false;

        private bool CheckIfAllContentIsVisible(ScrollViewer scrollViewer)
        {
            if (scrollVIew.ViewportHeight >= scrollVIew.ExtentHeight)
            {
                return true;
            }
            return false;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                //   this.ImgUri = uri;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
        public static SectionView openedSectionView;

        public enum SectionType
        {
            None,
            Artist,
            Search,
            MyListAudio,
            PlayList,
            UserPlayListList,
            UserSection,
            MessConv,
            ConversDialogs,
            LoadFriends
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            openedSectionView = this;



            var section = e.Parameter as Section;
            if (section == null) return;
            this.section = section;

        }




        ResponseData artist = null;
        private async Task LoadArtistSection(string artistId)
        {
            try
            {
                if (artist == null)
                    artist = await VK.vkService.GetAudioArtistAsync(artistId);
                loadSection(artist.Catalog.DefaultSection);
            }
            catch (Exception ex)
            {

            }
        }
        ResponseData res = null;
        private async Task LoadSearchSection(string query)
        {

            try
            {
                if (query == null && nowOpenSearchSug) return;

                if (res == null)
                    res = await VK.vkService.GetAudioSearchAsync(query);

                if (query == null)
                {

                    try
                    {
                        res.Catalog.Sections[0].Blocks[1].Suggestions = res.Suggestions;
                        loadBlocks(res.Catalog.Sections[0].Blocks);
                        nowOpenSearchSug = true;
                    }
                    catch (Exception ex)
                    {
                        loadBlocks(res.Catalog.Sections[0].Blocks);
                    }

                    return;
                }

                nowOpenSearchSug = false;
                loadSection(res.Catalog.DefaultSection);

            }
            catch (Exception ex)
            {


            }
        }


        public async Task LoadAsync()
        {
            try
            {

                if (tracksFull != null)
                {
                    tracksFull.DispatcherQueue.TryEnqueue(() =>
                    {
                        tracksFull.sectionAudio.GetTracks();
                    });
                    return;
                }


                await (sectionType switch
                {
                    SectionType.None => loadSection(section.Id),
                    SectionType.Artist => LoadArtistSection(section.Id),
                    SectionType.Search => LoadSearchSection(section.Id),
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            finally
            {

            }
        }

        bool blockLoad = false;



        private void HideLoad()
        {
            LoadingIndicator.Visibility = Visibility.Collapsed;
        }

        bool hidedLoad = false;
        private async Task loadSection(string sectionID, bool showTitle = false)
        {

            try
            {
                // string key = string.IsNullOrEmpty(block.Layout?.Name) ? block.DataType : $"{block.DataType}_{block.Layout.Name}";
                if (nextLoad == null || loadedAll)
                {
                    if (hidedLoad) return;
                    hidedLoad = true;
                    HideLoad();
                    return;
                }
                if (blockLoad) return;

                blockLoad = true;

                var sectin = await VK.vkService.GetSectionAsync(sectionID, nextLoad);
                nextLoad = sectin.Section.NextFrom;
                if (sectin.Section.NextFrom == null)
                {
                    loadedAll = true;
                }
                this.section = sectin.Section;
                if (section.Blocks.Count() == 0)
                {
                    blockLoad = false;
                    loadedAll = true;
                    return;
                }
                blockLoad = false;

                loadBlocks(sectin.Section.Blocks);
            }
            catch (Exception e)
            {

                blockLoad = false;
                throw e;
            }
        }
        ListTracksFull _tracksFull = null;
        ListTracksFull tracksFull
        {
            get
            {
                if (_tracksFull != null)
                {
                    return _tracksFull;
                }

                var lastItem = ListBlocks.Items.LastOrDefault();
                if (lastItem != null)
                {
                    var container = ListBlocks.ContainerFromItem(lastItem) as ListViewItem;
                    if (container == null)
                    {
                        return null;
                    }

                    var dataTemplate = container.ContentTemplateRoot as ListTracksFull;
                    if (dataTemplate != null && _tracksFull == null)
                    {
                        _tracksFull = dataTemplate;
                        if (!listened)
                        {
                            listened = true;
                            _tracksFull.sectionAudio.onListUpdate += (SectionAudio_onListUpdate);
                        }
                    }
                }

                return _tracksFull;
            }
            set
            {
                _tracksFull = value;
            }
        }

        bool listened = false;

        bool connectedLoad = false;
        private void loadBlocks(List<Block> block)
        {
            foreach (var item in block)
            {

                if (tracksFull != null)
                {


                    foreach (var audio in item.Audios)
                    {
                        this.DispatcherQueue.TryEnqueue(() =>
                        {
                            tracksFull.sectionAudio.listAudio.Add(new ExtendedAudio(audio, tracksFull.sectionAudio));
                            tracksFull.sectionAudio.Next = item.NextFrom;
                            if (item.NextFrom == null) tracksFull.sectionAudio.itsAll = true;
                            tracksFull.sectionAudio.NotifyOnListUpdate();

                        });
                    }
                    continue;
                }

                this.DispatcherQueue.TryEnqueue(() =>
                        {
                            blocks.Add(item);
                        });



            }
            if (CheckIfAllContentIsVisible(scrollViewer))
            {
                LoadAsync();
            }
            if (nextLoad == null || loadedAll)
            {
                if (hidedLoad) return;
                hidedLoad = true;
                HideLoad();
            }


        }


        private void SectionAudio_onListUpdate(object sender, EventArgs e)
        {
            if ((sender as SectionAudio).itsAll)
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    HideLoad();
                });

        }

        private bool nowOpenSearchSug = false;


        public ObservableCollection<Block> blocks = new ObservableCollection<Block>();


        internal async void ReplaceBlocks(string replaceId)
        {
            nowOpenSearchSug = false;
            try
            {
                var replaces = await (VK.vkService.ReplaceBlockAsync(replaceId).ConfigureAwait(false));

                var toReplaceBlockIds = replaces.Replacements.ReplacementsModels.SelectMany(b => b.FromBlockIds)
                    .ToHashSet();

                // Найти блоки, которые нужно заменить
                var blocksToReplace = blocks.Where(block => toReplaceBlockIds.Contains(block.Id)).ToList();

                this.DispatcherQueue.TryEnqueue(() =>
                {
                    // Удалить блоки
                    foreach (var block in blocksToReplace)
                    {
                        blocks.Remove(block);
                    }

                    // Добавить новые блоки
                    foreach (var block in replaces.Replacements.ReplacementsModels.SelectMany(b => b.ToBlocks))
                    {
                        blocks.Add(block);
                    }
                });
            }
            catch (Exception ex)
            {
                // Обработка исключений
            }
        }




        private void scrollVIew_ViewChanged(ScrollView sender, object args)
        {
            var scrollViewer = sender as ScrollView;

            var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 50;
            if (isAtBottom)
            {
                if (nextLoad == null || loadedAll)
                {
                    if (hidedLoad) return;
                    hidedLoad = true;
                    HideLoad();
                }

                if (!loadedAll)
                {
                    _ = LoadAsync();
                }
            }
        }
    }



    public class BlockTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate? SelectTemplateCore(object? item, DependencyObject container)
        {
            if (item is Block block)
            {
                BlockControl blockControl = new BlockControl();
                string key = string.IsNullOrEmpty(block.Layout?.Name) ? block.DataType : $"{block.DataType}_{block.Layout.Name}";

                if (blockControl.Resources.TryGetValue(key, out object resource) || blockControl.Resources.TryGetValue(block.DataType, out resource) || blockControl.Resources.TryGetValue("default", out resource))
                {
                    return resource as DataTemplate;
                }
            }

            return null;
        }
    }
}
