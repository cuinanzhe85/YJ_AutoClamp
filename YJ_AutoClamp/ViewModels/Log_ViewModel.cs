using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;

namespace YJ_AutoClamp.ViewModels
{
    public class Log_ViewModel : Child_ViewModel
    {
        #region //// ICommand Property
        #endregion

        private DateTime? _StartDate;
        public DateTime? StartDate
        {
            get { return _StartDate; }
            set 
            { 
                SetValue(ref _StartDate, value);
                UpdateSelectedDates();
            }
        }
        private DateTime? _EndDate;
        public DateTime? EndDate
        {
            get { return _EndDate; }
            set { 
                SetValue(ref _EndDate, value);
                UpdateSelectedDates();
            }
        }
        private ObservableCollection<DateTime> _SelectedDates = new ObservableCollection<DateTime>();
        public ObservableCollection<DateTime> SelectedDates
        {
            get { return _SelectedDates; }
            set { SetValue(ref _SelectedDates, value); }
        }
        public Log_ViewModel() 
        { 
        }
        private void UpdateSelectedDates()
        {
            SelectedDates.Clear();
            if (StartDate.HasValue && EndDate.HasValue)
            {
                for (DateTime date = StartDate.Value; date <= EndDate.Value; date = date.AddDays(1))
                {
                    SelectedDates.Add(date);
                }
            }
        }
        public void SelectedDatesChange(object obj)
        {
            string[] SelectedDates = ((IEnumerable)obj).Cast<object>()
                                 .Select(x => x.ToString())
                                 .OrderBy(x => Convert.ToDateTime(x))
                                 .ToArray();

            for (int i = 0; i < SelectedDates.Count(); i++)
                SelectedDates[i] = Convert.ToDateTime(SelectedDates[i]).ToString("yyyy-MM-dd");

            if(SelectedDates.Count() != 0)
            {
                StartDate = Convert.ToDateTime(SelectedDates[0]);
                EndDate = Convert.ToDateTime(SelectedDates[SelectedDates.Count() - 1]);
            }
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
        }
        protected override void DisposeManaged()
        {
            // SelectedDates 컬렉션 비우기 및 해제
            if (SelectedDates != null)
            {
                SelectedDates.Clear();
                SelectedDates = null;
            }

            base.DisposeManaged();
        }
        #endregion
    }
}
