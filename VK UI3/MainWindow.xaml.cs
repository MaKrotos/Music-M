#define SKIP_ERROR_PRONE_CODE
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Octokit;
using SetupLib;
using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Views;
using VK_UI3.Views.LoginWindow;

using VK_UI3.VKs;
using Windows.ApplicationModel;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;

using Windows.Win32;
using System.IO;
using NAudio.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using VkNet.Enums;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Microsoft.UI.Xaml.Window
    {
        internal static HWND hvn;
        public MainWindow()
        {
            
            this.InitializeComponent();
            contentFrame = ContentFrame;
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);

   
       


            var navigationInfo = new NavigationInfo { SourcePageType = this };

             if (AccountsDB.GetAllAccounts().Count == 0) {

                GoLogin();
           }else
              ContentFrame.Navigate(typeof(MainView), navigationInfo, new DrillInNavigationTransitionInfo());
            
            
            dispatcherQueue = this.DispatcherQueue;

             SubClassing();

            string assetsPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            
            // Составить путь к иконке
            string iconPath = Path.Combine(assetsPath, "icon.ico");
            LoadIcon(iconPath);



            hvn = (HWND)WinRT.Interop.WindowNative.GetWindowHandle(this);

            checkUpdate();
        }










        public static DispatcherQueue dispatcherQueue { get; private set; }

        public static Frame contentFrame;

        public void GoLogin()
        {

            ContentFrame.Navigate(typeof(Login), this, new DrillInNavigationTransitionInfo());
        }


        Microsoft.Win32.SafeHandles.SafeFileHandle iIcon;
        private void LoadIcon(string aIconName)
        {
            // Resolve icon path
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, aIconName);
            // Get window handle
            var hwnd = new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this));
            // Load our image
            iIcon = PInvoke.LoadImage(null, iconPath, GDI_IMAGE_TYPE.IMAGE_ICON, 16, 16, IMAGE_FLAGS.LR_LOADFROMFILE);
            //Set our icon
            PInvoke.SendMessage(hwnd, PInvoke.WM_SETICON, new WPARAM(0), new LPARAM(iIcon.DangerousGetHandle()));
        }



        private void checkUpdate()
        {
            

            Task.Run(async () =>
            {

                Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
                PackageId packageId = package.Id;
                var version = packageId.Version;
                var currentVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

                var appUpdater = new AppUpdater(currentVersion);




                var updateAvailable = await appUpdater.CheckForUpdates();

                if (updateAvailable)
                {
                 
                    // Создайте новое окно
                    dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () => {
                        ContentFrame.Navigate(typeof(UpdatePage), appUpdater, new DrillInNavigationTransitionInfo());
                    });
                    
                }
            });
      
        }



        private delegate IntPtr WinProc(HWND hWnd, uint Msg, WPARAM wParam, IntPtr lParam);
        private WinProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;

      
        private void SubClassing()
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            if (hwnd == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to get window handler: error code {error}");
            }

            newWndProc = new(NewWindowProc);


            oldWndProc = SetWindowLong((HWND)hwnd, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(newWndProc)); 
            
            if (oldWndProc == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to set GWL_WNDPROC: error code {error}");
            }
        }



        const uint WM_GETMINMAXINFO = 0x0024;
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }
        private static IntPtr SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, IntPtr newProc)
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return PInvoke.SetWindowLongPtr(hWnd, nIndex, newProc);
            }
            else
            {
                return new IntPtr(PInvoke.SetWindowLong(hWnd, nIndex, newProc.ToInt32()));
            }
        }






        private IntPtr NewWindowProc(HWND hWnd, uint Msg, WPARAM wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case WM_GETMINMAXINFO:
                    var dpi = PInvoke.GetDpiForWindow(hWnd);
                    float scalingFactor = (float)dpi / 96;

                    MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    minMaxInfo.ptMinTrackSize.x = (int)(500 * scalingFactor);
                    minMaxInfo.ptMinTrackSize.y = (int)(500 * scalingFactor);
                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;
            }
            WNDPROC oldWndProcDelegate = (WNDPROC)Marshal.GetDelegateForFunctionPointer(oldWndProc, typeof(WNDPROC));
            return PInvoke.CallWindowProc(oldWndProcDelegate, hWnd, Msg, wParam, lParam);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
    }

}
