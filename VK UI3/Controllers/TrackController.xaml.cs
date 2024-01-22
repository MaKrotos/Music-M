using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using VkNet.Model;
using System.Threading.Tasks;
using VK_UI3.Helpers;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Web.Http;
using System.Collections.ObjectModel;
using System.Security.Policy;
using VK_UI3.Helpers.Animations;
using VK_UI3.VKs;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI.Text;
using VkNet.Model.Attachments;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Controllers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 


    public sealed partial class TrackController : UserControl
    {


        private readonly DependencyProperty trackDataProperty = DependencyProperty.Register(
              "TrackData", typeof(ExtendedAudio), typeof(TrackController), new PropertyMetadata(default(ExtendedAudio), OnTrackDataChanged));
        AnimationsChangeImage changeImage = null;

        private void onNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //      var controller = (TrackController)d;
            //        controller.NumberInList = (int)e.NewValue;
          
        }

        public bool HasLabelValue { get; set; }

    
        private static void OnTrackDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var controller = (TrackController)d;
            // var newTrackData = (Audio)e.NewValue;

            // controller.TrackData = newTrackData;
       
        }

        private void LoadImage()
        {
           
            this.changeImage.ChangeImageWithAnimation(Thumbnail);
        }

        public ExtendedAudio TrackData
        {
            get
            {
                return (ExtendedAudio)GetValue(TrackDataProperty);
            }
            set { SetValue(TrackDataProperty, value); }
        }
        public string Thumbnail
        {
            get
            {
                if (TrackData == null)
                    return "null";
                if (TrackData.Audio.Album == null)
                    return "null";
                var a= TrackData.Audio.Album.Thumb.Photo600
                     ?? TrackData.Audio.Album.Thumb.Photo300
                     ?? TrackData.Audio.Album.Thumb.Photo270
                     ?? TrackData.Audio.Album.Thumb.Photo68
                     ?? TrackData.Audio.Album.Thumb.Photo34;
                if (a != null)
                    return a;

                return "null";
            }
           
        }

        public DependencyProperty TrackDataProperty => trackDataProperty;

        public TrackController()
        {
            this.InitializeComponent();
            changeImage = new AnimationsChangeImage(this.ImageThumb, DispatcherQueue);
            this.DataContextChanged += (s, e) => { 
                Bindings.Update();
                LoadImage();

                changeIconPlayBTN = new AnimationsChangeIcon(PlayBTN);
                if (TrackData != null)
                {
                    //

                    if (!attached)
                    {
                        TrackData.userAudio.AudioPlayedChangeEvent += UserAudio_AudioPlayedChangeEvent;
                        attached = true;
                    }


                    Symbol symbol = TrackData.PlayThis ? Symbol.Pause : Symbol.Play;
                    ChangeSymbolIcon(symbol);
                    HandleAnimation(TrackData.PlayThis);
                }
            };    
            this.DefaultStyleKey = typeof(TrackController);
            Loaded += TrackController_Loaded;

            //this.Image.ImageSource = new BitmapImage(new Uri(Cover));


          
          
        }

        private void UserAudio_AudioPlayedChangeEvent(object sender, EventArgs e)
        {
            try
            {
                Symbol symbol = TrackData.PlayThis ? Symbol.Pause : Symbol.Play;
                ChangeSymbolIcon(symbol);
                HandleAnimation(TrackData.PlayThis);
            }
            catch (Exception ex) { }
        }

        private void ChangeSymbolIcon(Symbol symbol)
        {
            if (GridPlayIcon.Opacity != 0)
            {
                changeIconPlayBTN.ChangeSymbolIconWithAnimation(symbol);
            }
            else
            {
                PlayBTN.Symbol = symbol;
            }
        }

        private void HandleAnimation(bool playThis)
        {
            if (playThis)
            {
                FadeOutAnimationGridPlayIcon.Pause();
                FadeInAnimationGridPlayIcon.Begin();
            }
            else if (!entered)
            {
                FadeInAnimationGridPlayIcon.Pause();
                FadeOutAnimationGridPlayIcon.Begin();
            }
        }


        bool attached = false;

        private void TrackController_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private async Task<IRandomAccessStream> GetImageStreamAsync(string uri)
        {
            var httpClient = new HttpClient();
            var buffer = await httpClient.GetBufferAsync(new Uri(uri));
            var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(buffer);
            stream.Seek(0);
            return stream;
        }



        public async void getIMG(Audio audio)
        {
           

                    if (audio != null && audio.Album != null)
                    {
                        VK vk = new VK(null);
                        //var uri = await vk.GetAlbumCover(audio.Album.OwnerId, audio.Album.Id);
                        this.DispatcherQueue.TryEnqueue(async () =>
                        {
                        //   this.ImgUri = uri;
                        });
                    }
              
        }
        AnimationsChangeIcon changeIconPlayBTN = null;
        private bool entered = false;

        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {



            //   AudioPlayer.PlayTrack(TrackData.AudioList, TrackData.NumberList);


            // РџРѕР»СѓС‡РµРЅРёРµ РѕР±СЉРµРєС‚Р° MediaPlaybackSession
            //var playbackSession = AudioPlayer.mediaPlayer.PlaybackSession;

            // РџРѕР»СѓС‡РµРЅРёРµ РїСЂРѕРіСЂРµСЃСЃР° Р±СѓС„РµСЂРёР·Р°С†РёРё
            //double bufferingProgress = playbackSession.BufferingProgress;


            /*
            M3U8Downloader m3U8Downloader = new M3U8Downloader(this.TrackData.Url.ToString(), TrackData.Title);
      
            var datasegment = m3U8Downloader.getSegmentByTime(0);
            
            using (var mp3Stream = new MemoryStream(datasegment.segment))
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(mp3Stream);
                player.Play();

            }
            */

            //   AudioPlayer.PlayTrack(TrackData.AudioList, TrackData.NumberList);

            TrackData.userAudio.currentTrack = TrackData.NumberInList; 
            AudioPlayer.PlayList(TrackData.userAudio);

            



        }

        private void TextBlock_PointerEntered(object sender, PointerRoutedEventArgs e)
        {


            TextBlock textBlock = sender as TextBlock;
            textBlock.TextDecorations = TextDecorations.Underline;
          
        }

        private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;
            textBlock.TextDecorations = TextDecorations.None;
        }

        private void UCcontrol_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //FadeInAnimation.Begin();
            entered = true;
           
            Symbol symbol = TrackData.PlayThis ? Symbol.Pause : Symbol.Play;
            if (GridPlayIcon.Opacity != 0) 
            { 
                changeIconPlayBTN.ChangeSymbolIconWithAnimation(symbol); 
            }
            else
            {
                PlayBTN.Symbol = symbol;
            }


            FadeOutAnimationGridPlayIcon.Pause();
            FadeInAnimationGridPlayIcon.Begin();
        }

        private void UCcontrol_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            entered = false;
            if (TrackData.PlayThis) return;
            FadeInAnimationGridPlayIcon.Pause();
            FadeOutAnimationGridPlayIcon.Begin();

            //FadeOutAnimation.Begin();
        }
    }
}
