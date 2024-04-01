using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.Controllers;
using VK_UI3.DownloadTrack;
using VK_UI3.Helpers.Animations;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Download
{
    public sealed partial class DownloadController : UserControl
    {
        public DownloadController()
        {
            this.InitializeComponent();

            this.DataContextChanged += DownloadController_DataContextChanged;
            this.Loaded += DownloadController_Loaded;
            this.Unloaded += DownloadController_Unloaded;
           
        }

        private void DownloadController_Unloaded(object sender, RoutedEventArgs e)
        {
            if (playListDownload != null)
            {
                playListDownload.OnTrackDownloaded -= (OnTrackDownloaded_Event);
                playListDownload.onStatusUpdate -= (OnTrackDownloaded_Event);

            }
        }

        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFontIcon = null;

        private void DownloadController_Loaded(object sender, RoutedEventArgs e)
        {
            if (animationsChangeFontIcon == null)
                animationsChangeFontIcon = new Helpers.Animations.AnimationsChangeFontIcon(PlayIcon, this.DispatcherQueue);
        }

        PlayListDownload playListDownload { get; set; }
        private void DownloadController_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (playListDownload != null)
            {
                playListDownload.OnTrackDownloaded -=(OnTrackDownloaded_Event);
                playListDownload.onStatusUpdate-=(OnTrackDownloaded_Event);
            }
            playListDownload = (DataContext as PlayListDownload);
            DownloadTitle.Text = playListDownload.iVKGetAudio.name;

            if (DownloadTitle.Text.Length > 20)
            {
                DownloadTitle.Text = DownloadTitle.Text.Substring(0, 20) + "...";
            }


            playListDownload.OnTrackDownloaded +=(OnTrackDownloaded_Event);
            playListDownload.onStatusUpdate +=(OnTrackDownloaded_Event);
     
            string original = playListDownload.path;
            if (original.Length > 20)
            {
                original = "..." + original.Substring(original.Length - 20);
            }
            pathText.Text = original + "\\...";
            updateUI();
        }

        private void OnTrackDownloaded_Event(object sender, EventArgs e)
        {
            updateUI();
        }

        private void updateUI()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (playListDownload.error)
                    {
                        DownloadProgressBar.ShowError = true;
                    }

                    if (playListDownload.isPause())
                    {
                        {
                            animationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
                            DownloadProgressBar.ShowPaused = true;
                        }
                    }
                    else
                    {
                        animationsChangeFontIcon.ChangeFontIconWithAnimation("\uE769");
                    
                        DownloadProgressBar.ShowPaused = false;
                    }
                    dx.Text = $"{playListDownload.downloaded} из {playListDownload.iVKGetAudio.countTracks}";


                    double totalTracks = (double)playListDownload.iVKGetAudio.countTracks;
                    double downloadedTracks = playListDownload.downloaded;
                    double percentageDownloaded = (downloadedTracks / totalTracks) * 100;

                    DownloadProgressBar.Value = Math.Round(percentageDownloaded);
                }
                catch { }
            });
        }

        private void UCcontrol_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (Directory.Exists(playListDownload.location))
            {
                Process.Start("explorer.exe", playListDownload.location);
            }
            else
            {
             
            }
        }

        private void Cancel_clicked(object sender, RoutedEventArgs e)
        {
            playListDownload.Cancel();
        }

        private void PlayPause_clicked(object sender, RoutedEventArgs e)
        {
            if (playListDownload.isPause())
            {
                playListDownload.Resume();
            }
            else
            {
                playListDownload.Pause();
            }
        }
    }
}
