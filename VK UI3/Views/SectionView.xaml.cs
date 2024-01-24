using Microsoft.AppCenter.Crashes;
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
using System;
using System.Collections.Generic;
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            openedSectionView = this;
            // Параметр передается как объект, поэтому его нужно привести к нужному типу
            var section = e.Parameter as Section;

            if (section != null)
            {
           
                loadSection(section.Id);
            }
            else
            {
                 var sectionID = e.Parameter as String;
                if (sectionID != null)
                {
                   
                    loadSection(sectionID);
                }
            }

        }

        private async void loadSection(string sectionID)
        {
            var sectin =  await VK.vkService.GetSectionAsync(sectionID);
            this.section = sectin.Section;



            OnPropertyChanged(nameof(section));
        }
        private bool nowOpenSearchSug = false;
        internal async void ReplaceBlocks(string replaceId)
        {
            nowOpenSearchSug = false;
            try
            {
                var replaces = await VK.vkService.ReplaceBlockAsync(replaceId).ConfigureAwait(false);

                var toReplaceBlockIds = replaces.Replacements.ReplacementsModels.SelectMany(b => b.FromBlockIds)
                    .ToHashSet();

                var newBlocks = new List<Block>();

                foreach (var block in section.Blocks)
                {
                    if (!toReplaceBlockIds.Contains(block.Id))
                    {
                        newBlocks.Add(block);
                    }
                    else
                    {
                        newBlocks.AddRange(replaces.Replacements.ReplacementsModels
                            .Where(b => b.FromBlockIds.Contains(block.Id))
                            .SelectMany(b => b.ToBlocks));
                    }
                }

                section.Blocks = newBlocks;
                OnPropertyChanged(nameof(section));
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
           
                var control = (ListViewItem)container;
         
                    if (item is Block)
                    {
                        BlockControl blockControl = new BlockControl();
                        var block = (Block)item;
                        object resource;
                        string key = (string.IsNullOrEmpty(block.Layout?.Name) ? block.DataType : $"{block.DataType}_{block.Layout.Name}");
                        if (blockControl.Resources.TryGetValue(key, out resource))
                        {
                            return resource as DataTemplate;
                        }
                        else if (blockControl.Resources.TryGetValue(block.DataType, out resource))
                        {
                            return resource as DataTemplate;
                        }
                        else
                        {
                            blockControl.Resources.TryGetValue("default", out resource);
                            return resource as DataTemplate;
                        }
                        
                    }
                
          

            return null;
        }


        DefaultControl? FallbackTemplate = new DefaultControl();
   
    }
}
