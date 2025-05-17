using Common.Commands;
using Common.Mvvm;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows.Input;

namespace YJ_AutoClamp.ViewModels
{
    public class DioDisplayData : BindableAndDisposable
    {
        private string _Address;
        public string Address
        {
            get { return _Address; }
            set { SetValue(ref _Address, value); }
        }
        private string _Label;
        public string Label
        {
            get { return _Label; }
            set { SetValue(ref _Label, value); }
        }
        private bool? _Status;
        public bool? Status
        {
            get { return _Status; }
            set { SetValue(ref _Status, value); }
        }
        public int Index { get; set; }

        protected override void DisposeManaged()
        {
            Address = null;
            Label = null;
            Status = null;
            base.DisposeManaged();
        }
    }
    public class Dio_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand PageButton_Command { get; private set; }
        public ICommand OutputToggle_Command { get; private set; }

        public int InputLastpage { get; set; } = (SingletonManager.instance.Ez_Dio.Dio_InputCount / 16) + 1;
        public int OutputLastpage { get; set; } = (SingletonManager.instance.Ez_Dio.Dio_OutputCount / 16) + 1;
        public int[] DioRange { get; set; } = { SingletonManager.instance.Ez_Dio.Dio_InputCount, SingletonManager.instance.Ez_Dio.Dio_OutputCount };
        #endregion
        private int _InputCurrentPage = 1;
        public int InputCurrentPage
        {
            get { return _InputCurrentPage; }
            set { SetValue(ref _InputCurrentPage, value); }
        }
        private int _OutputCurrentPage = 1;
        public int OutputCurrentPage
        {
            get { return _OutputCurrentPage; }
            set { SetValue(ref _OutputCurrentPage, value); }
        }
        private ObservableCollection<DioDisplayData> _DisplayedInputData = new ObservableCollection<DioDisplayData>();
        public ObservableCollection<DioDisplayData> DisplayedInputData
        {
            get { return _DisplayedInputData; }
            set { SetValue(ref _DisplayedInputData, value); }
        }
        private ObservableCollection<DioDisplayData> _DisplayedOutputData = new ObservableCollection<DioDisplayData>();
        public ObservableCollection<DioDisplayData> DisplayedOutputData
        {
            get { return _DisplayedOutputData; }
            set { SetValue(ref _DisplayedOutputData, value); }
        }
        private Timer UupdateTimer;
        public Dio_ViewModel()
        {
            for (int i = 0; i < 16; i++)
            {
                DisplayedInputData.Add(new DioDisplayData());
                DisplayedOutputData.Add(new DioDisplayData());
            }

            UupdateTimer_Initialize();
            UpdateInputPageDisplay();
            UpdateOutputPageDisplay();
        }
        private void UupdateTimer_Initialize()
        {
            UupdateTimer = new Timer(1000);
            UupdateTimer.Elapsed += OnTimerElapsed;
            UupdateTimer.AutoReset = true;
            UupdateTimer.Start();
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateStatus();
        }
        private void UpdateStatus()
        {
            for (int i = 0; i < DisplayedInputData.Count; i++)
            {
                if (DisplayedInputData[i].Status != SingletonManager.instance.Ez_Dio.DI_RAW_DATA[(InputCurrentPage - 1) * 16 + i])
                    DisplayedInputData[i].Status = SingletonManager.instance.Ez_Dio.DI_RAW_DATA[(InputCurrentPage - 1) * 16 + i];
            }
            for (int i = 0; i < DisplayedOutputData.Count; i++)
            {
                if (DisplayedOutputData[i].Status != SingletonManager.instance.Ez_Dio.DO_RAW_DATA[(OutputCurrentPage - 1) * 16 + i])
                    DisplayedOutputData[i].Status = SingletonManager.instance.Ez_Dio.DO_RAW_DATA[(OutputCurrentPage - 1) * 16 + i];
            }
        }
        private void UpdateInputPageDisplay()
        {
            int startIndex = (InputCurrentPage - 1) * 16;

            for (int i = 0; i < 16; i++)
            {
                // 기존 객체 업데이트
                DisplayedInputData[i].Address = SingletonManager.instance.Ez_Dio.Input_Address[startIndex + i];
                DisplayedInputData[i].Label = SingletonManager.instance.Ez_Dio.Input_Label[startIndex + i];
                DisplayedInputData[i].Status = SingletonManager.instance.Ez_Dio.DI_RAW_DATA[startIndex + i];
            }
        }
        private void UpdateOutputPageDisplay()
        {
            int startIndex = (OutputCurrentPage - 1) * 16;

            for (int i = 0; i < 16; i++)
            {
                // 기존 객체의 데이터 갱신
                DisplayedOutputData[i].Address = SingletonManager.instance.Ez_Dio.Output_Address[startIndex + i];
                DisplayedOutputData[i].Label = SingletonManager.instance.Ez_Dio.Output_Label[startIndex + i];
                DisplayedOutputData[i].Status = SingletonManager.instance.Ez_Dio.DO_RAW_DATA[startIndex + i];
                DisplayedOutputData[i].Index = startIndex + i;
            }
        }
        private void OnPageButton_Command(object obj)
        {
            string command = obj.ToString();
            // Left Button Command
            if (command.Contains("_L"))
            {
                switch (command.ToString())
                {
                    case "Prev_L":
                        if (InputCurrentPage == 1)
                            return;
                        InputCurrentPage--;
                        break;
                    case "Next_L":
                        if (InputCurrentPage >= InputLastpage)
                            return;
                        InputCurrentPage++;
                        break;
                    case "First_L":
                        if (InputCurrentPage == 1)
                            return;
                        InputCurrentPage = 1;
                        break;
                    case "Last_L":
                        if (InputCurrentPage >= InputLastpage)
                            return;
                        InputCurrentPage = InputLastpage;
                        break;
                }
                UpdateInputPageDisplay();
            }
            // Right Button Command
            else
            {
                switch (command.ToString())
                {
                    case "Prev_R":
                        if (OutputCurrentPage == 1)
                            return;
                        OutputCurrentPage--;
                        break;
                    case "Next_R":
                        if (OutputCurrentPage >= OutputLastpage)
                            return;
                        OutputCurrentPage++;
                        break;
                    case "First_R":
                        if (OutputCurrentPage == 1)
                            return;
                        OutputCurrentPage = 1;
                        break;
                    case "Last_R":
                        if (OutputCurrentPage >= OutputLastpage)
                            return;
                        OutputCurrentPage = OutputLastpage;
                        break;
                }
                UpdateOutputPageDisplay();
            }
        }
        private void OnOutputToggle_Command(object parameter)
        {
            int startIndex = (OutputCurrentPage - 1) * 16;
            int index = (int)parameter + startIndex;
            if (SingletonManager.instance.Ez_Dio.DO_RAW_DATA[index] == false)
                SingletonManager.instance.Ez_Dio.SetIO_OutputData(index, true);
            else
                SingletonManager.instance.Ez_Dio.SetIO_OutputData(index, false);
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            PageButton_Command = new RelayCommand(OnPageButton_Command);
            OutputToggle_Command = new RelayCommand(OnOutputToggle_Command);
        }
        protected override void DisposeManaged()
        {
            // Dispose PageButton_Command
            PageButton_Command = null;
            OutputToggle_Command = null;

            // Dispose Timer
            if (UupdateTimer != null)
            {
                UupdateTimer.Stop();
                UupdateTimer.Elapsed -= OnTimerElapsed;
                UupdateTimer.Dispose();
                UupdateTimer = null;
            }

            // Dispose DisplayedInputData
            foreach (var item in DisplayedInputData)
                item.Dispose();
            DisplayedInputData.Clear();
            DisplayedInputData = null;

            // Dispose DisplayedOutputData
            foreach (var item in DisplayedOutputData)
                item.Dispose();
            DisplayedOutputData.Clear();
            DisplayedOutputData = null;

            base.DisposeManaged();
        }
        #endregion
    }
}
