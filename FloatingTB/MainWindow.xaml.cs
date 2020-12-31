using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        IntPtr taskbarHandle;
        IntPtr startHandle;
        IntPtr localHandle;


        public MainWindow()
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            taskbarHandle = User32.FindWindowEx(User32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null), IntPtr.Zero, "ReBarWindow32", null);
            startHandle = User32.FindWindowEx(User32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null), IntPtr.Zero, "Start", null);
            localHandle = new WindowInteropHelper(this).Handle;
            RECT taskbarRect;
            RECT startRect;
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
    }
}
