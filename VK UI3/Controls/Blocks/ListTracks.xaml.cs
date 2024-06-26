﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using VK_UI3.Helpers;
using VK_UI3.VKs.IVK;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public partial class ListTracks : UserControl, IBlockAdder
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
            if (connected)
            {
                sectionAudio.onListUpdate -= SectionAudio_onListUpdate;
                connected = false;
            }
            myControl.loadMore = null;

        }

    

        private void ListTracks_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                sectionAudio?.NotifyOnListUpdate();
            }
            catch { }

            myControl.loadMore = sectionAudio.GetTracks;
            if (myControl.CheckIfAllContentIsVisible()) sectionAudio.GetTracks();
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
                if (!connected)
                {
                    sectionAudio.onListUpdate += SectionAudio_onListUpdate;
                    connected = true;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void SectionAudio_onListUpdate(object sender, EventArgs e)
        {
            if (myControl.CheckIfAllContentIsVisible())
            {
                sectionAudio.GetTracks();
            }
        }

        public void AddBlock(Block block)
        {
            foreach (var item in block.Audios)
            {
                sectionAudio.listAudioTrue.Add(new ExtendedAudio(item, sectionAudio));
            }
            sectionAudio.Next = block.NextFrom;
        }

        private SectionAudio sectionAudio;
    }
}

