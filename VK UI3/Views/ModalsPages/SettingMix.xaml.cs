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

        public ObservableCollection<MixSettingsCategoryViewModel> Categories { get; set; } = [];

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
            Categories = new(settings.Settings.Categories.Select(b => new MixSettingsCategoryViewModel(b)));
        }
    }




    public class MyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PicturedButtonTemplate { get; set; }
        public DataTemplate ButtonTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var viewModel = item as MixSettingsCategoryViewModel;
            if (viewModel != null)
            {
                if (viewModel.Type == "pictured_button_horizontal_group")
                {
                    return PicturedButtonTemplate;
                }
                else if (viewModel.Type == "button_horizontal_group")
                {
                    return ButtonTemplate;
                }
            }
            return base.SelectTemplateCore(item, container);
        }
    }

    public class MixSettingsCategoryViewModel : Grid
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public ObservableCollection<MixSettingsOptionViewModel> Options { get; set; }

        public MixSettingsCategoryViewModel(MixCategory category)
        {
            Title = category.Title;
            Id = category.Id;
            Type = category.Type;
            Options = new(category.Options.Select(o => new MixSettingsOptionViewModel(o)));
        }
    }

    public class MixSettingsOptionViewModel : Grid
    {
        public string Title { get; set; }
        public string Id { get; set; }

        public byte[]? Icon { get; set; }

        public bool Selected { get; set; }

        public MixSettingsOptionViewModel(MixOption option)
        {
            Title = option.Title;
            Id = option.Id;
            if (!string.IsNullOrEmpty(option.IconUri))
                Load(option.IconUri);
        }

        private async Task Load(string uri)
        {
            using var httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            });

            await using var stream = await httpClient.GetStreamAsync(uri);

            var jsonObject = await JsonNode.ParseAsync(stream);

            if (jsonObject is null) return;

            var base64Image = jsonObject["assets"]![0]!["p"]!.AsValue().ToString();

            Icon = Convert.FromBase64String(base64Image["data:image/png;base64,".Length..]);
        }
    }

}
