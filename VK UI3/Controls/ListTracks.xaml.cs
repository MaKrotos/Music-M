using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.Helpers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Services;
using VK_UI3.VKs;
using Windows.ApplicationModel.Email;
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

        
          //  this.Loaded += ListTracks_Loaded;
            this.DataContextChanged += ListTracks_DataContextChanged;

         
    

        }

        private void ListTracks_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;

                sectionAudio = new SectionAudio(block, this.DispatcherQueue);
                sectionAudio.countTracks = block.Audios.Count;
                OnPropertyChanged(nameof(sectionAudio));

                sectionAudio.onListUpdate += SectionAudio_onListUpdate;
            }
            catch (Exception ex)
            {
                AppCenterHelper.SendCrash(ex);
     

            }
        }

        private SectionAudio sectionAudio;

     
        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                //   this.ImgUri = uri;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
              
            });
           
        }
        private void SectionAudio_onListUpdate(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(sectionAudio));
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


        int itCounts = 0;
       

    }
}

