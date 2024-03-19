using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.IO;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.ApplicationModel.DataTransfer;
using VkNet.Model.Attachments;
using MusicX.Core.Services;
using System.Threading.Tasks;
using VK_UI3.VKs;
using VK_UI3.DB;
using Microsoft.AppCenter.Crashes;
using ProtoBuf.Meta;
using System.Collections.Generic;
using VK_UI3.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePlayList : Microsoft.UI.Xaml.Controls.Page
    {
        public CreatePlayList(AudioPlaylist audioPlaylist)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audioPlaylist = audioPlaylist;
        }
        public CreatePlayList(AudioPlaylist audioPlaylist, Audio audio)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audioPlaylist = audioPlaylist;
            this.audio = audio;
        }

        public CreatePlayList(Audio audio)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audio = audio;
        }
        public CreatePlayList()
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
        }
        
        public EventHandler cancelPressed;
        public UIElement GetFirstChild()
        {
          
            return MainGrid;
        }
        AudioPlaylist audioPlaylist { get; set; } = null;
        Audio audio { get; set; } = null;   

        private void CreatePlayList_Loaded(object sender, RoutedEventArgs e)
        {
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(PlaylistImage, this.DispatcherQueue);

            if (audioPlaylist != null)
            {
                animationsChangeImage.ChangeImageWithAnimation(audioPlaylist.Cover);
                this.Title.Text = audioPlaylist.Title;
                this.Description.Text = audioPlaylist.Description;
                this.HideFromSearch.IsOn = audioPlaylist.No_discover;
                //this.HideFromSearch = audioPlaylist.hid

            }
            else
            {

                SaveBTN.Content = "Создать";
            }
        }

        string CoverPath;

        private async Task UploadCoverPlaylist()
        {
            if (!string.IsNullOrEmpty(CoverPath))
            {
                var uploadServer = await VK.vkService.GetPlaylistCoverUploadServerAsync(AccountsDB.activeAccount.id, audioPlaylist.Id);

                var image = await VK.vkService.UploadPlaylistCoverAsync(uploadServer, CoverPath);

                await VK.vkService.SetPlaylistCoverAsync(AccountsDB.activeAccount.id, audioPlaylist.Id, image.Hash, image.Photo);
            }
        }
        private async Task EditAsync()
        {
            try
            {
                await VK.api.Audio.EditPlaylistAsync(AccountsDB.activeAccount.id, Convert.ToInt32(audioPlaylist.Id), this.Title.Text, this.Description.Text, No_discover: HideFromSearch.IsOn);
                await UploadCoverPlaylist();
                audioPlaylist = await VK.api.Audio.GetPlaylistByIdAsync(audioPlaylist.OwnerId, audioPlaylist.Id);
                cancelPressed?.Invoke(audioPlaylist, EventArgs.Empty);
                
            }
            catch (Exception ex)
            {
            }
        }

        private async Task create()
        {
            if (string.IsNullOrEmpty(Title.Text) || string.IsNullOrWhiteSpace(Title.Text))
            {
               // _snackbarService.Show("Обязательные поля не заполнены", "Вы должны заполнить название плейлиста");
                return;
            }

             
            if (audio == null)
            {
                audioPlaylist = await VK.api.Audio.CreatePlaylistAsync(AccountsDB.activeAccount.id, this.Title.Text, this.Description.Text, No_discover: HideFromSearch.IsOn);
            }
            else
            {
                List<string> audios = new();
                audios.Add(audio.OwnerId + "_" + audio.Id);
                audioPlaylist = await VK.api.Audio.CreatePlaylistAsync(AccountsDB.activeAccount.id, this.Title.Text, this.Description.Text, audios, No_discover: HideFromSearch.IsOn);
            }
            await UploadCoverPlaylist();
            audioPlaylist = await VK.api.Audio.GetPlaylistByIdAsync(audioPlaylist.OwnerId, audioPlaylist.Id);
            cancelPressed?.Invoke(audioPlaylist, EventArgs.Empty);
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
                        CoverPath = storageFile.Path; 
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
                CoverPath = file.Path;
            }
  



        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cancelPressed?.Invoke(this, EventArgs.Empty);
        }

        private async void SaveBTN_Click(object sender, RoutedEventArgs e)
        {
            if (audioPlaylist == null)
            {

              await  create();

            }
            else
            {
              await  EditAsync();
            }
            TempPlayLists.TempPlayLists.updateNextRequest = true;
        }
    }

}
