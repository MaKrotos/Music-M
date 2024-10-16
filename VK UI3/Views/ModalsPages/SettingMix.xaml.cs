using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models.Mix;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using MusicX.Core.Services;
using VK_UI3.VKs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingMix : Page
    {

        public string Title { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public bool IsLoading { get; set; }

    

        public ICommand? ApplyCommand { get; set; }
        public ICommand? ResetCommand { get; set; }


 
        public SettingMix()
        {
            this.InitializeComponent();
        }

        public async Task LoadSettings(string mixId)
        {
            IsLoading = true;

            var settings = await VK.vkService.GetStreamMixSettings(mixId);

            IsLoading = false;
            Title = settings.Settings.Title;
            Subtitle = settings.Settings.Subtitle;

            foreach (var item in settings.Settings.Categories)
            {
                this.DispatcherQueue.TryEnqueue(() => {
                  
                    mixCategories.Add(item);

                    });
            }
        }
        public ObservableCollection<MixCategory> mixCategories = new ObservableCollection<MixCategory>();
    }

   




}
