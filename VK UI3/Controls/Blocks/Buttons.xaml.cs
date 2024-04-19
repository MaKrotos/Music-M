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


            foreach (var item in bloc.Actions)
            {
                var action = item;

                var text = new TextBlock();


                var button = new BlockButtonView()
                {
                    Margin = new Thickness(0, 10, 15, 10),
                    DataContext = new BlockBTN(action, parentBlock: block),
                    Height = 45,
                    blockBTN = new BlockBTN(action, parentBlock: block)
                };


                if (button.DataContext is BlockBTN viewModel)
                {
                    button.Command = button.InvokeCommand;
                }


                button.MinWidth = 170;

                button.Refresh();

                gridV.Items.Add(button);
            }

        }
    }
}
