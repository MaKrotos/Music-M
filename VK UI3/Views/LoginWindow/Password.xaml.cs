using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.VKs;
using VkNet.AudioBypassService.Models.Auth;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.LoginWindow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public  partial class Password : Page
    {
        internal VK vk;

        public int CodeLength { get; set; }
        public string? Info { get; set; }
     


        public string FirstName { get; set;  } = "Незнакомец";
        public string Photo200 { get; set; } = "null";
        public string Phone { get; set; } = "***********";
       


       Helpers.Animations.AnimationsChangeImage AnimationsChangeImage { get; set; }



        public TaskCompletionSource<string?> Submitted { get; private set; } = new();

        public Password()
        {
            this.InitializeComponent();
            
        }
        

        private async void BackButton(object sender, RoutedEventArgs e)
        {
            // _ = await InputTextDialogAsync("hello!", this.XamlRoot);
            AccountsDB.activeAccount = new AccountsDB.Accounts();
            this.Frame.Navigate(typeof(Login));
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Параметр передается как объект, поэтому его нужно привести к нужному типу
            var viewModel = e.Parameter as Password;

            if (viewModel != null)
            {
              

                CodeLength = (int)viewModel.CodeLength;
                Info =  viewModel.Info;

                this.FirstName = viewModel.FirstName;
                this.Photo200 = viewModel.Photo200;
                this.Phone = viewModel.Phone;
              
                
     

                Submitted =   viewModel.Submitted;

                this.vk = viewModel.vk;

                if (FirstName == "Незнакомец") NameT.Visibility = Visibility.Collapsed;
                if (Photo200 == "null") imagesName.Visibility = Visibility.Collapsed;
                else
                {
                    AnimationsChangeImage = new Helpers.Animations.AnimationsChangeImage(imagesName, this.DispatcherQueue);
                    AnimationsChangeImage.ChangeImageWithAnimation(Photo200);
                }
                if (Phone == "***********") phoneText.Visibility = Visibility.Collapsed;
            }
           
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            sumbit();
        }

        private void ShowAnotherVerificationMethodsButton_Click(object sender, RoutedEventArgs e)
        {
            Submitted.SetResult(null);
            Submitted = new TaskCompletionSource<string?>();
        }

        private void passText_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {

                sumbit();
            }
        }

        private void sumbit()
        {
            vk.AuthAsync(passText.Password);
        }
    }
}
