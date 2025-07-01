using Common.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels
{
    public class Initialize_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand Init_Command { get; private set; }
        #endregion
        public enum InitializeList
        {
            Out_Handler,
            Bottom_Handler,
            Top_Handler,
            Lift,
            Max
        }
        private bool _BusyStatus;
        public bool BusyStatus
        {
            get { return _BusyStatus; }
            set { SetValue(ref _BusyStatus, value); }
        }
        private string _BusyContent;
        public string BusyContent
        {
            get { return _BusyContent; }
            set { SetValue(ref _BusyContent, value); }
        }
        private bool _SafetyInterlock = false;
        public bool SafetyInterlock
        {
            get { return _SafetyInterlock; }
            set { SetValue(ref _SafetyInterlock, value); }
        }

        private Initialize_Model InitModel;
        private EziDio_Model Dio = SingletonManager.instance.Dio;
        private EzMotion_Model_E Ez_Model = SingletonManager.instance.Motion;
        public ObservableCollection<ServoSlaveViewModel> ServoSlaves { get; set; }
        public Initialize_ViewModel()
        {
            InitModel = new Initialize_Model();

            ServoSlaves = new ObservableCollection<ServoSlaveViewModel>();

            for (int i = 0; i < (int)InitializeList.Max; i++)
            {
                ServoSlaves.Add(new ServoSlaveViewModel()
                {
                    Name = ((InitializeList)i).ToString().Replace("_", " "),
                    Color = "White",
                    SlaveID = i,
                    IsChecked = false
                });
            }
        }
        private async void OnInit_Command(object obj)
        {
            string cmd = obj as string;
            bool result = false;
            switch (cmd)
            {
                case "All":
                    for (int i = 0; i < ServoSlaves.Count; i++)
                        ServoSlaves[i].IsChecked = true;
                    break;
                case "Cancel":
                    for (int i = 0; i < ServoSlaves.Count; i++)
                        ServoSlaves[i].IsChecked = false;
                    break;
                case "Init":
                    if (DoorOpenCheck() == true)
                        break;
                    BusyStatus = true;
                    // Functions
                    string failedSlave = string.Empty;
                    BusyContent = "Initializing Start...";

                    foreach (var slave in ServoSlaves.Where(s => s.IsChecked))
                    {
                        if (slave.Name == "Bottom Handler")
                        {
                            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_2, false);
                            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_UP_SOL, false);

                            if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == true
                                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_LZ_VACUUM_SS] == true
                                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMPING_CV_DETECT_SS_4] == true)
                            {
                                BusyContent = string.Empty;
                                BusyStatus = false;
                                Global.instance.ShowMessagebox("There is a product in the Bottom handler, please remove it and proceed.");
                                return;
                            }
                            BusyContent = "Bottom Handler Initializing...";
                            result = await InitModel.BottomHandlerInit();
                            slave.Color = result ? "LawnGreen" : "White";
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            //slave.IsChecked = false;
                        }
                        // Y,Z Handler Ready 위치 이동
                        else if (slave.Name == "Out Handler")
                        {
                            if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
                            {
                                BusyContent = string.Empty;
                                BusyStatus = false;
                                Global.instance.ShowMessagebox("There is a product in the Z handler, please remove it and proceed.");
                                return;
                            }
                            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_LD_Z_GRIP_SOL, false);
                            BusyContent = "Out Z Initializing...";
                            result = await InitModel.ServoInitZ();
                            slave.Color = result ? "LawnGreen" : "White";
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            if (result == true)
                            {
                                BusyContent = "Out Y Initializing...";
                                result = await InitModel.ServoInitY();
                                slave.Color = result ? "LawnGreen" : "White";
                                if (!result)
                                {
                                    if (!string.IsNullOrEmpty(failedSlave))
                                        failedSlave += ", ";
                                    failedSlave += slave.Name;
                                }
                            }
                            if (!string.IsNullOrEmpty(failedSlave))
                            {
                                string failedMessage = $"Failed to Out Handler Init: {failedSlave}";
                                Global.Mlog.Error(failedMessage);
                                Global.instance.ShowMessagebox(failedMessage);
                            }
                            
                            //slave.IsChecked = false;
                        }
                        
                        else if (slave.Name == "Top Handler")
                        {
                            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);

                            if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS] != true
                                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMPING_CV_DETECT_SS_1] == true)
                            {
                                BusyContent = string.Empty;
                                BusyStatus = false;
                                Global.instance.ShowMessagebox("There is a product in the Top handler, please remove it and proceed.\r\n(탑 핸들 클램프 제거 해주세요)");
                                return;
                            }
                            BusyContent = "Top Handler Initializing...";
                            result = await InitModel.ServoInitX();
                            slave.Color = result ? "LawnGreen" : "White";
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            //slave.IsChecked = false;
                        }
                        else if (slave.Name == "Lift")
                        {
                            BusyContent = "Lift Initializing...";
                            result = await InitModel.LiftInit();
                            slave.Color = result ? "LawnGreen" : "White";
                            //slave.IsChecked = false;
                        }
                    }
                    if (!string.IsNullOrEmpty(failedSlave))
                    {
                        failedSlave += " Initial faile";
                        Global.instance.ShowMessagebox(failedSlave);
                    }
                    else
                    {
                        Global.instance.ShowMessagebox("Initialize Success",false);
                    }
                    BusyContent = string.Empty;
                    BusyStatus = false;
                    break;
            }
        }
       
        private bool DoorOpenCheck()
        {
            // Safety 먼저 체크
            if (!Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.FRONT_DOOR_SS]
            || !Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.REAR_DOOR_SS]
            || !Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.LEFT_L_DOOR_SS]
            || !Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.LEFT_R_DOOR_SS])
            {
                Application.Current.Dispatcher.BeginInvoke(
                                (ThreadStart)(() =>
                                {
                                    // Todo : Interlock Loop Stop. 진행중인 작업 모두 정지
                                    Global.instance.InspectionStop();
                                    // Safety Popup
                                    Window window = new Safety_View();
                                    Safety_ViewModel safety_ViewModel = new Safety_ViewModel();
                                    window.DataContext = safety_ViewModel;
                                    window.ShowDialog();
                                    // Close
                                    safety_ViewModel.Dispose();
                                    safety_ViewModel = null;
                                    window.Close();
                                    window = null;
                                }), DispatcherPriority.Send);

                return true;
            }
            else
                return false;
        }
        #region // override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            Init_Command = new RelayCommand(OnInit_Command);
        }
        protected override void DisposeManaged()
        {
            // ICommands 정리
            Init_Command = null;

            // ServoSlaves 컬렉션 정리
            if (ServoSlaves != null)
            {
                foreach (var slave in ServoSlaves)
                {
                    slave.Dispose(); // ServoSlaveViewModel이 IDisposable을 상속받는 경우
                }
                ServoSlaves.Clear();
                ServoSlaves = null;
            }

            // 부모 클래스의 DisposeManaged 호출
            base.DisposeManaged();
        }
        #endregion
    }
}
