using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.Upload
{
    public sealed partial class UploadList : UserControl
    {

        public ObservableCollection<UploadTrack> uploadTracks { get { return UploadTrack.UploadsTracks; } }
        public UploadList()
        {
            this.InitializeComponent();


        }





    }
}
