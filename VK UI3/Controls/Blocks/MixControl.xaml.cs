using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MusicX.Core.Models;
using System;
using VK_UI3.Controllers;
using VK_UI3.Helpers.Animations;
using VK_UI3.VKs.IVK;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class MixControl : UserControl
    {

        // Массив цветов
        Color[] colors = new Color[]
        {
            Colors.Red,
            Colors.Green,
            Colors.Blue,
            Colors.Yellow,
            Colors.Purple,
            Colors.Orange,
            Colors.Pink,
            Colors.Cyan,
            Colors.Magenta,
            Colors.Lime,
            Colors.Gold,
            Colors.Teal,
            Colors.Salmon,

        };
        public MixControl()
        {
            this.InitializeComponent();

            this.Loading += MixControl_Loading;
            this.Loaded += MixControl_Loaded;
            this.Unloaded += MixControl_Unloaded;
            this.DataContextChanged += MixControl_DataContextChanged;
        }

        private void MixControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MixControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loading -= MixControl_Loading;
            this.Unloaded -= MixControl_Unloaded;
            this.DataContextChanged -= MixControl_DataContextChanged;
            AudioPlayer.oniVKUpdate -= AudioPlayer_oniVKUpdate;
            /*
          
            */

        }

        private void MixControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var a = args.NewValue;

            if (DataContext is not Block block)
            {
                return;
            }

            this.block = block;
            //   block.s
            startPalki();
        }
        Block block;

        Random random = new Random();
        private void MixControl_Loading(FrameworkElement sender, object args)
        {
            foreach (var child in columns.Children)
            {
                if (child is Palka palka)
                {
                    // Установка цвета фона для каждого элемента palka
                    int colorIndex = random.Next(colors.Length);
                    palka.Background = new SolidColorBrush(colors[colorIndex]);
                }
            }
            AudioPlayer.oniVKUpdate += AudioPlayer_oniVKUpdate;

        }

        private void AudioPlayer_oniVKUpdate(object sender, EventArgs e)
        {
            startPalki();
        }

        private void startPalki()
        {
            if (block == null) return;
            if (AudioPlayer.iVKGetAudio != null && AudioPlayer.iVKGetAudio is MixAudio mixAudio
            && mixAudio.mix_id == block.Audio_Stream_Mixes_Ids[0]
            )
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {

                    foreach (var child in columns.Children)
                    {
                        if (child is Palka palka)
                        {
                            palka.StartAnimation();
                        }
                    }
                });
            }
            else
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var child in columns.Children)
                    {
                        if (child is Palka palka)
                        {
                            palka.StopAnimation();
                        }
                    }
                });
            }
        }

        private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            new MixAudio(block.Audio_Stream_Mixes_Ids[0], this.DispatcherQueue);
        }
    }
}
