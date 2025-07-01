using Common.Commands;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Telerik.Windows.Data;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels
{
    public class DioManager_ViewModel : Child_ViewModel
    {
        #region // ICommand Property
        public ICommand Dio_Command { get; private set; }
        #endregion
        private EziDio_Model _Dio = SingletonManager.instance.Dio;
        public EziDio_Model Dio
        {
            get { return _Dio; }
            set { SetValue(ref _Dio, value); }
        }
        private EzMotion_Model_E _Motion = SingletonManager.instance.Motion;
        public EzMotion_Model_E Motion
        {
            get { return _Motion; }
            set { SetValue(ref _Motion, value); }
        }
        private RadObservableCollection<bool> _DioUI;
        public RadObservableCollection<bool> DioUI
        {
            get { return _DioUI; }
            set { SetValue(ref _DioUI, value); }
        }
        private Timer DioTimer;
        private Timer AgingDetectTimer;
        public DioManager_ViewModel()
        {
            DioUI = new RadObservableCollection<bool>();
            for (int i=0; i<Dio.DO_RAW_DATA.Count; i++)
            {
                DioUI.Add(false);
            }
            UupdateTimer_Dio();
            AgingDetectTimerInit();
        }
        private void UupdateTimer_Dio()
        {
            DioTimer = new Timer(500);
            DioTimer.Elapsed += OnTimerElapsed;
            DioTimer.AutoReset = true;
            DioTimer.Start();
        }
        private void AgingDetectTimerInit()
        {
            AgingDetectTimer = new Timer(200);
            AgingDetectTimer.Elapsed += OnAgingDetectTimer;
            AgingDetectTimer.AutoReset = true;
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // 검사 정지 상태일때만 실행한다.
            if (SingletonManager.instance.IsInspectionStart == false)
            {
                for (int i = 0; i < Dio.DO_RAW_DATA.Count; i++)
                {
                    DioUI[i] = Dio.DO_RAW_DATA[i];
                }
            }
        }
        private void OnAgingDetectTimer(object sender, ElapsedEventArgs e)
        {
            if (SingletonManager.instance.IsInspectionStart == false)
            {
                AgingCvStopControl();
            }
        }
        private async void OnDio_Command(object obj)
        {
            switch(obj.ToString())
            {
                case "SetUpDown":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TRANSFER_LZ_DOWN_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_DOWN_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                    break;
                case "SetCenter":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.IN_SET_CV_CENTERING] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.IN_SET_CV_CENTERING, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.IN_SET_CV_CENTERING, false);
                    break;
                case "Turn":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TRANSFER_LZ_TURN_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_TURN_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_TURN_SOL, false);
                    break;
                case "SetCvRunStop":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.INPUT_SET_CV_RUN] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.INPUT_SET_CV_RUN, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.INPUT_SET_CV_RUN, false);
                    break;
                case "Vacuum":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TRANSFER_LZ_VACUUM_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_VACUUM_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_VACUUM_SOL, false);
                    break;
                case "LR":
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_DOWN_SOL, false);

                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TRANSFER_FORWARD_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_FORWARD_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_FORWARD_SOL, false);
                    break;
                case "BottomUpDown":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TRANSFER_RZ_DOWN_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                    break;
                case "BottomGripUnGrip":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TRANSFER_RZ_GRIP_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_GRIP_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_GRIP_SOL, false);
                    break;
                case "BottomCenter":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_2] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_2, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_2, false);
                    break;
                case "CvUpDown":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.CLAMPING_CV_UP_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_UP_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_UP_SOL, false);
                    break;
                case "BottomCvRunStop":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.CLAMPING_CV_RUN] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_RUN, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_RUN, false);
                    break;
                case "TopUpDown1":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                    break;
                case "TopUpDown2":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                    break;
                case "TopGripUnGrip":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_GRIP_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                    break;
                case "TopCenter":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_1] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_1, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                    break;
                case "TopCvRun":
                    Motion.MoveTopReturnCvRun();
                    break;
                case "TopCvStop":
                    Motion.MoveTopReturnCvStop();
                    break;
                case "Z_GripUnGrip":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.CLAMPING_LD_Z_GRIP_SOL] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_LD_Z_GRIP_SOL, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_LD_Z_GRIP_SOL, false);
                    break;
                case "LiftCvRunStop1":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.LIFT_CV_RUN_1] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_1, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_1, false);
                    break;
                case "LiftCvRunStop2":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.LIFT_CV_RUN_2] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_2, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_2, false);
                    break;
                case "LiftCvRunStop3":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.LIFT_CV_RUN_3] == false)
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_3, true);
                    else
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_3, false);
                    break;
                case "Upper1":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1] == false
                        || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2] == false)
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, true);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, true);
                    }
                    else
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, false);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, false);
                    }
                    break;
                case "Upper2":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1] == false
                        || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2] == false)
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, true);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, true);
                    }
                    else
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, false);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, false);
                    }
                    break;
                case "Upper3":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1] == false
                        || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2] == false)
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, true);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, true);
                    }
                    else
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, false);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, false);
                    }
                    break;
                case "Low1":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1] == false
                        || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2] == false)
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, true);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, true);
                    }
                    else
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, false);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, false);
                    }
                    break;
                case "Low2":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1] == false
                        || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2] == false)
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, true);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, true);
                    }
                    else
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, false);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, false);
                    }
                    break;
                case "Low3":
                    if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1] == false
                        || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2] == false)
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, true);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2, true);
                    }
                    else
                    {
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, false);
                        Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2, false);
                    }
                    break;
                case "StepRun1":
                    await AgingCvStepRun(0);
                    break;
                case "StepRun2":
                    await AgingCvStepRun(1);
                    break;
                case "StepRun3":
                    await AgingCvStepRun(2);
                    break;
                case "StepRun4":
                    await AgingCvStepRun(3);
                    break;
                case "StepRun5":
                    await AgingCvStepRun(4);
                    break;
                case "StepRun6":
                    await AgingCvStepRun(5);
                    break;
            }
        }
        private async Task AgingCvStepRun(int Index)
        {
            if (Index == 0)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_1_2_UPPER_DETECT_SS_2] != true)
                {
                    AgingDetectTimer.Start();
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, true);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, true);
                    await Task.Delay(SingletonManager.instance.SystemModel.AgingCvStepTime);

                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, false);
                    AgingDetectTimer.Stop();
                }
            }
            else if (Index == 1)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_2_2_UPPER_DETECT_SS_2] != true)
                {
                    AgingDetectTimer.Start();
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, true);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, true);

                    await Task.Delay(SingletonManager.instance.SystemModel.AgingCvStepTime);

                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, false);
                    AgingDetectTimer.Stop();
                }
            }
            else if (Index == 2)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_3_2_UPPER_DETECT_SS_2] != true)
                {
                    AgingDetectTimer.Start();
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, true);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, true);

                    await Task.Delay(SingletonManager.instance.SystemModel.AgingCvStepTime);

                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, false);
                    AgingDetectTimer.Stop();
                }
            }
            else if (Index == 3)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_1_2_LOW_DETECT_SS_2] != true)
                {
                    AgingDetectTimer.Start();
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, true);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, true);

                    await Task.Delay(SingletonManager.instance.SystemModel.AgingCvStepTime);

                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, false);
                    AgingDetectTimer.Stop();
                }
            }
            else if (Index == 4)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_2_2_LOW_DETECT_SS_2] != true)
                {
                    AgingDetectTimer.Start();
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, true);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, true);

                    await Task.Delay(SingletonManager.instance.SystemModel.AgingCvStepTime);

                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, false);
                    AgingDetectTimer.Stop();
                }
            }
            else if (Index == 5)
            {
                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_3_2_LOW_DETECT_SS_2] != true)
                {
                    AgingDetectTimer.Start();
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, true);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2, true);

                    await Task.Delay(SingletonManager.instance.SystemModel.AgingCvStepTime);

                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2, false);
                    AgingDetectTimer.Stop();
                }
            }
        }
        private void AgingCvStopControl()
        {
            if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_1_2_UPPER_DETECT_SS_2] == true)
            {
                if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1] == true
                   || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2] == true)
                {
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, false);
                }
            }
            else if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_2_2_UPPER_DETECT_SS_2] == true)
            {
                if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1] == true
                   || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2] == true)
                {
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, false);
                }
            }
            else if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_3_2_UPPER_DETECT_SS_2] == true)
            {
                if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1] == true
                   || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2] == true)
                {
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, false);
                }
            }
            else if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_1_2_LOW_DETECT_SS_2] == true)
            {
                if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1] == true
                   || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2] == true)
                {
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, false);
                }
            }
            else if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_2_2_LOW_DETECT_SS_2] == true)
            {
                if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1] == true
                   || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1] == true)
                {
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, false);
                }
            }
            else if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_3_2_LOW_DETECT_SS_2] == true)
            {
                if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1] == true
                   || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2] == true)
                {
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2, false);
                }
            }
        }
        #region // override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            // RelayCommand
            Dio_Command = new RelayCommand(OnDio_Command);
        }
        protected override void DisposeManaged()
        {
            Dio_Command = null;

            // Dispose Timer
            if (DioTimer != null)
            {
                DioTimer.Stop();
                DioTimer.Elapsed -= OnTimerElapsed;
                DioTimer.Dispose();
                DioTimer = null;
            }
            base.DisposeManaged();
        }
        #endregion
    }
}
