using Common.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Threading;

namespace YJ_AutoClamp.Models
{
    public enum AgingCvList
    {
        Upper1,
        Upper2,
        Upper3,
        Low1,
        Low2,
        Low3,
        Max
    }
    public class Aging_Model : BindableAndDisposable
    {
        public ObservableCollection<AgingSetpList> AgingList { get; set; }
        public Aging_Model()
        {
            AgingList = new ObservableCollection<AgingSetpList>();
            for (int i = 0; i < 28; i++)
            {
                AgingList.Add(new AgingSetpList());
            }
        }

        // Dispose에서 Timer 해제
        protected override void DisposeManaged()
        {

            base.DisposeManaged();
        }
    }
    public class AgingSetpList : BindableAndDisposable
    {
        private ObservableCollection<bool> _Floor;
        public ObservableCollection<bool> Floor
        {
            get { return _Floor; }
            set { SetValue(ref _Floor, value); }
        }
        Stopwatch _AgingTime;
        public AgingSetpList()
        {
            Floor = new ObservableCollection<bool>();
            for (int i = 0; i < (int)Floor_Index.Max; i++)
                Floor.Add(true);

            _AgingTime = new Stopwatch();
        }
    }
}
