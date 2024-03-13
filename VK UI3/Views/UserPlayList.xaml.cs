using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System.Collections.ObjectModel;
using VK_UI3.DB;
using VkNet.Model.Attachments;
using VkNet.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserPlayList : Microsoft.UI.Xaml.Controls.Page
    {
        public UserPlayList()
        {
            this.InitializeComponent();
            this.Loaded += UserPlayList_Loaded;
            this.Loading += UserPlayList_Loading;
        }
        ObservableCollection<AudioPlaylist> audioPlaylists = new ObservableCollection<AudioPlaylist>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var vkCollection = e.Parameter as VkCollection<AudioPlaylist>;
            if (vkCollection == null)
                return;

            foreach (var item in vkCollection)
            {
                if (!(item.OwnerId == AccountsDB.activeAccount.id && item.Original == null))
                    continue;
                audioPlaylists.Add(item);
            }
        }

            private void UserPlayList_Loading(FrameworkElement sender, object args)
        {
           

        }

        private void UserPlayList_Loaded(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
