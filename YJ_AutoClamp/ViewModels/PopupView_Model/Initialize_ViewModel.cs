using Common.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using YJ_AutoClamp.Models;
using static YJ_AutoClamp.Models.MESSocket;

namespace YJ_AutoClamp.ViewModels
{
    public class Initialize_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand Init_Command { get; private set; }
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
        private EziDio_Model Dio = SingletonManager.instance.Ez_Dio;
        private EzMotion_Model_E Ez_Model = SingletonManager.instance.Ez_Model;
        public ObservableCollection<ServoSlaveViewModel> ServoSlaves { get; set; }
        public Initialize_ViewModel()
        {
            ServoSlaves = new ObservableCollection<ServoSlaveViewModel>();

            for (int i = 0; i < (int)ServoSlave_List.Max; i++)
            {
                if (i != (int)ServoSlave_List.Top_CV_X
                    && i != (int)ServoSlave_List.Lift_1_Z
                    && i != (int)ServoSlave_List.Lift_2_Z
                    && i != (int)ServoSlave_List.Lift_3_Z)
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
                    BusyStatus = true;
                    // Functions
                    string failedSlave = string.Empty;
                    BusyContent = "Initializing servos...";

                    // 핸들러 Up
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                    await Task.Delay(1000); // 1초 대기
                    // Bottom Handler Left Move
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_FORWARD_SOL, true);

                    foreach (var slave in ServoSlaves.Where(s => s.IsChecked))
                    {
                        if (slave.Name == "Out Y handler Y")
                        {
                            result = await ServoInitY();
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            slave.IsChecked = false;
                        }
                        else if (slave.Name == "Out Handler Z")
                        {
                            result = await ServoInitZ();
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            slave.IsChecked = false;
                        }
                        else if (slave.Name == "Top X Handler X")
                        {
                            result = await ServoInitX();
                            if (!result)
                            {
                                if (!string.IsNullOrEmpty(failedSlave))
                                    failedSlave += ", ";
                                failedSlave += slave.Name;
                            }
                            slave.IsChecked = false;
                        }
                    }
                    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == true)
                    {
                        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Bottom_Step = Unit_Model.BottomHandle.Bottom_PutDown_Down;
                    }
                    else
                    {
                        SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Bottom_Step = Unit_Model.BottomHandle.Idle;
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
            if (result == true)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
                {
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Out_Handle_Grip_Check;
                }
                else
                {
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
                }
            }
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
            if (result == true)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
                {
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Out_Handle_Grip_Check;
                }
                else
                {
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
                }
            }
            return result; // 성공 여부 반환
        }
        private async Task<bool> ServoInitX()
        {
            if (Ez_Model.IsOutHandlerPickupPosY() == true)
            {
                Global.instance.ShowMessagebox("X initialization failed. Move the Y axis to Ready position.");
                return false; // 실패 시 false 반환
            }
            if (Ez_Model.MoveTopHandlerPickUpPos() == false)
            {
                return false; // 실패 시 false 반환
            }
            bool result = false;
            await Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                while (true)
                {
                    if (Ez_Model.IsTopHandlerPickUpPos() == true)
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
            if (result == true)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS] == true)
                {
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Top_Handle_Step = Unit_Model.TopHandle.Top_Handle_Grip_Lock_Check;
                }
                else
                {
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Top_Handle_Step = Unit_Model.TopHandle.Idle;
                }
            }
            return result; // 성공 여부 반환
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
