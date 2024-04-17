using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CaptchaEnter : Page
    {
        public string captchaUri { get; set; }
        public TaskCompletionSource<string?> Submitted { get; set; } = new();
        public CaptchaEnter()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Параметр передается как объект, поэтому его нужно привести к нужному типу
            var viewModel = e.Parameter as CaptchaEnter;

            if (viewModel != null)
            {
                this.Submitted = viewModel.Submitted;
                captchaUri = viewModel.captchaUri;

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Submitted.SetResult(CodeBox.Text);
            this.Frame.GoBack();
        }
    }
}
