using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using TagLib.Matroska;
using VK_UI3.VKs.IVK;
using System.Collections.ObjectModel;
using VK_UI3.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public sealed partial class RecomUsersControl : UserControl
    {
        public RecomUsersControl()
        {
            this.InitializeComponent();

            DataContextChanged += RecomUsersControl_DataContextChanged;
        }
        RecommendedPlaylist _PlayList;
        private void RecomUsersControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            _PlayList = (DataContext as RecommendedPlaylist);
          
            update();
        }

        PlayListVK playListVK = null;
        ObservableCollection<ExtendedAudio> extendedAudios = new ObservableCollection<ExtendedAudio>();
        private void update()
        {
            playListVK = new PlayListVK(_PlayList, DispatcherQueue);

            foreach (var item in playListVK.listAudioTrue)
            {
                extendedAudios.Add(item);
            }
        }
    }
}
