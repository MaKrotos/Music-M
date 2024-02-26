using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using MusicX.Core.Models;
using System;
using VK_UI3.Helpers.Animations;
using VK_UI3.Views;
using VK_UI3.Views.Controls;
using static VK_UI3.Views.Controls.BlockButtonView;

using Button = Microsoft.UI.Xaml.Controls.Button;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{
    public sealed partial class ArtistBannerBlockControl : UserControl
    {
        public ArtistBannerBlockControl()
        {
     
    
            InitializeComponent();

            this.DataContextChanged += ArtistBannerBlockControl_DataContextChanged;
            animationsChangeImage = new AnimationsChangeImage(ArtistBannerImage, this.DispatcherQueue);
        }

        private void ArtistBannerBlockControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is not Block block || block.Actions is null)
                return;


            animationsChangeImage.ChangeImageWithAnimation(block.Artists[0].Photo[2].Url);

            ArtistText.Text = block.Artists[0].Name;

            var sectionView = FindParent<SectionView>(this);

            for (var i = 0; i < block.Actions.Count; i++)
            {
                var action = block.Actions[i];

                var text = new TextBlock();


                var button = new BlockButtonView()
                {
                    Margin = new Thickness(0, 10, 15, 10),
                    DataContext = new BlockBTN(action, block.Artists[0], block),
                    Height = 45,
                    blockBTN = new BlockBTN(action, block.Artists[0], block)
                };


                if (button.DataContext is BlockBTN viewModel)
                {
                    button.Command = button.InvokeCommand;
                }

                ActionsGrid.ColumnDefinitions.Add(new());
                button.MinWidth = 170;
                button.SetValue(Grid.ColumnProperty, i);
                ActionsGrid.Children.Add(button);

                button.Refresh();





            }
        }

        AnimationsChangeImage animationsChangeImage = null;

      
        public static T? FindParent<T>(DependencyObject? startNode) where T : DependencyObject
        {
            DependencyObject? parent = VisualTreeHelper.GetParent(startNode);

            while (parent != null)
            {
                if (parent is T typedParent)
                {
                    return typedParent;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}
