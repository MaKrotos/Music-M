using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VkNet.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Share.ShareControllers
{
    public sealed partial class UserControllers : UserControl
    {
        public UserControllers()
        {

            this.InitializeComponent();
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(userIcon, this.DispatcherQueue);
        }
        Helpers.Animations.AnimationsChangeImage animationsChangeImage;

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext is not User user)
            { return; }

            animationsChangeImage.ChangeImageWithAnimation(user.Photo50);



        }
    }
}
