using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MusicX.Core.Models;
using VK_UI3.Views.Controls;
using static VK_UI3.Views.Controls.BlockButtonView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controls.Blocks
{

    public sealed partial class Buttons : UserControl
    {
        Block block { get { return DataContext as Block; } }
        public Buttons()
        {

            this.InitializeComponent();

            this.DataContextChanged += Buttons_DataContextChanged;
        }



        private void Buttons_DataContextChanged(Microsoft.UI.Xaml.FrameworkElement sender, Microsoft.UI.Xaml.DataContextChangedEventArgs args)
        {
            if (DataContext is not Block bloc) return;

            gridV.Items.Clear();


            foreach (var action in bloc.Actions)
            {
                UIElement buttonElement = null;

                // 1. Определяем тип кнопки по layout и стилю
                if (bloc.Layout.Name == "crop_slider")
                {
                    // Плитки жанров и настроений
                    buttonElement = new MixCropButton()
                    {
                        Margin = new Thickness(0, 10, 15, 10),
                        DataContext = action,
                        Button = action
                    };
                }
                else if (bloc.Layout.Name == "large_slider" && bloc.Layout.Style == "artist_mix")
                {
                    // Карточки миксов по артистам
                    buttonElement = new ArtistMixButton()
                    {
                        Margin = new Thickness(0, 10, 15, 10),
                        DataContext = action,
                        Button = action
                    };
                }
                else if (bloc.Layout.Name == "horizontal_buttons")
                {
                    // Fallback: обычная кнопка для неизвестных типов
                    var viewModel = new BlockBTN(action, parentBlock: block);
                    var button = new BlockButtonView()
                    {
                        Margin = new Thickness(0, 10, 15, 10),
                        DataContext = viewModel,
                        blockBTN = viewModel,
                        Height = 45,
                        MinWidth = 170
                    };

                    if (viewModel != null)
                    {
                        button.Command = button.InvokeCommand;
                    }

                    button.Refresh();
                    buttonElement = button;
                }

                if (buttonElement != null)
                {
                    gridV.Items.Add(buttonElement);
                }
            }
        }
    }
}
