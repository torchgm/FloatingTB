using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
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

            User32.SetParent(taskbarHandle, localHandle);
            User32.SetParent(startHandle, localHandle);
            
            User32.SetWindowPos(taskbarHandle, IntPtr.Zero, startWidth + taskbarPadding + taskbarHeight + 1, taskbarPadding, 800, taskbarHeight, User32.SetWindowPosFlags.SWP_SHOWWINDOW);
            User32.SetWindowPos(startHandle, IntPtr.Zero, taskbarPadding, taskbarPadding, startWidth, taskbarHeight, 0);
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
        }
    }
}
