﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using System;
using VK_UI3.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls
{
    public partial class TitleBlockControl : UserControl
    {
        public TitleBlockControl()
        {
            InitializeComponent();

            this.Loading += TitleBlockControl_Loading;
            this.Unloaded += TitleBlockControl_Unloaded;
            AnimationsChangeImage = new Helpers.Animations.AnimationsChangeImage(Shapka, this.DispatcherQueue);
        }

        Helpers.Animations.AnimationsChangeImage AnimationsChangeImage;
        private void TitleBlockControl_Unloaded(object sender, RoutedEventArgs e)
        {

            this.Loading -= TitleBlockControl_Loading;
            this.Unloaded -= TitleBlockControl_Unloaded;
            Buttons.SelectionChanged -= ButtonsComboBox_SelectionChanged;
        }

        private void TitleBlockControl_Loading(FrameworkElement sender, object args)
        {
            // throw new NotImplementedException();
            if (DataContext is not Block block)
                return;


            Buttons.SelectionChanged += ButtonsComboBox_SelectionChanged;

            if (block.Layout.Name == "header_compact")
            {
                Title.Opacity = 0.5;
                Title.FontSize = 15;
            }

            Title.Text = block.Layout.Title;

            if (block.Layout.TopTitle is not null || block.Layout.Subtitle is not null)
            {
                Subtitle.Text = block.Layout.TopTitle?.Text ?? block.Layout.Subtitle;
                Subtitle.Visibility = Visibility.Visible;
            }

            if (block.Layout.TopTitle is not null && block.Layout.TopTitle.Icon is not null)
            {
                AnimationsChangeImage.ChangeImageWithAnimation(block.Layout.TopTitle.Icon);
                Shapka.Visibility = Visibility.Visible;
            }

                if (block.Badge != null)
            {
                BadgeHeader.Text = block.Badge.Text;
                BadgeHeader.Visibility = Visibility.Visible;
            }
           

            if (block.Buttons != null && block.Buttons.Count > 0) //ios
            {
                if (block.Buttons[0].Options.Count > 0)
                {
                    ButtonsGrid.Visibility = Visibility.Visible;
                    //  TitleButtons.Text = block.Buttons[0].Title;
                    Buttons.Visibility = Visibility.Visible;
                    MoreFontIcon.Visibility = Visibility.Collapsed;

                    foreach (var option in block.Actions[0].Options)
                    {
                        Buttons.Items.Add(new TextBlock() { Text = option.Text });
                        if (option.Text == block.Actions[0].Title)
                            Buttons.SelectedIndex = Buttons.Items.Count - 1;
                    }

                    Buttons.SelectedIndex = 0;
                    isProgrammingChange = false;
                    return;
                }
                else
                {
                    MoreFontIcon.Visibility = Visibility.Visible;
                    BackgrD.Opacity = 0.45;
                    //MoreButton.Content = block.Buttons[0].Title;

                    return;

                }
            }
            else
            {

                if (block.Actions.Count > 0)
                {
                    if (block.Actions[0].Options.Count > 0) //android
                    {
                        ButtonsGrid.Visibility = Visibility.Visible;
                        // TitleButtons.Text = block.Actions[0].Title;
                        Buttons.Visibility = Visibility.Visible;
                        MoreFontIcon.Visibility = Visibility.Collapsed;


                        foreach (var option in block.Actions[0].Options)
                        {
                            Buttons.Items.Add(new TextBlock() { Text = option.Text });
                            if (option.Text == block.Actions[0].Title)
                                Buttons.SelectedIndex = Buttons.Items.Count - 1;
                        }


                        isProgrammingChange = false;
                        return;
                    }
                    else
                    {
                        MoreFontIcon.Visibility = Visibility.Visible;
                        BackgrD.Opacity = 0.45;
                        //MoreButton.Content = block.Actions[0].Title;


                        return;

                    }

                }

                return;
            }
        }




   
        bool isProgrammingChange = true;
        private async void ButtonsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isProgrammingChange) return;
            if (DataContext is not Block block)
                return;
            try
            {
                var comboBox = sender as ComboBox;

                var current = comboBox.SelectedIndex;

                OptionButton option;
                if (block.Buttons != null)
                {
                    option = block.Buttons[0].Options[current];

                }
                else
                {
                    option = block.Actions[0].Options[current];

                }


                SectionView.openedSectionView.ReplaceBlocks(option.ReplacementId);
            }
            catch (Exception ex)
            {



            }


            //throw new NotImplementedException();
        }

        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            try
            {
                if (block.Actions.Count == 0 || string.IsNullOrEmpty(block.Actions[0].SectionId)) return;

                if (block.Actions.Count > 0)
                {
                    var bnt = block.Actions[0];

                    MainView.OpenSection(bnt.SectionId);
                    return;
                }

                var button = block.Buttons[0];

                MainView.OpenSection(button.SectionId);
            }
            catch (Exception ex)
            {



            }



        }

        private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            if (block.Actions.Count == 0 || string.IsNullOrEmpty(block.Actions[0].SectionId)) return;

            MoveLeftStoryboard.Pause();
            MoveRightStoryboard.Begin();
        }

        private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            if (block.Actions.Count == 0 || string.IsNullOrEmpty(block.Actions[0].SectionId)) return;
            MoveRightStoryboard.Pause();
            MoveLeftStoryboard.Begin();

        }
    }

}

