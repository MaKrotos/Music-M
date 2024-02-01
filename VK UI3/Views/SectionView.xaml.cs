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
    public sealed partial class SectionView : Page, INotifyPropertyChanged
    {
        public Section section;
        public string SectionID;
        public SectionType sectionType;
       
        public SectionView()
        {
            this.InitializeComponent();
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
            Search
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            openedSectionView = this;
            
           
         
            var section = e.Parameter as SectionView;

            if (section == null) return;

            this.section = section.section;
            this.sectionType = section.sectionType;
            this.SectionID = section.SectionID;

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
                if (query == null && nowOpenSearchSug) return;

                var res = await VK.vkService.GetAudioSearchAsync(query);

                if (query == null)
                {

                    try
                    {
                        res.Catalog.Sections[0].Blocks[1].Suggestions = res.Suggestions;
                         loadBlocks(res.Catalog.Sections[0].Blocks, null);
                        nowOpenSearchSug = true;
                    }
                    catch (Exception ex)
                    {
                         loadBlocks(res.Catalog.Sections[0].Blocks, null);
                    }

                    return;
                }

                nowOpenSearchSug = false;
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
                    _ => throw new ArgumentOutOfRangeException()
                });
            }
            finally
            {
              //  ContentState = ContentState.Loaded;
            }
        }


        private async Task loadSection(string sectionID, bool showTitle = false)
        {
            blocks.Clear();
            var sectin =  await VK.vkService.GetSectionAsync(sectionID);
            this.section = sectin.Section;
            loadBlocks(section.Blocks, section.NextFrom);
        }

        private void loadBlocks(List<Block> block, string nextValue)
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
