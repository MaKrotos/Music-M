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

            animationsChangeText = new AnimationsChangeText(dx, this.DispatcherQueue);
        }
        Helpers.Animations.AnimationsChangeText animationsChangeText;

        private void DownloadController_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_task != null)
            {
                _task.StatusChanged -= (OnDoingStatusUpdate);
                _task.ProgressChanged -= (OnDoingStatusUpdate);

            }
        }

        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFontIcon = null;
        Helpers.Animations.AnimationsChangeFontIcon animationsChangeFontIconCancel = null;
        private void DownloadController_Loaded(object sender, RoutedEventArgs e)
        {
            if (animationsChangeFontIcon == null)
                animationsChangeFontIcon = new Helpers.Animations.AnimationsChangeFontIcon(PlayIcon, this.DispatcherQueue);
            if (animationsChangeFontIconCancel == null)
                animationsChangeFontIconCancel = new Helpers.Animations.AnimationsChangeFontIcon(CancelIcon, this.DispatcherQueue);
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

            string original = _task.subTextTask;
            if (original.Length > 20)
            {
                original = "..." + original.Substring(original.Length - 20);
            }
            DownloadProgressBar.Maximum = _task.total;
            pathText.Text = original + "...";
            updateUI();
        }

        private void PlayListDownload_onDoingStatusUpdate(object sender, EventArgs e)
        {
            updateUI();
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



                    switch (_task.Status)
                    {
                        case Statuses.Resume:
                            {
                                animationsChangeFontIcon.ChangeFontIconWithAnimation("\uE769");
                                DownloadProgressBar.ShowPaused = false;
                                DownloadProgressBar.ShowError = false;
                            }
                            break;
                        case Statuses.Pause:
                            {
                                animationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
                                DownloadProgressBar.ShowPaused = true;
                                DownloadProgressBar.ShowError = false;
                            }
                            break;
                        case Statuses.Completed:
                            {
                                animationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
                                animationsChangeFontIconCancel.ChangeFontIconWithAnimation("\uE74D");
                            }
                            break;
                        case Statuses.Error:
                            {
                                animationsChangeFontIconCancel.ChangeFontIconWithAnimation("\uE74D");
                                animationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
                                DownloadProgressBar.ShowPaused = false;
                                DownloadProgressBar.ShowError = true;
                            }
                            break;
                        case Statuses.Cancelled:
                            {
                                animationsChangeFontIconCancel.ChangeFontIconWithAnimation("\uE74D");
                                animationsChangeFontIcon.ChangeFontIconWithAnimation("\uF5B0");
                                DownloadProgressBar.ShowPaused = false;
                                DownloadProgressBar.ShowError = true;
                            }
                            break;
                    }
                    string txt = "";
                    if (_task.Status != Statuses.Completed)
                    {
                         txt = $"{_task.Progress} из {_task.total}";
                    }
                    else
                    {
                        txt = "✔";
                    }
                    animationsChangeText.ChangeTextWithAnimation(txt);
               
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
            if (_task.Status == Statuses.Error || _task.Status == Statuses.Completed) {
                _task.delete();
            }
            {
                _task.Cancel();
            }   
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
