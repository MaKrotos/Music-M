using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using VK_UI3.DownloadTrack;
using VK_UI3.Views.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Notification
{
    public sealed partial class NotificationList : UserControl
    {

     
        public NotificationList()
        {
            this.InitializeComponent();

            this.Loading += DownloadsList_Loading;
            this.Unloaded += DownloadsList_Unloaded;
            Notification.Notifications.CollectionChanged += Notifications_CollectionChanged;

        }

        private void DownloadsList_Unloaded(object sender, RoutedEventArgs e)
        {
            Notification.Notifications.CollectionChanged -= Notifications_CollectionChanged;
        }

        private void DownloadsList_Loading(FrameworkElement sender, object args)
        {
            Notification.Notifications.CollectionChanged += Notifications_CollectionChanged;
        }

        private void Notifications_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (TaskAction newTask in e.NewItems)
                {
                    tasks.Add(newTask);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (TaskAction oldTask in e.OldItems)
                {
                    tasks.Remove(oldTask);
                }
            }
        }

     


        ObservableCollection<TaskAction> tasks = new ObservableCollection<TaskAction>();
   
    }
}
