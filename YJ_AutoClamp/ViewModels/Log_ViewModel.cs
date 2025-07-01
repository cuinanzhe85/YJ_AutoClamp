using Common.Commands;
using Common.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;

namespace YJ_AutoClamp.ViewModels
{
    public class LogItem
    {
        public DateTime LocalTime { get; set; }
        public string ErrorName { get; set; }
    }

    public class Log_ViewModel : Child_ViewModel
    {
        #region // ICommand Property
        public ICommand Database_Command { get; private set; }
        #endregion
        public ObservableCollection<LogItem> LogItems { get; set; } = new ObservableCollection<LogItem>();
        private string BusyIndicatorProgressString { get; set; } = string.Empty;
        private int BusyIndicatorProgressValue { get; set; } = 0;

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
        public void LoadDatabase_LogItems()
        {
            if (!StartDate.HasValue || !EndDate.HasValue)
                return;

            Global.instance.BusyStatus = true;
            LogItems.Clear();

            var dateStrings = SelectedDates.Select(d => d.ToString("yyyyMMdd")).ToArray();

            var tempList = new List<LogItem>();
            for (int i = 0; i < dateStrings.Length; i++)
            {
                BusyIndicatorProgressString = $"Loading Result Datas ({dateStrings[i]})... ";
                BusyIndicatorProgressValue = 0;
               // Global.instance.BusyContent = BusyIndicatorProgressString + BusyIndicatorProgressValue + " %";

                try
                {
                    string path = Global.instance.AlarmLogPath + $@"\{dateStrings[i]}.txt";
                    var lines = File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        // "yyyyMMdd HH:mm:ss:fff,message" 형식
                        int commaIdx = line.IndexOf(',');
                        if (commaIdx > 0)
                        {
                            string timeStr = line.Substring(0, commaIdx);
                            string msg = line.Substring(commaIdx + 1);
                            var item = new LogItem
                            {
                                LocalTime = DateTime.ParseExact(timeStr, "yyyyMMdd HH:mm:ss:fff", null),
                                ErrorName = msg
                            };
                            tempList.Add(item);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Global.ExceptionLog.Info($"Error loading log file {dateStrings[i]}: {ex.Message}");
                    continue;
                }
                // UI 스레드에서 한 번에 추가
                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in tempList)
                        LogItems.Add(item);
                });

                tempList.Clear();

            }
            // 임시 리스트 해제
            tempList.Clear();
            tempList = null;
            BusyIndicatorProgressString = "Complete";
            BusyIndicatorProgressValue = 100;
            Global.instance.BusyStatus = false;

        }
        private void OnDatabase_Command(object obj)
        {
            string cmd = obj.ToString();
            switch (cmd)
            {
                case "Search":
                    LoadDatabase_LogItems();
                    break;
                case "Clear":
                    LogItems.Clear();
                    break;
                case "Today":
                case "7Day":
                case "Month":
                    SetDateRange(cmd);
                    break;
                case "Csv":
                    ExportToCsv();
                    break;
            }
        }
        private void SetDateRange(string type)
        {
            var today = DateTime.Today;
            switch (type)
            {
                case "Today":
                    StartDate = today;
                    EndDate = today;
                    break;
                // 오늘 포함 7일
                case "7Day":
                    StartDate = today.AddDays(-6);
                    EndDate = today;
                    break;
                case "Month":
                    StartDate = new DateTime(today.Year, today.Month, 1);
                    EndDate = today;
                    break;
                default:
                    break;
            }
            LoadDatabase_LogItems();
        }
        private void ExportToCsv()
        {
            if (LogItems == null || LogItems.Count == 0)
                return;

            // 저장할 파일 경로 선택
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV 파일 (*.csv)|*.csv",
                FileName = $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            if (dialog.ShowDialog() != true)
                return;

            var filePath = dialog.FileName;

            try
            {
                using (var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    // 헤더
                    writer.WriteLine("DATE,C/N,ERROR_NAME");
                    foreach (var item in LogItems)
                    {
                        var line = string.Format("\"{0}\",\"{1}\",\"{2}\"",
                            item.LocalTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            item.ErrorName
                        );
                        writer.WriteLine(line);
                    }
                }
                Global.instance.ShowMessagebox("CSV File Save Complete.", false);
            }
            catch (Exception ex)
            {
                Global.instance.ShowMessagebox("CSV File Save Fail : " + ex.Message);
            }
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            Database_Command = new RelayCommand(OnDatabase_Command);
        }
        protected override void DisposeManaged()
        {
            Database_Command = null;
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
