using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using VkNet.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Share.ShareControllers
{
    public sealed partial class UserControllers : UserControl
    {
        public bool isDisabled { get; set; } = true;
        public UserControllers()
        {
            this.InitializeComponent();
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(userIcon, this.DispatcherQueue);
        }
        Helpers.Animations.AnimationsChangeImage animationsChangeImage;
        User user;
        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            
            if (DataContext is not User user)
            { return; }

            if (this.user == user) return;
            this.user = user;
            userIcon.Opacity = 0;

            nameTXT.Text = $"{user.FirstName} {user.LastName}";
            animationsChangeImage.ChangeImageWithAnimation(user.Photo200Orig);

            if ((bool)user.Online)
                onlineStatus.Visibility = Visibility.Visible;
            else
                onlineStatus.Visibility = Visibility.Collapsed;


            if (isDisabled && !user.CanSeeAudio)
                this.IsEnabled = false;
            else
            {
                this.IsEnabled = true;
            }
    
        }




        private void StackPanel_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MainView.OpenSection(user.Id.ToString(), sectionType: SectionView.SectionType.UserSection);

            //  if (e.GetCurrentPoint(sender as UIElement).Properties.IsLeftButtonPressed)
            //  {

            //      MainView.OpenPlayList(_PlayList.Playlist);
            //  }

        }

        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            HideAnimation.Pause();
            ShowAnimation.Begin();
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            ShowAnimation.Pause();
            HideAnimation.Begin();
        }
    }
}