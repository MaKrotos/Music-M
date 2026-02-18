using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;
using MusicX.Core.Models;
using System;
using VK_UI3.Controllers;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views.ModalsPages;
using VK_UI3.VKs.IVK;
using Windows.UI;
using MvvmHelpers.Commands;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using WinRT.Interop;
using MusicX.Core.Models.Mix;

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
            ChoosenControl.onChangeSelected += ChoosenControl_onChangeSelected;
            upTextBoxAnim = new AnimationsChangeText(upTextBox, this.DispatcherQueue);
            DownTextBoxAnim = new AnimationsChangeText(downTextBox, this.DispatcherQueue);

        }

        AnimationsChangeText upTextBoxAnim;
        AnimationsChangeText DownTextBoxAnim;
        private ImmutableDictionary<string, ImmutableArray<string>>? _options;
        private void ChoosenControl_onChangeSelected(object sender, EventArgs e)
        {
            if (ChoosenControl.choosen == 0)
            {
                _= settingBTN.ShowButton();

                DownTextBoxAnim.ChangeTextWithAnimation("Музыкальные рекомендации для Вас");
                upTextBoxAnim.ChangeTextWithAnimation("Слушать VK микс");
            }
            else
            {
               
               _= settingBTN.HideButton();
               DownTextBoxAnim.ChangeTextWithAnimation("Любимые треки из Вашей колелкции");
               upTextBoxAnim.ChangeTextWithAnimation("Слушать мои треки");

            }
        }

        private void MixControl_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateGradientStops();
        }

        private void AnimateGradientStops()
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
            /*
            foreach (var child in columns.Children)
            {
                if (child is Palka palka)
                {
                    // Установка цвета фона для каждого элемента palka
                    int colorIndex = random.Next(colors.Length);
                    palka.Background = new SolidColorBrush(colors[colorIndex]);
                }
            }
            */
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
            && mixAudio.data.Id == block.Audio_Stream_Mixes_Ids[0]
            )
            {
                /*
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
                */
            }
            else
            {
                /*
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
                */
            }
        }



      

        private void UserControl_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var mixID = "common";
            if (ChoosenControl.choosen != 0)
            {
                mixID = "my_music";
            }

         
            new MixAudio(
                   new MixOptions(mixID, Options: _options)
                , this.DispatcherQueue);
        }

        private async void settingBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialog dialog = new CustomDialog();

                    dialog.Transitions = new TransitionCollection
                 {
                     new PopupThemeTransition()
                 };

                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                dialog.XamlRoot = this.XamlRoot;


                var a = new SettingMix();
                await a.LoadSettings("common");
                dialog.Content = a;
                dialog.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

                a.ApplyCommand = new AsyncCommand(async () =>
                {
                    SetOptions(a.mixCategories);
                    dialog.Hide();

                    new MixAudio(
                      new MixOptions("common", Options: _options)
                   , this.DispatcherQueue);
                    a.ResetCommand = null;
                    a.ApplyCommand = null;
                });


                a.ResetCommand = new AsyncCommand(async () =>
                {
                    _options = null;
                    dialog.Hide();

                    new MixAudio(
                      new MixOptions("common", Options: _options)
                   , this.DispatcherQueue);
                    a.ResetCommand = null;
                    a.ApplyCommand = null;
                });


               
                dialog.ShowAsync();
            }
            catch (Exception xe) { }

        }
        private void SetOptions(IEnumerable<MixCategory> categories)
        {
            var builder = ImmutableDictionary<string, ImmutableArray<string>>.Empty.ToBuilder();

            foreach (var category in categories)
            {
                foreach (var option in category.Options.Where(b => b.Selected))
                {
                    builder[category.Id] = builder.TryGetValue(category.Id, out var value)
                        ? value.Add(option.Id)
                        : [option.Id];
                }
            }

            _options = builder.Count == 0 ? null : builder.ToImmutable();
        }
    }
}
