using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using MusicX.Core.Models;

namespace VK_UI3.Views.Controls
{
    public sealed partial class WhatListeningItemControl : UserControl
    {
        public WhatListeningItemControl()
        {
            this.InitializeComponent();
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