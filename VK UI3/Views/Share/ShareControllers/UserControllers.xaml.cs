using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using VkNet.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Share.ShareControllers
{
    public sealed partial class UserControllers : UserControl
    {
        public bool isDisabled { get; set; }
        public UserControllers()
        {
            this.InitializeComponent();
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(userIcon, this.DispatcherQueue);
        }
        Helpers.Animations.AnimationsChangeImage animationsChangeImage;
        User user;
        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            
            if (DataContext is not UserListed user)
            { return; }
            this.isDisabled = user.isDisabled;
            if (this.user == user.user) return;
            this.user = user.user;
            userIcon.Opacity = 0;

            nameTXT.Text = $"{this.user.FirstName} {this.user.LastName}";
            AutomationProperties.SetName(nameTXT, $"{this.user.FirstName} {this.user.LastName}");
            animationsChangeImage.ChangeImageWithAnimation(this.user.Photo200Orig);

            if ((bool)this.user.Online)
                onlineStatus.Visibility = Visibility.Visible;
            else
                onlineStatus.Visibility = Visibility.Collapsed;


            if ((!isDisabled && !this.user.CanSeeAudio)
                 || (isDisabled && !this.user.CanWritePrivateMessage)
               )
            {
                this.IsEnabled = false;
                this.Opacity = 0.2;
            }
            else
            {
                this.IsEnabled = true;
                this.Opacity = 1;
            }
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (isDisabled) return;
            MainView.OpenSection(user.Id.ToString(), sectionType: SectionView.SectionType.UserSection);
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ShowAnimation.Pause();
            HideAnimation.Begin();
        }
    }
}
