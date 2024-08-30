
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TagLib.Ape;
using VK_UI3.VKs;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace VK_UI3.Views.Share
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class MessConv 
    {
       public Conversation conversation { get; set; }
        public User user { get; set; }
        public List<User> users = new List<User>();
        public VkNet.Model.Group group { get; set; }

        public bool isDisabled { get; set; } = false;
        
    }


    public class ConversationsListParams {
        public bool itsAll = false;
        public GetConversationsResult result;
        public VkNet.Model.Attachments.Audio audio;
        internal EventHandler selectedDialog;
    }




    public sealed partial class ConversationsList : Microsoft.UI.Xaml.Controls.Page
    {

        public bool isDisabled = false;
        ObservableCollection<MessConv> nmessConv = new ObservableCollection<MessConv>();
        public ConversationsList()
        {
            this.InitializeComponent();
            this.Loaded += FriendsList_Loaded;
        }
        ScrollViewer scrollViewer;
        private void FriendsList_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = FindScrollViewer(scrollView);
            if (scrollViewer != null)
            {
                // Подписываемся на событие изменения прокрутки
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged; ;
            }
        }

        private void Page_Loading(FrameworkElement sender, object args)
        {
           
          
        }
        bool itsAll = false;
        bool loadingNow = false;

        ConversationsListParams conversationsListParams = null;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var mar = e.Parameter as ConversationsListParams;
            if (mar == null)
                return;
            conversationsListParams = mar;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 50;
            if (isAtBottom || CheckIfAllContentIsVisible(sender as ScrollViewer))
            {

                if (itsAll)
                {
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
             
                LoadingIndicator.Visibility = Visibility.Visible;
                loadMoreConv();
                LoadingIndicator.IsActive = true;
            }
        }

        private bool CheckIfAllContentIsVisible(ScrollViewer scrollViewer)
        {
            if (scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight)
            {
                return true;
            }
            return false;
        }

        private ScrollViewer FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer)
                return d as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var sw = FindScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (sw != null) return sw;
            }

            return null;
        }
        


        int counted = 0;
        private async Task loadMoreConv()
        {
            if (itsAll)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    LoadingIndicator.IsActive = false;
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                });
                return;
            }
            if (loadingNow) return;
             loadingNow = true;

            int off = 50;

            var parameters = new GetConversationsParams();
            parameters.Count = 50;
            parameters.Offset = (ulong) counted;
            parameters.Extended = true;
            parameters.Fields = new string[] {
                "photo_max_orig",
                "online",
                "can_write_private_message"
            };
         
         
            counted += off;

            var a = (await VK.api.Messages.GetConversationsAsync(parameters));

            if (a.Items.Count < off) itsAll = true;

            addItems(a);


            if (itsAll)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    LoadingIndicator.IsActive = false;
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            loadingNow = false;
                if (CheckIfAllContentIsVisible(scrollViewer)) loadMoreConv();
        }

        private async Task addItems(GetConversationsResult a)
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                foreach (var item in a.Items)
                {

                    MessConv messConv = new MessConv();
                    messConv.isDisabled = this.isDisabled;
                    messConv.conversation = item.Conversation;


                    switch (item.Conversation.Peer.Type.ToString())
                    {
                        case "chat":

                            if (item.Conversation.ChatSettings.ActiveIds != null)
                                foreach (var item1 in item.Conversation.ChatSettings.ActiveIds)
                                {
                                    messConv.users.Add(a.Profiles.Where(profile => profile.Id == item1).FirstOrDefault());
                                }

                            break;
                        case "user":
                            messConv.user = a.Profiles.Where(profile => profile.Id == item.Conversation.Peer.LocalId).FirstOrDefault();
                            break;
                        case "group":
                            messConv.group = a.Groups.Where(group => group.Id == item.Conversation.Peer.LocalId).FirstOrDefault();
                            break;
                        case "email":

                            break;
                        default:
                            break;
                    }

                    nmessConv.Add(messConv);
                }
                if (CheckIfAllContentIsVisible(scrollViewer)) loadMoreConv();
            });
        }

        private void scrollView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            var a = scrollView.SelectedIndex;


            if (!nmessConv[a].conversation.CanWrite.Allowed)
            {
                scrollView.SelectedItem = null;
                return;
            }


            MessagesSendParams messagesSendParams = new MessagesSendParams();


            messagesSendParams.Attachments = new List<VkNet.Model.Attachments.Audio> { conversationsListParams.audio };


         
            messagesSendParams.PeerId = nmessConv[a].conversation.Peer.Id;



            messagesSendParams.RandomId = 0;
            try
            {
                VK.api.Messages.SendAsync(messagesSendParams);
            }
            catch (Exception ex) { }

            // Получение ID плейлиста
            conversationsListParams.selectedDialog?.Invoke(nmessConv[a], EventArgs.Empty);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            conversationsListParams.selectedDialog?.Invoke(null, EventArgs.Empty);

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (conversationsListParams != null)
            {
                itsAll = conversationsListParams.itsAll;
                counted += conversationsListParams.result.Items.Count();
                addItems(conversationsListParams.result);

            }
            else
            {
                loadMoreConv();
            }
            if (conversationsListParams.audio != null)
            {
                isDisabled = true;
                this.Width = 425;
                Cancel.Visibility = Visibility.Visible;
                Rectr.Visibility = Visibility.Visible;
                scrollView.SelectionMode = ListViewSelectionMode.Single;
                MainGrid.Background = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["AcrylicBackgroundFillColorDefaultBrush"];
            }
        }
    }
}
