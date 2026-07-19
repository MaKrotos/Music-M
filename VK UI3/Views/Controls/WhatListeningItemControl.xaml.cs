using DevWinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System.Collections.Generic;
using VK_UI3.Helpers.Animations;

namespace VK_UI3.Views.Controls
{
    public sealed partial class WhatListeningItemControl : UserControl
    {
        private AnimationsChangeImage _backgroundAnimations;

        public WhatListeningItemControl()
        {
            this.InitializeComponent();

            this.Loaded += WhatListeningItemControl_Loaded;
            this.DataContextChanged += OnDataContextChanged;
        }

        private void WhatListeningItemControl_Loaded(object sender, RoutedEventArgs e)
        {
            mainGrid.Lights.Add(new AmbLight());
            mainGrid.Lights.Add(new HoverLight());

            _backgroundAnimations = new AnimationsChangeImage(BackgroundImage, this.DispatcherQueue);
            
            // Если DataContext уже установлен до Loaded — применяем аватар сейчас
            TryApplyAvatar();
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"[WhatListeningItemControl] DataContextChanged: DataContext is {DataContext?.GetType().Name ?? "null"}");
            TryApplyAvatar();
        }

        private void TryApplyAvatar()
        {
            System.Diagnostics.Debug.WriteLine($"[WhatListeningItemControl] TryApplyAvatar: DataContext={DataContext?.GetType().Name}, _backgroundAnimations is null? {_backgroundAnimations == null}");

            if (DataContext is ListeningItem item)
            {
                System.Diagnostics.Debug.WriteLine($"[WhatListeningItemControl] Is ListeningItem, UserAvatar='{item.UserAvatar}', IsNullOrWhiteSpace={string.IsNullOrWhiteSpace(item.UserAvatar)}");
                
                if (!string.IsNullOrWhiteSpace(item.UserAvatar))
                {
                    System.Diagnostics.Debug.WriteLine($"[WhatListeningItemControl] Calling ChangeImageWithAnimation with '{item.UserAvatar}'");
                    _backgroundAnimations?.ChangeImageWithAnimation(item.UserAvatar);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[WhatListeningItemControl] DataContext is NOT ListeningItem");
            }
        }

        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register(
            nameof(UserName), typeof(string), typeof(WhatListeningItemControl), new PropertyMetadata(string.Empty));

        public string UserName
        {
            get => (string)GetValue(UserNameProperty);
            set => SetValue(UserNameProperty, value);
        }

        public static readonly DependencyProperty UserAvatarProperty = DependencyProperty.Register(
            nameof(UserAvatar), typeof(string), typeof(WhatListeningItemControl), new PropertyMetadata(string.Empty));

        public string UserAvatar
        {
            get => (string)GetValue(UserAvatarProperty);
            set => SetValue(UserAvatarProperty, value);
        }

        public static readonly DependencyProperty PlaylistsProperty = DependencyProperty.Register(
            nameof(Playlists), typeof(List<ListeningContentItem>), typeof(WhatListeningItemControl), new PropertyMetadata(null));

        public List<ListeningContentItem> Playlists
        {
            get => (List<ListeningContentItem>)GetValue(PlaylistsProperty);
            set => SetValue(PlaylistsProperty, value);
        }

        public static readonly DependencyProperty SocialLinksProperty = DependencyProperty.Register(
            nameof(SocialLinks), typeof(List<SocialLinkItem>), typeof(WhatListeningItemControl), new PropertyMetadata(null));

        public List<SocialLinkItem> SocialLinks
        {
            get => (List<SocialLinkItem>)GetValue(SocialLinksProperty);
            set => SetValue(SocialLinksProperty, value);
        }
    }
}