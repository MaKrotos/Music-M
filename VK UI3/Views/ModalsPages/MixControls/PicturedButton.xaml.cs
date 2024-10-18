using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicX.Core.Models.Mix;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.Helpers.Animations;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages.MixControls
{
    public sealed partial class PicturedButton : UserControl
    {

        private void EnsurePlaying()
        {


            Player.Pause();
                    _ = Player.PlayAsync(fromProgress: 0, toProgress: 1, looped: false);
                
            
        }


        public PicturedButton()
        {
            this.InitializeComponent();
            this.DataContextChanged += PicturedButton_DataContextChanged;
            animationsChangeLottie = new AnimationsChangeLottie(animPlayer, LottieVisualSource, this.DispatcherQueue);
        }
        MixOption mixOption = null;
        AnimationsChangeLottie animationsChangeLottie;
        private void PicturedButton_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null || !(DataContext is MixOption mixopt)) return;
            if (mixOption == mixopt) return;

            mixOption = mixopt;
            animationsChangeLottie.ChangeImageWithAnimation(mixopt.IconUri);
        }


     

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Player.PlaybackRate = 1;
            EnsurePlaying();
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Player.PlaybackRate = -1;
            EnsurePlaying();
        }
    }
}
