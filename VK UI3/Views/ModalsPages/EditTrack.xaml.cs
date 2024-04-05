using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VK_UI3.Views.Upload;
using VK_UI3.VKs;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.ModalsPages
{
    public sealed partial class EditTrack : UserControl
    {
        private string path = null;
        private Audio audio = null;
        public EditTrack(string path)
        {
            this.InitializeComponent();
            this.path = path;

            this.Loaded += EditTrack_Loaded;
            this.Unloaded += EditTrack_Unloaded;

            Save.Content = "Загрузить";
        }

        public EditTrack(Audio audio)
        {
            this.InitializeComponent();

           this.audio = audio;
            artist.Text = audio.Artist;
            name.Text = audio.Title;



            this.Loaded += EditTrack_Loaded;
            this.Unloaded += EditTrack_Unloaded;
            
        
        }

        private void EditTrack_Unloaded(object sender, RoutedEventArgs e)
        {
          this.Unloaded -= EditTrack_Unloaded; 
          this.Loaded -= EditTrack_Loaded;
        }

        private void EditTrack_Loaded(object sender, RoutedEventArgs e)
        {
            if (audio != null)
            if (audio.Genre != null)
            {
                int tag = (int)audio.Genre;

                ComboBoxItem itemToSelect = Genres.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(item => int.Parse((string)item.Tag) == tag);

                if (itemToSelect != null)
                {
                    ancyItem.IsSelected = false;
                    Genres.SelectedItem = itemToSelect;
                }
                Vivod.IsChecked = audio.No_search;
            }
            if (path != null)
            {
                var file = TagLib.File.Create(path);
                if (file.Tag.IsEmpty) return;

                if (file.Tag.Artists != null && file.Tag.Artists.Count() != 0)
                {
                    artist.Text = file.Tag.Artists[0];
                }
                else artist.Text = "Исполнитель не указан";
                if (file.Tag.Title != null || file.Tag.Title != "")
                {

                    name.Text = file.Tag.Title;
                }
                else
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                    name.Text = fileNameWithoutExtension;

                }


            
            
            
            }
        }

        public EventHandler cancelPressed;
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (path != null)
            {
                new UploadTrack(this.DispatcherQueue ,path, name.Text, artist.Text, null, int.Parse((string)(Genres.SelectedItem as ComboBoxItem).Tag), Vivod.IsChecked); ;
                cancelPressed.Invoke(null, System.EventArgs.Empty);
                return;
            }
            else
            {
                if (audio == null) return;
                var param = new AudioEditParams()
                {
                    OwnerId = (long)audio.OwnerId,
                    AudioId = (long)audio.Id,
                    Artist = artist.Text,
                    Title = name.Text,
                    NoSearch = Vivod.IsChecked,
                    GenreId = (VkNet.Enums.AudioGenre)int.Parse((string)(Genres.SelectedItem as ComboBoxItem).Tag)
                };

                await VK.api.Audio.EditAsync(param);
                var ids = new string[] { audio.OwnerId +"_"+ audio.Id };
                audio = (await VK.api.Audio.GetByIdAsync(ids)).ToList()[0];
                cancelPressed.Invoke(audio, System.EventArgs.Empty);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            cancelPressed.Invoke(null, System.EventArgs.Empty);
        }
    }
}
