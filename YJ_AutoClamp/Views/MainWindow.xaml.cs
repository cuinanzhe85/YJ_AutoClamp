using System.Windows;
using YJ_AutoClamp.ViewModels;

namespace YJ_AutoClamp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindow_ViewModel();
        }
    }
}