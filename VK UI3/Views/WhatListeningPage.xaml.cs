using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Services;

namespace VK_UI3.Views
{
    public sealed partial class WhatListeningPage : Page
    {
        private readonly IWhatListeningService _whatListeningService;
        private List<ListeningItem> _preloadedData;

        public WhatListeningPage()
        {
            this.InitializeComponent();
            
            // Get service from DI container
            _whatListeningService = StaticService.Container.GetRequiredService<IWhatListeningService>();
            
            this.Loaded += WhatListeningPage_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is List<ListeningItem> data)
            {
                _preloadedData = data;
            }
            base.OnNavigatedTo(e);
        }

        private async void WhatListeningPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_preloadedData != null && _preloadedData.Count > 0)
            {
                ListeningList.ItemsSource = new ObservableCollection<ListeningItem>(_preloadedData);
            }
            else
            {
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var data = await _whatListeningService.GetWhatListeningAsync();
                
                if (data != null && data.Count > 0)
                {
                    ListeningList.ItemsSource = new ObservableCollection<ListeningItem>(data);
                }
                else
                {
             
                }
            }
            catch (Exception ex)
            {
                // Simple error handling: could show a message to the user
                System.Diagnostics.Debug.WriteLine($"Error loading WhatListening data: {ex.Message}");
                LoadMockData();
            }
        }

        private void LoadMockData()
        {
            var items = new ObservableCollection<ListeningItem>();
            for (int i = 1; i <= 10; i++)
            {
                items.Add(new ListeningItem 
                { 
                    UserName = $"Ошибка загрузки {i}", 
                    UserAvatar = "gray",
                    Playlists = new List<ListeningContentItem>
                    {
                        new ListeningContentItem { Name = "Ошибка", Type = ContentType.Playlist }
                    },
                    SocialLinks = new List<SocialLinkItem>()
                });
            }
            ListeningList.ItemsSource = items;
        }
    }
}