using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using MusicX.Core.Services;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.Controls;
using VK_UI3.Helpers;
using VK_UI3.VKs;

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


          //  this.Loading += SectionView_Loading;
          //  this.Loaded += SectionView_Loaded;
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
            PlayList
        }
        string SectionID;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            openedSectionView = this;



            var section = e.Parameter as Section;
            if (section == null) return;

            this.section = section;
         //   this.sectionType = section;
            this.SectionID = section.Id;

            if (this.section != null && this.section.Blocks != null && this.section.Blocks.Count != 0)
            {
                loadBlocks(this.section.Blocks);
            }
            else
            {
                LoadAsync();
            }
            return;
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
                AppCenterHelper.SendCrash(ex);

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
                AppCenterHelper.SendCrash(ex);


            }
        }
        

        public async Task LoadAsync()
        {
          
            try
            {
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
              //  ContentState = ContentState.Loaded;
            }
        }

        bool blockLoad = false;
        bool hidedLoad = false;
        private async Task loadSection(string sectionID, bool showTitle = false)
        {
            blocks.Clear();
            var sectin =  await VK.vkService.GetSectionAsync(sectionID);
            this.section = sectin.Section;
            if (section.Blocks.Count() == 0)
            { 
                loadedAll = true;
                return;
            }
            blockLoad = false;
            loadBlocks(sectin.Section.Blocks);
        }

        private void loadBlocks(List<Block> block)
        {
            foreach (var item in block)
            {
                blocks.Add(item);
            }
            OnPropertyChanged(nameof(section));
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



        bool loadedAll = false;
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
                    LoadAsync();
                }
            }
        }

        private void HideLoad()
        {
            LoadingIndicator.Visibility = Visibility.Collapsed;
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
