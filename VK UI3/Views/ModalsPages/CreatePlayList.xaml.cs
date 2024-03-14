using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePlayList : UserControl
    {
        public CreatePlayList()
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
        }
        public UIElement GetFirstChild()
        {
          
            return MainGrid;
        }

        private void CreatePlayList_Loaded(object sender, RoutedEventArgs e)
        {
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(PlaylistImage, this.DispatcherQueue);
            FadeInStoryboard.Begin();
        }

        Helpers.Animations.AnimationsChangeImage animationsChangeImage;
      
        private async void PlaylistImage_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Any())
                {
                    var storageFile = items[0] as StorageFile;
                    var fileExtension = Path.GetExtension(storageFile.Name).ToLower();

                    // Проверка, является ли файл изображением
                    if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
                    {
                        animationsChangeImage.ChangeImageWithAnimation(storageFile.Path);
                        // PlaylistImage.Source = bitmapImage;
                    }
                    else
                    {
                        // Обработка случая, когда файл не является изображением
                    }
                }
            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void PlaylistImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.m_window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");



            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                animationsChangeImage.ChangeImageWithAnimation(file.Path);
            }


          
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           // Frame.GoBack();
        }
    }

}
