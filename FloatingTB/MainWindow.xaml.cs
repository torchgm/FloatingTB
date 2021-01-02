using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PInvoke;

namespace FloatingTB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public int taskbarPadding = 1;
        public bool collapsed = false;
        public int taskbarHeight = 0;
        public int taskbarWidth = 0;
        public int startHeight = 0;
        public int startWidth = 0;
        public double expandedWidth = 0;
        public double collapsedWidth = 0;
        public AppBarStates tbMode;

        public IntPtr localHandle;
        public IntPtr hostHandle;
        
        public IntPtr taskbarHandle;
        public RECT taskbarRect;
        public IntPtr startHandle;
        public RECT startRect;


        public MainWindow()
        {
        }

       

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hostHandle = User32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            taskbarHandle = User32.FindWindowEx(hostHandle, IntPtr.Zero, "ReBarWindow32", null);
            startHandle = User32.FindWindowEx(hostHandle, IntPtr.Zero, "Start", null);
            localHandle = new WindowInteropHelper(this).Handle;

            User32.GetWindowRect(taskbarHandle, out taskbarRect);
            User32.GetWindowRect(startHandle, out startRect);

            taskbarHeight = (taskbarRect.bottom - taskbarRect.top);
            //taskbarWidth = taskbarRect.right - taskbarRect.left;
            taskbarWidth = 800;
            startHeight = (startRect.bottom - startRect.top);
            startWidth = startRect.right - startRect.left;

            Height = taskbarHeight + (2 * taskbarPadding);
            Width = startWidth + taskbarWidth + (2 * taskbarPadding + taskbarHeight) + 1;
            
            expandedWidth = Width;
            collapsedWidth = startWidth + Height;
            Width = collapsedWidth;
            InOutAnimation.From = collapsedWidth;
            InOutAnimation.To = expandedWidth;
            tbMode = GetTaskbarState();

            // Copies taskbar window style; currently makes everything worse
            //User32.SetWindowLongPtr(localHandle, User32.WindowLongIndexFlags.GWL_STYLE, new IntPtr(96000000));
            //User32.SetWindowLongPtr(localHandle, User32.WindowLongIndexFlags.GWL_EXSTYLE, new IntPtr(00000088));

            User32.SetParent(taskbarHandle, localHandle);
            User32.SetParent(startHandle, localHandle);

            User32.SetWindowPos(taskbarHandle, IntPtr.Zero, startWidth + taskbarPadding + taskbarHeight + 1, taskbarPadding, 800, taskbarHeight, User32.SetWindowPosFlags.SWP_SHOWWINDOW);
            User32.SetWindowPos(startHandle, IntPtr.Zero, taskbarPadding, taskbarPadding, startWidth, taskbarHeight, 0);
            SetTaskbarState(AppBarStates.AutoHide);
            User32.ShowWindow(hostHandle, User32.WindowShowStyle.SW_HIDE);
        }

        private void PathIcon_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!collapsed)
            {
                TaskbarWindow.Width = TaskbarWindow.Height;
                collapsed = true;
            }
            else
            {
                TaskbarWindow.Width = taskbarWidth + (2 * taskbarPadding + taskbarHeight) + 1;
                collapsed = false;
            }
        }

        private void PathIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TaskbarWindow.DragMove();
        }

        private void InOutAnimation_Completed(object sender, EventArgs e)
        {
            if (Width == expandedWidth)
            {
                InOutAnimation.To = collapsedWidth;
                InOutAnimation.From = expandedWidth;
            }
            else
            {
                InOutAnimation.To = expandedWidth;
                InOutAnimation.From = collapsedWidth;
            }
        }

        private void TaskbarWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            User32.SetWindowPos(startHandle, IntPtr.Zero, taskbarPadding, taskbarPadding, startWidth, taskbarHeight, 0);
        }

        private void TaskbarWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            User32.SetWindowPos(startHandle, IntPtr.Zero, taskbarPadding, taskbarPadding, startWidth, taskbarHeight, 0);
        }

        private void TaskbarWindow_Closing(object sender, EventArgs e)
        {
            //Process.Start("cmd", "/c taskkill /im explorer.exe /f && explorer.exe");
            User32.SetWindowPos(taskbarHandle, IntPtr.Zero, taskbarRect.left, taskbarRect.top, taskbarRect.right - taskbarRect.left, taskbarRect.bottom - taskbarRect.top, 0);
            User32.SetParent(taskbarHandle, hostHandle);
            User32.SetWindowPos(startHandle, IntPtr.Zero, startRect.left, startRect.top, startRect.right - startRect.left, startRect.bottom - startRect.top, 0);
            User32.SetParent(startHandle, hostHandle);
            User32.ShowWindow(hostHandle, User32.WindowShowStyle.SW_SHOW);
            SetTaskbarState(tbMode);
        }














        [DllImport("shell32.dll")]
        static extern IntPtr SHAppBarMessage(uint dwMessage, [In] ref APPBARDATA pData);

        public enum AppBarMessages
        {
            New = 0x00,
            Remove = 0x01,
            QueryPos = 0x02,
            SetPos = 0x03,
            GetState = 0x04,
            GetTaskBarPos = 0x05,
            Activate = 0x06,
            GetAutoHideBar = 0x07,
            SetAutoHideBar = 0x08,
            WindowPosChanged = 0x09,
            SetState = 0x0a
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public UInt32 uCallbackMessage;
            public UInt32 uEdge;
            public Rectangle rc;
            public Int32 lParam;
        }

        public enum AppBarStates
        {
            AutoHide = 0x01,
            AlwaysOnTop = 0x02
        }

        /// <summary>
        /// Set the Taskbar State option
        /// </summary>
        /// <param name="option">AppBarState to activate</param>
        public void SetTaskbarState(AppBarStates option)
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = hostHandle;
            msgData.lParam = (Int32)(option);
            SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
        }

        /// <summary>
        /// Gets the current Taskbar state
        /// </summary>
        /// <returns>current Taskbar state</returns>
        public AppBarStates GetTaskbarState()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = hostHandle;
            return (AppBarStates)SHAppBarMessage((UInt32)AppBarMessages.GetState, ref msgData);
        }

        /// <summary>
        /// Unregisters the taskbar as an appbar
        /// </summary>
        /// <returns>True. Always true.</returns>
        public void MakeTaskbarSad()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = hostHandle;
            SHAppBarMessage((UInt32)AppBarMessages.Remove, ref msgData);
        }
    }
}
