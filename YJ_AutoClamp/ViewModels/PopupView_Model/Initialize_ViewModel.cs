using Common.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YJ_AutoClamp.Models;
using static YJ_AutoClamp.Models.MESSocket;

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
        private EziDio_Model Dio = SingletonManager.instance.Ez_Dio;
        private EzMotion_Model_E Ez_Model = SingletonManager.instance.Ez_Model;
        public ObservableCollection<ServoSlaveViewModel> ServoSlaves { get; set; }
        public Initialize_ViewModel()
        {
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
                    DoorOpenCheck();
                    BusyStatus = true;
                    // Functions
                    string failedSlave = string.Empty;
                    BusyContent = "Initializing Start...";

                    foreach (var slave in ServoSlaves.Where(s => s.IsChecked))
                    {
                        // Y,Z Handler Ready 위치 이동
                        if (slave.Name == "Out Handler")
                        {
                            if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
                            {
                                BusyContent = string.Empty;
                                BusyStatus = false;
                                Global.instance.ShowMessagebox("There is a product in the Z handler, please remove it and proceed.");
                                return;
                            }
                            BusyContent = "Out Z Initializing...";
                            result = await ServoInitZ();
                            slave.Color = result ? "Bisque" : "White";
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            if (result == true)
                            {
                                BusyContent = "Out Y Initializing...";
                                result = await ServoInitY();
                                slave.Color = result ? "Bisque" : "White";
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
                            // Step 초기화 설정
                            SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
                            slave.IsChecked = false;
                        }
                        else if (slave.Name == "Bottom Handler")
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
                            result = await BottomHandlerInit();
                            slave.Color = result ? "Bisque" : "White";
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            slave.IsChecked = false;
                        }
                        else if (slave.Name == "Top Handler")
                        {
                            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                            if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS] == true
                                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMPING_CV_DETECT_SS_1] == true)
                            {
                                BusyContent = string.Empty;
                                BusyStatus = false;
                                Global.instance.ShowMessagebox("There is a product in the Top handler, please remove it and proceed.");
                                return;
                            }
                            BusyContent = "Top Handler Initializing...";
                            result = await ServoInitX();
                            slave.Color = result ? "Bisque" : "White";
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            slave.IsChecked = false;
                        }
                    }
                    if (!string.IsNullOrEmpty(failedSlave))
                    {
                        failedSlave += " Initial faile";
                        Global.instance.ShowMessagebox(failedSlave);
                    }
                    BusyContent = string.Empty;
                    BusyStatus = false;
                    break;
            }
        }
        private async Task<bool> ServoInitY()
        {
            if (Ez_Model.IsOutHandlerReadyDoneZ() == false)
            {
                Global.instance.ShowMessagebox("Y initialization failed. Move the Z axis to Ready position.");
                return false; // 실패 시 false 반환
            }
            if (Ez_Model.MoveOutHandlerReadyY()== false)
                return false; // 실패 시 false 반환
            bool result = false;
            await Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                while (true)
                {
                    if (Ez_Model.IsMoveOutHandlerReadyY() == true)
                    {
                        result = true;
                        break; // 성공 시 루프 종료
                    }
                    if (sw.ElapsedMilliseconds > 10000)
                    {
                        result = false; // 10초 후에 중단
                        break; // 10초 후에 중단
                    }
                    Task.Delay(100).Wait();
                }
            });
            // Ready Position까지 이동했으면 
            //if (result == true)
            //{
            //    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
            //    {
            //        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Out_Handle_Grip_Check;
            //    }
            //    else
            //    {
            //        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
            //    }
            //}
            return result; // 성공 여부 반환
        }
        private async Task<bool> ServoInitZ()
        {
            if (Ez_Model.MoveOutHandlerRadyZ() == false)
                return false; // 실패 시 false 반환
            bool result = false;
            await Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                while (true)
                {
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true)
                    {
                        result = true;
                        break; // 성공 시 루프 종료
                    }
                    if (sw.ElapsedMilliseconds > 10000)
                    {
                        result = false; // 10초 후에 중단
                        break; // 10초 후에 중단
                    }
                    Task.Delay(100).Wait();
                }
            });
            // Ready Position까지 이동했으면 
            //if (result == true)
            //{
            //    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
            //    {
            //        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Out_Handle_Grip_Check;
            //    }
            //    else
            //    {
            //        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
            //    }
            //}
            return result; // 성공 여부 반환
        }
        private async Task<bool> ServoInitX()
        {
            if (Ez_Model.IsOutHandlerPickupPosY() == true)
            {
                Global.instance.ShowMessagebox("X initialization failed. Move the Y axis to Ready position.");
                return false; // 실패 시 false 반환
            }
            
            bool result = false;
            await Task.Run(() =>
            {
                
                Stopwatch sw = new Stopwatch();
                
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                sw.Restart();
                while (true)
                {
                    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                    {
                        result = true;
                        break; // 핸들러가 Down 상태로 이동했으면 루프 종료
                    }
                    if (sw.ElapsedMilliseconds > 3000)
                    {
                        Global.instance.ShowMessagebox("Top Handler Up failed.");
                        result = false;
                        break; // 3초 후에 중단
                    }
                }
                if (result == true)
                {
                    if (Ez_Model.MoveTopHandlerPickUpPos() == false)
                    {
                        result = false; // 실패 시 false 반환
                    }
                    sw.Restart();
                    while (true)
                    {
                        if (Ez_Model.IsTopHandlerPickUpPos() == true)
                        {
                            result = true;
                            break; // 성공 시 루프 종료
                        }
                        if (sw.ElapsedMilliseconds > 5000)
                        {
                            result = false; // 10초 후에 중단
                            break; // 10초 후에 중단
                        }
                        Task.Delay(100).Wait();
                    }
                }
            });
            // Ready Position까지 이동했으면 
            SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Top_Handle_Step = Unit_Model.TopHandle.Idle;
            SingletonManager.instance.IsY_PickupColl = false;

            return result; // 성공 여부 반환
        }
        private async Task<bool> BottomHandlerInit()
        {
            bool result = false;
           
            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_VACUUM_SOL, false);
            await Task.Run(() =>
            {
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                Stopwatch sw = new Stopwatch();
                sw.Restart();

                while (true)
                {
                    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true)
                    {
                        result = true;
                        break; // 핸들러가 Up 상태로 이동했으면 루프 종료
                    }
                    if (sw.ElapsedMilliseconds > 3000)
                    {
                        result = false;
                        Global.instance.ShowMessagebox("Bottom Handler Up failed.");
                    }
                    Task.Delay(100).Wait(); // 100ms 대기
                }
                if (result == true)
                {
                    // Bottom Handler Left Move
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_FORWARD_SOL, true);
                    sw.Restart();
                    while (true)
                    {
                        if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true)
                        {
                            result = true;
                            break; // 핸들러가 Up 상태로 이동했으면 루프 종료
                        }
                        if (sw.ElapsedMilliseconds > 3000)
                        {
                            result = false;
                            Global.instance.ShowMessagebox("Bottom Handler Left move failed.");
                            break;
                        }
                        Task.Delay(100).Wait(); // 100ms 대기
                    }
                }
            });
            SingletonManager.instance.BottomClampDone = false;
            SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Bottom_Step = Unit_Model.BottomHandle.Idle;
            return result; // 성공 여부 반환
        }
        private void DoorOpenCheck()
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
            }
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
