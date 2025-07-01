using System;
using System.Windows;

namespace YJ_AutoClamp
{
    /// <summary>
    /// MessageBox_View.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MessageBoxYesNo_View : Window
    {
        public string TitleContent { get; set; }
        public string Message { get; set; }
        public bool IsColor { get; set; }
        public MessageBoxYesNo_View(string message)
        {
            InitializeComponent();
            this.Message = message;
            this.TitleContent = "Message Box";
            this.Name = "Messagebox_" + Guid.NewGuid().ToString("N");
            this.DataContext = this;
        }
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
