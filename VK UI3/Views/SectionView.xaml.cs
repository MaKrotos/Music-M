using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using MusicX.Core.Services;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VK_UI3.Controls;
using VK_UI3.Helpers;
using VK_UI3.Services;
using VK_UI3.Views.LoginWindow;
using VK_UI3.VKs;
using VkNet.AudioBypassService.Models.Auth;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static NLog.LayoutRenderers.Wrappers.ReplaceLayoutRendererWrapper;

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


            this.Loading += SectionView_Loading;
            this.Loaded += SectionView_Loaded;
        }

        private void SectionView_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = GetScrollViewer(ListBlocks);
            scrollViewer.ViewChanged += Scrollvi_ViewChanged;
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

        private void SectionView_Loading(FrameworkElement sender, object args)
        {
            
        }

        ScrollViewer scrollViewer = null;
      
        bool loadedAll = false;

        private bool CheckIfAllContentIsVisible(ScrollViewer scrollViewer)
        {
            if (scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight)
            {
                return true;
            }
            return false;
        }


        private void Scrollvi_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 50;
            if (isAtBottom)
            {
                 if (!loadedAll)
                 {
                    LoadAsync();
                 }
            }
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            openedSectionView = this;



            var section = e.Parameter as Section;
            if (section == null) return;

            this.section = section;
            //   this.sectionType = section;

          
        }
    
       

        public static ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer) return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
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
        private async Task loadSection(string sectionID, bool showTitle = false)
        {
            if (nextLoad == null) return;
            if (blockLoad) return;
            if (loadedAll) return;
            blockLoad = true;
            var sectin =  await VK.vkService.GetSectionAsync(sectionID, nextLoad);
            nextLoad = sectin.Section.NextFrom;
            if (sectin.Section.NextFrom == null) { 
                loadedAll = true; 
            }
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
            if (CheckIfAllContentIsVisible(scrollViewer))
            {
                LoadAsync();
            }
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
