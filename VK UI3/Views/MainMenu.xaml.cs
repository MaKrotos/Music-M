using System;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Octokit;
using VK_UI3.DB;
using Microsoft.UI.Xaml.Navigation;
using VK_UI3.Helpers;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using VK_UI3.VKs.IVK;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenu : Microsoft.UI.Xaml.Controls.Page
    {
        public UserAudio userAudio = null;
     //   ObservableCollection<ExtendedAudio> extendedAudios = new ObservableCollection<ExtendedAudio>();

        public MainMenu()
        {
            this.InitializeComponent();


            this.Loaded += MainMenu_Loaded;

        }

        ScrollViewer scrollViewer;
        private void MainMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // Находим ScrollViewer внутри ListView
             scrollViewer = FindScrollViewer(TrackListView);
            if (scrollViewer != null)
            {
                // Подписываемся на событие изменения прокрутки
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
                var isAtBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight-50;
                if (isAtBottom)
                {
                
                    if (userAudio.itsAll) return;
                    userAudio.GetTracks();
                    LoadingIndicator.IsActive = false;
                    if (userAudio.itsAll)
                        LoadingIndicator.Visibility = Visibility.Collapsed;
                }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            userAudio = e.Parameter as UserAudio;
            if (userAudio == null)
            {
                userAudio = new UserAudio(AccountsDB.activeAccount.id, this.DispatcherQueue);
            }


            userAudio.listAudio.CollectionChanged += ListAudio_CollectionChanged;

            base.OnNavigatedTo(e);
        }

        private void ListAudio_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LoadingIndicator.IsActive = false;
            if (userAudio.itsAll)
                LoadingIndicator.Visibility = Visibility.Collapsed;

        }


        public event PropertyChangedEventHandler PropertyChanged;

       
      

        // Метод для поиска ScrollViewer внутри элемента
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

        private void Timer_Tick(object sender, object e)
        {
          //  Tracks.Add(new TrackData { Cover = ("https://is1-ssl.mzstatic.com/image/thumb/Music124/v4/b1/d4/f8/b1d4f882-498c-dfdd-b0c9-1ae511eea612/859715405439_cover.jpg/1200x1200bf-60.jpg"), Author = "Author2", duration = 22 });


          //  Shuffle(Tracks);
            // Добавьте новый элемент в Tracks здесь
        }



    }
}
