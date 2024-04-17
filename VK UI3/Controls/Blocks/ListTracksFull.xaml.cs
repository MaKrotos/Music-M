using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VK_UI3.Helpers;
using VK_UI3.VKs.IVK;



namespace VK_UI3.Controls.Blocks
{
    public partial class ListTracksFull : UserControl
    {


        public ListTracksFull()
        {
            InitializeComponent();




            this.DataContextChanged += ListTracks_DataContextChanged;

            this.Unloaded += ListTracksFull_Unloaded;

        }

        private void ListTracksFull_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged -= ListTracks_DataContextChanged;
            try
            {
                if (sectionAudio != null)
                    sectionAudio.onListUpdate -= SectionAudio_onListUpdate;
            }
            catch { }
            this.Unloaded -= ListTracksFull_Unloaded;
            connected = false;
        }
        bool connected = false;

        private void ListTracks_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;

                sectionAudio = new SectionAudio(block, this.DispatcherQueue);
                sectionAudio.countTracks = block.Audios.Count;
                OnPropertyChanged(nameof(sectionAudio));

                if (!connected)
                {
                    connected = true;
                    sectionAudio.onListUpdate += SectionAudio_onListUpdate;
                }
            }
            catch (Exception ex)
            {
                AppCenterHelper.SendCrash(ex);
            }
        }

        public SectionAudio sectionAudio;


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



    }
}

