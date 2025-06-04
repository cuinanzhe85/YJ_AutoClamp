using System;
using System.Windows;

namespace YJ_AutoClamp
{
    /// <summary>
    /// MessageBox_View.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MessageBox_View : Window
    {
        public string TitleContent { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public MessageBox_View(string message, bool isError)
        {
            InitializeComponent();
            this.Message = message;
            this.IsError = isError;
            this.TitleContent = isError ? "Error Message Box" : "Information Message Box";
            this.Name = "Messagebox_" + Guid.NewGuid().ToString("N");
            this.DataContext = this;
        }
    }
}
