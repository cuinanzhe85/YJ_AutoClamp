using Common.Commands;
using Common.Managers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YJ_AutoClamp.ViewModels;

namespace YJ_AutoClamp
{
    /// <summary>
    /// TcpSerial_View.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Number_View : Window
    {
        public string ResultPassword { get; private set; } = "";
        public Number_View()
        {
            InitializeComponent();
        }
        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                InputLable.Content += button.Content.ToString();
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            string Input = InputLable.Content.ToString();
            if (!string.IsNullOrEmpty(Input) && Input.Length > 0)
            {
                InputLable.Content = Input.Substring(0, Input.Length - 1);
            }
        }
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            ResultPassword = InputLable.Content.ToString();
            DialogResult = true; // 창 종료
        }
    }
}
