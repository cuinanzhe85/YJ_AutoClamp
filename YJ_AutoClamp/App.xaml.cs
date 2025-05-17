using System.Threading;
using System;
using System.Windows;

namespace YJ_AutoClamp
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            string mutexName = "Auto Clalmping";
            bool isCreatedNew = false;
            try
            {
                _mutex = new Mutex(true, mutexName, out isCreatedNew);

                if (isCreatedNew)
                {
                    base.OnStartup(e);
                }
                else
                {
                    MessageBox.Show("[ Auto Clamping ] Application already started.", "Software Already Run", MessageBoxButton.OK, MessageBoxImage.Information);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace + "\n\n" + "Application Existing...", "Exception thrown");
                Environment.Exit(0);
            }
        }
    }
}
