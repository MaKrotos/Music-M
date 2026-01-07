using System;

namespace VK_UI3.Services.Player
{
    public class VolumeChangedEventArgs : EventArgs
    {
        public double Volume { get; }
        public bool IsMuted { get; }

        public VolumeChangedEventArgs(double volume, bool isMuted = false)
        {
            Volume = volume;
            IsMuted = isMuted;
        }
    }
}