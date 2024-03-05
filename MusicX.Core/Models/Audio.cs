using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Audio : VkNet.Model.Attachments.Audio
    {
    

        public string DurationString
        {
            get
            {
                var t = TimeSpan.FromSeconds(Duration);
                if (t.Hours > 0)
                    return t.ToString("h\\:mm\\:ss");
                return t.ToString("m\\:ss");
            }
        }




        public bool IsAvailable { get; set; } = true;

        public string? ParentBlockId { get; set; }

        public string? DownloadPlaylistName { get; set; }
        public bool Equals(Audio? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Id == other.Id;
        }
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Audio))
                return false;
            return Equals((Audio)obj);
        }
        

        public bool Equals(VkNet.Model.Attachments.Audio? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (other.GetType() != typeof(Audio))
                return false;
            return Equals((VkNet.Model.Attachments.Audio)other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
