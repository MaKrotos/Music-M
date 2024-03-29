using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using VK_UI3.Helpers;
using VK_UI3.VKs.IVK;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public partial class ListTracks : UserControl
    {


        public ListTracks()
        {
            InitializeComponent();
            this.DataContextChanged += ListTracks_DataContextChanged;
        
            this.Loaded += ListTracks_Loaded;
            this.Unloaded += ListTracks_Unloaded;

        }

        private void ListTracks_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged -= ListTracks_DataContextChanged;
            this.Loaded -= ListTracks_Loaded;
            this.Unloaded -= ListTracks_Unloaded;
        }

        private void ListTracks_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = SmallHelpers.FindScrollViewer(gridV);
            try
            {
                sectionAudio?.NotifyOnListUpdate();
            }
            catch { }
        }

        private bool CheckIfAllContentIsVisible(ScrollViewer scrollViewer)
        {
            if (scrollViewer.ViewportWidth >= scrollViewer.ExtentWidth)
            {
                return true;
            }
            return false;
        }

      

        ScrollViewer scrollViewer { get; set; }

        private void ListTracks_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (DataContext is not Block block)
                    return;

                sectionAudio = new SectionAudio(block, this.DispatcherQueue);
                sectionAudio.countTracks = block.Audios.Count;
                sectionAudio.onListUpdate.AddHandler(SectionAudio_onListUpdate);
            }
            catch (Exception ex)
            {
                AppCenterHelper.SendCrash(ex);
            }
        }

        private void SectionAudio_onListUpdate(object sender, EventArgs e)
        {
            if (CheckIfAllContentIsVisible(scrollViewer))
            {
                sectionAudio.GetTracks();
            }
        }

        private SectionAudio sectionAudio;

     
       

       
      
       

    }
}

