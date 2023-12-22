using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Octokit;
using SetupLib;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VK_UI3.DB;
using VK_UI3.Helpers;
using VK_UI3.Views;
using VK_UI3.Views.LoginFrames;
using Windows.ApplicationModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
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

           checkUpdate();
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

        public static DispatcherQueue dispatcherQueue { get; private set; }

        public static Frame contentFrame;

        public void GoLogin()
        {
            
            ContentFrame.Navigate(typeof(Login), this, new DrillInNavigationTransitionInfo());
        }

        








   public delegate IntPtr WinProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);
   private WinProc newWndProc = null;
   private IntPtr oldWndProc = IntPtr.Zero;
   [DllImport("user32")]
   private static extern IntPtr SetWindowLong(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);
   [DllImport("user32.dll")]
   static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);
   private void SubClassing()
   {
       var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

       if (hwnd == IntPtr.Zero)
       {
           int error = Marshal.GetLastWin32Error();
           throw new InvalidOperationException($"Failed to get window handler: error code {error}");
       }

       newWndProc = new(NewWindowProc);

       // Here we use the NativeMethods class 👇
       oldWndProc = NativeMethods.SetWindowLong(hwnd, PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
       if (oldWndProc == IntPtr.Zero)
       {
           int error = Marshal.GetLastWin32Error();
           throw new InvalidOperationException($"Failed to set GWL_WNDPROC: error code {error}");
       }
   }


   public static class NativeMethods
   {
       // We have to handle the 32-bit and 64-bit functions separately.
       // 'SetWindowLongPtr' is the 64-bit version of 'SetWindowLong', and isn't available in user32.dll for 32-bit processes.
       [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
       private static extern IntPtr SetWindowLong32(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);

       [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
       private static extern IntPtr SetWindowLong64(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);

       // This does the selection for us, based on the process architecture.
       public static IntPtr SetWindowLong(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc)
       {
           if (IntPtr.Size == 4) // 32-bit process
           {
               return SetWindowLong32(hWnd, nIndex, newProc);
           }
           else // 64-bit process
           {
               return SetWindowLong64(hWnd, nIndex, newProc);
           }
       }
   }


   int MinWidth = 600;
   int MinHeight = 500;

   [StructLayout(LayoutKind.Sequential)]
   struct MINMAXINFO
   {
       public PInvoke.POINT ptReserved;
       public PInvoke.POINT ptMaxSize;
       public PInvoke.POINT ptMaxPosition;
       public PInvoke.POINT ptMinTrackSize;
       public PInvoke.POINT ptMaxTrackSize;
   }

   private IntPtr NewWindowProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
   {
       switch (Msg)
       {
           case PInvoke.User32.WindowMessage.WM_GETMINMAXINFO:
               var dpi = PInvoke.User32.GetDpiForWindow(hWnd);
               float scalingFactor = (float)dpi / 96;

               MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
               minMaxInfo.ptMinTrackSize.x = (int)(MinWidth * scalingFactor);
               minMaxInfo.ptMinTrackSize.y = (int)(MinHeight * scalingFactor);
               Marshal.StructureToPtr(minMaxInfo, lParam, true);
               break;

       }
       return CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
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
