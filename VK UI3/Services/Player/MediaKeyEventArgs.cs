using System;

namespace VK_UI3.Services.Player
{
    #region Media Key Hook Classes

    public class MediaKeyEventArgs : EventArgs
    {
        public MediaKey Key { get; }

        public MediaKeyEventArgs(MediaKey key)
        {
            Key = key;
        }
    }

    #endregion
}