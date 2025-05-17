using Common.Commands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace YJ_AutoClamp.Utils
{
    public class WindowManager
    {
        #region // Singleton

        // Singleton
        private class Nested
        {
            internal static readonly WindowManager Instance = new WindowManager();
        }
        // Singleton
        public static WindowManager Instance
        {
            get { return Nested.Instance; }
        }

        #endregion //Singleton

        public ICommand MoveCommand { get; private set; }
        public ICommand HideCommand { get; private set; }
        public ICommand MinimizeCommand { get; private set; }
        public ICommand MaximizeCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }

        WindowManager()
        {
            HideCommand = new RelayCommand(OnWindowHide);
            MoveCommand = new RelayCommand(OnWindowMove);
            MinimizeCommand = new RelayCommand(OnWindowMinimize);
            MaximizeCommand = new RelayCommand(OnWindowMaximize);
            CloseCommand = new RelayCommand(OnWindowClose);
        }

        const int SW_RESTORE = 9;

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        public void BringProcessToFront(Process process)
        {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
            {
                ShowWindow(handle, SW_RESTORE);
            }

            SetForegroundWindow(handle);
        }
        private void OnWindowClose(object obj)
        {
            var name = obj as string;
            if (string.IsNullOrEmpty(name))
                return;

            var window = App.Current.Windows.OfType<Window>().FirstOrDefault(f => f.Name == name);
            if (window != null)
            {
                if (name == "Main")
                    Application.Current.Shutdown();
                else
                    window.Close();
            }
        }
        private void OnWindowHide(object obj)
        {
            var name = obj as string;
            if (string.IsNullOrEmpty(name))
                return;

            var window = App.Current.Windows.OfType<Window>().FirstOrDefault(f => f.Name == name);
            if (window != null)
            {
                window.Hide();
            }
        }
        private void OnWindowMove(object obj)
        {
            var name = obj as string;
            if (string.IsNullOrEmpty(name))
                return;

            var window = App.Current.Windows.OfType<Window>().FirstOrDefault(f => f.Name == name);
            if (window != null)
            {
                if (Mouse.RightButton == MouseButtonState.Pressed)
                    return;
                window.DragMove();
            }
        }
        private void OnWindowMinimize(object obj)
        {
            var name = obj as string;
            if (string.IsNullOrEmpty(name))
                return;

            var window = App.Current.Windows.OfType<Window>().FirstOrDefault(f => f.Name == name);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
        private void OnWindowMaximize(object obj)
        {
            var name = obj as string;
            if (string.IsNullOrEmpty(name))
                return;

            var window = App.Current.Windows.OfType<Window>().FirstOrDefault(f => f.Name == name);
            if (window != null)
            {
                if(window.WindowState == WindowState.Maximized)
                    window.WindowState = WindowState.Normal;
                else
                    window.WindowState = WindowState.Maximized;
            }
        }
    }
}
