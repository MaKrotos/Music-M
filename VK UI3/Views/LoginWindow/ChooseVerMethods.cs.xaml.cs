using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VK_UI3.Controllers;
using VK_UI3.Helpers;
using VK_UI3.VKs;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.Ecosystem;

using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.LoginWindow
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChooseVerMethods : Page, INotifyPropertyChanged
    {

        private readonly TaskCompletionSource<EcosystemVerificationMethod> _taskSource = new();

        public ObservableCollection<EcosystemVerificationMethod> VerificationMethods;

        public event PropertyChangedEventHandler PropertyChanged;
        internal VK vk;

        public ChooseVerMethods()
        {
            this.InitializeComponent();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {
                //   this.ImgUri = uri;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


            // Параметр передается как объект, поэтому его нужно привести к нужному типу
            var viewModel = e.Parameter as ChooseVerMethods;

            if (viewModel != null)
            {
                    this.VerificationMethods = viewModel.VerificationMethods;
                this.vk = viewModel.vk;
            }

           
        }



        private async void BackButton(object sender, RoutedEventArgs e)
        {
            // _ = await InputTextDialogAsync("hello!", this.XamlRoot);
            this.Frame.GoBack();
        }

        private void LoginsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            var se = VerificationMethods[LoginsListView.SelectedIndex].Name;


            vk.NextStepAsync(se);

        }
    }
}
