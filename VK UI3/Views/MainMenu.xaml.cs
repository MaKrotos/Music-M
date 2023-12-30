using System;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using VK_UI3.VKs;
using System.ComponentModel;
using Octokit;
using VK_UI3.DB;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenu : Microsoft.UI.Xaml.Controls.Page, INotifyPropertyChanged
    {
        UserAudio userAudio = null;

        public MainMenu()
        {
            this.InitializeComponent();
            Loaded += loaded;
        }

        private void loaded(object sender, RoutedEventArgs e)
        {




            userAudio = new UserAudio(AccountsDB.activeAccount.id);
            userAudio.onListUpdate += (sender, e) => updateList(sender, e);
          
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                //   this.ImgUri = uri;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });

        }

        private void updateList(object sender, EventArgs e)
        {

            OnPropertyChanged(nameof(userAudio));
        }

        private void Timer_Tick(object sender, object e)
        {
          //  Tracks.Add(new TrackData { Cover = ("https://is1-ssl.mzstatic.com/image/thumb/Music124/v4/b1/d4/f8/b1d4f882-498c-dfdd-b0c9-1ae511eea612/859715405439_cover.jpg/1200x1200bf-60.jpg"), Author = "Author2", duration = 22 });


          //  Shuffle(Tracks);
            // Добавьте новый элемент в Tracks здесь
        }

        public void Shuffle<T>(ObservableCollection<T> collection)
        {
            var random = new Random();
            for (int i = collection.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = collection[i];
                collection[i] = collection[j];
                collection[j] = temp;
            }
        }

    }
}
