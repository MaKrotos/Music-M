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
                _task.StatusChanged -= (OnDoingStatusUpdate);
                _task.ProgressChanged -= (OnDoingStatusUpdate);

            }
        }

        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFontIcon = null;

        private void DownloadController_Loaded(object sender, RoutedEventArgs e)
        {
            if (animationsChangeFontIcon == null)
                animationsChangeFontIcon = new Helpers.Animations.AnimationsChangeFontIcon(PlayIcon, this.DispatcherQueue);
        }

        TaskAction _task { get; set; }
        private void DownloadController_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            if (_task != null)
            {
                _task.ProgressChanged -= PlayListDownload_onDoingStatusUpdate;
                _task.StatusChanged -= (OnDoingStatusUpdate);
            }
            _task = (DataContext as TaskAction);
            DownloadTitle.Text = _task.nameTask;

            if (DownloadTitle.Text.Length > 20)
            {
                DownloadTitle.Text = DownloadTitle.Text.Substring(0, 20) + "...";
            }


            _task.ProgressChanged += PlayListDownload_onDoingStatusUpdate;
            _task.StatusChanged += (OnDoingStatusUpdate);

            string original = _task.nameTask;
            if (original.Length > 20)
            {
                original = "..." + original.Substring(original.Length - 20);
            }
            DownloadProgressBar.Maximum = _task.total;
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



                    if (_task.Status == Statuses.Pause|| _task.Status == Statuses.Error)
                    {
                        {
                            animationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
                            if ((_task.Status == Statuses.Error))
                                DownloadProgressBar.ShowPaused = true;
                        }
                    }
                    else
                    {
                        animationsChangeFontIcon.ChangeFontIconWithAnimation("\uE769");

                        if (!(_task.Status == Statuses.Error))
                            DownloadProgressBar.ShowPaused = false;
                    }
                    dx.Text = $"{_task.Progress}";
                   
                    dx.Text += $" из {_task.total}";
               
                    DownloadProgressBar.Value = _task.Progress;
                }
                catch { }
            });
        }
 
        private void UCcontrol_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _task.onClick();
        }

        private void Cancel_clicked(object sender, RoutedEventArgs e)
        {
            _task.Cancel();
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
