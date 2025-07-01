using Common.Commands;
using Common.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YJ_AutoClamp.Models;
using static YJ_AutoClamp.Models.EziDio_Model;

namespace YJ_AutoClamp.ViewModels
{
    public class ServoSlaveViewModel : BindableAndDisposable
    {
        public string Name { get; set; }
        public int SlaveID { get; set; }

        private string _Color;
        public string Color
        {
            get { return _Color; }
            set { SetValue(ref _Color, value); }
        }
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { SetValue(ref _IsChecked, value); }
        }
    }
    public class Origin_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand Servo_Command { get; private set; }
        #endregion
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
        public ObservableCollection<ServoSlaveViewModel> ServoSlaves { get; set; }
        public EzMotion_Model_E Motion = SingletonManager.instance.Motion;
        public EziDio_Model Dio = SingletonManager.instance.Dio;
        public Origin_ViewModel()
        {
            ServoSlaves = new ObservableCollection<ServoSlaveViewModel>();

            for (int i = 0; i < (int)ServoSlave_List.Max; i++)
            {
                if (i != (int)ServoSlave_List.Top_CV_X)
                {
                    ServoSlaves.Add(new ServoSlaveViewModel()
                    {
                        Name = ((ServoSlave_List)i).ToString().Replace("_", " "),
                        Color = "White",
                        SlaveID = i,
                        IsChecked = false
                    });
                }
            }
        }
        private async void OnServo_Command(object obj)
        {
            string cmd = obj as string;
            bool result = false;
            switch (cmd)
            {
                case "All":
                    for (int i = 0; i < ServoSlaves.Count; i++)
                    {
                        ServoSlaves[i].IsChecked = true;
                    }
                    break;
                case "On":
                    foreach (var slave in ServoSlaves.Where(s => s.IsChecked))
                    {
                        result = Motion.SetServoOn(slave.SlaveID, true);
                        slave.Color = result ? "Bisque" : "White";
                        //slave.IsChecked = false;
                    }
                    if (SingletonManager.instance.Servo_Model[(int)ServoSlave_List.Top_CV_X].IsServoOn == false)
                        Motion.SetServoOn((int)ServoSlave_List.Top_CV_X, true);
                    break;
                case "Off":
                    string failedSlaves = string.Empty;
                    foreach (var slave in ServoSlaves.Where(s => s.IsChecked))
                    {
                        result = Motion.SetServoOn(slave.SlaveID, false);
                        if (!result)
                        {
                            if (!string.IsNullOrEmpty(failedSlaves))
                                failedSlaves += ", ";
                            failedSlaves += slave.Name;
                        }
                        //slave.IsChecked = false;
                    }
                    if (!string.IsNullOrEmpty(failedSlaves))
                    {
                        string failedMessage = $"Failed to turn off the following Servo: {failedSlaves}";
                        Global.Mlog.Error(failedMessage);
                        Global.instance.ShowMessagebox(failedMessage);
                    }
                    break;
                case "AlarmReset":
                    string failedSlave = string.Empty;
                    foreach (var slave in ServoSlaves.Where(s => s.IsChecked))
                    {
                        result = Motion.ServoAlarmReset(slave.SlaveID);
                        if (!result)
                        {
                            if (!string.IsNullOrEmpty(failedSlave))
                                failedSlave += ", ";
                            failedSlave += slave.Name;
                        }
                        //slave.IsChecked = false;
                    }
                    if (!string.IsNullOrEmpty(failedSlave))
                    {
                        string failedMessage = $"Failed to Alam Reset the following Servo: {failedSlave}";
                        Global.Mlog.Error(failedMessage);
                        Global.instance.ShowMessagebox(failedMessage);
                    }
                    break;
                case "Origin":
                    if (DoorOpenCheck() == true)
                        break;

                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_2, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_UP_SOL, false);

                    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == true
                        || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_LZ_VACUUM_SS] == true
                        || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMPING_CV_DETECT_SS_4] == true)
                    {
                        BusyContent = string.Empty;
                        BusyStatus = false;
                        Global.instance.ShowMessagebox("There is a product in the Bottom handler, please remove it and proceed.\r\n(바텀 핸들 세트/클램프 제거 해주세요)");
                        return;
                    }
                    SingletonManager.instance.BottomClampDone = false;
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Bottom_Step = Unit_Model.BottomHandle.Idle;
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.In_CV].TopCvStep = Unit_Model.Top_CV.Idle;

                    BusyStatus = true;
                    // 선택된 슬레이브 필터링
                    var selectedSlaves = ServoSlaves.Where(slave => slave.IsChecked).ToList();
                    var failedSlavesList = new List<string>();
                    SingletonManager.instance.Dio.Set_HandlerUpDown(true);

                    // Servo Origin
                    var Slave = ServoSlaves[(int)ServoSlave_List.Out_Z_Handler_Z];
                    if (Slave.IsChecked == true)
                    {
                        if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
                        {
                            BusyContent = string.Empty;
                            BusyStatus = false;
                            Global.instance.ShowMessagebox("There is a product in the Z handler, please remove it and proceed.\r\n(로딩 핸들 클램프 제거 해주세요)");
                            return;
                        }
                        BusyContent = $"Please Wait. Now Servo Origin...{Slave.Name}";
                        result = await Motion.ServoOrigin(Slave.SlaveID);
                        Slave.Color = result ? "LawnGreen" : "White";
                        //Slave.IsChecked = false;
                        if (!result)
                        {
                            failedSlavesList.Add(Slave.Name);
                        }
                        // Step 초기화 설정
                        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
                    }
                    
                    Slave = ServoSlaves[(int)ServoSlave_List.Top_X_Handler_X];
                    if (Slave.IsChecked == true)
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                        if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS] == true
                            || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMPING_CV_DETECT_SS_1] == true)
                        {
                            BusyContent = string.Empty;
                            BusyStatus = false;
                            Global.instance.ShowMessagebox("There is a product in the Top handler, please remove it and proceed.\r\n(탑 핸들 클램프 제거 해주세요)");
                            return;
                        }
                        BusyContent = $"Please Wait. Now Servo Origin...{Slave.Name}";
                        result = await Motion.ServoOrigin(Slave.SlaveID);
                        Slave.Color = result ? "LawnGreen" : "White";
                        //Slave.IsChecked = false;
                        if (!result)
                        {
                            failedSlavesList.Add(Slave.Name);
                        }
                        // Ready Position까지 이동했으면 
                        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Top_Handle_Step = Unit_Model.TopHandle.Idle;
                        SingletonManager.instance.IsY_PickupColl = false;
                    }
                        
                    Slave = ServoSlaves[(int)ServoSlave_List.Out_Y_Handler_Y];
                    if (Slave.IsChecked == true)
                    {
                        if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
                        {
                            BusyContent = string.Empty;
                            BusyStatus = false;
                            Global.instance.ShowMessagebox("There is a product in the Z handler, please remove it and proceed.\r\n(로딩 핸들 클램프 제거 해주세요)");
                            return;
                        }
                        BusyContent = $"Please Wait. Now Servo Origin...{Slave.Name}";
                        result = await Motion.ServoOrigin(Slave.SlaveID);
                        Slave.Color = result ? "LawnGreen" : "White";
                        //Slave.IsChecked = false;
                        if (!result)
                        {
                            failedSlavesList.Add(Slave.Name);
                        }
                        // Step 초기화 설정
                        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
                        SingletonManager.instance.IsY_PickupColl = false;
                    }
                    
                    if (selectedSlaves.Any())
                    {
                        // 병렬로 작업 실행
                        var tasks = selectedSlaves.Select(async slave =>
                        {
                            if (slave.SlaveID == (int)ServoSlave_List.Lift_1_Z
                            || slave.SlaveID == (int)ServoSlave_List.Lift_2_Z
                            || slave.SlaveID == (int)ServoSlave_List.Lift_3_Z)
                            {
                                if (slave.IsChecked == true)
                                {
                                    BusyContent = $"Please Wait. Now Servo Origin...{slave.Name}";
                                    result = await Motion.ServoOrigin(slave.SlaveID);
                                    slave.Color = result ? "LawnGreen" : "White";
                                    //slave.IsChecked = false;
                                    if (!result)
                                    {
                                        failedSlavesList.Add(slave.Name);
                                    }
                                    else
                                    {
                                        Motion.MoveLiftLoding(slave.SlaveID); // 로딩 위치로 이동
                                    }
                                }
                            }
                        });
                        // 모든 작업 완료 대기
                        await Task.WhenAll(tasks);
                        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Lift_1].AgingCVStep = Unit_Model.Aging_CV_Step.Idle;
                    }
                    // 실패한 슬레이브가 있는 경우 메시지 표시
                    if (failedSlavesList.Any())
                    {
                        string failedMessage = $"Failed to complete origin operation for the following Servo(s): {string.Join(", ", failedSlavesList)}";
                        Global.Mlog.Error(failedMessage);
                        Global.instance.ShowMessagebox(failedMessage);
                    }
                    else
                    {
                        Global.instance.ShowMessagebox("Origin Success", false);
                    }

                    BusyContent = string.Empty;
                    BusyStatus = false;
                    break;
                case "Connect":
                    BusyStatus = true;
                    BusyContent = "Ez Connect Start";
                    string error = "";
                    await Task.Run(async () =>
                    {
                        BusyContent = "Ez Motion Connect ...";
                        for (int i = 0; i < (int)ServoSlave_List.Max; i++)
                        {
                            BusyContent = $"Ez Motion Slave {i}";
                            Motion.Close(i);
                            await Task.Delay(1000); // 잠시 대기
                            
                            if (Motion.Connect(i) == false)
                            {
                                if (string.IsNullOrEmpty(error) == false)
                                    error += ", ";
                                error += (ServoSlave_List.Out_Y_Handler_Y + i).ToString();
                            }
                        }
                        BusyContent = "Ez DIO Connect ...";
                        for (int i = 0; i < (int)EziDio_Model.DI_MAP.DI_MAX / 16; i++)
                        {
                            BusyContent = $"Ez DIO Slave {Dio_Slave.DIO_1 + i}";
                            Dio.Close(i);
                            await Task.Delay(1000); // 잠시 대기
                            if (Dio.Connect(i) == false)
                            {
                                if (string.IsNullOrEmpty(error) == false)
                                    error += ", ";
                                error += $"Dio Slave {Dio_Slave.DIO_1 + i}";
                            }
                        }
                    });
                    
                    Global.instance.BusyStatus = false;
                    Global.instance.BusyContent = string.Empty;
                    if (string.IsNullOrEmpty(error) == false)
                    {
                        error += " Ez Connect Fail";
                        Global.instance.ShowMessagebox(error);
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
            return false;
        }
        #region // override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            Servo_Command = new RelayCommand(OnServo_Command);
        }
        protected override void DisposeManaged()
        {
            // ICommands 정리
            Servo_Command = null;

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
