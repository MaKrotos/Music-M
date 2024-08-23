using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using VK_UI3.VKs;
using System.Threading;
using VK_UI3.VKs.IVK;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;


namespace VK_UI3.Controls.Blocks
{
    public sealed partial class SuggestionsList : UserControl, IBlockAdder
    {

        ObservableCollection<Suggestion> Suggestions = new();

    

        public SuggestionsList()
        {
            this.InitializeComponent();


            this.Loading += SuggestionsList_Loading;
            this.Loaded += SuggestionsList_Loaded;
            this.Unloaded += SuggestionsList_Unloaded;
            

        }
     

        private void SuggestionsList_Loaded(object sender, RoutedEventArgs e)
        {

            if (localBlock.Layout.Name == "categories_list")
            {

                myControl.scrollVi.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                myControl.scrollVi.HorizontalScrollMode = ScrollMode.Disabled;
                myControl.scrollVi.IsScrollInertiaEnabled = false;
                myControl.scrollVi.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                myControl.scrollVi.VerticalScrollMode = ScrollMode.Disabled;
            }
            else
            {
             
            }

            if (myControl.CheckIfAllContentIsVisible())
                load();

        }

        private void SuggestionsList_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= SuggestionsList_Unloaded;
            myControl.loadMore = null;

        }


        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        public bool itsAll
        {
            get
            {
                if (localBlock == null) return true;
                if (localBlock.NextFrom == null) return true; else return false;
            }
        }
        private async void load()
        {
            await semaphore.WaitAsync();
            try
            {
                if (localBlock.NextFrom == null) return;
                var a = await VK.vkService.GetSectionAsync(localBlock.Id, localBlock.NextFrom);
                localBlock.NextFrom = a.Section.NextFrom;
                this.DispatcherQueue.TryEnqueue(async () => {
                    foreach (var item in a.Suggestions)
                    {
                        Suggestions.Add(item);
                    }
                    if (myControl.CheckIfAllContentIsVisible())
                        load();
                });
            }
            finally
            {
                semaphore.Release();
            }
        }


        Block localBlock;
        private void SuggestionsList_Loading(FrameworkElement sender, object args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;
                Suggestions.Clear();
                localBlock = block;
                switch (localBlock.Layout.Name)
                {
                    case "list":
                        myControl.disableLoadMode = true;
                        myControl.ItemTemplate = myControl.Resources["defaultTemplate"] as DataTemplate;
                        break;
                    case "categories_list":
                        myControl.disableLoadMode = false;
                        myControl.ItemTemplate = myControl.Resources["compactTemplate"] as DataTemplate;
                        break;
                    default:
                        myControl.loadMore = load;
                        myControl.ItemTemplate = myControl.Resources["defaultTemplate"] as DataTemplate;
                        myControl.ItemsPanelTemplate = (ItemsPanelTemplate)myControl.Resources["default"];
                        myControl.disableLoadMode = false;
                        break;
                }


                // Применяем DataTemplate к свойству ItemTemplate UniversalControl



                var pl = (DataContext as Block).Suggestions;


                foreach (var item in pl)
                {
                    Suggestions.Add(item);
                }
              
            }
            catch (Exception ex)
            {



            }
        }

        public void AddBlock(Block block)
        {
            localBlock.NextFrom = block.NextFrom;
            this.DispatcherQueue.TryEnqueue(async () => {
                foreach (var item in block.Suggestions)
                {
                    Suggestions.Add(item);
                }
              
            });
        }
    }
}
