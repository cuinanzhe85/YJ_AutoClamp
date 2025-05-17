using Common.Mvvm;
using System.Linq;
using YJ_AutoClamp.ViewModels;

namespace YJ_AutoClamp
{
    /// <summary>
    /// Log_View.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Log_View : BaseUserControl
    {
        public Log_View()
        {
            InitializeComponent();
        }

        private void RadCalendar_SelectionDatesChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Telerik.Windows.Controls.RadCalendar cal = (Telerik.Windows.Controls.RadCalendar)sender;

            (DataContext as Log_ViewModel).SelectedDatesChange(cal.SelectedDates.ToArray());
        }
    }
}
