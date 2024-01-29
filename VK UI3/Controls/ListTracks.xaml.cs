using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public partial class ListTracks : UserControl, INotifyPropertyChanged
    {


        public ListTracks()
        {
            InitializeComponent();

            this.Unloaded += ListTracks_Unloaded;
            this.Loaded += ListTracks_Loaded;
        }

        private void ListTracks_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is not Block block)
                    return;



                foreach (var audio in ((Block)DataContext).Audios)
                {
                    _tracks.Add(audio);


                }

            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);


            }
        }

        private void ListTracks_Unloaded(object sender, RoutedEventArgs e)
        {
         //   this.StackPanelTracks.Children.Clear();
        }

        public static readonly DependencyProperty TracksProperty =
         DependencyProperty.Register("Tracks", typeof(List<Audio>), typeof(ListTracks), new PropertyMetadata(new List<Audio>()));

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Audio> Tracks
        {
            get { return (List<Audio>)GetValue(TracksProperty); }
            set
            {
                SetValue(TracksProperty, value);
            }
        }
        ObservableCollection<Audio> _tracks = new ObservableCollection<Audio>();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
          

        }
    }
}
