using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{
    public class SumbitCaptcha : EventArgs
    { 
    
    }


    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CaptchaEnter : Page
    {
        internal event EventHandler sumbitPressed;

        public string captchaUri { get; set; }
        public TaskCompletionSource<string?> Submitted { get; set; } = new();
        public CaptchaEnter()
        {
            this.InitializeComponent();
        }

        public CaptchaEnter(CaptchaEnter captchaEnter)
        {
            this.Submitted = captchaEnter.Submitted;
            captchaUri = captchaEnter.captchaUri;
               this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Submitted.SetResult(CodeBox.Text);
        }
    }
}
