using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Handlers;
using System.Threading.Tasks;
using VK_UI3.Views.Tasks;
using VK_UI3.VKs;
using VkNet.Model;
using VkNet.Model.Attachments;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Web.Http;
using HttpClient = System.Net.Http.HttpClient;

// To learn more about WinUI, the WinUI project structure,
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{

    public class CreatePostTask : TaskAction
    {
        string ImagePath;
        long? UserID;
        private List<MediaAttachment> photoAttachments = new List<MediaAttachment>();
        string textPost;
        Group? Group;
        AudioPlaylist audioPlaylist;
        Audio audio;
        private DateTime? dateTime;

        public CreatePostTask(
            string ImagePath, 
            long? UserID,
            Group? group,
            string textpost,
            AudioPlaylist? audioPlaylist
,
            Audio audio,
            DateTime? dateTime


        ) : base(
                100, 
                "Создание поста  " + ((UserID != null) ? "на мою стр." : ("в " + group.Name)), 
                null, 
                (audioPlaylist == null) ? audio.Title : audioPlaylist.Title)
        {
            this.ImagePath = ImagePath;
            this.UserID = UserID;
            this.Group = group;
            this.textPost = textPost;
            this.audioPlaylist = audioPlaylist;
            this.audio = audio;
            this.dateTime = dateTime;

            this.canPause = false;

            CreatePost();
        }



        public override void Cancel()
        {
            
        }

        public override void onClick()
        {
            if (postID == null) return;

            var id = (UserID != null) ? UserID : Group.Id * -1;

            var url = $"https://vk.com/wall{id}_{postID}";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        public override void Pause()
        {
            
        }

        public override void Resume()
        {
           
        }


        private async void CreatePost()
        {
            try
            {
                var param = new VkNet.Model.RequestParams.WallPostParams();
                param.Message = textPost;


                photoAttachments.Clear();

                if (this.audioPlaylist != null)
                {
                    this.photoAttachments.Add(this.audioPlaylist);
                }

                if (this.audio != null)
                {
                    this.photoAttachments.Add(this.audio);
                }

                await UploadPhotoToServer();

                if (dateTime != null)
                {
                    param.PublishDate = this.dateTime;
                }


                param.Attachments = this.photoAttachments;
                param.OwnerId = this.UserID ?? Group.Id * -1;
                param.FromGroup = (this.Group != null);



                postID = await VK.api.Wall.PostAsync(param);
                this.Status = Statuses.Completed;
                return;
            }
            catch (Exception e)
            {
                this.Status = Statuses.Error;
                return;
            }

        }
        long postID;

        private async Task UploadPhotoToServer()
        {
            if (string.IsNullOrEmpty(ImagePath)) return;

            var server = await VK.api.Photo.GetWallUploadServerAsync(groupId: Group?.Id);
            string responseFile;
            using (var httpClient = new HttpClient())
            {
                var fileContent = new ByteArrayContent(File.ReadAllBytes(ImagePath));
                var totalBytes = fileContent.Headers.ContentLength ?? 1;
                var progressHandler = new ProgressMessageHandler(new HttpClientHandler());
                progressHandler.HttpSendProgress += (sender, e) =>
                {
                    Progress = (int)(e.BytesTransferred * 100 / totalBytes);
                };

                using (var formData = new MultipartFormDataContent())
                {
                    formData.Add(fileContent, "file", Path.GetFileName(ImagePath));
                    using (var client = new HttpClient(progressHandler))
                    {
                        var response = await client.PostAsync(server.UploadUrl, formData);
                        responseFile = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            var photo = await VK.api.Photo.SaveWallPhotoAsync(responseFile, (ulong?)UserID, groupId: (ulong?)Group?.Id);
            photoAttachments.AddRange(photo);
        }






    }

    public sealed partial class CreatePost : Microsoft.UI.Xaml.Controls.Page
    {
        Group? Group = null;
        long? UserID = null;

        public static Dictionary<long, DateTime> datetimeGroups = new Dictionary<long, DateTime>();

        public CreatePost(AudioPlaylist audioPlaylist, Group group)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audioPlaylist = audioPlaylist;
            this.Group = group;
       
        }

        public CreatePost(AudioPlaylist audioPlaylist, long userID)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audioPlaylist = audioPlaylist;
            this.UserID = userID;

        }


        public CreatePost(Audio audio, long userID)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;
            this.audio = audio;
            this.UserID = userID;

        }

        public CreatePost(Audio audio, Group group)
        {
            this.InitializeComponent();
            this.Loaded += CreatePlayList_Loaded;

            this.Group = group;
            this.audio = audio;
        }

  
        public CreatePost()
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
        VkNet.Model.Attachments.Audio audio { get; set; } = null;

        private void CreatePlayList_Loaded(object sender, RoutedEventArgs e)
        {
            var id = UserID ?? Group.Id;
            animationsChangeImage = new Helpers.Animations.AnimationsChangeImage(PlaylistImage,this.DispatcherQueue);

            if (datetimeGroups.ContainsKey(id))
            {
                DateTRimeEnable.IsOn = true;
                PickDate.IsEnabled = true;
                PickTime.IsEnabled = true;

                PickDate.Date = datetimeGroups[id];
                PickTime.Time = datetimeGroups[id].TimeOfDay.Add(TimeSpan.FromHours(1));
            }
            else
            {
                PickDate.Date = DateTime.Now;
                PickTime.Time = DateTime.Now.TimeOfDay;
            }
            PickDate.MinDate = DateTime.Now.Date;
    
            
        }

        string CoverPath;

        

        List<MediaAttachment> photoAttachments = new List<MediaAttachment>();



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

            if (string.IsNullOrEmpty(CoverPath))
            {
              
                ToggleThemeTeachingTip1.IsOpen = true;
                

                return;
            }

            datetimeGroups.Remove(UserID ?? Group.Id);
            DateTime? dateTime = null;
            if (DateTRimeEnable.IsOn)
            {
                var date = PickDate.Date.Value.Date;
                var time = PickTime.Time;
                dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds);

                datetimeGroups.Add(UserID ?? Group.Id, (DateTime) dateTime);
            }
            else
            { 
            }


            new CreatePostTask(
                CoverPath, 
                this.UserID, 
                this.Group,
                textPost.Text,
                audioPlaylist,
                audio,
                dateTime         
                );

            cancelPressed?.Invoke(this, EventArgs.Empty);
        }

        private void DateTRimeEnable_Toggled(object sender, RoutedEventArgs e)
        {
            PickDate.IsEnabled = DateTRimeEnable.IsOn;
            PickTime.IsEnabled = DateTRimeEnable.IsOn;
        }

        private void PickTime_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            try
            {
                if (PickTime.Time.TotalMinutes <= DateTime.Now.TimeOfDay.TotalMinutes || PickDate.Date == DateTime.Now.Date)
                {
                    PickTime.Time = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(15));

                }
            }
            catch (Exception ex) { }
        }

      
    }

}
