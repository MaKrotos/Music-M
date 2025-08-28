using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Services;
using VK_UI3.VKs;
using VK_UI3.VKs.IVK;
using VkNet.Model.Attachments;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePlayList : Microsoft.UI.Xaml.Controls.Page
    {
        private bool genByPlayList = false;
        string genBy = null;
        string unicid = null;
        public CreatePlayList(AudioPlaylist audioPlaylist, bool genByPlayList = false, string genBy = null, string unicID = null)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audioPlaylist = audioPlaylist;
            this.genByPlayList = genByPlayList;
            this.genBy = genBy;
            this.unicid = unicID;
        }
        public CreatePlayList(AudioPlaylist audioPlaylist, List<VkNet.Model.Attachments.Audio> audio)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audioPlaylist = audioPlaylist;
            this.audioList = audio;
        }

        public CreatePlayList( List<VkNet.Model.Attachments.Audio> audio)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audioList = audio;
        }
        public CreatePlayList()
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
        }

        public CreatePlayList(IVKGetAudio iVKGetAudio, string genBy, string unicID)
        {
            this.InitializeComponent();
            this.iVKGetAudio = iVKGetAudio;
            this.genBy = genBy;
            this.unicid = unicID;
            this.Loaded += CreatePlayList_Loaded;
        }

        public EventHandler cancelPressed;

        public UIElement GetFirstChild()
        {

            return MainGrid;
        }
        AudioPlaylist audioPlaylist { get; set; } = null;
        List<VkNet.Model.Attachments.Audio> audioList { get; set; } = null;

        private void CreatePlayList_Loaded(object sender, RoutedEventArgs e)
        {
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(PlaylistImage, this.DispatcherQueue);
            deleteFromAlbum.Visibility = Visibility.Collapsed;
            if (audioPlaylist != null && !genByPlayList)
            {
                if (!string.IsNullOrEmpty(audioPlaylist.Photo.GetBestAvailablePhoto()) &&audioPlaylist.Thumbs == null)
                {
                    animationsChangeImage.ChangeImageWithAnimation(audioPlaylist.Photo.GetBestAvailablePhoto());
                    deleteFromAlbum.Visibility = Visibility.Visible;
                }
                
                    this.Title.Text = audioPlaylist.Title;
                this.Description.Text = audioPlaylist.Description;
                this.HideFromSearch.IsOn = audioPlaylist.No_discover;
            }
            else
            if (iVKGetAudio != null || genByPlayList)
            {
                SaveBTN.Content = "Генерировать";
                if (
                    iVKGetAudio != null
                )
                {
                    GenText.Visibility = Visibility.Visible;
                    GenValue.Visibility = Visibility.Visible;
                }
                if (!string.IsNullOrEmpty(genBy))
                {
                    Title.Text = genBy;
                    Description.Text = $"Сгенерированный плейлист с помощью VK M Desktop на основе {genBy}";
                }
                else
                {
                    Title.Text = "Сгенерированный плейлист";
                    Description.Text = $"Сгенерированный плейлист с помощью VK M Desktop";
                }
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

                var image = await VK.vkService.UploadPhotoToServer(uploadServer, CoverPath);

                await VK.vkService.SetPlaylistCoverAsync(AccountsDB.activeAccount.id, audioPlaylist.Id, image.Hash, image.Photo);
            }
            else
            {
                if (pressedDeleteImage)
                {
                    await VK.vkService.DeletePlayListCoveAsync(audioPlaylist.OwnerId, audioPlaylist.Id);
                }
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

            if (audioList == null)
            {
                audioPlaylist = await VK.api.Audio.CreatePlaylistAsync(AccountsDB.activeAccount.id, this.Title.Text, this.Description.Text, No_discover: HideFromSearch.IsOn);
            }
            else
            {
                List<string> audios = new();
                foreach (var item in audioList)
                {
                    audios.Add(item.OwnerId + "_" + item.Id);
                }
              
                audioPlaylist = await VK.api.Audio.CreatePlaylistAsync(AccountsDB.activeAccount.id, this.Title.Text, this.Description.Text, audios, No_discover: HideFromSearch.IsOn);
            }
            await UploadCoverPlaylist();
            audioPlaylist = await VK.api.Audio.GetPlaylistByIdAsync(audioPlaylist.OwnerId, audioPlaylist.Id);
            cancelPressed?.Invoke(audioPlaylist, EventArgs.Empty);
        }

        private async Task createGenerateAsync()
        {
            if (iVKGetAudio != null)
            {
                new GeneratorAlbumVK(iVKGetAudio, name: Title.Text, deepGen: (int)GenValue.Value, description: Description.Text, noDiscover: HideFromSearch.IsOn, CoverPath: CoverPath, unicId: unicid).GenerateAsync();
                
            }
            else
            {

                if (audioPlaylist != null)
                {
                    var pla = await VK.vkService.GetPlaylistAsync(1000, audioPlaylist.Id, audioPlaylist.AccessKey, audioPlaylist.OwnerId);
                    List<VkNet.Model.Attachments.Audio> track_playlist = new List<VkNet.Model.Attachments.Audio>(pla.Audios.Cast<VkNet.Model.Attachments.Audio>().ToList());

                  

                   

                    if (pla != null) {
                        new GeneratorAlbumVK(track_playlist, name: Title.Text, deepGen: 100, description: Description.Text, noDiscover: HideFromSearch.IsOn, CoverPath: CoverPath, unicId: unicid).GenerateAsync();
                    }
                }
            }
        }

        Helpers.Animations.AnimationsChangeImage animationsChangeImage;
        private IVKGetAudio iVKGetAudio;

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
                        deleteFromAlbum.Visibility = Visibility.Visible;
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
                deleteFromAlbum.Visibility = Visibility.Visible;
            }




        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cancelPressed?.Invoke(this, EventArgs.Empty);
        }

        private async void SaveBTN_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Title.Text) || string.IsNullOrWhiteSpace(Title.Text))
            {
                return;
            }

            if (iVKGetAudio != null || genByPlayList)
            {
                createGenerateAsync();
                cancelPressed?.Invoke(audioPlaylist, EventArgs.Empty);
                return;
            } 
            else
            if (audioPlaylist == null)
            {

                await create();

            }
            else
            {
                await EditAsync();
            }
            TempPlayLists.TempPlayLists.updateNextRequest = true;
        }
        bool pressedDeleteImage = false;
        private void deleteFromAlbum_Click(object sender, RoutedEventArgs e)
        {
            pressedDeleteImage = true;
            deleteFromAlbum.Visibility = Visibility.Collapsed;
            animationsChangeImage.ChangeImageWithAnimation((string) null);
            CoverPath = null;
        }
    }

}
