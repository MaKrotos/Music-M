using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using MusicX.Services;
using MvvmHelpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Web.Http;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class MiniAppViewModel(string appId, string url) 
    {
        public string AppId { get; } = appId;
        public string Url { get; } = url;

        public bool IsLoading { get; set; } = true;

        public string? ErrorMessage { get; set; }
        public string ErrorDetails { get; set; } = string.Empty;
    }



    public sealed partial class MiniAppView : Page
    {
        private MiniAppViewModel ViewModel { get; set; }
        private bool _isNavigating;

        public MiniAppView()
        {
            InitializeComponent();
            Loaded += MiniAppView_Loaded;
            Unloaded += MiniAppView_Unloaded;
        }


        private void MiniAppView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAsync().SafeFireAndForget();
        }

        private async Task LoadAsync()
        {
          
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (e.Parameter is MiniAppViewModel viewModelParam)
            {
                ViewModel = viewModelParam;
                // Здесь можно проверить ViewModel
                Debug.WriteLine($"Navigating back from MiniAppView with VM ID: {viewModelParam.AppId}");
            }

        }

        private void WebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            _isNavigating = true;
            ViewModel.IsLoading = true;
        }

        private void WebView_OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            _isNavigating = false;
            ViewModel.IsLoading = false;

            if (args.IsSuccess || args.WebErrorStatus == CoreWebView2WebErrorStatus.OperationCanceled) return;

            ViewModel.ErrorMessage = "Произошла ошибка при загрузке страницы";
            ViewModel.ErrorDetails = args.HttpStatusCode == 0
                ? args.WebErrorStatus.ToString()
                : $"Код ошибки сервера: {(HttpStatusCode)args.HttpStatusCode}";
        }

        private void MiniAppView_Unloaded(object sender, RoutedEventArgs e)
        {
            WebView.Close();
        }

        private async void WebView_Loaded(object sender, RoutedEventArgs e)
        {
            await WebView.EnsureCoreWebView2Async();



            var settings = WebView.CoreWebView2.Settings;
#if !DEBUG
        settings.AreDevToolsEnabled = false;
        settings.IsStatusBarEnabled = false;
        settings.AreDefaultContextMenusEnabled = false;
        settings.AreBrowserAcceleratorKeysEnabled = false;
        settings.AreDefaultScriptDialogsEnabled = false;
#endif
            settings.IsPasswordAutosaveEnabled = false;
            settings.IsGeneralAutofillEnabled = false;
            settings.IsZoomControlEnabled = false;
            settings.IsBuiltInErrorPageEnabled = false;



            settings.UserAgent = "VKAndroidApp/8.99-23423 (Android 12; SDK 32; arm64-v8a; MusicX; ru; 2960x1440)";

            var bridgeService = StaticService.Container.GetRequiredService<VkBridgeService>();

            bridgeService.Load(ViewModel.AppId, ViewModel.Url);

            WebView.CoreWebView2.AddHostObjectToScript("bridge", bridgeService);

            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
    """
const requestPropsMap = {
  VKWebAppInit: () => [],
  VKWebAppAddToCommunity: () => [],
  VKWebAppAddToHomeScreen: () => [],
  VKWebAppAddToHomeScreenInfo: () => [],
  VKWebAppAllowMessagesFromGroup: ({group_id, key}) => [group_id, key], // { group_id: number; key?: string }
  VKWebAppAllowNotifications: () => [],
  OKWebAppCallAPIMethod: () => [], // { method: string; params: OKCallApiParams };
  VKWebAppCallAPIMethod: ({method, params}) => [method, JSON.stringify(params)], /* {
    method: string;
    params: Record<'access_token' | 'v', string> & Record<string, string | number>;
  }; */
  VKWebAppCopyText: ({text}) => [text], // { text: string };
  VKWebAppCreateHash: ({payload}) => [payload], // { payload: string };
  VKWebAppDownloadFile: ({url, filename}) => [url, filename], // { url: string; filename: string };
  VKWebAppGetAuthToken: ({app_id, scope}) => [app_id, scope], // { app_id: number; scope: PersonalAuthScope | string };
  VKWebAppClose: () => [status, JSON.stringify(payload)], // { status: AppCloseStatus; payload?: any };
  VKWebAppOpenApp: ({app_id, location}) => [app_id, location], // { app_id: number; location?: string };
  VKWebAppDenyNotifications: () => [],
  VKWebAppFlashGetInfo: () => [],
  VKWebAppFlashSetLevel: ({level}) => [level], // { level: number };
  VKWebAppGetClientVersion: () => [],
  VKWebAppGetCommunityToken: ({app_id, group_id, scope}) => [app_id, group_id, scope], // { app_id: number, group_id: number, scope: CommunityAuthScope | string };
  VKWebAppGetConfig: () => [],
  VKWebAppGetLaunchParams: () => [],
  VKWebAppAudioPause: () => [],
  VKWebAppGetEmail: () => [],
  VKWebAppGetFriends: ({multi}) => [multi], // { multi?: boolean };
  VKWebAppGetGeodata: () => [],
  VKWebAppGetGrantedPermissions: () => [],
  VKWebAppGetPersonalCard: ({type}) => [type], // { type: PersonalCardType[] };
  VKWebAppGetPhoneNumber: () => [],
  VKWebAppGetUserInfo: ({user_id, user_ids}) => [user_id, user_ids], // { user_id?: number; user_ids?: string };
  VKWebAppJoinGroup: ({group_id}) => [group_id], // { group_id: number };
  VKWebAppLeaveGroup: ({group_id}) => [group_id], // { group_id: number };
  VKWebAppAddToMenu: () => [],
  VKWebAppOpenCodeReader: () => [],
  VKWebAppOpenContacts: () => [],
  VKWebAppOpenPayForm: () => [], // VKPayProps<VKPayActionType>; // not planned
  VKWebAppOpenQR: () => [],
  VKWebAppResizeWindow: ({width, height}) => [width, height], // { width: number; height?: number };
  VKWebAppScroll: ({top, speed}) => [top, speed], // { top: number; speed?: number };
  VKWebAppSendToClient: ({fragment}) => [fragment], // { fragment?: string };
  VKWebAppSetLocation: ({location, replace_state}) => [location, replace_state], // { location: string; replace_state?: boolean };
  VKWebAppSetViewSettings: ({status_bar_style, action_bar_color, navigation_bar_color}) => [status_bar_style, action_bar_color, navigation_bar_color], // { status_bar_style: AppearanceType; /** Android only */ action_bar_color?: 'none' | string; /** Android only */ navigation_bar_color?: string; };
  VKWebAppShare: ({link}) => [link], // { link?: string };
  VKWebAppShowCommunityWidgetPreviewBox: ({type, group_id, code}) => [type, group_id, code], // { type: CommunityWidgetType | string, group_id: number, /* execute method code */ code: string };
  VKWebAppShowImages: ({images, start_index}) => [images, start_index], // { images: string[]; start_index?: number };
  VKWebAppShowInviteBox: () => [],
  VKWebAppShowLeaderBoardBox: ({user_result}) => [user_result], // { user_result: number };
  VKWebAppShowMessageBox: () => [], // MessageRequestOptions; // not supported rn
  VKWebAppCheckBannerAd: () => [],
  VKWebAppHideBannerAd: () => [],
  VKWebAppShowBannerAd: () => [], // ShowBannerAdRequest;
  VKWebAppShowNativeAds: () => [], // ShowNativeAdsRequest;
  VKWebAppCheckNativeAds:() => [], // CheckNativeAdsRequest;
  VKWebAppShowOrderBox:() => [], // OrderRequestOptions;
  VKWebAppShowRequestBox:() => [], // RequestForRequestOptions;
  VKWebAppShowWallPostBox:() => [], // WallPostRequestOptions;
  VKWebAppShowSubscriptionBox:() => [], // ShowSubscriptionBoxRequest;
  VKWebAppOpenWallPost: ({post_id, owner_id}) => [post_id, owner_id], // { post_id: number; owner_id: number };
  VKWebAppStorageGet: ({keys}) => [keys], // { keys: string[] };
  VKWebAppStorageGetKeys: ({count, offset}) => [count, offset], // { count: number; offset: number };
  VKWebAppStorageSet: ({key, value}) => [key, value], // { key: string; value: string };
  VKWebAppTapticImpactOccurred: ({style}) => [style], // { style: TapticVibrationPowerType };
  VKWebAppTapticNotificationOccurred: ({type}) => [type], // { type: TapticNotificationType };
  VKWebAppTapticSelectionChanged: () => [],
  VKWebAppAddToFavorites: () => [],
  VKWebAppSendPayload: ({group_id, payload}) => [group_id, payload], // { group_id: number; payload: any };
  VKWebAppDisableSwipeBack: () => [],
  VKWebAppEnableSwipeBack: () => [],
  VKWebAppSetSwipeSettings: ({history}) => [history], // { history: boolean };
  VKWebAppShowStoryBox: () => [], // ShowStoryBoxOptions; // not supported rn
  VKWebAppAccelerometerStart: ({refresh_rate}) => [refresh_rate], // { refresh_rate?: string };
  VKWebAppAccelerometerStop: () => [],
  VKWebAppGyroscopeStart: () => [],
  VKWebAppGyroscopeStop: () => [],
  VKWebAppDeviceMotionStart: () => [],
  VKWebAppDeviceMotionStop: () => [],
  VKWebAppSubscribeStoryApp: () => [], // SubscribeStoryAppOptions; // not supported rn
  VKWebAppGetGroupInfo: ({group_id}) => [group_id], // { group_id: number };
  VKWebAppLibverifyRequest: () => [], // { phone: string }; // not supported rn
  VKWebAppLibverifyCheck: () => [], // { code: string }; // not supported rn
  VKWebAppRetargetingPixel: () => [], // RetargetingPixelOptions; // ???
  VKWebAppCheckAllowedScopes: ({scopes}) => [scopes], // { scopes: string };
  VKWebAppConversionHit: () => [], // ConversionHitRequest;
  VKWebAppCheckSurvey: () => [],
  VKWebAppShowSurvey: () => [],
  VKWebAppScrollTop: () => [],
  VKWebAppScrollTopStart: () => [],
  VKWebAppScrollTopStop: () => [],
  VKWebAppShowSlidesSheet: () => [], // ShowSlidesSheetRequest;
  VKWebAppTranslate: () => [], // TranslateRequest;
  VKWebAppCallStart: () => [],
  VKWebAppCallJoin: () => [], // CallJoinRequest;
  VKWebAppCallGetStatus: () => [],
  VKWebAppRecommend: () => [],
  VKWebAppAddToProfile: () => [], // AddToProfileRequest;
  SetSupportedHandlers: () => [],
  VKWebAppTrackEvent: () => [], // TrackEventRequest;
}

window.ReactNativeWebView = {
    postMessage: async (message) => {
        try {
            const { handler, params } = JSON.parse(message)

            const argsMapper = requestPropsMap[handler]

            if (!argsMapper) return

            if ('request_id' in params) {
                await window.chrome.webview.hostObjects.bridge.SetNextRequestId(params.request_id)
            }
            
            const fn = await window.chrome.webview.hostObjects.bridge.getHostProperty(handler)
            return await fn.applyHostFunction(argsMapper(params))
        } catch (e) {
            console.error(`Error calling bridge ${handler} with params ${JSON.stringify(params)}`)
            console.error(e)
        }
    }
}

document.addEventListener('message', (event) => {
    console.log(`VKWebAppEvent ${event?.data}`)
})

window.chrome.webview.hostObjects.bridge.addEventListener('VKWebAppEvent', (event) => {
    const ev = new Event('message')
    ev.data = event
    document.dispatchEvent(ev)
})                                                                            
""");

#if DEBUG
            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("chrome.webview.hostObjects.options.log = console.log.bind(console);");
#endif

            WebView.Source = new Uri(ViewModel.Url);
        }

        private void WebView_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {

        }
    }
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var reverse = parameter?.ToString() == "reverse";
            var isEmpty = string.IsNullOrEmpty(value?.ToString());

            return (isEmpty ^ reverse) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
