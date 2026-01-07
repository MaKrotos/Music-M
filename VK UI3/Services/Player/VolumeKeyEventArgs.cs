using System;

namespace VK_UI3.Services.Player
{
    #region Media Key Hook Classes

    public class VolumeKeyEventArgs : EventArgs
    {
        public VolumeKey Key { get; }

        public VolumeKeyEventArgs(VolumeKey key)
        {
            Key = key;
        }
    }

    #endregion
}