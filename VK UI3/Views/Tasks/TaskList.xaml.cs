using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using VK_UI3.DownloadTrack;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Tasks
{
    public sealed partial class TaskList : UserControl
    {

     
        public TaskList()
        {
            this.InitializeComponent();

            this.Loading += DownloadsList_Loading;
            this.Unloaded += DownloadsList_Unloaded;
            foreach (var task in Task.tasks)
            {
                tasks.Add(task);
            }
            Task.tasks.CollectionChanged += Tasks_CollectionChanged;
        }

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (Task newTask in e.NewItems)
                {
                    tasks.Add(newTask);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (Task oldTask in e.OldItems)
                {
                    tasks.Remove(oldTask);
                }
            }
        }


        ObservableCollection<Task> tasks = new ObservableCollection<Task>();
        private void DownloadsList_Unloaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void DownloadsList_Loading(FrameworkElement sender, object args)
        {
          
        }
    }
}
