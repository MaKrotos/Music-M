using System.Collections.Generic;

namespace MusicX.Core.Models
{
    public enum SocialLinkType
    {
        Unknown,
        VK,
        Telegram,
        Discord,
        YouTube,
        GitHub,
        Twitter
    }

    public enum ContentType
    {
        Playlist,
        Artist,
        Album,
        Track
    }

    public class ListeningItem
    {
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public List<ListeningContentItem> Playlists { get; set; }
        public List<SocialLinkItem> SocialLinks { get; set; }
    }

    public class ListeningContentItem
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
        public ContentType Type { get; set; }
        
        public string Icon 
        { 
            get 
            {
                return Type switch
                {
                    ContentType.Playlist => "\uE141", 
                    ContentType.Artist => "\uE189",   
                    ContentType.Album => "\uE1D3",    
                    ContentType.Track => "\uE100",    
                    _ => "\uE10C" 
                };
            }
        }
    }

    public class SocialLinkItem
    {
        public string Name { get; set; }
        public SocialLinkType LinkType { get; set; }
        
        public string Icon 
        { 
            get 
            {
                return LinkType switch
                {
                    SocialLinkType.VK => "\uE783", 
                    SocialLinkType.Telegram => "\uE1D6", 
                    SocialLinkType.Discord => "\uE100", 
                    SocialLinkType.YouTube => "\uE10F", 
                    SocialLinkType.GitHub => "\uE11A", 
                    SocialLinkType.Twitter => "\uE142", 
                    _ => "\uE10C" 
                };
            }
        }
    }
}