using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;

namespace RogueEssence.Dev.Native
{
    [SupportedOSPlatform("windows")]
    internal class Windows : OS.IBackend
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[] ppszOtherDirs);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll", SetLastError = false)]
        private static extern void ILFree(IntPtr pidl);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder, int cild, IntPtr apidl, int dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        public void SetupApp(AppBuilder builder)
        {
            // Fix drop shadow issue on Windows 10
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            {
                Window.WindowStateProperty.Changed.AddClassHandler<Window>((w, _) => FixWindowFrameOnWin10(w));
                Control.LoadedEvent.AddClassHandler<Window>((w, _) => FixWindowFrameOnWin10(w));
            }
        }

        public void SetupWindow(Window window)
        {
            window.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            window.ExtendClientAreaToDecorationsHint = true;
            window.Classes.Add("fix_maximized_padding");

            Win32Properties.AddWndProcHookCallback(window, (IntPtr hWnd, uint msg, IntPtr _, IntPtr lParam, ref bool handled) =>
            {
                // Custom WM_NCHITTEST
                if (msg == 0x0084)
                {
                    handled = true;

                    if (window.WindowState == WindowState.FullScreen || window.WindowState == WindowState.Maximized)
                        return 1; // HTCLIENT

                    var p = IntPtrToPixelPoint(lParam);
                    GetWindowRect(hWnd, out var rcWindow);

                    var borderThickness = (int)(4 * window.RenderScaling);
                    int y = 1;
                    int x = 1;
                    if (p.X >= rcWindow.left && p.X < rcWindow.left + borderThickness)
                        x = 0;
                    else if (p.X < rcWindow.right && p.X >= rcWindow.right - borderThickness)
                        x = 2;

                    if (p.Y >= rcWindow.top && p.Y < rcWindow.top + borderThickness)
                        y = 0;
                    else if (p.Y < rcWindow.bottom && p.Y >= rcWindow.bottom - borderThickness)
                        y = 2;

                    var zone = y * 3 + x;
                    return zone switch
                    {
                        0 => 13, // HTTOPLEFT
                        1 => 12, // HTTOP
                        2 => 14, // HTTOPRIGHT
                        3 => 10, // HTLEFT
                        4 => 1, // HTCLIENT
                        5 => 11, // HTRIGHT
                        6 => 16, // HTBOTTOMLEFT
                        7 => 15, // HTBOTTOM
                        _ => 17,
                    };
                }

                return IntPtr.Zero;
            });
        }

        private void FixWindowFrameOnWin10(Window w)
        {
            // Schedule the DWM frame extension to run in the next render frame
            // to ensure proper timing with the window initialization sequence
            Dispatcher.UIThread.Post(() =>
            {
                var platformHandle = w.TryGetPlatformHandle();
                if (platformHandle == null)
                    return;

                var margins = new MARGINS { cxLeftWidth = 1, cxRightWidth = 1, cyTopHeight = 1, cyBottomHeight = 1 };
                DwmExtendFrameIntoClientArea(platformHandle.Handle, ref margins);
            }, DispatcherPriority.Render);
        }

        private PixelPoint IntPtrToPixelPoint(IntPtr param)
        {
            var v = IntPtr.Size == 4 ? param.ToInt32() : (int)(param.ToInt64() & 0xFFFFFFFF);
            return new PixelPoint((short)(v & 0xffff), (short)(v >> 16));
        }
    }
}
