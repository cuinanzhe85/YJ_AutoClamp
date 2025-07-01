using Common.Managers;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YJ_AutoClamp.Utils;
using static YJ_AutoClamp.Models.EziDio_Model;

namespace YJ_AutoClamp.Models
{
    public enum Direction
    {
        CW = 0,
        CCW = 1
    }
    public enum MotionUnit_List
    {
        Out_Y,
        Out_Z,
        Top_X,
        Top_CV,
        Lift_1,
        Lift_2,
        Lift_3,
        In_CV,
        Out_CV,
        Max
    }
    public enum ServoSlave_List
    {
        Out_Y_Handler_Y,
        Out_Z_Handler_Z,
        Top_X_Handler_X,
        Top_CV_X,
        Lift_1_Z,
        Lift_2_Z,
        Lift_3_Z,
        Max
    }
    public enum Lift_Index
    {
        Lift_1,
        Lift_2,
        Lift_3,
        Max
    }
    public enum Floor_Index
    {
        Floor_1,
        Floor_2,
        Floor_3,
        Floor_4,
        Floor_5,
        Max
    }
    public class Unit_Model
    {
        public MotionUnit_List UnitGroup { get; set; }
        public int UnitID { get; set; }
        public List<ServoSlave_List> ServoNames { get; set; }

        // Top Clamp 안착작업 완료시 사용되는 변수
        // Bottom NG 배출용 변수
        private bool[] AgingCvFull = { false, false, false, false, false, false };
        private bool[] AgingCvStart = { false, false, false, false, false, false };
        private bool[] AgingCvEndStopCondition = { false, false, false, false, false, false };
        private bool[] AgingCvInStopCondition = { false, false, false, false, false, false };
        private int _AgingPassCvIndex = 0; // Aging CV Index

        public bool _NoneSetTest = false; // Set Test Mode

        Stopwatch _TimeDelay = new Stopwatch();
        Stopwatch _TopHandlerTimeDelay = new Stopwatch();
        Stopwatch _BottomHandlerTimeDelay = new Stopwatch();
        Stopwatch _TopCvTimeDelay = new Stopwatch();
        Stopwatch _TopCvTimeDelay2 = new Stopwatch();
        Stopwatch _BottomCvTimeDelay = new Stopwatch();
        Stopwatch _BottomCvTimeDelay2 = new Stopwatch();
        Stopwatch _AgingCvStepRunTime = new Stopwatch();
        Stopwatch _SetNgCvDelay = new Stopwatch();


        public string ClampFailMassage= string.Empty;
        private int ClampRetryCount = 0;
        public Unit_Model(MotionUnit_List unit)
        {
            UnitGroup = unit;
            UnitID = (int)unit;
            ServoNames = new List<ServoSlave_List>();
        }
        private EziDio_Model Dio = SingletonManager.instance.Dio;
        private EzMotion_Model_E Ez_Model = SingletonManager.instance.Motion;
        private bool _isLoopRunning = false;
        private int _BarCodeRetryCount = 0;
        private bool _BarCodeReadResult = false;
        private string NFC_Data = string.Empty;
        // Steps
        public enum InCvSequence
        {
            Idle,
            In_Sensor_Check,
            CV_Run_Wait,
            CV_Off_Check,
            CV_Off_Wait,
            CV_Centering_Check,
            In_Cv_Detect_Done
        }
        public enum BottomHandle
        {
            Idle,
            Out_Position_Tray_Check,
            ClampInSensorCheck,
            Vacuum_Skip_Step,
            Set_PutDown,
            Set_Handler_Up,
            Bottom_Handler_Grip_Check,
            Set_PutDown_Done,
            Bottom_Clmap_Pickup,
            Bottom_RZ_Down_Wait,
            Bottom_Clamp_Grip,
            Bottom_Handler_Up,
            Bottom_PicUp_Done,
            Set_Handler_Down,
            Set_PickUp_Down,
            Handler_Down_Check,
            Ungrip_InCenteringBwd_Check,
            Set_Vacuum_On,
            MES_Receive_Check,
            Set_PickUp_Up,
            Set_PickUp_Done,
            Bottom_PutDown_Done
        }
        public enum TopHandle
        {
            Idle,
            Top_Handle_Up_Check,
            Top_Handle_Pickup_Position_Check,
            Top_PickUp_Time_Wait,
            Top_BarCode_Read,
            Top_BarCode_Read_Done,
            Top_Clamp_PickupDown,
            Top_Handle_Down_Check,
            Top_Handle_Grip_Lock_Check,
            Top_Handle_PutUp_Check,
            Top_Handle_Tray_Out_Wait,
            Top_Handle_NG_Port_Move,
            Top_Handle_NG_Port_Move_Check,
            Top_Handle_NG_Port_Down_check,
            Top_Handle_NG_Port_UnGrip_check,
            Top_Handle_NG_Port_Up_check,
            Buttom_Clamp_Arrival_Check,
            Top_Handle_PutDown_Move_Check,
            Top_Handle_Centering_FWD_Check,
            Top_Handle_PutDown_Check,
            Top_Handle_Grip_Unlock_Check,
            Top_Handle_PutDown_Up_Check
        }
        public enum OutHandle
        {
            Idle,
            Out_Handle_Z_Up,
            Out_Handle_Z_Up_Done,
            Out_Handle_Y_Ready_Done,
            Top_Tray_Sensor_Check,
            Out_Handle_Y_Pickup_Pos_Check,
            Y_Move_Wait,
            Y_Move_Start,
            Out_Handle_Z_Down_Done,
            Out_Handle_Grip_Check,
            Out_Handle_Grip_Wait,
            Out_Handle_Z_Pickup_Up_Done,
            Out_Handle_Y_Safety_Pos,
            Out_Handle_Y_PutDown_Pos_Check,
            Lift_Loding_Move_Done,
            Out_Handle_Z_PutDown_Done,
            Out_Handle_UnGrip_Check,
            Out_Handle_Z_Ready_Check,
            Lift_Out_Wait
        }
        public enum OutCvSequence
        {
            Idle,
            Out_CV_On_Wait,
            Out_CV_Tray_OK_Out,
            Out_CV_Tray_NG_Out,
            Out_Clamp_CV_Stop,
            Out_Clamp_CV_StopperUp_Wait,
            Out_CV_Tray_NG_Check,
            Out_CV_Off_Check,
            Out_CV_Off_Wait,
            Out_CV_Centering_FWD_Check,
            Out_CV_Centering_BWD_Check
        }
        public enum Aging_CV_Step
        {
            Idle,
            Lift_Loading_Pos_Check,
            CV_On_Condition_Wait,
            Loading_Life_Up_Wait,
            Low_Lift_Down,
            Unclamping_IF_Send,
            Unclamping_IF_Receive,
            Lift_CV_Forward,
            Aging_CV_Forward,
            CV_Stop,
            CV_Stop_Wait,
            Unclamping_IF_Set_Off,
            Low_Lift_Up_Start,
            Low_Lift_Up_Wait,
            Cv_Step_Run_Start,
            Cv_Step_Run_Stop,
            Cv_Step_Run_IF_Send,
            Cv_Step_Run_IF_Return_Check,
            Cv_Step_IF_Run,
            Cv_Step_IF_Clamp_OutWait,
            Cv_Step_IF_Stop,
            Cv_Step_IF_Off

        }
        public enum Aging_CV_Pass_Step
        {
            Idle,
            CV_Run,
            CV_End_Sensor_Wait,
            Interfase_Send,
            Unclamp_Interfase_Wait,
            Clamp_Out_Start,
            CV_Off_Wait,
            CV_Off,
            Next_CV_Start
        }
        
        public enum Aging_Lift_Step
        {
            Idle,
            CV_On_Wait,
            CV_Full_Sensor_Check,
            Lift_Upper_Step,
            Upper_Stop_Sensor_Check
        }
        public enum Rtn_Top_CV_1
        {
            Idle,
            Top_CV_Stop,
            Top_CV_Start_Wait
        }
        public enum Rtn_Top_CV_2
        {
            Idle,
            Top_CV_Stop,
            Top_Unclalmp_IF_Send,
            Top_Unclamp_IF_Off
        }
        public enum Top_CV
        {
            Idle,
            Top_CV_Run,
            Top_CV_Stop,
            Top_CV_Stop_Wait
        }
        public enum Rtn_BTM_CV
        {
            Idle,
            Rtn_BTM_CV_Stop,
            Rtn_BTM_CV_Stop_Wait,
            Rtn_BTM_CV_Start_Wait
        }
        public enum Rtn_BTM_CV2
        {
            Idle,
            Rtn_BTM_CV_Stop,
            Rtn_BTM_CV_Stop_Wait,
            Rtn_BTM_Unclmap_IF_Send,
            Rtn_BTM_Unclmap_IF_Off
        }
        // 일단 보류
        public enum Top_NG
        {
            Idle,
            Top_CV_Run,
            Top_CV_Stop
        }
        public enum Set_NG_Step
        {
            Idle,
            CV_Run_Wait,
            CV_Off_Wait,
            CV_Off,
            CV_Detect_Check,
            CV_Last_Out,
            CV_Out_Complete
        }
        public InCvSequence In_Cv_Step = InCvSequence.Idle;
        public OutCvSequence Out_Cv_Step = OutCvSequence.Idle;
        public BottomHandle Bottom_Step = BottomHandle.Idle;
        public TopHandle Top_Handle_Step = TopHandle.Idle;
        public OutHandle Out_Handle_Step = OutHandle.Idle;
        public Aging_CV_Step AgingCVStep = Aging_CV_Step.Idle;

        public Rtn_Top_CV_1 RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
        public Rtn_Top_CV_2 RetTopCV_2_Step = Rtn_Top_CV_2.Idle;
        public Rtn_BTM_CV RetBtmCV_Step = Rtn_BTM_CV.Idle;
        public Rtn_BTM_CV2 RetBtmCV_2_Step = Rtn_BTM_CV2.Idle;
        public Top_NG TopNgStep = Top_NG.Idle;
        public Top_CV TopCvStep = Top_CV.Idle;
        public Set_NG_Step SetNgCvStep = Set_NG_Step.Idle;

        public Aging_CV_Pass_Step AgingCvPassStep = Aging_CV_Pass_Step.Idle;
        public void Loop()
        {
            // Task.Delay를 사용하는경우 Loop 동작 확인후 리턴. 중복호출 방지
            if (_isLoopRunning)
                return;

            switch (UnitGroup)
            {
                case MotionUnit_List.Top_X:
                     Top_Handel_Logic();
                     Bottom_Handel_Logic();
                    break;
                case MotionUnit_List.Out_Y:
                     Out_Handle_Y_Logic();
                    break;
                case MotionUnit_List.Lift_1:
                     Aging_CV_StepRun_Logic();
                    break;
                case MotionUnit_List.In_CV:
                    In_CV_Logic();
                    Top_Cv();
                    Return_Bottom_CV_1_Logic();
                    Return_Bottom_CV_2_Logic();
                    Return_Top_CV_1_Logic();
                    Return_Top_CV_2_Logic();
                    break;
                case MotionUnit_List.Out_CV:
                    Bottom_Out_CV_Logic();
                    Set_Ng_CV_Logic();
                    Top_NG_CV_Logic();
                    break;
            }
        }
        public void In_CV_Logic()
        {
            switch (In_Cv_Step)
            {
                case InCvSequence.Idle:
                    In_Cv_Step = InCvSequence.In_Sensor_Check;
                    _TimeDelay.Restart();
                    break;
                case InCvSequence.In_Sensor_Check:
                    // 제품 투입 sensor 와 도착 위치에 제품이 없을때 cv on
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_SS_3] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.INTERFACE_FRONT_MC_SAFETY] == true)
                    {
                        _TimeDelay.Restart();
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.TOWER_LAMP_RED] == false)
                        {
                            Dio_Output(DO_MAP.TOWER_LAMP_YELLOW, false);
                            Dio_Output(DO_MAP.TOWER_LAMP_GREEN, true);
                        }
                            
                        In_Cv_Step = InCvSequence.CV_Run_Wait;
                    }
                    // 이미 제품이 투입되있으면 Off 로 이동 시킨다.
                    else if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_SS_3] == true)
                    {
                        In_Cv_Step = InCvSequence.CV_Off_Check;
                    }
                    else if (_TimeDelay.ElapsedMilliseconds > 30000)
                    {
                        _TimeDelay.Reset();
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.TOWER_LAMP_YELLOW] == false)
                        {
                            Dio_Output(DO_MAP.TOWER_LAMP_YELLOW, true);
                            Dio_Output(DO_MAP.TOWER_LAMP_GREEN, false);
                        }

                        Global.instance.TactTimeStart = false;
                        SingletonManager.instance.Channel_Model[0].TactTime = "0.0";
                    }
                    if (_TimeDelay.ElapsedMilliseconds == 0)
                        _TimeDelay.Restart();
                    break;
                case InCvSequence.CV_Run_Wait:
                    if (_TimeDelay.ElapsedMilliseconds > 100)
                    {
                        Dio_Output(DO_MAP.INPUT_SET_CV_RUN, true);
                        In_Cv_Step = InCvSequence.CV_Off_Check;
                    }
                    break;
                case InCvSequence.CV_Off_Check:
                    // cv out 센서 감지 됬을때 cv off 하고 centering 전진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_SS_3] == true)
                    {
                        _TimeDelay.Restart();

                        In_Cv_Step = InCvSequence.CV_Off_Wait;
                        Global.instance.InputCountPlus();
                    }
                    break;
                case InCvSequence.CV_Off_Wait:
                    if (_TimeDelay.ElapsedMilliseconds > 300)
                    {
                        // Centering 전진
                        Dio_Output(DO_MAP.IN_SET_CV_CENTERING, true);
                        // cv off
                        Dio_Output(DO_MAP.INPUT_SET_CV_RUN, false);
                        In_Cv_Step = InCvSequence.CV_Centering_Check;
                    }
                    break;
                case InCvSequence.CV_Centering_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_UNALIGN_CYL_SS] == true)
                    {
                        In_Cv_Step = InCvSequence.In_Cv_Detect_Done;
                    }
                    break;
                case InCvSequence.In_Cv_Detect_Done:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_SS_3] == false)
                    {
                        In_Cv_Step = InCvSequence.Idle;
                    }
                    break;
            }
        }
        private void Bottom_Out_CV_Logic()
        {
            switch (Out_Cv_Step)
            {
                case OutCvSequence.Idle:
                    // 배출위치 도착센서가 감지 되지않으면  out cv 스토퍼 up
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] != true 
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // 스토퍼 상승
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_STOPER_UP_SOL] == false)
                            Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_RUN] == true)
                            Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                        Out_Cv_Step = OutCvSequence.Out_CV_On_Wait;
                    }
                    break;
                case OutCvSequence.Out_CV_On_Wait:
                    // bottom tray ok/ng 확인 & Pannel 장착 완료 상태 확인 후 cv on
                    if (SingletonManager.instance.BottomClampDone == true)
                    {
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == true         // Bottom clamp 센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == false // Top clamp  센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == false // Top clamp  센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_3] == false // Top clamp  센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true)    // Set grap handler up
                                || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)   
                        {
                            if (SingletonManager.instance.ClampResult == true)
                            {
                                // OK 일때
                                Out_Cv_Step = OutCvSequence.Out_CV_Tray_OK_Out;
                            }
                            else
                            {
                                // NG 일때
                                Out_Cv_Step = OutCvSequence.Out_CV_Tray_NG_Out;
                                _TimeDelay.Restart();
                            }
                            SingletonManager.instance.BottomClampDone = false;
                        }
                    }
                    break;
                case OutCvSequence.Out_CV_Tray_OK_Out:
                    // Clmap 끝단 제품이 있는지 확인 한다.제품 없을때 cv on
                    
                    // 스토퍼 상승
                    if (Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_STOPER_UP_SOL] == false)
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                    else
                    {
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_CENTERING_SOL_1] == true)
                            Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                        //cv on
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, true);
                        Out_Cv_Step = OutCvSequence.Out_CV_Off_Check;

                        _TimeDelay.Restart();
                    }
                    break;
                case OutCvSequence.Out_CV_Tray_NG_Out:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_1] == true)
                    {
                        ClampFailMassage = "SET NG Conveyor is full.\r\n(SET NG 컨베어를 비워주세요)";
                    }
                    // ng 이면 도착 센서 감지 후 스토퍼 하강
                    else if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] != true
                        && (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_1] == false || Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == false))
                    {
                        // cv On
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, true);
                        //Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, true);
                        // 스토퍼 하강
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, false);
                        Out_Cv_Step = OutCvSequence.Out_Clamp_CV_Stop;
                        _TimeDelay.Restart();

                        // MES NG 발생시 알람 발생 하고 생산 이어서 진행
                        Application.Current.Dispatcher.BeginInvoke(
                                    (ThreadStart)(() =>
                                    {
                                        Global.instance.ShowMessagebox("SET MES NG OUT", true, false, true);
                                    }), DispatcherPriority.Send);
                    }
                    else if (_TimeDelay.ElapsedMilliseconds > 2000)
                    {
                        ClampFailMassage = "Please empty the buffer section SET ng port ";
                    }
                    break;
                case OutCvSequence.Out_Clamp_CV_Stop:
                    // Clamp CV에 제품 없을때 까지 Conveyor구동
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == false
                        && _TimeDelay.ElapsedMilliseconds > 2000) // Top clamp  센서)
                    {
                        // cv Off
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                        Out_Cv_Step = OutCvSequence.Out_Clamp_CV_StopperUp_Wait;
                        _TimeDelay.Restart();
                    }
                    break;
                case OutCvSequence.Out_Clamp_CV_StopperUp_Wait:
                    if (_TimeDelay.ElapsedMilliseconds > 500)
                    {
                        // 스토퍼 상승
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        //Out_Cv_Step = OutCvSequence.Out_CV_Tray_NG_Check;
                        Out_Cv_Step = OutCvSequence.Idle;
                    }
                    break;
                //case OutCvSequence.Out_CV_Tray_NG_Check:
                //    // Tray 도착 감지 센서 꺼질때까지 대기 후 Cv Off 
                //    if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == true)
                //    {
                //        // 스토퍼 상승되여 있지 않으면 다시 상승
                //        if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] != true)
                //            Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                //        // cv off
                //        Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                //        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, false);
                        
                //        Out_Cv_Step = OutCvSequence.Idle;
                //    }
                //    break;
                case OutCvSequence.Out_CV_Off_Check:
                    // top 조립 위치 도착 센서 받으면 CV Off 센터링 전징
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true)
                    {
                        _TimeDelay.Restart();
                        // cv Off
                        Out_Cv_Step = OutCvSequence.Out_CV_Off_Wait;
                        break;
                    }
                    // CV On 이후 5초간 제품이 end 지점에 도착하지 않으면 step 다시 시작
                    //else if (_TimeDelay.ElapsedMilliseconds > 5000)
                    //{
                    //    Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                    //    Out_Cv_Step = OutCvSequence.Idle;
                    //}
                    break;
                case OutCvSequence.Out_CV_Off_Wait:
                    if (_TimeDelay.ElapsedMilliseconds > 150)
                    {
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                        Out_Cv_Step = OutCvSequence.Idle;
                    }
                    break;
            }
        }
        public void Set_Ng_CV_Logic()
        {
            switch (SetNgCvStep)
            {
                case Set_NG_Step.Idle:
                    SetNgCvStep = Set_NG_Step.CV_Run_Wait;
                    break;
                case Set_NG_Step.CV_Run_Wait:
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == false && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true)
                        && (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_1] == false || Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == false)
                        && SingletonManager.instance.ClampResult == false)
                    {
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, true);
                        SetNgCvStep = Set_NG_Step.CV_Off_Wait;
                    }
                    break;
                case Set_NG_Step.CV_Off_Wait:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_1] == true)
                    {
                        _SetNgCvDelay.Restart();
                        SetNgCvStep = Set_NG_Step.CV_Off;
                    }
                    break;
                case Set_NG_Step.CV_Off:
                    if (_SetNgCvDelay.ElapsedMilliseconds > 2000)
                    {
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, false);

                        if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == true)
                        {
                            SetNgCvStep = Set_NG_Step.CV_Detect_Check;
                        }
                        else
                        {
                            SetNgCvStep = Set_NG_Step.Idle;
                        }
                    }
                    break;
                case Set_NG_Step.CV_Detect_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == false)
                    {
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, true);
                        SetNgCvStep = Set_NG_Step.CV_Last_Out;
                        _SetNgCvDelay.Restart();
                    }
                    break;
                case Set_NG_Step.CV_Last_Out:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == true)
                    {
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, false);
                        SetNgCvStep = Set_NG_Step.Idle;
                    }
                    else if (_SetNgCvDelay.ElapsedMilliseconds > 3000)
                    {
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, false);
                        SetNgCvStep = Set_NG_Step.Idle;
                    }
                    break;
            }
        }
        public void Top_Cv()
        {
            switch (TopCvStep)
            {
                case Top_CV.Idle:
                    TopCvStep = Top_CV.Top_CV_Run;
                    break;
                case Top_CV.Top_CV_Run:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_SS_2] == true)
                    {
                        Ez_Model.MoveTopReturnCvRun();
                        TopCvStep = Top_CV.Top_CV_Stop;
                        _TimeDelay.Restart();
                    }
                    break;
                case Top_CV.Top_CV_Stop:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == true)
                    {
                        _TimeDelay.Restart();
                        TopCvStep = Top_CV.Top_CV_Stop_Wait;
                    }
                    break;
                case Top_CV.Top_CV_Stop_Wait:
                    if (_TimeDelay.ElapsedMilliseconds > 300)
                    {
                        Ez_Model.MoveTopReturnCvStop();
                        TopCvStep = Top_CV.Idle;
                    }
                    break;
            }
        }
        
        public void Return_Top_CV_1_Logic()
        {
            switch(RetTopCV_1_Step)
            {
                case Rtn_Top_CV_1.Idle:
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == false
                        || Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_SS_2] == false)
                        && SingletonManager.instance.IsInspectionStart == true)
                    {
                        // cv on
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, true);
                        //Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, true);
                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_CV_Stop;
                        _TopCvTimeDelay.Restart();
                    }
                    break;
                case Rtn_Top_CV_1.Top_CV_Stop:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_SS_2] == true)
                    {
                        // cv off
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, false);
                        //Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                        RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
                        //_TopCvTimeDelay.Restart();
                    }
                    else if (_TopCvTimeDelay.ElapsedMilliseconds > 2000)
                    {
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, false);
                        //Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_CV_Start_Wait;
                        _TopCvTimeDelay.Restart();
                    }
                    else
                    {
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN] != true || Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN_2] != true)
                        {
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, true);
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, true);
                        }
                    }
                    break;
                case Rtn_Top_CV_1.Top_CV_Start_Wait:
                    if (_TopCvTimeDelay.ElapsedMilliseconds > 1000)
                    {
                        // not Receive for Unclamp
                        RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
                    }
                    break;
                    //case Rtn_Top_CV_1.Top_Unclalmp_IF_Send:
                    //    // unclamp에서 I/F신호다 들오와있고 Top Return C/V투입에 제품이 없으면 I/F ON
                    //    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_2_1] == false
                    //        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == true)
                    //    {
                    //        // Unclamp Top Return Interface On
                    //        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, true);
                    //        RetTopCV_1_Step = Rtn_Top_CV_1.Top_Unclamp_IF_Off;
                    //    }
                    //    else if (_TopCvTimeDelay.ElapsedMilliseconds > 1000)
                    //    {
                    //        // not Receive for Unclamp
                    //        RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
                    //    }
                    //    break;
                    //case Rtn_Top_CV_1.Top_Unclamp_IF_Off:
                    //    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == false)
                    //    {
                    //        // Unclamp Top Return Interface On
                    //        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, false);
                    //        RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
                    //    }
                    //    break;
            }
        }
        public void Return_Top_CV_2_Logic()
        {
            switch (RetTopCV_2_Step)
            {
                case Rtn_Top_CV_2.Idle:
                    if (SingletonManager.instance.IsInspectionStart == true)
                    {
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_2_2] == false
                            || Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN] == true)
                        {
                            // cv on
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, true);
                            RetTopCV_2_Step = Rtn_Top_CV_2.Top_CV_Stop;
                            _TopCvTimeDelay2.Restart();
                        }
                    }
                    break;
                case Rtn_Top_CV_2.Top_CV_Stop:
                    if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_2_2] == true)
                    {
                        // cv off
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                        RetTopCV_2_Step = Rtn_Top_CV_2.Top_Unclalmp_IF_Send;
                        _TopCvTimeDelay2.Restart();
                    }
                    else if (_TopCvTimeDelay2.ElapsedMilliseconds > 2000)
                    {
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                        RetTopCV_2_Step = Rtn_Top_CV_2.Top_Unclalmp_IF_Send;
                        _TopCvTimeDelay2.Restart();
                    }
                    else if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_2_1] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == true)
                    {
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                        RetTopCV_2_Step = Rtn_Top_CV_2.Top_Unclalmp_IF_Send;
                        _TopCvTimeDelay2.Restart();
                    }
                    break;
                case Rtn_Top_CV_2.Top_Unclalmp_IF_Send:
                    // unclamp에서 I/F신호다 들오와있고 Top Return C/V투입에 제품이 없으면 I/F ON
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_2_1] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == true)
                    {
                        // Unclamp Top Return Interface On
                        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, true);
                        RetTopCV_2_Step = Rtn_Top_CV_2.Top_Unclamp_IF_Off;
                    }
                    else if (_TopCvTimeDelay2.ElapsedMilliseconds > 1000)
                    {
                        // not Receive for Unclamp
                        RetTopCV_2_Step = Rtn_Top_CV_2.Idle;
                    }
                    break;
                case Rtn_Top_CV_2.Top_Unclamp_IF_Off:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == false)
                    {
                        // Unclamp Top Return Interface On
                        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, false);
                        RetTopCV_2_Step = Rtn_Top_CV_2.Idle;
                    }
                    break;
            }
        }
        public void Return_Bottom_CV_1_Logic()
        {
            switch(RetBtmCV_Step)
            {
                case Rtn_BTM_CV.Idle:
                    // Return Botton CV 도착위치에 clamp가 없으면  CV run
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == false
                        && SingletonManager.instance.IsInspectionStart == true)
                    {
                        if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.BTM_RETURN_CV_RUN] == false)
                            Dio_Output(DO_MAP.BTM_RETURN_CV_RUN, true);
                        //if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.BTM_RETURN_CV_RUN_2] == false)
                        //    Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, true);
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_CV_Stop;
                        _BottomCvTimeDelay.Restart();
                    }
                    break;
                case Rtn_BTM_CV.Rtn_BTM_CV_Stop:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == true)
                    {
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_CV_Stop_Wait;
                        _BottomCvTimeDelay.Restart();
                    }
                    else if (_BottomCvTimeDelay.ElapsedMilliseconds > 2000)
                    {
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN, false);
                        //Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, false);
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_CV_Start_Wait;
                        _BottomCvTimeDelay.Restart();
                    }
                    else if (_BottomCvTimeDelay.ElapsedMilliseconds == 0)
                        _BottomCvTimeDelay.Restart();
                    break;
                case Rtn_BTM_CV.Rtn_BTM_CV_Stop_Wait:

                    if (_BottomCvTimeDelay.ElapsedMilliseconds > 250)
                    {
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN, false);
                        //Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, false);
                        RetBtmCV_Step = Rtn_BTM_CV.Idle;
                        _BottomCvTimeDelay.Restart();
                    }
                    else if (_BottomCvTimeDelay.ElapsedMilliseconds == 0)
                        _BottomCvTimeDelay.Restart();
                    break;
                case Rtn_BTM_CV.Rtn_BTM_CV_Start_Wait:
                    if (_BottomCvTimeDelay.ElapsedMilliseconds >1000)
                        RetBtmCV_Step = Rtn_BTM_CV.Idle;
                    break;
                //case Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Send:
                //    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_2_1] == false
                //        && Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == true)
                //    {
                //        Dio_Output(DO_MAP.BOTTOM_RETURN_CV_INTERFACE, true);
                //        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Off;
                //    }
                //    else if (_BottomCvTimeDelay.ElapsedMilliseconds > 1000)
                //    {
                //        RetBtmCV_Step = Rtn_BTM_CV.Idle;
                //    }
                //    break;
                //case Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Off:
                //    if (Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == false)
                //    {
                //        Dio_Output(DO_MAP.BOTTOM_RETURN_CV_INTERFACE, false);
                //        RetBtmCV_Step = Rtn_BTM_CV.Idle;
                //    }
                //    break;
            }
        }
        public void Return_Bottom_CV_2_Logic()
        {
            switch (RetBtmCV_2_Step)
            {
                case Rtn_BTM_CV2.Idle:
                    if (SingletonManager.instance.IsInspectionStart == true)
                    {
                        // Return Botton CV 도착위치에 clamp가 없으면  CV run
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_2_2] == false)
                        {
                            if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.BTM_RETURN_CV_RUN_2] == false)
                                Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, true);
                            RetBtmCV_2_Step = Rtn_BTM_CV2.Rtn_BTM_CV_Stop;
                            _BottomCvTimeDelay2.Restart();
                        }
                        else if (Dio.DO_RAW_DATA[(int)DO_MAP.BTM_RETURN_CV_RUN] == true)
                        {
                            Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, true);
                            RetBtmCV_2_Step = Rtn_BTM_CV2.Rtn_BTM_CV_Stop;
                            _BottomCvTimeDelay2.Restart();
                        }
                    }
                    break;
                case Rtn_BTM_CV2.Rtn_BTM_CV_Stop:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_2_2] == true
                        && Dio.DO_RAW_DATA[(int)DO_MAP.BTM_RETURN_CV_RUN] == false)
                    {
                        RetBtmCV_2_Step = Rtn_BTM_CV2.Rtn_BTM_CV_Stop_Wait;
                    }
                    else if (_BottomCvTimeDelay2.ElapsedMilliseconds > 2000)
                    {
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, false);
                        RetBtmCV_2_Step = Rtn_BTM_CV2.Rtn_BTM_Unclmap_IF_Send;
                        _BottomCvTimeDelay2.Restart();
                    }
                    else if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_2_1] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == true)
                    {
                        RetBtmCV_2_Step = Rtn_BTM_CV2.Rtn_BTM_CV_Stop_Wait;
                    }
                    else if (_BottomCvTimeDelay2.ElapsedMilliseconds == 0)
                        _BottomCvTimeDelay2.Restart();
                    break;
                case Rtn_BTM_CV2.Rtn_BTM_CV_Stop_Wait:
                    Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, false);
                    RetBtmCV_2_Step = Rtn_BTM_CV2.Rtn_BTM_Unclmap_IF_Send;
                    _BottomCvTimeDelay2.Restart();
                    break;
                case Rtn_BTM_CV2.Rtn_BTM_Unclmap_IF_Send:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_2_1] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == true)
                    {
                        Dio_Output(DO_MAP.BOTTOM_RETURN_CV_INTERFACE, true);
                        RetBtmCV_2_Step = Rtn_BTM_CV2.Rtn_BTM_Unclmap_IF_Off;
                    }
                    else if (_BottomCvTimeDelay2.ElapsedMilliseconds > 1000)
                    {
                        RetBtmCV_2_Step = Rtn_BTM_CV2.Idle;
                    }
                    break;
                case Rtn_BTM_CV2.Rtn_BTM_Unclmap_IF_Off:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == false)
                    {
                        Dio_Output(DO_MAP.BOTTOM_RETURN_CV_INTERFACE, false);
                        RetBtmCV_2_Step = Rtn_BTM_CV2.Idle;
                    }
                    break;
            }
        }
        public void Top_NG_CV_Logic()
        {
            // Top Tray CV Logic
            // Top Tray 도착 센서 감지시 Cv Off 그전 계속 On
            switch(TopNgStep)
            {
                case Top_NG.Idle:
                    TopNgStep = Top_NG.Top_CV_Run;
                    break;
                case Top_NG.Top_CV_Run:
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.NG_TOP_CV_DETECT_SS_1] == true          // NG 도착 신호 감지
                        && Dio.DI_RAW_DATA[(int)DI_MAP.NG_TOP_CV_DETECT_SS_2] != true       // NG OUT 신호 미 감지
                        && Ez_Model.IsMoveTopNGPortDone() != true)
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.NG_TOP_JIG_CV_RUN, true);
                        TopNgStep = Top_NG.Top_CV_Stop;
                    }
                    else if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.NG_TOP_JIG_CV_RUN] == true)
                    {
                        Dio_Output(DO_MAP.NG_TOP_JIG_CV_RUN, false);
                    }
                    break;
                case Top_NG.Top_CV_Stop:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_TOP_CV_DETECT_SS_2] == true)
                    {
                        Dio_Output(DO_MAP.NG_TOP_JIG_CV_RUN, false);
                        TopNgStep = Top_NG.Idle;
                    }
                    break;
            }
            
        }
        private void Bottom_Handel_Logic()
        {
            switch (Bottom_Step)
            {
                case BottomHandle.Idle:
                    //Bottom_Step = BottomHandle.Out_Position_Tray_Check;

                    //SingletonManager.instance.Channel_Model[0].CnNomber = "--";
                    //SingletonManager.instance.Channel_Model[0].MesResult = "--";

                //    Global.Mlog.Info($"Bottom_Step => Next Step : Out_Position_Tray_Check");
                //    break;
                //case BottomHandle.Out_Position_Tray_Check:
                    // Set Grip UP
                    if (Dio.DO_RAW_DATA[(int)DO_MAP.TRANSFER_LZ_DOWN_SOL] == true)
                        Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                    // Bottom Grip UP
                    if (Dio.DO_RAW_DATA[(int)DO_MAP.TRANSFER_RZ_DOWN_SOL] == true)
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                    // UnGrip
                    if (Dio.DO_RAW_DATA[(int)DO_MAP.TRANSFER_RZ_GRIP_SOL] == true)
                        Dio_Output(DO_MAP.TRANSFER_RZ_GRIP_SOL, false);
                    // 트레이가 없으면
                    Bottom_Step = BottomHandle.ClampInSensorCheck;
                    Global.Mlog.Info($"Bottom_Step => Next Step : ClampInSensorCheck");
                    break;
                case BottomHandle.ClampInSensorCheck:
                    // Bottom Handler Up,Ungrip상태 확인
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UNGRIP_CYL_SS] == true)
                        && SingletonManager.instance.IsY_PickupColl == false
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // Set handler 90도 Turn
                        Dio_Output(DO_MAP.TRANSFER_LZ_TURN_SOL, true);
                        // Bottom Pickup Move , false:Right  true:Left
                        Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, false);

                        if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == true
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_2, true);
                            Global.Mlog.Info($"Bottom_Step => Clamp CV Centering FWD");
                        }
                        Global.Mlog.Info($"Bottom_Step => Left Z Turn On");
                        Global.Mlog.Info($"Bottom_Step => X Sol Right Move");

                        Bottom_Step = BottomHandle.Set_Handler_Down;
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_Handler_Down");
                    }
                    break;
                case BottomHandle.Set_Handler_Down:
                    // right 위치 도착 확인
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_RIGHT_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_RETURN_CYL_SS] == true)
                    {
                        // Bottom Clamp가 있고 Vacuum이 인식되면
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_2_BWD] == false)
                            || _NoneSetTest == true
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            // Bottom clamp가 놓여져 있으면 Put Down 동작을 한다.
                            Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, true);

                            // Bottom Clamp가 진입되여 있으면 일단 RZ Down상태로 set 안착 완료를 대기한다
                            if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == true
                                || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                            {
                                Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                                Global.Mlog.Info($"Bottom_Step => Right Z Down");
                            }
                            Bottom_Step = BottomHandle.Set_PutDown;

                            Global.Mlog.Info($"Bottom_Step => Left Z Down");
                            Global.Mlog.Info($"Bottom_Step => Next Step : Set_PutDown");
                        }
                        // Clamp 놓여져있는데 Vacuum이 인식되지 않으면 제품이 이동중 탈착된것으로 인지한다.
                        else if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] == false)
                            && SingletonManager.instance.EquipmentMode == EquipmentMode.Auto)
                        {
                            ClampFailMassage = "SET Handler Vacuum Error\r\n(SET 흡착알람)";
                            Bottom_Step = BottomHandle.Vacuum_Skip_Step;
                        }
                        // Right 위치에서 Vacuum이 인식되지 않으면 Bottom Clamp Pickup 진행한다.
                        else
                        {
                            // Bottom Clamp가 없으면 Clamp Pickup 으로 이동한다.
                            Bottom_Step = BottomHandle.Bottom_Clmap_Pickup;
                            Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Clmap_Pickup");
                            _BottomHandlerTimeDelay.Restart();
                        }
                    }
                    break;
                case BottomHandle.Vacuum_Skip_Step:
                    // Set Vacuum error 발생시 수동 안착후 다음 스템으로 이어서 진행.
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_2_BWD] == false)
                            || _NoneSetTest == true
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // Bottom clamp가 놓여져 있으면 Put Down 동작을 한다.
                        Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, true);

                        // Bottom Clamp가 진입되여 있으면 일단 RZ Down상태로 set 안착 완료를 대기한다
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == true 
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                            Global.Mlog.Info($"Bottom_Step => Right Z Down");
                        }
                        Bottom_Step = BottomHandle.Set_PutDown;

                        Global.Mlog.Info($"Bottom_Step => Left Z Down");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_PutDown");
                    }
                    break;
                case BottomHandle.Set_PutDown:
                    // Set Handler Down 확인
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_DOWN_CYL_SS] == true)
                    {
                        // Vacuum Off
                        // Blow On
                        // Set Handler Up
                        Dio_Output(DO_MAP.TRANSFER_LZ_VACUUM_SOL, false);
                        Dio_Output(DO_MAP.TRANSFER_LZ_BOLW_SOL, true);
                        Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                        Global.Mlog.Info($"Bottom_Step => Vacuum Off, Bolw On");
                        Global.Mlog.Info($"Bottom_Step => Left Z Up");

                        if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_DOWN_CYL_SS] == true)
                        {
                            Dio_Output(DO_MAP.TRANSFER_RZ_GRIP_SOL, true);
                            Global.Mlog.Info($"Bottom_Step => Right Z Grip");
                        }
                        Bottom_Step = BottomHandle.Set_Handler_Up;
                        
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_Handler_Up");
                    }
                    break;
                case BottomHandle.Set_Handler_Up:
                    // Set Handler Up,Vacuum Off
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true
                     && (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] == false
                    || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry))
                    {
                        // Bolw off
                        // centering backward
                        Dio_Output(DO_MAP.TRANSFER_LZ_BOLW_SOL, false);
                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_2, false);
                        Dio_Output(DO_MAP.CLAMPING_CV_UP_SOL, false);
                        Global.Mlog.Info($"Bottom_Step => Bolw Off");
                        Global.Mlog.Info($"Bottom_Step => CV Side Centering BWD");
                        Global.Mlog.Info($"Bottom_Step => CV Up Centering BWD");

                        // RZ Pincup Up
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == true)
                        {
                            Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                            Global.Mlog.Info($"Bottom_Step => Right Z UP");
                        }
                        _BottomHandlerTimeDelay.Restart();
                        Bottom_Step = BottomHandle.Set_PutDown_Done;
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_PutDown_Done");
                    }
                    break;
                case BottomHandle.Set_PutDown_Done:
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_2_BWD] == true
                       && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DOWN_CYL_SS] == true)
                       || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // Bottom Handler Pickup 완료 상태이면 
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == true)
                        {
                            Bottom_Step = BottomHandle.Bottom_PicUp_Done;
                            Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_PicUp_Done");
                        }
                        else
                        {
                            _BottomHandlerTimeDelay.Restart();
                            Bottom_Step = BottomHandle.Bottom_Clmap_Pickup;
                            Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Clmap_Pickup");
                        }
                        // 여기서 CV Move 한다.
                        SingletonManager.instance.BottomClampDone = true;
                        Global.Mlog.Info($"Bottom_Step => BottomClampDone : true");
                    }
                    break;
                case BottomHandle.Bottom_Clmap_Pickup:
                    // Bottom Handler Pickup 완료 상태이면 
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == true)
                    {
                        Bottom_Step = BottomHandle.Bottom_PicUp_Done;
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_PicUp_Done");
                        break;
                    }
                    // Return CV에 bottom clamp가 있으면 
                    if ( Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        _BottomHandlerTimeDelay.Restart();
                        Bottom_Step = BottomHandle.Bottom_Clamp_Grip;
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);

                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_RZ_Down_Wait");
                    }
                    // Y Pickup 필요할때 바텀지그가 3초안에 들어오지 않으면 left이동했다가 다시 픽업으로온다
                    else if (SingletonManager.instance.IsY_PickupColl == true
                        && _BottomHandlerTimeDelay.ElapsedMilliseconds > 1000)
                    {
                        // Bottom Pickup Move , false:Right  true:Left
                        Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, true);

                        Global.Mlog.Info($"Bottom_Step => Right Handler Pickup Timeout => Left Move");
                        Global.Mlog.Info($"Bottom_Step => Next Step : ClampInSensorCheck");
                        Bottom_Step = BottomHandle.ClampInSensorCheck;
                    }
                    break;
                case BottomHandle.Bottom_Clamp_Grip:
                    // Bottom clamp Grip
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_DOWN_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_GRIP_SOL, true);
                        Bottom_Step = BottomHandle.Bottom_Handler_Up;

                        Global.Mlog.Info($"Bottom_Step => Grip");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Handler_Up");
                        _BottomHandlerTimeDelay.Restart();
                    }
                    break;
                case BottomHandle.Bottom_Handler_Up:
                    // Bottom handler Up
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                        Bottom_Step = BottomHandle.Bottom_PicUp_Done;

                        Global.Mlog.Info($"Bottom_Step => Right Z Up");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_PicUp_Done");
                    }
                    else if (_BottomHandlerTimeDelay.ElapsedMilliseconds > 2000)
                    {
                        // Grip이 안되면 에러로 처리한다.
                        Dio_Output(DO_MAP.TRANSFER_RZ_GRIP_SOL, false);
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                        ClampFailMassage = "Bottom Clamp Pickup Grip Fail\r\n(바텀 클램프 그립 실패)";
                        Bottom_Step = BottomHandle.Bottom_Clmap_Pickup;
                        Global.Mlog.Info($"Bottom_Step => Bottom Clamp Pickup Grip Fail");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Clmap_Pickup");
                    }
                    break;
                case BottomHandle.Bottom_PicUp_Done:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true)
                    {
                        // Panel Grip Return
                        Dio_Output(DO_MAP.TRANSFER_LZ_TURN_SOL, false);
                        // Panel Pickup Move
                        Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, true);
                        Bottom_Step = BottomHandle.Set_PickUp_Down;
                        ClampRetryCount = 0;
                        Global.Mlog.Info($"Bottom_Step => Left Z Turn Off");
                        Global.Mlog.Info($"Bottom_Step => X Left Move");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_PickUp_Down");
                    }
                    break;
                case BottomHandle.Set_PickUp_Down:
                    // Set Pickup으로 Turn하고 전진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_TURN_CYL_SS] == true)
                    {
                        // In CV 시료 있고
                        // In CV 정지 상태 & Incv Centering FWD
                        // Bottom putdown 위치 클램프 없고 cv 정지 상태
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == true      // In set sensor
                            && Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_SS_3] == true       // In set sensor
                            && Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_UNALIGN_CYL_SS] == true    // In set CV Sentering FWD
                            && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == false// Bottom Clamp Putdown 센서 Off
                            && Dio.DO_RAW_DATA[(int)DO_MAP.INPUT_SET_CV_RUN] == false       // In set CV Stop상태
                            && Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_RUN] == false)
                            || (_NoneSetTest == true && Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_RUN] == false)
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)       // Clamp CV Stop상태
                        {
                            // Conveyor 센터링 풀고 동시 Down한다.
                            Dio_Output(DO_MAP.CLAMPING_CV_UP_SOL, false);
                            Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_2, false);

                            Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, true);
                            Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);

                            Global.Mlog.Info($"Bottom_Step => CV UP Stopper Sol Off");
                            Global.Mlog.Info($"Bottom_Step => Clamp Centering BWD");
                            Global.Mlog.Info($"Bottom_Step => Right Z Down");
                            Global.Mlog.Info($"Bottom_Step => Left Z Down");
                            
                            // Down 하고나서 NFC Use 이면 먼저 MES SEND를 하고 나중에 결과를 확인한다.
                            Global.Mlog.Info($"Bottom_Step => NFC UseNotuse : {SingletonManager.instance.SystemModel.NfcUseNotUse.ToString()}");
                            if (SingletonManager.instance.SystemModel.NfcUseNotUse == "Use")
                            {
                                NFC_Data = SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].NfcData;
                                Global.Mlog.Info($"Bottom_Step => NFC Data : {NFC_Data}");
                                if (!string.IsNullOrEmpty(NFC_Data))
                                {
                                    //SingletonManager.instance.HttpJsonModel.SendRequest("getPrevInspInfo", nfc, "");
                                    SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Mes].SendMes(NFC_Data); // MES Data 전송
                                    
                                    Global.Mlog.Info($"Bottom_Step => MES Data Send : {NFC_Data}");
                                    SingletonManager.instance.Channel_Model[0].CnNomber = NFC_Data;
                                    SingletonManager.instance.Channel_Model[0].MesResult = "--";
                                }
                                else
                                {
                                    SingletonManager.instance.ClampResult = false;

                                    SingletonManager.instance.Channel_Model[0].CnNomber = "Empty";
                                    SingletonManager.instance.Channel_Model[0].MesResult = "NG"; 
                                    Global.Mlog.Info($"Bottom_Step => NFC Read = Empty");
                                    Global.instance.WriteAlarmLog("NFC Read = Empty");
                                }
                            }
                            else
                            {
                                SingletonManager.instance.Channel_Model[0].CnNomber = "--";
                                SingletonManager.instance.Channel_Model[0].MesResult = "Not Use";

                                SingletonManager.instance.ClampResult = true;
                                Global.Mlog.Info($"Bottom_Step => NFC Result = Not Use");
                            }
                            
                            Bottom_Step = BottomHandle.Handler_Down_Check;
                            Global.Mlog.Info($"Bottom_Step => Next Step : Handler_Down_Check");
                            break;
                        }
                    }
                    break;
                case BottomHandle.Handler_Down_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_DOWN_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_DOWN_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_GRIP_SOL, false);
                        Dio_Output(DO_MAP.IN_SET_CV_CENTERING, false);
                        Bottom_Step = BottomHandle.Ungrip_InCenteringBwd_Check;
                        Global.Mlog.Info($"Bottom_Step => Next Step : Ungrip_InCenteringBwd_Check");
                    }
                    break;
                case BottomHandle.Ungrip_InCenteringBwd_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_UNALIGN_CYL_SS] == false)
                    {
                        // R Z UP
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                        
                        // CV Up Stopper 상승
                        Global.Mlog.Info($"Bottom_Step => Cmaping CV Up Sol On");
                        Dio_Output(DO_MAP.CLAMPING_CV_UP_SOL, true);

                        Global.Mlog.Info($"Bottom_Step => Vacuum On");
                        if (SingletonManager.instance.EquipmentMode == EquipmentMode.Auto)
                            Dio_Output(DO_MAP.TRANSFER_LZ_VACUUM_SOL, true);

                        Global.Mlog.Info($"Bottom_Step => Righ Z UP");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_Vacuum_On");
                        Bottom_Step = BottomHandle.Set_Vacuum_On;
                    }
                    break;
                case BottomHandle.Set_Vacuum_On:
                    // Vacuum On
                    // RZ Down 센서가 Off이면
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_DOWN_CYL_SS] != true
                        && (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] == true
                        || _NoneSetTest == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry))
                    {
                        if (SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            Bottom_Step = BottomHandle.Set_PickUp_Up;
                            SingletonManager.instance.ClampResult = true;
                        }

                        if (SingletonManager.instance.SystemModel.NfcUseNotUse == "Use")
                        {
                            if (!string.IsNullOrEmpty(NFC_Data))
                            {
                                _BottomHandlerTimeDelay.Restart();
                                Bottom_Step = BottomHandle.MES_Receive_Check;
                            }
                            else
                            {
                                Bottom_Step = BottomHandle.Set_PickUp_Up;
                                // Set Handler Up
                                Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                            }
                        }
                        else
                        {
                            Bottom_Step = BottomHandle.Set_PickUp_Up;
                            // Set Handler Up
                            Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                        }
                    }
                    break;
                case BottomHandle.MES_Receive_Check:
                    if (SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Mes].IsReceived == true)
                    {
                        Global.Mlog.Info($"Bottom_Step => MES Result : {SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Mes].MesResult}");
                        string nfc;//= SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].NfcData;
                        if (SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Mes].MesResult == "OK")
                        {
                            // 진입 중인 SET cn 넘버가 리딩될수 있어서 보고시 사용했던 변수 사용한다.
                            nfc = SingletonManager.instance.Channel_Model[0].CnNomber;
                            SingletonManager.instance.Channel_Model[0].MesResult = "OK";

                            Global.instance.MES_LOG(nfc, "OK");
                            SingletonManager.instance.ClampResult = true;
                            Global.Mlog.Info($"Bottom_Step => MES Result : OK");

                            // Set Handler Up 할때 NFC DATA 초기화
                            SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].NfcData = string.Empty;
                            SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].IsReceived = false;
                        }
                        else
                        {
                            nfc = SingletonManager.instance.Channel_Model[0].CnNomber;
                            SingletonManager.instance.Channel_Model[0].MesResult = "NG";

                            Global.instance.MES_LOG(nfc, "NG");
                            Global.Mlog.Info($"Bottom_Step => MES Result : NG");
                            SingletonManager.instance.ClampResult = false;
                            Global.instance.WriteAlarmLog($"MES {nfc} = NG");

                            // Set Handler Up 할때 NFC DATA 초기화
                            SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].NfcData = string.Empty;
                            SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].IsReceived = false;
                        }

                        // Set Handler Up
                        Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                        Bottom_Step = BottomHandle.Set_PickUp_Up;
                    }
                    else if (_BottomHandlerTimeDelay.ElapsedMilliseconds > 2000)
                    {
                        string nfc = SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].NfcData;
                        // MES 응답이 없으면
                        SingletonManager.instance.Channel_Model[0].CnNomber = nfc;
                        SingletonManager.instance.Channel_Model[0].MesResult = "TIMEOUT";
                        Global.instance.MES_LOG(nfc, "TIMEOUT");
                        Global.Mlog.Info($"Bottom_Step => MES Result : TIMEOUT");
                        Global.instance.WriteAlarmLog($"MES {nfc} = TIMEOUT");

                        // Set Handler Up 할때 NFC DATA 초기화
                        SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].NfcData = string.Empty;
                        SingletonManager.instance.SerialModel[(int)Serial_Model.SerialIndex.Nfc].IsReceived = false;

                        SingletonManager.instance.ClampResult = false;
                        // Set Handler Up
                        Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                        Bottom_Step = BottomHandle.Set_PickUp_Up;
                    }
                    break;
                case BottomHandle.Set_PickUp_Up:
                    // Set CV Out centering Backward
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLMAPING_CV_UP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_LZ_TURN_SOL, true);
                        Bottom_Step = BottomHandle.Bottom_PutDown_Done;
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_PutDown_Down");
                    }
                    break;
                case BottomHandle.Bottom_PutDown_Done:
                    // Out X Handler 동작하지 않을때를 까지 대기한다.
                    if (SingletonManager.instance.IsY_PickupColl == false
                        && Ez_Model.IsOutHandlerYSafetyPos() == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == false
                        && SingletonManager.instance.BottomClampDone == false)
                    {
                        Global.Mlog.Info($"Bottom_Step => BottomClampDone : false");
                        Global.Mlog.Info($"Bottom_Step => IsY_PickupColl : false");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Idle");
                        Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, false);
                        Bottom_Step = BottomHandle.Idle;
                    }
                    break;
            }
            int step = (int)Bottom_Step;
            Global.instance.Write_Sequence_Log("BOTTOM_STEP", step.ToString());
        }
        private void Top_Handel_Logic()
        {
            switch (Top_Handle_Step)
            {
                case TopHandle.Idle:
                    // Up sensor Check 하여 하강 상태이면 Up하고 unGrip한다
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == false)
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == false)
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS] == true)
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                    Top_Handle_Step = TopHandle.Top_Handle_Up_Check;

                    Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_Up_Check");
                    break;
                case TopHandle.Top_Handle_Up_Check:
                    // up 상태이면 Pickup위치로 이동하여 대기한다.
                    // Y 축 pickup 위치 확인하는거는 의미 없는거 같은데 충돌 방지를 위해서 일단 두자
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS] == true
                        && Ez_Model.IsOutHandlerPickupPosY() != true)
                    {
                        Global.Mlog.Info($"TopHandle => MoveTopHandlerPickUpPos");
                        if (Ez_Model.MoveTopHandlerPickUpPos() == true)
                        {
                            Top_Handle_Step = TopHandle.Top_Handle_Pickup_Position_Check;
                            Global.Mlog.Info($"TopHandle =>  Next Step : Top_Handle_Pickup_Position_Check");
                        }
                    }
                    
                    break;
                case TopHandle.Top_Handle_Pickup_Position_Check:
                    // pickup위치에 도착확인
                    if (Ez_Model.IsTopHandlerPickUpPos() == true)
                    {
                        // Top Clamping 완료한 상태이면 X Handle Coll Flag ON
                        //Global.Mlog.Info($"TopHandle =>  TopClampingDone : {TopClampingDone.ToString()}");
                        // Top Clamp가 조립되여 있으면
                        //if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        //    && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                        //{
                        //    // X Handle Pickup완료 후 Clamping 상태 변경
                        //    SingletonManager.instance.IsY_PickupColl = true;
                        //    Global.Mlog.Info($"TopHandle =>  IsY_PickupColl : true");
                        //}
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            _TopHandlerTimeDelay.Restart();
                            Top_Handle_Step = TopHandle.Top_PickUp_Time_Wait;
                            Global.Mlog.Info($"TopHandle =>  Next Step : Top_PickUp_Time_Wait");
                        }
                    }
                    break;
                case TopHandle.Top_PickUp_Time_Wait:
                    if (_TopHandlerTimeDelay.ElapsedMilliseconds > 200)
                    {
                        Global.Mlog.Info($"TopHandle =>  BarcodeUseNotUse : {SingletonManager.instance.SystemModel.BcrUseNotUse}");
                        if (SingletonManager.instance.SystemModel.BcrUseNotUse == "Use")
                        {
                            Top_Handle_Step = TopHandle.Top_BarCode_Read;
                            Global.Mlog.Info($"TopHandle =>  Next Step : Top_BarCode_Read");
                        }
                        else
                        {
                            Top_Handle_Step = TopHandle.Top_Clamp_PickupDown;
                            Global.Mlog.Info($"TopHandle =>  Next Step : Top_Clamp_PickupDown");
                        }
                    }
                    break;
                case TopHandle.Top_BarCode_Read:
                    Global.Mlog.Info($"TopHandle =>  Barcode Trig");
                    SingletonManager.instance.SerialModel[0].SendBcrTrig();
                    _TopHandlerTimeDelay.Restart();
                    Top_Handle_Step = TopHandle.Top_BarCode_Read_Done;

                    Global.Mlog.Info($"TopHandle =>  Next Step : Top_BarCode_Read_Done");
                    break;
                case TopHandle.Top_BarCode_Read_Done:
                    // 1초 안에 barcode data가 들어오는지 확인한다.
                    if (_TopHandlerTimeDelay.ElapsedMilliseconds < 2000)
                    {
                        if (SingletonManager.instance.SerialModel[0].IsReceived == true)
                        {
                            // barcode data가 들어오면 컨베어 도착위치 도착할때까지 남은시간 더 대기하고 down한다.
                            _BarCodeRetryCount = 0;
                            _BarCodeReadResult = true;
                            Global.Mlog.Info($"TopHandle =>  Barcode Read Success");
                            Global.Mlog.Info($"TopHandle =>  Barcode : {SingletonManager.instance.SerialModel[0].Barcode}");

                            var myIni = new IniFile(Global.instance.IniAgingPath + "\\AgingRecord.ini");
                            myIni.Write(SingletonManager.instance.SerialModel[0].Barcode, DateTime.Now.ToString("yyyyMMddHHmmss"), "AGING");
                            Top_Handle_Step = TopHandle.Top_Clamp_PickupDown;

                            Global.Mlog.Info($"TopHandle =>  Next Step : Top_Clamp_PickupDown");
                        }
                    }
                    else
                    {
                        _BarCodeRetryCount++;
                        if (_BarCodeRetryCount > 1)
                        {
                            // 1회 Retry하고 ng이면 ng 배출한다.
                            _BarCodeRetryCount = 0;
                            _BarCodeReadResult = false;
                            Top_Handle_Step = TopHandle.Top_Clamp_PickupDown;
                            Global.Mlog.Info($"TopHandle =>  Barcode Read Fail");
                            Global.Mlog.Info($"TopHandle =>  Next Step : Top_Clamp_PickupDown");
                        }
                        else
                        {
                            Top_Handle_Step = TopHandle.Top_BarCode_Read;
                            Global.Mlog.Info($"TopHandle =>  Barcode Read Retry");
                        }
                    }
                    break;
                
                case TopHandle.Top_Clamp_PickupDown:

                    Global.Mlog.Info($"TopHandle => Down Sol 1,2 Down");
                    Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, true);
                    if (SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                    else
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, true);
                    Top_Handle_Step = TopHandle.Top_Handle_Down_Check;

                    Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_Down_Check");
                    break;
                case TopHandle.Top_Handle_Down_Check:
                    // Down완료 후 Grip Lock
					//이부분 센서 오락가락 확인 필요**************/
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true
                        && (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_2] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry))
                    {
                        Global.Mlog.Info($"TopHandle => Grip");
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, true);
                        Top_Handle_Step = TopHandle.Top_Handle_Grip_Lock_Check;

                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_Grip_Lock_Check");
                    }
                    break;
                case TopHandle.Top_Handle_Grip_Lock_Check:
                    // Grip완료 후 up
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        Top_Handle_Step = TopHandle.Top_Handle_PutUp_Check;

                        Global.Mlog.Info($"TopHandle => Next Step : Down Sol 1,2 Up");
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_PutUp_Check");
                    }
                    else if (SingletonManager.instance.EquipmentMode == EquipmentMode.Dry
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS] == false)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        Top_Handle_Step = TopHandle.Top_Handle_PutUp_Check;
                    }
                    break;
                case TopHandle.Top_Handle_PutUp_Check:
                    // Up 완료 하면 Clamp 배출위치에 제품이 있는지 확인 한다
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                        && SingletonManager.instance.IsY_PickupColl == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == false
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Global.Mlog.Info($"TopHandle => Barcode UseNotUse : {SingletonManager.instance.SystemModel.BcrUseNotUse}");
                        if (SingletonManager.instance.SystemModel.BcrUseNotUse == "Use")
                        {
                            if (_BarCodeReadResult == true)
                            {
                                _BarCodeReadResult = false;
                                // Tray OK이면 안착위치로 이동
                                Top_Handle_Step = TopHandle.Buttom_Clamp_Arrival_Check;
                                Global.Mlog.Info($"TopHandle => Barcode : OK");
                                Global.Mlog.Info($"TopHandle => Next Step : Buttom_Clamp_Arrival_Check");
                            }
                            else
                            {
                                Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Move;
                                Global.Mlog.Info($"TopHandle => Barcode : NG");
                                Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_NG_Port_Move");
                            }
                        }
                        else
                        {
                            // Tray OK이면 안착위치로 이동
                            Global.Mlog.Info($"TopHandle => MoveTopHandlerPutDownPos");
                            if (Ez_Model.MoveTopHandlerPutDownPos() == true)
                            {
                                Top_Handle_Step = TopHandle.Top_Handle_PutDown_Move_Check;
                                Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_PutDown_Move_Check");
                            }
                        }
                        //if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        //   && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                        //{
                        //    // X Handle Pickup완료 후 Clamping 상태 변경
                        //    SingletonManager.instance.IsY_PickupColl = true;

                        //    //Global.Mlog.Info($"TopHandle => TopClampingDone : true");
                        //    Global.Mlog.Info($"TopHandle => IsY_PickupColl : true");
                        //}
                    }
                    // Lift 에 배출 대기가 싸여있을때 Y Pickup Coll Flag가 false로 되는 현상이 있다.
                    else if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                    {
                        if (SingletonManager.instance.IsY_PickupColl == false)
                        {
                            SingletonManager.instance.IsY_PickupColl = true;
                            Global.Mlog.Info($"Bottom_Step => IsY_PickupColl : true");
                        }
                    }
                    else if (SingletonManager.instance.SystemModel.BcrUseNotUse == "Use")
                    {
                        if (_BarCodeReadResult == false
                        && SingletonManager.instance.IsY_PickupColl == false)
                        {
                            Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Move;
                            Global.Mlog.Info($"TopHandle => Barcode : NG");
                            Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_NG_Port_Move");
                        }
                    }
                    // Dry run mode
                    if (SingletonManager.instance.IsY_PickupColl == false && SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // Tray OK이면 안착위치로 이동
                        Top_Handle_Step = TopHandle.Buttom_Clamp_Arrival_Check;

                    }
                    break;
                case TopHandle.Top_Handle_NG_Port_Move:
                    if (SingletonManager.instance.IsY_PickupColl == false)
                    {
                        Global.Mlog.Info($"TopHandle => MoveTopHandlerNGPort");
                        if (Ez_Model.MoveTopHandlerNGPort() == true)
                        {
                            Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Move_Check;
                            Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_NG_Port_Move_Check");
                        }
                    }
                    break;
                case TopHandle.Top_Handle_NG_Port_Move_Check:
                    // NG 안착위치에 도착했는지 확인 후 Down 한다
                    if (Ez_Model.IsMoveTopNGPortDone() == true)
                    {
                        // NG Port에 Clmap가 없으면 
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_TOP_CV_DETECT_SS_1] != true)
                        {
                            Global.Mlog.Info($"TopHandle => IsMoveTopNGPortDone");
                            Global.Mlog.Info($"TopHandle => TR Z Down 1,2");
                            Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_NG_Port_Down_check");
                            Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, true);
                            Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, true);
                            Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Down_check;
                        }
                    }
                    break;
                case TopHandle.Top_Handle_NG_Port_Down_check:
                    // NG Port Down 완료 후 Grip Unlock
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == false)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                        Top_Handle_Step = TopHandle.Top_Handle_NG_Port_UnGrip_check;

                        Global.Mlog.Info($"TopHandle => Ungrip");
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_NG_Port_UnGrip_check");
                    }
                    break;
                case TopHandle.Top_Handle_NG_Port_UnGrip_check:
                    // NG Port Down 완료 후 Grip Unlock
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Up_check;
                        Global.Mlog.Info($"TopHandle => TR Z Up");
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_NG_Port_Up_check");
                    }
                    break;
                case TopHandle.Top_Handle_NG_Port_Up_check:
                    // NG Port Down 완료 후 Grip Unlock
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                    {
                        Global.Mlog.Info($"TopHandle => Next Step : Idle");
                        Top_Handle_Step = TopHandle.Idle;
                    }
                    break;
                case TopHandle.Buttom_Clamp_Arrival_Check:
                    // Bottom Clamp 도착 확인 후 PutDown 한다
                    // Stoper가 Down되있는 상태는 Bottom Clamp NG 배출이기때문에 
                    //if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                    //    && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == true
                    //    && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == false
                    //    && SingletonManager.instance.IsY_PickupColl == false)
                    //    || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    if ((SingletonManager.instance.IsY_PickupColl == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == false)
                       || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Global.Mlog.Info($"TopHandle => MoveTopHandlerPutDownPos");
                        if (Ez_Model.MoveTopHandlerPutDownPos() == true)
                        {
                            Top_Handle_Step = TopHandle.Top_Handle_PutDown_Move_Check;
                            Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_PutDown_Move_Check");
                        }
                    }
                    break;
                case TopHandle.Top_Handle_PutDown_Move_Check:
                    // Putdown 위치도착후 clamping CV 불량 배출아니고 정지상태일때
                    // Bottom sensor On Top sensor Off 상태일때
                    if (Ez_Model.IsTopHandlerPutDownPos() == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_RUN] == false)
                    {
                        Global.Mlog.Info($"TopHandle => IsTopHandlerPutDownPos Done");

                        Global.Mlog.Info($"TopHandle => Centering FWD");
                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, true);
                        Top_Handle_Step = TopHandle.Top_Handle_Centering_FWD_Check;
                    }
                    break;
                case TopHandle.Top_Handle_Centering_FWD_Check:
                    // centering 전진 센서 확인 후 후진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_1_BWD] == false)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, true);

                        Global.Mlog.Info($"TopHandle => TR Z 1 Down");
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_PutDown_Check");
                        Top_Handle_Step = TopHandle.Top_Handle_PutDown_Check;
                        _TopHandlerTimeDelay.Restart();
                    }
                    else
                    {
                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, true);
                    }
                    break;
                case TopHandle.Top_Handle_PutDown_Check:
                    // PutDown 완료 후 grip UnLock
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                        Top_Handle_Step = TopHandle.Top_Handle_Grip_Unlock_Check;

                        Global.Mlog.Info($"TopHandle => Ungrip");
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_Grip_Unlock_Check");
                    }
                    else if (_TopHandlerTimeDelay.ElapsedMilliseconds > 2000)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        ClampFailMassage = "TOP Clmap Down Timeout\r\n(탑 클램프 조립 실패)";

                        Global.Mlog.Info($"TopHandle => Ungrip");
                        Global.Mlog.Info($"TopHandle => Z 1 Up");

                        Top_Handle_Step = TopHandle.Top_Handle_Centering_FWD_Check;
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_Centering_FWD_Check");
                    }
                    break;
                case TopHandle.Top_Handle_Grip_Unlock_Check:
                    // grip UnLock 
                    // Grip 센서만 해제 되면 바로 up 한다.
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS] == false)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);

                        Global.Mlog.Info($"TopHandle => TR Z Up");
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_PutDown_Up_Check");

                        Top_Handle_Step = TopHandle.Top_Handle_PutDown_Up_Check;
                    }
                    break;
                case TopHandle.Top_Handle_PutDown_Up_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                    && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                    {
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                           && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                        {
                            // X Handle Pickup완료 후 Clamping 상태 변경
                            SingletonManager.instance.IsY_PickupColl = true;

                            Global.Mlog.Info($"TopHandle => IsY_PickupColl : true");
                        }
                        // Top Clamping 완료 상태 변경
                        Top_Handle_Step = TopHandle.Idle;
                        Global.Mlog.Info($"TopHandle => Next Step : Idle");
                        Ez_Model.MoveTopHandlerPickUpPos();
                    }
                    break;
            }
            int step = (int)Top_Handle_Step;
            Global.instance.Write_Sequence_Log("TOP_STEP", step.ToString());
            Global.instance.Write_Sequence_Log("Y_PICKUP_COLL_FLAG", SingletonManager.instance.IsY_PickupColl.ToString());
        }
        private void Out_Handle_Y_Logic()
        {
            switch(Out_Handle_Step)
            {
                case OutHandle.Idle:
                    Out_Handle_Step = OutHandle.Out_Handle_Z_Up;
                    break;
                case OutHandle.Out_Handle_Z_Up:
                    if (Ez_Model.IsOutHandlerReadyDoneZ() != true)
                        Ez_Model.MoveOutHandlerRadyZ();
                    Out_Handle_Step = OutHandle.Out_Handle_Z_Up_Done;
                    Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Z_Up_Done");
                    break;
                case OutHandle.Out_Handle_Z_Up_Done:
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true)
                    {
                        // Out Handle Z up 완료 후 Tray Sensor 확인
                        Out_Handle_Step = OutHandle.Top_Tray_Sensor_Check;
                        if (SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] == 0)
                        {
                            _AgingCvStepRunTime.Restart();
                        }
                        Global.Mlog.Info($"Out_Handle_Step => IsOutHandlerReadyDoneZ");
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Top_Tray_Sensor_Check");
                    }
                    break;
                case OutHandle.Out_Handle_Y_Ready_Done:
                    if (Ez_Model.IsMoveOutHandlerReadyY() == true)
                    {
                        Global.Mlog.Info($"Out_Handle_Step => IsMoveOutHandlerReadyY");
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Top_Tray_Sensor_Check");
                        Out_Handle_Step = OutHandle.Top_Tray_Sensor_Check;
                    }
                    break;
                case OutHandle.Top_Tray_Sensor_Check:
                    // Tray Sensor 들어오고 & Top Handle이 PIckup위치에 있을때
                    // Bottom Handle  Set Pickup위치에서 Down된 상태이면 
                    // Out Handle Pickup위치로 이동한다.
                    if (SingletonManager.instance.IsY_PickupColl == true
                        && Ez_Model.IsTopHandlerPutDownPos() != true                     // Top Handle PutDown위치가 아니면 
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true   // Out CV 배출 Bottom Tray Sensor
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true   // Out CV 배출 Top Tray Sensor
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true)   // Bottom Handle Left 위치 도착
                        //&& (SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] < 3)) 
                    {
                        Global.Mlog.Info($"Out_Handle_Step => Pickup Start");
                        // Out Handle Y축 Pickup위치로 이동
                        if (Ez_Model.MoveOutHandlerPickUpY() == true)
                        {
                            Global.Mlog.Info($"Out_Handle_Step => Move Pickup Position");
                            Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Y_Pickup_Pos_Check");
                            Out_Handle_Step = OutHandle.Out_Handle_Y_Pickup_Pos_Check;
                        }
                    }
                    else if (Ez_Model.IsMoveOutHandlerReadyY() != true)
                    {
                        // Pickup 준비완료 되지 않았으면 Y를 Ready위치로 먼저 이동한다.
                        Ez_Model.MoveOutHandlerReadyY();
                        Out_Handle_Step = OutHandle.Out_Handle_Y_Ready_Done;
                        Global.Mlog.Info($"Out_Handle_Step => Move Ready Y (Waiting)");
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Y_Ready_Done");
                    }
                    else if (SingletonManager.instance.EquipmentMode == EquipmentMode.Auto 
                        && SingletonManager.instance.SystemModel.PickUpWaitTimeOutY > 0)
                    {
                        // Y PickUp Time out 발생시 현제 로딩한 만큼 배출
                        if (SingletonManager.instance.SystemModel.PickUpWaitTimeOutY * 1000 < _AgingCvStepRunTime.ElapsedMilliseconds)
                        {
                            Global.Mlog.Info($"OutHandle => Y PickUp Time Out : {SingletonManager.instance.SystemModel.PickUpWaitTimeOutY.ToString()}");
                            Global.Mlog.Info($"OutHandle => Lift No : {SingletonManager.instance.LoadStageNo.ToString()}");
                            // Clamp 배출한다.
                            SingletonManager.instance.LoadComplete[SingletonManager.instance.LoadAgingCvIndex] = true;
                            // 적제 층수 초기화
                            SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = 0;
                            // Lift 인덱스 증가
                            SingletonManager.instance.LoadStageNo += 1;
                            if (SingletonManager.instance.LoadStageNo >= (int)Lift_Index.Max)
                                SingletonManager.instance.LoadStageNo = 0;

                            SingletonManager.instance.LoadAgingCvIndex += 1;
                            if (SingletonManager.instance.LoadAgingCvIndex >= 6)
                                SingletonManager.instance.LoadAgingCvIndex = 0;
                            Out_Handle_Step = OutHandle.Idle;
                        }
                        else if (_AgingCvStepRunTime.ElapsedMilliseconds == 0)
                            _AgingCvStepRunTime.Restart();
                    }
                    // Dry run mode
                    if (SingletonManager.instance.IsY_PickupColl == true
                        && Ez_Model.IsTopHandlerPickUpPos() == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true
                        && SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // Out Handle Y축 Pickup위치로 이동
                        if (Ez_Model.MoveOutHandlerPickUpY() == true)
                        {
                            Global.instance.LoadingTactTimeStart();
                            Global.instance.TactTimeStart = true;
                            Out_Handle_Step = OutHandle.Out_Handle_Y_Pickup_Pos_Check;
                        }
                    }
                    break;
                case OutHandle.Out_Handle_Y_Pickup_Pos_Check:
                    // Out Handle Pickup위치 도착완료하면 z Down 한다.
                    if (Ez_Model.IsOutHandlerPickupPosY() == true)
                    {
                        // Ungrip 상태가 아니면 알람 처리
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_UNGRIP_CYL_SS] == false)
                        {
                            ClampFailMassage = "Loading Handler not Ungrip\r\n(로딩 언그립 알람)";
                            break;
                        }
                        Global.Mlog.Info($"Out_Handle_Step => IsOutHandlerPickupPosY");
                        // Top Clamping 완료 위치 제품있는지 확인 
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            Global.Mlog.Info($"Out_Handle_Step => Move Pickup Z");
                            // Out Handle Z down
                            Ez_Model.MoveOutHandlerPickUpZ();
                            Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                            Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, false); // stopper down
                            
                            Out_Handle_Step = OutHandle.Out_Handle_Z_Down_Done;
                            Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Z_Down_Done");
                        }
                    }
                    // Y 축 충돌 방지용
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] != true)
                    {
                        Ez_Model.ServoMovePause((int)ServoSlave_List.Out_Y_Handler_Y,1);
                        Out_Handle_Step = OutHandle.Y_Move_Wait;
                    }
                    break;
                case OutHandle.Y_Move_Wait:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true)
                    {
                        Ez_Model.ServoMovePause((int)ServoSlave_List.Out_Y_Handler_Y, 0);
                        Out_Handle_Step = OutHandle.Out_Handle_Y_Pickup_Pos_Check;
                    }
                    break;
                case OutHandle.Out_Handle_Z_Down_Done:
                    // Out Handle Z down 완료하면 Grip lock
                    if (Ez_Model.IsOutHandlerPickUpZ() == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_1_BWD] == true) 
                    {
                        Global.Mlog.Info($"Out_Handle_Step => Loading Handler Grip");
                        // Out Handle Grip Lock
                        Dio_Output(DO_MAP.CLAMPING_LD_Z_GRIP_SOL, true);
                        Out_Handle_Step = OutHandle.Out_Handle_Grip_Check;
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Grip_Check");
                    }
                    else if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] != false
                        || Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_1_BWD] != true)
                    {
                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, false); // stopper down
                    }
                    break;
                case OutHandle.Out_Handle_Grip_Check:
                    // Grip lock완료하면 Z up
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_UNGRIP_CYL_SS] == false)
                    {
                        _TimeDelay.Restart();
                        Out_Handle_Step = OutHandle.Out_Handle_Grip_Wait;
                    }
                    else if ((SingletonManager.instance.EquipmentMode == EquipmentMode.Dry
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true)
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == false)
                    {
                        _TimeDelay.Restart();
                        Out_Handle_Step = OutHandle.Out_Handle_Grip_Wait;
                    }
                    break;
                case OutHandle.Out_Handle_Grip_Wait:
                    if (_TimeDelay.ElapsedMilliseconds >100)
                    {
                        Global.Mlog.Info($"Out_Handle_Step => Move Ready Z");
                        // Out Handle Z up
                        Ez_Model.MoveOutHandlerRadyZ();
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        Out_Handle_Step = OutHandle.Out_Handle_Z_Pickup_Up_Done;
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Z_Pickup_Up_Done");
                    }
                    break;
                case OutHandle.Out_Handle_Z_Pickup_Up_Done:
                    // Z up완료 후 X PutDown위치 이동
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true 
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == true)
                    {
                        // Grip UP 후 Bottom clamp가 남아있으면 알람 처리
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true)
                        {
                            ClampFailMassage = "Loading Clamp Grip Fail\r\n(로딩 그립퍼 픽업 알람)";
                            Out_Handle_Step = OutHandle.Out_Handle_Y_Pickup_Pos_Check;
                            Global.Mlog.Info($"Out_Handle_Step => Loading Clamp Grip Fail");
                            Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Z_Pickup_Up_Done");
                            break;
                        }
                        // Dry run 일때는 Put Down 하기전에 이전 데이터를 초기화
                        if (SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            int floorMax = 5;
                            if (SingletonManager.instance.SystemModel.LoadFloorCount > 0)
                                floorMax = SingletonManager.instance.SystemModel.LoadFloorCount;

                            if (SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] >= floorMax)
                            {
                                SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = 0;
                                // Auto Ui 초기화
                                for (int i = 0; i < floorMax; i++)
                                    SingletonManager.instance.Display_Lift[SingletonManager.instance.LoadStageNo].Floor[i] = false;
                            }
                        }
                        // Out Handle X PutDown위치로 이동
                        if (Ez_Model.MoveOutHandlerPutDownY() == true)
                        {
                            Global.Mlog.Info($"Out_Handle_Step => Move Putdown Y");
                            Out_Handle_Step = OutHandle.Out_Handle_Y_PutDown_Pos_Check;
                            Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Y_PutDown_Pos_Check");
                        }
                    }
                    // stopper가 없이 되여 있지 않으면 다시 up을 한다.
                    else if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] != true)
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                    break;
                case OutHandle.Out_Handle_Y_Safety_Pos:
                    // Y Pickup 후 Lift 1번 또는 Lift 1번위치보다 멀면
                    if (Ez_Model.IsOutHandlerYSafetyPos() == true)
                    {
                        SingletonManager.instance.IsY_PickupColl = false;
                        Out_Handle_Step = OutHandle.Out_Handle_Y_PutDown_Pos_Check;
                        Global.Mlog.Info($"Out_Handle_Step => IsY_PickupColl : false");
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Y_PutDown_Pos_Check");
                    }
                    break;
                case OutHandle.Out_Handle_Y_PutDown_Pos_Check:
                    // X Putdown위치 도착하면 기록한 층수위에 안착한다.
                    // 안착 포지션은 1,2,3 순으로 놓는다
                    // Y Pickup 후 Lift 1번 또는 Lift 1번위치보다 멀면
                    if (Ez_Model.IsOutHandlerYSafetyPos() == true)
                    {
                        SingletonManager.instance.IsY_PickupColl = false;
                        Out_Handle_Step = OutHandle.Out_Handle_Y_PutDown_Pos_Check;
                        Global.Mlog.Info($"Out_Handle_Step => IsY_PickupColl : false");
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Y_PutDown_Pos_Check");
                    }
                    if (Ez_Model.IsOutHandlerYPutDownPos() == true)
                    {
                        bool DetectSS = false;
                        if (SingletonManager.instance.LoadStageNo == (int)Lift_Index.Lift_1)
                        {
                            DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_IN_SS_1];
                        }
                        else if (SingletonManager.instance.LoadStageNo == (int)Lift_Index.Lift_2)
                        {
                            DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_IN_SS_1];
                        }
                        else if (SingletonManager.instance.LoadStageNo == (int)Lift_Index.Lift_3)
                        {
                            DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_IN_SS_1];
                        }
                        //SingletonManager.instance.IsY_PickupColl = false;
                        // Out Handle Z down
                        if (Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true
                            && SingletonManager.instance.LoadComplete[SingletonManager.instance.LoadAgingCvIndex] == false
                            && ((SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] == 0 && DetectSS == false)
                                || SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] > 0))
                        {
                            //Ez_Model.MoveLiftLoding(SingletonManager.instance.LoadStageNo);
                            //Global.Mlog.Info($"Out_Handle_Step => Lift Move Loading Position");
                            //Out_Handle_Step = OutHandle.Lift_Loding_Move_Done;
                            // Out Handle Z down
                            if (Ez_Model.MoveOutHandlerPutDownZ() == true)
                            {
                                Global.Mlog.Info($"Out_Handle_Step => Move PutDown Z  Floor : {SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] + 1}");
                                Out_Handle_Step = OutHandle.Out_Handle_Z_PutDown_Done;
                                Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Y_PutDown_Pos_Check");
                                _TimeDelay.Restart();
                            }
                            else
                            {
                                ClampFailMassage = "Loading Z PutDown Fail.\r\n(로딩 Z 축 적제 실패, 서보 조치 후 재 시작하세요)";
                            }
                        }
                    }
                    
                    break;
                //case OutHandle.Lift_Loding_Move_Done:
                //    if (Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true)
                //    {
                //        // Out Handle Z down
                //        if (Ez_Model.MoveOutHandlerPutDownZ() == true)
                //        {
                //            Global.Mlog.Info($"Out_Handle_Step => Move PutDown Z  Floor : {SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo]+1}");
                //            Out_Handle_Step = OutHandle.Out_Handle_Z_PutDown_Done;
                //            Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Y_PutDown_Pos_Check");
                //        }
                //    }
                //    break;
                case OutHandle.Out_Handle_Z_PutDown_Done:
                    // Z Down완료 후 Ungrip
                    if (Ez_Model.IsOutHandlerPutDownDoneZ() == true)
                    {
                        Global.Mlog.Info($"Out_Handle_Step => Loading Z Ungrip");
                        // Out Handle UnGrip 
                        Dio_Output(DO_MAP.CLAMPING_LD_Z_GRIP_SOL, false);
                        Out_Handle_Step = OutHandle.Out_Handle_UnGrip_Check;
                        // Tact Time 종료
                        Global.instance.TactTimeStart = false;
                        Global.Mlog.Info($"TactTime : {SingletonManager.instance.Channel_Model[0].TactTime.ToString()}");
                        Global.TTlog.Info($"TACKTIME : {SingletonManager.instance.Channel_Model[0].TactTime.ToString()}");

                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_UnGrip_Check");
                    }
                    else if (_TimeDelay.ElapsedMilliseconds > 5000)
                    {
                        ClampFailMassage = "Loading Z PutDown Timeout.\r\n(로딩 Z 축 적제 실패, 서보 조치 후 재 시작하세요)";
                        Out_Handle_Step = OutHandle.Out_Handle_Z_PutDown_Done;
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Y_PutDown_Pos_Check");
                    }
                    break;
                case OutHandle.Out_Handle_UnGrip_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_UNGRIP_CYL_SS] == true)
                    {
                        Global.instance.LoadingTactTimeStart();
                        Global.instance.TactTimeStart = true;

                        Ez_Model.MoveOutHandlerRadyZ();
                        Out_Handle_Step = OutHandle.Out_Handle_Z_Ready_Check;
                        Global.Mlog.Info($"Out_Handle_Step => Move Ready Z");
                        Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Z_Ready_Check");
                    }
                    break;
                case OutHandle.Out_Handle_Z_Ready_Check:
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true)
                    {
                        // 적제 단수 증가
                        int floor = SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo];
                        SingletonManager.instance.Display_Lift[SingletonManager.instance.LoadStageNo].Floor[floor] = true;

                        SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] += 1;

                        int floorMax = 5;
                        if (SingletonManager.instance.SystemModel.LoadFloorCount > 0)
                            floorMax = SingletonManager.instance.SystemModel.LoadFloorCount;
                        Global.Mlog.Info($"Out_Handle_Step => Now Floor - {SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo]}");

                        if (SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] >= floorMax)
                        {
                            // 적제 완료 했으면 Complete 상태 변경한다. false 원복은 Aging C/V에 배출 후 변경한다.
                            SingletonManager.instance.LoadComplete[SingletonManager.instance.LoadAgingCvIndex] = true;

                            SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = 0;
                            //_TimeDelay.Restart();
                            SingletonManager.instance.LoadStageNo += 1;
                            if (SingletonManager.instance.LoadStageNo >= (int)Lift_Index.Max)
                                SingletonManager.instance.LoadStageNo = 0;

                            SingletonManager.instance.LoadAgingCvIndex += 1;
                            if (SingletonManager.instance.LoadAgingCvIndex >= 6)
                                SingletonManager.instance.LoadAgingCvIndex = 0;

                            //Out_Handle_Step = OutHandle.Lift_Out_Wait;
                            Global.Mlog.Info($"Out_Handle_Step => Next Lift - {SingletonManager.instance.LoadStageNo+1}");
                            Global.Mlog.Info($"Out_Handle_Step => Next Step : Out_Handle_Z_Ready_Check");
                        }
                        Global.Mlog.Info($"Out_Handle_Step => Next Floor - {SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo]}");
                        //else
                        Out_Handle_Step = OutHandle.Idle;

                        Global.instance.LoadCountPlus();
                    }
                    break;
                case OutHandle.Lift_Out_Wait:
                    //bool LiftWait = false;
                    //if (_TimeDelay.ElapsedMilliseconds < 200)
                    //    break;
                    // 적제 완료 후 이동 대기하는 부분 삭제 . 5단 적제로 변경. 20250614 (김승완 프로)
                    /*
                    if (SingletonManager.instance.LoadStageNo == 0)
                    {
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_IN_SS_1] == false
                            && Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_OUT_SS_2] == false)
                            || (SingletonManager.instance.AgingCvIndex < 3 && Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true)
                            || SingletonManager.instance.AgingCvIndex >= 3)
                        {
                            LiftWait = true;
                        }
                    }
                    else if(SingletonManager.instance.LoadStageNo == 1)
                    {
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_IN_SS_1] == false
                            && Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_OUT_SS_2] == false)
                            || (SingletonManager.instance.AgingCvIndex < 3 && Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true)
                            || SingletonManager.instance.AgingCvIndex >= 3)
                        {
                            LiftWait = true;
                        }
                    }
                    else if (SingletonManager.instance.LoadStageNo == 2)
                    {
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_IN_SS_1] == false
                            && Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_OUT_SS_2] == false)
                            || (SingletonManager.instance.AgingCvIndex < 3 && Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true)
                            || SingletonManager.instance.AgingCvIndex >= 3)
                        {
                            LiftWait = true;
                        }
                    }
                    if (LiftWait == true )
                    */
                    {
                        // 7단 적제 완료하면 다음 Load 위치로 설정
                        SingletonManager.instance.LoadStageNo += 1;
                        if (SingletonManager.instance.LoadStageNo >= (int)Lift_Index.Max)
                            SingletonManager.instance.LoadStageNo = 0;

                        //LiftNextIndex(SingletonManager.instance.AgingCvIndex);

                        Out_Handle_Step = OutHandle.Idle;
                    }
                    break;
            }
            int step = (int)Out_Handle_Step;
            Global.instance.Write_Sequence_Log("OUT_HANDLER_STEP", step.ToString());
            Global.instance.Write_Sequence_Log("OUT_LOAD_FLOOR", SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo].ToString());
            Global.instance.Write_Sequence_Log("OUT_LOAD_STAGE", SingletonManager.instance.LoadStageNo.ToString());
        }
        public void Aging_CV_Logic()
        {
            switch(AgingCVStep)
            {
                case Aging_CV_Step.Idle:
                    // 변수 초기화
                    AgingCvFull[SingletonManager.instance.AgingCvIndex] = false;
                    AgingCvStart[SingletonManager.instance.AgingCvIndex] = false;
                    AgingCvEndStopCondition[SingletonManager.instance.AgingCvIndex] = false;
                    AgingCvInStopCondition[SingletonManager.instance.AgingCvIndex] = false;

                    AgingCVStep = Aging_CV_Step.CV_On_Condition_Wait;
                    break;
                case Aging_CV_Step.CV_On_Condition_Wait:
                    // Lift에 Clamp가 있는지 확인한다.
                    bool DetectSS= false;
                    if (SingletonManager.instance.AgingCvIndex == 0 || SingletonManager.instance.AgingCvIndex == 3) 
                    { DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_IN_SS_1]; }
                    if (SingletonManager.instance.AgingCvIndex == 1 || SingletonManager.instance.AgingCvIndex == 4) 
                    { DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_IN_SS_1]; }
                    if (SingletonManager.instance.AgingCvIndex == 2 || SingletonManager.instance.AgingCvIndex == 5) 
                    { DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_IN_SS_1]; }

                    if (SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] == true
                        && DetectSS == true)
                    {
                        
                        GetAgingCVStartEndSS(SingletonManager.instance.AgingCvIndex);
                        // Upper
                        if (SingletonManager.instance.AgingCvIndex < 3)
                        {
                            Ez_Model.MoveLiftUp(SingletonManager.instance.AgingCvIndex);
                            AgingCVStep = Aging_CV_Step.Loading_Life_Up_Wait;
                        }
                        // Low
                        else
                        {
                            // Lift Down Step으로 이동
                            if (LiftLowMoveConditon(SingletonManager.instance.AgingCvIndex) == true)
                            {
                                Ez_Model.MoveLiftDown(SingletonManager.instance.AgingCvIndex);
                                AgingCVStep = Aging_CV_Step.Low_Lift_Down;
                            }
                        }
                    }
                    break;
                case Aging_CV_Step.Loading_Life_Up_Wait:
                    if (Ez_Model.IsMoveLiftUpDone(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        if (AgingCvFull[SingletonManager.instance.AgingCvIndex] == true)
                        {
                            // Unclamping IF on step 이동
                            AgingCVStep = Aging_CV_Step.Unclamping_IF_Send;
                        }
                        else
                        {
                            // CV 전진 step 이동
                            AgingCVStep = Aging_CV_Step.Lift_CV_Forward;
                        }
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Down:
                    if (Ez_Model.IsMoveLiftDownDone(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        if (AgingCvFull[SingletonManager.instance.AgingCvIndex] == true)
                        {
                            AgingCVStep = Aging_CV_Step.Unclamping_IF_Send;
                        }
                        else
                        {
                            AgingCVStep = Aging_CV_Step.Lift_CV_Forward;
                        }
                    }

                    break;
                case Aging_CV_Step.Unclamping_IF_Send:

                    SetUnclampInterfase(SingletonManager.instance.AgingCvIndex, true);

                    AgingCVStep = Aging_CV_Step.Unclamping_IF_Receive;
                    break;
                case Aging_CV_Step.Unclamping_IF_Receive:
                    
                    if (UnclampInterfaseReturnOn(SingletonManager.instance.AgingCvIndex) == true)
                        AgingCVStep = Aging_CV_Step.Lift_CV_Forward;

                    break;
                case Aging_CV_Step.Lift_CV_Forward:
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_1, true);
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_2, true);
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_3, true);

                    AgingCVStep = Aging_CV_Step.Aging_CV_Forward;
                    break;
                case Aging_CV_Step.Aging_CV_Forward:
                    int Lift_CV_Out_IO = 0;
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_1_CV_DETECT_OUT_SS_2;
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_2_CV_DETECT_OUT_SS_2;
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_3_CV_DETECT_OUT_SS_2;
                    // Lift CV out sensor check
                    if (Dio.DI_RAW_DATA[Lift_CV_Out_IO] == true)
                    {
                        // Aging CV Run
                        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, true);
                        AgingCVStep = Aging_CV_Step.CV_Stop;
                    }
                    break;
                case Aging_CV_Step.CV_Stop:
                    if (GetAgingCVStartSS(SingletonManager.instance.AgingCvIndex) == false)
                    {
                        AgingCVStep = Aging_CV_Step.CV_Stop_Wait;
                    }
                    break;
                    case Aging_CV_Step.CV_Stop_Wait:
                    if (GetAgingCVStartSS(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        // Aging CV Stop
                        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false);

                        if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                        //AgingCVStep = Aging_CV_Step.Unclamping_IF_Set_Off;

                        //SingletonManager.instance.LoadFloor[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = 0;
                        for (int i = 0; i < (int)Floor_Index.Max; i++)
                            SingletonManager.instance.Display_Lift[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].Floor[i] = false;

                        // Interfase 신호를 on했으면 Off 조건확인 시퀀스로 이동한다.
                        if (GetSendInterfaseStatus(SingletonManager.instance.AgingCvIndex) == true)
                            AgingCVStep = Aging_CV_Step.Unclamping_IF_Set_Off;
                        else
                            AgingCVStep = Aging_CV_Step.Low_Lift_Up_Start;
                    }
                    break;
                case Aging_CV_Step.Unclamping_IF_Set_Off:
                    if (GetUnclampInterfaseOff(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        SetUnclampInterfase(SingletonManager.instance.AgingCvIndex, false);
                        // 1층이면 Lift Up을 한다.
                        //if (Index == 3 || Index == 4 || Index == 5)
                        //    AgingCVStep[Index] = Aging_CV_Step.Low_Lift_Up_Start;
                        //else
                        //    AgingCVStep[Index] = Aging_CV_Step.Idle;
                        AgingCVStep = Aging_CV_Step.Low_Lift_Up_Start;
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Up_Start:
                    if (LiftLowMoveConditon(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        Ez_Model.MoveLiftLoding(GetLiftNomber(SingletonManager.instance.AgingCvIndex));
                        AgingCVStep = Aging_CV_Step.Low_Lift_Up_Wait;
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Up_Wait:
                    if (Ez_Model.IsMoveLiftLodingDone(GetLiftNomber(SingletonManager.instance.AgingCvIndex)) ==true)
                    {
                        SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = false;
                        // 다음 에이징 컨베아 Index로  변수증가 Upper:0,1,2 Low:3,4,5
                        SingletonManager.instance.AgingCvIndex += 1;
                        if (SingletonManager.instance.AgingCvIndex >= 6)
                            SingletonManager.instance.AgingCvIndex = 0;
                        AgingCVStep = Aging_CV_Step.Idle;
                    }
                    break;
            }
            int step = (int)AgingCVStep;
            Global.instance.Write_Sequence_Log("AGING_CV_STEP", step.ToString());
            Global.instance.Write_Sequence_Log("AGING_CV_INDEX", SingletonManager.instance.AgingCvIndex.ToString());
            //Global.instance.Write_Sequence_Log("LOAD_COMPLETE_FLAG", SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].ToString());
        }
        public void Aging_CV_StepRun_Logic()
        {
            switch (AgingCVStep)
            {
                case Aging_CV_Step.Idle:
                    // 변수 초기화
                    AgingCvFull[SingletonManager.instance.AgingCvIndex] = false;
                    AgingCvStart[SingletonManager.instance.AgingCvIndex] = false;
                    AgingCvEndStopCondition[SingletonManager.instance.AgingCvIndex] = false;
                    AgingCvInStopCondition[SingletonManager.instance.AgingCvIndex] = false;

                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);

                    if (Ez_Model.IsMoveLiftLodingDone(GetLiftNomber(SingletonManager.instance.AgingCvIndex)) != true)
                        Ez_Model.MoveLiftLoding(GetLiftNomber(SingletonManager.instance.AgingCvIndex));
                    AgingCVStep = Aging_CV_Step.Lift_Loading_Pos_Check;
                    break;
                case Aging_CV_Step.Lift_Loading_Pos_Check:
                    if (Ez_Model.IsMoveLiftLodingDone(GetLiftNomber(SingletonManager.instance.AgingCvIndex)) == true)
                    {
                        AgingCVStep = Aging_CV_Step.CV_On_Condition_Wait;
                    }
                    break;
                case Aging_CV_Step.CV_On_Condition_Wait:

                    // Lift에 Clamp가 있는지 확인한다.
                    bool DetectSS = false;
                    if (SingletonManager.instance.AgingCvIndex == 0 || SingletonManager.instance.AgingCvIndex == 3)
                    { 
                        DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_IN_SS_1]; 
                    }
                    else if (SingletonManager.instance.AgingCvIndex == 1 || SingletonManager.instance.AgingCvIndex == 4)
                    { 
                        DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_IN_SS_1]; 
                    }
                    else if (SingletonManager.instance.AgingCvIndex == 2 || SingletonManager.instance.AgingCvIndex == 5)
                    {
                        DetectSS = Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_IN_SS_1]; 
                    }

                    if (SingletonManager.instance.LoadComplete[SingletonManager.instance.AgingCvIndex] == true
                        && DetectSS == true)
                    {
						// Aging Upper/Low skip 확인 
                        AgingCvNextIndex(SingletonManager.instance.AgingCvIndex);
                        GetAgingCVStartEndSS(SingletonManager.instance.AgingCvIndex);
                        // Upper
                        if (SingletonManager.instance.AgingCvIndex < 3)
                        {
                            Ez_Model.MoveLiftUp(SingletonManager.instance.AgingCvIndex);
                            AgingCVStep = Aging_CV_Step.Loading_Life_Up_Wait;
                        }
                        // Low
                        else
                        {
                            // Lift Down Step으로 이동
                            if (LiftLowMoveConditon(SingletonManager.instance.AgingCvIndex) == true)
                            {
                                Ez_Model.MoveLiftDown(SingletonManager.instance.AgingCvIndex);
                                AgingCVStep = Aging_CV_Step.Low_Lift_Down;
                            }
                        }
                    }
                    // 적제 complete true 이지만 Lift에 Clamp가 없으면 Step Time으로 전진
                    else if (SingletonManager.instance.LoadComplete[SingletonManager.instance.AgingCvIndex] == true
                        && DetectSS == false)
                    {
                        GetAgingCVStartEndSS(SingletonManager.instance.AgingCvIndex);
                        if (AgingCvFull[SingletonManager.instance.AgingCvIndex] == true)
                        {
                            AgingCVStep = Aging_CV_Step.Cv_Step_Run_IF_Send;
                        }
                        else
                        {
                            AgingCVStep = Aging_CV_Step.Cv_Step_Run_Start;
                        }
                    }
                    else
                    {
                        SingletonManager.instance.AgingCvIndex += 1;
                        if (SingletonManager.instance.AgingCvIndex >= 6)
                            SingletonManager.instance.AgingCvIndex = 0;
                    }
                    break;
                case Aging_CV_Step.Loading_Life_Up_Wait:
                    if (Ez_Model.IsMoveLiftUpDone(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        if (AgingCvFull[SingletonManager.instance.AgingCvIndex] == true)
                        {
                            // Unclamping IF on step 이동
                            AgingCVStep = Aging_CV_Step.Unclamping_IF_Send;
                        }
                        else
                        {
                            // CV 전진 step 이동
                            AgingCVStep = Aging_CV_Step.Lift_CV_Forward;
                        }
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Down:
                    if (Ez_Model.IsMoveLiftDownDone(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        if (AgingCvFull[SingletonManager.instance.AgingCvIndex] == true)
                        {
                            AgingCVStep = Aging_CV_Step.Unclamping_IF_Send;
                        }
                        else
                        {
                            AgingCVStep = Aging_CV_Step.Lift_CV_Forward;
                        }
                    }
                    break;
                case Aging_CV_Step.Unclamping_IF_Send:

                    SetUnclampInterfase(SingletonManager.instance.AgingCvIndex, true);

                    AgingCVStep = Aging_CV_Step.Unclamping_IF_Receive;
                    break;
                case Aging_CV_Step.Unclamping_IF_Receive:

                    if (UnclampInterfaseReturnOn(SingletonManager.instance.AgingCvIndex) == true)
                        AgingCVStep = Aging_CV_Step.Lift_CV_Forward;

                    break;
                case Aging_CV_Step.Lift_CV_Forward:
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_1, true);
                    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_2, true);
                    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_3, true);

                    AgingCVStep = Aging_CV_Step.Aging_CV_Forward;
                    break;
                case Aging_CV_Step.Aging_CV_Forward:
                    int Lift_CV_Out_IO = 0;
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_1_CV_DETECT_OUT_SS_2;
                    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_2_CV_DETECT_OUT_SS_2;
                    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_3_CV_DETECT_OUT_SS_2;
                    // Lift CV out sensor check
                    if (Dio.DI_RAW_DATA[Lift_CV_Out_IO] == true)
                    {
                        // Aging CV Run
                        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, true);
                        AgingCVStep = Aging_CV_Step.CV_Stop_Wait;
                        _TimeDelay.Restart();
                    }
                    break;
                //case Aging_CV_Step.CV_Stop:
                //    if (GetAgingCVStartSS(SingletonManager.instance.AgingCvIndex) == false)
                //    {
                //        AgingCVStep = Aging_CV_Step.CV_Stop_Wait;
                //        break;
                //    }
                //    if (GetAgingCVEndSS(SingletonManager.instance.AgingCvIndex) == true)
                //    {
                //         if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                //            Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                //        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                //            Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                //        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                //            Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                //        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false);
                //        AgingCVStep = Aging_CV_Step.Cv_Step_Run_IF_Send;
                //    }
                //    break;
                case Aging_CV_Step.CV_Stop_Wait:
                    // Aging cv 투입중 end 신호가 먼저 들어오면 배출하면서 투입 동시 진행
                    if (GetAgingCVEndSS(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false);
                        AgingCVStep = Aging_CV_Step.Cv_Step_Run_IF_Send;
                    }
                    //if (GetAgingCVStartSS(SingletonManager.instance.AgingCvIndex) == true)
                    else if (_TimeDelay.ElapsedMilliseconds >= SingletonManager.instance.SystemModel.AgingCvStepTime)
                    {
                        // Aging CV Stop
                        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false);

                        if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                        //AgingCVStep = Aging_CV_Step.Unclamping_IF_Set_Off;

                        //SingletonManager.instance.LoadFloor[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = 0;
                        for (int i = 0; i < (int)Floor_Index.Max; i++)
                            SingletonManager.instance.Display_Lift[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].Floor[i] = false;

                        // Interfase 신호를 on했으면 Off 조건확인 시퀀스로 이동한다.
                        if (GetSendInterfaseStatus(SingletonManager.instance.AgingCvIndex) == true)
                            AgingCVStep = Aging_CV_Step.Unclamping_IF_Set_Off;
                        else
                            AgingCVStep = Aging_CV_Step.Low_Lift_Up_Start;
                    }
                    break;
                
                case Aging_CV_Step.Unclamping_IF_Set_Off:
                    if (GetUnclampInterfaseOff(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        SetUnclampInterfase(SingletonManager.instance.AgingCvIndex, false);
                        AgingCVStep = Aging_CV_Step.Low_Lift_Up_Start;
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Up_Start:
                    if (LiftLowMoveConditon(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        Ez_Model.MoveLiftLoding(GetLiftNomber(SingletonManager.instance.AgingCvIndex));
                        AgingCVStep = Aging_CV_Step.Low_Lift_Up_Wait;
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Up_Wait:
                    if (Ez_Model.IsMoveLiftLodingDone(GetLiftNomber(SingletonManager.instance.AgingCvIndex)) == true)
                    {
                        SingletonManager.instance.LoadComplete[SingletonManager.instance.AgingCvIndex] = false;
                        // 다음 에이징 컨베아 Index로  변수증가 Upper:0,1,2 Low:3,4,5
                        SingletonManager.instance.AgingCvIndex += 1;
                        if (SingletonManager.instance.AgingCvIndex >= 6)
                            SingletonManager.instance.AgingCvIndex = 0;
                        AgingCVStep = Aging_CV_Step.Idle;
                    }
                    break;
                
                case Aging_CV_Step.Cv_Step_Run_Start:
                    // Aging CV Stop
                    Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, true);
                    _TimeDelay.Restart();
                    AgingCVStep = Aging_CV_Step.Cv_Step_Run_Stop;
                    break;
                case Aging_CV_Step.Cv_Step_Run_Stop:
                    // 마지막 clmap 일때 Unclamp Conveyor에 제품이 들어갈수 있도록 CV Off Time+3초 
                    if (_TimeDelay.ElapsedMilliseconds > (SingletonManager.instance.SystemModel.AgingCvStepTime)
                        || GetAgingCVEndSS(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        // Aging CV Stop
                        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false);

                        SingletonManager.instance.LoadComplete[SingletonManager.instance.AgingCvIndex] = false;
                        // 다음 에이징 컨베아 Index로  변수증가 Upper:0,1,2 Low:3,4,5
                        SingletonManager.instance.AgingCvIndex += 1;
                        if (SingletonManager.instance.AgingCvIndex >= 6)
                            SingletonManager.instance.AgingCvIndex = 0;

                        AgingCVStep = Aging_CV_Step.Idle;
                     }
                    if (_TimeDelay.ElapsedMilliseconds == 0)
                        _TimeDelay.Restart();
                    break;
                case Aging_CV_Step.Cv_Step_Run_IF_Send:
                    SetUnclampInterfase(SingletonManager.instance.AgingCvIndex, true);

                    AgingCVStep = Aging_CV_Step.Cv_Step_Run_IF_Return_Check;
                    break;
                case Aging_CV_Step.Cv_Step_Run_IF_Return_Check:
                    if (UnclampInterfaseReturnOn(SingletonManager.instance.AgingCvIndex) == true)
                    {
                        AgingCVStep = Aging_CV_Step.Cv_Step_IF_Run;
                    }
                    break;
                case Aging_CV_Step.Cv_Step_IF_Run:
                    int ioIndex =0;
                    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                        ioIndex = (int)DI_MAP.LIFT_1_CV_DETECT_OUT_SS_2;
                    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                        ioIndex = (int)DI_MAP.LIFT_2_CV_DETECT_OUT_SS_2;
                    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                        ioIndex = (int)DI_MAP.LIFT_3_CV_DETECT_OUT_SS_2;
                    // Lift CV out sensor check
                    if (Dio.DI_RAW_DATA[ioIndex] == true)
                    {
                        if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_1, true);
                        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_2, true);
                        else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_3, true);
                    }
                    // Aging CV Start
                    Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, true);
                    _TimeDelay.Restart();
                    AgingCVStep = Aging_CV_Step.Cv_Step_IF_Clamp_OutWait;
                    break;
                case Aging_CV_Step.Cv_Step_IF_Clamp_OutWait:
                    // Clamp가 aging end seneor에 감지되지않을때 cv stop 체크들어간다
                    if (GetAgingCVEndSS(SingletonManager.instance.AgingCvIndex) == false)
                    {
                        AgingCVStep = Aging_CV_Step.Cv_Step_IF_Stop;
                    }                    
                    break;
                case Aging_CV_Step.Cv_Step_IF_Stop:
                    // 다음 clamp가 감지되든가 Step 이동 시간 초과시 aging cv stop
                    if (GetAgingCVEndSS(SingletonManager.instance.AgingCvIndex) == true
                        || _TimeDelay.ElapsedMilliseconds >= SingletonManager.instance.SystemModel.AgingCvStepTime)
                    {
                        for (int i = 0; i < (int)Floor_Index.Max; i++)
                            SingletonManager.instance.Display_Lift[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].Floor[i] = false;

                        // Aging CV Start
                        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false); 
                        AgingCVStep = Aging_CV_Step.Cv_Step_IF_Off;
                    }
                    break;
                case Aging_CV_Step.Cv_Step_IF_Off:
                    if (UnclampInterfaseReturnOn(SingletonManager.instance.AgingCvIndex) == false)
                    {
                        SingletonManager.instance.LoadComplete[SingletonManager.instance.AgingCvIndex] = false;

                        if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                        if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                        if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                        SetUnclampInterfase(SingletonManager.instance.AgingCvIndex, false);
                        
                        Ez_Model.MoveLiftLoding(GetLiftNomber(SingletonManager.instance.AgingCvIndex));
                        SingletonManager.instance.AgingCvIndex += 1;
                        if (SingletonManager.instance.AgingCvIndex >= 6)
                            SingletonManager.instance.AgingCvIndex = 0;

                        AgingCVStep = Aging_CV_Step.Idle;
                    }
                    break;

            }
            int step = (int)AgingCVStep;
            Global.instance.Write_Sequence_Log("AGING_CV_STEP", step.ToString());
            //Global.instance.Write_Sequence_Log("AGING_CV_INDEX", SingletonManager.instance.AgingCvIndex.ToString());
            //Global.instance.Write_Sequence_Log("LOAD_COMPLETE_FLAG", SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].ToString());
        }
        public void AgingConveyorPass()
        {
            switch (AgingCvPassStep)
            {
                case Aging_CV_Pass_Step.Idle:
                    // Main Logic의 변수 초기화
                    SingletonManager.instance.AgingCvIndex = 0;
                    SingletonManager.instance.LoadStageNo = 0;
                    _AgingPassCvIndex = 0;
                    AgingCvPassStep = Aging_CV_Pass_Step.CV_Run;
                    Global.instance.SendMainUiLog("Aging Pass Step Start");
                    Global.instance.SendMainUiLog("Aging_CV_Pass_Step => CV_Run");
                    break;
                case Aging_CV_Pass_Step.CV_Run:
                    // Aging Conveyour를 실행한다.
                    for (int i=0; i<6; i++)
                    {
                        if(GetAgingCVEndSS(i) == false)
                        {
                            // 끝단에 클램프 감지가 되지 않는 cv는 run 한다.
                            Dio_Aging_CV_Control(i, true);
                        }
                    }
                    _TimeDelay.Restart();
                    AgingCvPassStep = Aging_CV_Pass_Step.CV_End_Sensor_Wait;
                    Global.instance.SendMainUiLog("Aging_CV_Pass_Step => CV_End_Sensor_Wait");
                    break;
                case Aging_CV_Pass_Step.CV_End_Sensor_Wait:
                    // Aging Conveyour의 End Sensor가 들어오면 conveyor를 정지한다.
                    for (int i=0; i<6; i++)
                    {
                        if (GetAgingCVEndSS(i) == true)
                        {
                            Global.instance.SendMainUiLog($"CV [{i+1}]  => End Arrival");
                            Dio_Aging_CV_Control(i, false);
                            Global.instance.SendMainUiLog($"CV [{i + 1}]  => Off");
                        }
                    }
                    if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2] == false)
                    {
                        Global.instance.SendMainUiLog("Aging_CV_Pass_Step => Interfase_Send");
                        AgingCvPassStep = Aging_CV_Pass_Step.Interfase_Send;
                    }
                    // 60초 동안 구동하고 clamp가 감지되지 않으면 clamp 없는걸로 인식하고  다음 conveyour를 구동한다.
                    if (_TimeDelay.ElapsedMilliseconds > 240000)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            Dio_Aging_CV_Control(i, false);
                        }
                        if (GetAgingCVEndSS(0) == false && GetAgingCVEndSS(1) == false && GetAgingCVEndSS(2) == false
                            && GetAgingCVEndSS(3) == false && GetAgingCVEndSS(4) == false && GetAgingCVEndSS(5) == false)
                        {
                            SingletonManager.instance.IsInspectionStart = false;
                            AgingCvPassStep = Aging_CV_Pass_Step.Idle;
                            Global.instance.ShowMessagebox("Auto aging pass time out");
                        }
                    }
                    break;
                case Aging_CV_Pass_Step.Interfase_Send:
                    // clamp가 있으면 Unclamp Interfase를 보낸다.
                    if (GetAgingCVEndSS(_AgingPassCvIndex) == true)
                    {
                        if (SetUnclampInterfase(_AgingPassCvIndex, true) == true)
                        {
                            Global.instance.SendMainUiLog($"CV {_AgingPassCvIndex+1} => Interface Send");
                            // 인터페이스가 정상 전달되면 다음으로 이동한다.
                            Global.instance.SendMainUiLog("Aging_CV_Pass_Step => Unclamp_Interfase_Wait");
                            AgingCvPassStep = Aging_CV_Pass_Step.Unclamp_Interfase_Wait;
                        }
                    }
                    else
                    {
                        _AgingPassCvIndex++;
                        if (_AgingPassCvIndex >= 6)
                        {
                            // 전부 aging cv 제품이 없으면 aging pass 기능 종료
                            if (GetAgingCVEndSS(0) == false && GetAgingCVEndSS(1) == false && GetAgingCVEndSS(2) == false
                                && GetAgingCVEndSS(3) == false && GetAgingCVEndSS(4) == false && GetAgingCVEndSS(5) == false)
                            {
                                SingletonManager.instance.IsInspectionStart = false;
                                SingletonManager.instance.EquipmentMode = EquipmentMode.Auto;
                                AgingCvPassStep = Aging_CV_Pass_Step.Idle;
                                Global.instance.ShowMessagebox("Auto aging pass completion", false);
                            }
                            _AgingPassCvIndex = 0;
                        }
                    }
                    break;
                case Aging_CV_Pass_Step.Unclamp_Interfase_Wait:
                    // Unclamp Interfase Return을 확인한다.
                    if (UnclampInterfaseReturnOn(_AgingPassCvIndex) == true)
                    {
                        Global.instance.SendMainUiLog($"CV {_AgingPassCvIndex + 1} => Interface Receive On");
                        Global.instance.SendMainUiLog("Aging_CV_Pass_Step => Clamp_Out_Start");
                        AgingCvPassStep = Aging_CV_Pass_Step.Clamp_Out_Start;
                    }
                    break;
                case Aging_CV_Pass_Step.Clamp_Out_Start:
                    // Unclamp가 Clamp 받을 준비가 되여있으면 conveyou를 전진시킨다.
                    Dio_Aging_CV_Control(_AgingPassCvIndex, true);
                    Global.instance.SendMainUiLog($"CV {_AgingPassCvIndex + 1} => Aging CV Run");

                    Global.instance.SendMainUiLog("Aging_CV_Pass_Step => CV_Off_Wait");
                    AgingCvPassStep = Aging_CV_Pass_Step.CV_Off_Wait;
                    break;
                case Aging_CV_Pass_Step.CV_Off_Wait:
                    // 끝단에 있는 clamp 하나가 나가는것을 감지한다.
                    if (GetAgingCVEndSS(_AgingPassCvIndex) == false)
                    {
                        Global.instance.SendMainUiLog("Aging_CV_Pass_Step => CV_Off");
                        AgingCvPassStep = Aging_CV_Pass_Step.CV_Off;
                        _TimeDelay.Restart();
                    }
                    break;
                case Aging_CV_Pass_Step.CV_Off:
                    // 다음 clmap가 sensor까지 오면 conveyou 정지한다.
                    if (GetAgingCVEndSS(_AgingPassCvIndex) == true)
                    {
                        Global.instance.SendMainUiLog($"CV {_AgingPassCvIndex + 1} => Aging CV Off");
                        Dio_Aging_CV_Control(_AgingPassCvIndex, false);
                        Global.instance.SendMainUiLog("Aging_CV_Pass_Step => Next_CV_Start");
                        AgingCvPassStep = Aging_CV_Pass_Step.Next_CV_Start;

                    }
                    else if (_TimeDelay.ElapsedMilliseconds >30000)
                    {
                        Global.instance.SendMainUiLog($"CV {_AgingPassCvIndex } => Aging End Sensor Check Time Out (Last Clamp)");
                        Dio_Aging_CV_Control(_AgingPassCvIndex, false);
                        AgingCvPassStep = Aging_CV_Pass_Step.Next_CV_Start;
                        Global.instance.SendMainUiLog("Aging_CV_Pass_Step => Next_CV_Start");
                    }
                    break;
                case Aging_CV_Pass_Step.Next_CV_Start:
                    // Unclamp Interfase가 꺼지면 send Interface Off 하고 다음 conveyou를 구동한다.
                    if (UnclampInterfaseReturnOn(_AgingPassCvIndex) == false)
                    {
                        Global.instance.SendMainUiLog($"CV {_AgingPassCvIndex + 1} => Interface Receive Off");
                        SetUnclampInterfase(_AgingPassCvIndex, false);
                        Global.instance.SendMainUiLog($"CV {_AgingPassCvIndex + 1} => Interface Send Off");
                        _AgingPassCvIndex++;
                        if (_AgingPassCvIndex >= 6)
                        {
                            _AgingPassCvIndex = 0;
                        }
                        Global.instance.SendMainUiLog($"CV {_AgingPassCvIndex + 1} => Next CV Start");
                        AgingCvPassStep = Aging_CV_Pass_Step.Interfase_Send;
                        Global.instance.SendMainUiLog("Aging_CV_Pass_Step => Interfase_Send");
                    }
                    break;
            }
            int step = (int)AgingCvPassStep;
            Global.instance.Write_Sequence_Log("AGING_PASS_STEP", step.ToString());
            Global.instance.Write_Sequence_Log("AGING_PASS_CV_INDEX", _AgingPassCvIndex.ToString());
        }
       
        private int GetLiftNomber(int Index)
        {
            int LiftNO = 0;
            if (Index == 0 || Index == 3) LiftNO = 0;
            else if (Index == 1 || Index == 4) LiftNO = 1;
            else if (Index == 2 || Index == 5) LiftNO = 2;
            return LiftNO;
        }
        private bool Dio_Output(DO_MAP io, bool OnOff)
        {
            bool result = false;
            result = Dio.SetIO_OutputData((int)io, OnOff);
            
            return result;
        }
        private void Dio_Aging_CV_Control(int Index, bool OnOff)
        {
            if (Index == 0)
            {
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, OnOff);
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, OnOff);
            }
            if (Index == 1)
            {
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, OnOff);
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, OnOff);
            }
            if (Index == 2)
            {
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, OnOff);
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, OnOff);
            }
            if (Index == 3)
            {
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, OnOff);
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, OnOff);
            }
            
            if (Index == 4)
            {
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, OnOff);
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, OnOff);
            }
            
            if (Index == 5)
            {
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, OnOff);
                if (Dio.DO_RAW_DATA[(int)DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2] != OnOff)
                    Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2, OnOff);
            }
        }
        private void GetAgingCVStartEndSS(int Index)
        {
            if (Index == 0)
            {
                AgingCvFull[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_1_2_UPPER_DETECT_SS_2];
                AgingCvStart[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_1_1_UPPER_DETECT_SS_1];
            }
            if (Index == 1)
            {
                AgingCvFull[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_2_2_UPPER_DETECT_SS_2];
                AgingCvStart[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_2_1_UPPER_DETECT_SS_1];
            }
            if (Index == 2)
            {
                AgingCvFull[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_3_2_UPPER_DETECT_SS_2];
                AgingCvStart[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_3_1_UPPER_DETECT_SS_1];
            }
            if (Index == 3)
            {
                AgingCvFull[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_1_2_LOW_DETECT_SS_2];
                AgingCvStart[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_1_1_LOW_DETECT_SS_1];
            }
            
            if (Index == 4)
            {
                AgingCvFull[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_2_2_LOW_DETECT_SS_2];
                AgingCvStart[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_2_1_LOW_DETECT_SS_1];
            }
            if (Index == 5)
            {
                AgingCvFull[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_3_2_LOW_DETECT_SS_2];
                AgingCvStart[Index] = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_3_1_LOW_DETECT_SS_1];
            }
        }
        private bool GetAgingCVStartSS(int Index)
        {
            bool ret = false;
            if (Index == 0)
            {
                ret = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_1_1_UPPER_DETECT_SS_1];
            }
            if (Index == 1)
            {
                ret = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_2_1_UPPER_DETECT_SS_1];
            }
            if (Index == 2)
            {
                ret = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_3_1_UPPER_DETECT_SS_1];
            }
            if (Index == 3)
            {
                ret = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_1_1_LOW_DETECT_SS_1];
            }

            if (Index == 4)
            {
                ret = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_2_1_LOW_DETECT_SS_1];
            }
            if (Index == 5)
            {
                ret = Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_3_1_LOW_DETECT_SS_1];
            }
            return ret;
        }
        private bool GetAgingCVEndSS(int Index)
        {
            if (Index == 0)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_1_2_UPPER_DETECT_SS_2] == true)
                    return true;
            }
            if (Index == 1)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_2_2_UPPER_DETECT_SS_2] == true)
                    return true;
            }
            if (Index == 2)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_3_2_UPPER_DETECT_SS_2] == true)
                    return true;
            }
            if (Index == 3)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_1_2_LOW_DETECT_SS_2] == true)
                    return true;
            }

            if (Index == 4)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_2_2_LOW_DETECT_SS_2] == true)
                    return true;
            }
            if (Index == 5)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_3_2_LOW_DETECT_SS_2] == true)
                    return true;
            }
            return false;
        }
        private bool LiftLowMoveConditon(int index)
        {
            // Lift Donw 하는데 간섭이 있는지 확인
            if (index == 0 || index == 3)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_OUT_SS_2] == false)
                    return true;
            }
            if (index == 1 || index == 4)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_OUT_SS_2] == false)
                    return true;
            }
            if (index == 2 || index == 5)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_OUT_SS_2] == false)
                    return true;
            }
            return false; 
        }
        private bool UnclampInterfaseReturnOn(int index)
        {
            if (index == 0)
            { 
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_1] == true)
                    return true;
            }
            if (index == 1)
            { 
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_2] == true)
                    return true;
            }
            if (index == 2)
            { 
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_3] == true)
                    return true;
            }
            if (index == 3)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_1] == true)
                    return true;
            }
            if (index == 4)
            { 
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_2] == true)
                    return true;
            }
            if (index == 5)
             { 
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_3] == true)
                    return true;
             }
            return false;
        }
        private bool GetUnclampInterfaseOff(int index)
        {
            if (index == 0)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_1] == false)
                    return true;
            }
            if (index == 1)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_2] == false)
                    return true;
            }
            if (index == 2)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_3] == false)
                    return true;
            }
            if (index == 3)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_1] == false)
                    return true;
            }
            if (index == 4)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_2] == false)
                    return true;
            }
            if (index == 5)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_3] == false)
                    return true;
            }
            return false;
        }
        private bool SetUnclampInterfase(int index, bool OnOff)
        {
            bool ret = false;
            if (index == 0)
            {
                ret = Dio_Output(DO_MAP.AGING_CV_UPPER_INTERFACE_1, OnOff);
            }
            else if (index == 1)
            {
                ret = Dio_Output(DO_MAP.AGING_CV_UPPER_INTERFACE_2, OnOff);
            }
            else if (index == 2)
            {
                ret = Dio_Output(DO_MAP.AGING_CV_UPPER_INTERFACE_3, OnOff);
            }
            else if (index == 3)
            {
                ret = Dio_Output(DO_MAP.AGING_CV_LOW_INTERFACE_1, OnOff);
            }
            else if (index == 4)
            {
                ret = Dio_Output(DO_MAP.AGING_CV_LOW_INTERFACE_2, OnOff);
            }
            else if (index == 5)
            {
                ret = Dio_Output(DO_MAP.AGING_CV_LOW_INTERFACE_3, OnOff);
            }
            return ret;
        }
        private bool GetSendInterfaseStatus(int index)
        {
            bool ret = false;
            if (index == 0)
            {
                ret = Dio.DO_RAW_DATA[(int)DO_MAP.AGING_CV_UPPER_INTERFACE_1];
            }
            else if (index == 1)
            {
                ret = Dio.DO_RAW_DATA[(int)DO_MAP.AGING_CV_UPPER_INTERFACE_2];
            }
            else if (index == 2)
            {
                ret = Dio.DO_RAW_DATA[(int)DO_MAP.AGING_CV_UPPER_INTERFACE_3];
            }
            else if (index == 3)
            {
                ret = Dio.DO_RAW_DATA[(int)DO_MAP.AGING_CV_LOW_INTERFACE_1];
            }
            else if (index == 4)
            {
                ret = Dio.DO_RAW_DATA[(int)DO_MAP.AGING_CV_LOW_INTERFACE_2];
            }
            else if (index == 5)
            {
                ret = Dio.DO_RAW_DATA[(int)DO_MAP.AGING_CV_LOW_INTERFACE_3];
            }
            return ret;
        }
        private void AgingCvNextIndex(int CvIndex)
        {
            if (SingletonManager.instance.SystemModel.AgingCvNotUse == "Upper")
            {
                if (SingletonManager.instance.AgingCvIndex == 3)
                    SingletonManager.instance.AgingCvIndex = 0;
                else if (SingletonManager.instance.AgingCvIndex == 4)
                    SingletonManager.instance.AgingCvIndex = 1;
                else if (SingletonManager.instance.AgingCvIndex == 5)
                    SingletonManager.instance.AgingCvIndex = 2;
            }
            else if (SingletonManager.instance.SystemModel.AgingCvNotUse == "Low")
            {
                if (SingletonManager.instance.AgingCvIndex == 0)
                    SingletonManager.instance.AgingCvIndex = 3;
                else if (SingletonManager.instance.AgingCvIndex == 1)
                    SingletonManager.instance.AgingCvIndex = 4;
                else if (SingletonManager.instance.AgingCvIndex == 2)
                    SingletonManager.instance.AgingCvIndex = 5;
            }
        }
        public void StartReady()
        {
            var myIni = new IniFile(Global.instance.IniSequencePath);

            string value="";
            value = myIni.Read("OUT_HANDLER_STEP", "SEQUENCE");
            if (string.IsNullOrEmpty(value) == true)
            {
                Out_Handle_Step = OutHandle.Idle;
                Top_Handle_Step = TopHandle.Idle;
                Bottom_Step = BottomHandle.Idle;
                SingletonManager.instance.LoadStageNo = 0;
                SingletonManager.instance.IsY_PickupColl = false;
                SingletonManager.instance.UnitLastPositionSet = false;
                SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = 0;
                AgingCVStep = Aging_CV_Step.Idle;
                SingletonManager.instance.AgingCvIndex = 0;
                SingletonManager.instance.LoadComplete[0] = false;
                SingletonManager.instance.LoadComplete[1] = false;
                SingletonManager.instance.LoadComplete[2] = false;
                SingletonManager.instance.LoadComplete[3] = false;
                SingletonManager.instance.LoadComplete[4] = false;
                SingletonManager.instance.LoadComplete[5] = false;
                _NoneSetTest = false;
                return;
            }
            Out_Handle_Step = (OutHandle)Convert.ToInt16(value);
            value = myIni.Read("TOP_STEP", "SEQUENCE");
            Top_Handle_Step = (TopHandle)Convert.ToInt16(value);
            value = myIni.Read("BOTTOM_STEP", "SEQUENCE");
            Bottom_Step = (BottomHandle)Convert.ToInt16(value);
            value = myIni.Read("Y_PICKUP_COLL_FLAG", "SEQUENCE");
            SingletonManager.instance.IsY_PickupColl =bool.Parse(value);
            value = myIni.Read("OUT_LOAD_STAGE", "SEQUENCE");
            SingletonManager.instance.LoadStageNo = Convert.ToInt32(value);
            value = myIni.Read("OUT_LOAD_FLOOR", "SEQUENCE");
            SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = Convert.ToInt32(value);
            value = myIni.Read("AGING_CV_STEP", "SEQUENCE");
            if (value == "")
                AgingCVStep = Aging_CV_Step.Idle;
            else
                AgingCVStep = (Aging_CV_Step)Convert.ToInt16(value);
            value = myIni.Read("AGING_CV_INDEX", "SEQUENCE");
            if (value == "")
                SingletonManager.instance.LoadAgingCvIndex = 0;
            else
                SingletonManager.instance.LoadAgingCvIndex = Convert.ToInt32(value);
            //value = myIni.Read("LOAD_COMPLETE_FLAG", "SEQUENCE");
            //if(value == "")
            //    SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = false;
            //else
            //    SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = bool.Parse(value);

            value = myIni.Read("NONE_SET_TEST", "SEQUENCE");
            if (value == "")
                _NoneSetTest = false;
            else
                _NoneSetTest = bool.Parse(value);

        }
    }
}
