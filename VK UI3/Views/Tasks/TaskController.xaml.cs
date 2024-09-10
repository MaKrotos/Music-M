using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using System.IO;
using VK_UI3.DownloadTrack;
using VK_UI3.Helpers.Animations;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Tasks
{
    public sealed partial class TaskController : UserControl
    {
        public TaskController()
        {
            this.InitializeComponent();

            this.DataContextChanged += DownloadController_DataContextChanged;
            this.Loaded += DownloadController_Loaded;
            this.Unloaded += DownloadController_Unloaded;

        }

        private void DownloadController_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_task != null)
            {
                _task.onDoingStatusUpdate -= (OnDoingStatusUpdate);
                _task.onStatusUpdate -= (OnDoingStatusUpdate);

            }
        }

        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFontIcon = null;

        private void DownloadController_Loaded(object sender, RoutedEventArgs e)
        {
            if (animationsChangeFontIcon == null)
                animationsChangeFontIcon = new Helpers.Animations.AnimationsChangeFontIcon(PlayIcon, this.DispatcherQueue);
        }

        Task _task { get; set; }
        private void DownloadController_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (_task != null)
            {
                _task.onDoingStatusUpdate -= PlayListDownload_onDoingStatusUpdate;
                _task.onStatusUpdate -= (OnDoingStatusUpdate);
            }
            _task = (DataContext as Task);
            DownloadTitle.Text = _task.Name;

            if (DownloadTitle.Text.Length > 20)
            {
                DownloadTitle.Text = DownloadTitle.Text.Substring(0, 20) + "...";
            }


            _task.onDoingStatusUpdate += PlayListDownload_onDoingStatusUpdate;
            _task.onStatusUpdate += (OnDoingStatusUpdate);

            string original = _task.Name;
            if (original.Length > 20)
            {
                original = "..." + original.Substring(original.Length - 20);
            }
            DownloadProgressBar.Maximum = _task.maxTaskDoing;
            pathText.Text = original + "\\...";
            updateUI();
        }

        private void PlayListDownload_onDoingStatusUpdate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnDoingStatusUpdate(object sender, EventArgs e)
        {
            updateUI();
        }

        private void updateUI()
        {
            this.DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    if (animationsChangeFontIcon == null) { animationsChangeFontIcon = new AnimationsChangeFontIcon(PlayIcon, this.DispatcherQueue); }



                    if (_task.Status == Statuses.Pause|| _task.Status == Statuses.error)
                    {
                        {
                            animationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
                            if ((_task.Status == Statuses.error))
                                DownloadProgressBar.ShowPaused = true;
                        }
                    }
                    else
                    {
                        animationsChangeFontIcon.ChangeFontIconWithAnimation("\uE769");

                        if (!(_task.Status == Statuses.error))
                            DownloadProgressBar.ShowPaused = false;
                    }
                    dx.Text = $"{_task.doingStatus}";
                   
                    dx.Text += $" из {_task.doingStatus}";
               
                    DownloadProgressBar.Value = _task.doingStatus;
                }
                catch { }
            });
        }
 
        private void UCcontrol_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _task.ClickTaskInvoke();
        }

        private void Cancel_clicked(object sender, RoutedEventArgs e)
        {
            _task.CancelTask();
        }

        private void PlayPause_clicked(object sender, RoutedEventArgs e)
        {
            if (_task.Status == Statuses.Pause)
            {
                _task.Resume();
            }
            else
            {
                _task.Pause();
            }
        }
    }
}
