using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VK_UI3.VKs;
using VkNet.Model;
using VkNet.Utils;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Share
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FriendsList : Page
    {

        ObservableCollection<User> friends = new ObservableCollection<User>();
        public FriendsList()
        {
            this.InitializeComponent();
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
            loadMoreFriends();
        }

        private async Task loadMoreFriends()
        {
            var parameters = new VkParameters
            {
                { "count", 10 },
                { "fields", "can_see_audio,photo_50,online" }
            };

            var a = (await VK.api.CallAsync("friends.get", parameters)).ToVkCollectionOf<User>(
                x => parameters["fields"] != null
                      ? x
                      : new User
                      {
                          Id = x
                      }
                );

            this.DispatcherQueue.TryEnqueue(() =>
            {
                foreach (var item in a)
                {
                    friends.Add(item);

                }
            });


        }
    }
}
