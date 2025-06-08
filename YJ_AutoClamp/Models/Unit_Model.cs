using Common.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Floor_6,
        Floor_7,
        Max
    }
    public class Unit_Model
    {
        public MotionUnit_List UnitGroup { get; set; }
        public int UnitID { get; set; }
        public List<ServoSlave_List> ServoNames { get; set; }

        // Top Clamp 안착작업 완료시 사용되는 변수
        private bool TopClampingDone = false;
        // Bottom NG 배출용 변수
        private bool[] AgingCvFull = { false, false, false, false, false, false };
        private bool[] AgingCvStart = { false, false, false, false, false, false };
        private bool[] AgingCvEndStopCondition = { false, false, false, false, false, false };
        private bool[] AgingCvInStopCondition = { false, false, false, false, false, false };
        private int _AgingPassCvIndex = 0; // Aging CV Index

        private bool _NoneSetTest = true; // Set Test Mode

        Stopwatch _TimeDelay = new Stopwatch();
        Stopwatch _TopHandlerTimeDelay = new Stopwatch();
        Stopwatch _BottomHandlerTimeDelay = new Stopwatch();
        Stopwatch _TopCvTimeDelay = new Stopwatch();
        Stopwatch _BottomCvTimeDelay = new Stopwatch();
        public Unit_Model(MotionUnit_List unit)
        {
            UnitGroup = unit;
            UnitID = (int)unit;
            ServoNames = new List<ServoSlave_List>();
        }
        private EziDio_Model Dio = SingletonManager.instance.Ez_Dio;
        private EzMotion_Model_E Ez_Model = SingletonManager.instance.Ez_Model;
        private bool _isLoopRunning = false;
        private int _BarCodeRetryCount = 0;
        private bool _BarCodeReadResult = false;    
        // Steps
        public enum InCvSequence
        {
            Idle,
            In_Sensor_Check,
            CV_Run_Wait,
            CV_Off_Check,
            CV_Off_Wait
        }
        public enum BottomHandle
        {
            Idle,
            Out_Position_Tray_Check,
            TrayInSecsorCheck,
            Set_PutDown,
            Set_Handler_Up,
            Set_PutDown_Done,
            Bottom_Clmap_Pickup,
            Bottom_RZ_Down_Wait,
            Bottom_Clamp_Grip,
            Bottom_Handler_Up,
            Bottom_PicUp_Done,
            Set_Handler_Down,
            Bottom_Handler_Forward,
            Set_PickUp_Down,
            Set_PickUp_Down_Wait,
            Set_Centering_Fwd_Check,
            Set_Vacuum_On,
            Http_Receive_Check,
            Set_Centering_Bwd,
            Set_PickUp_Up,
            Set_PickUp_Done,
            Bottom_PutDown_Down,
            Bottom_UnGrip,
            Bottom_PutDown_Up,
            Bottom_Centering_Fwd,
            Bottom_PutDown_Done,
            Set_PutDown_Right

        }
        public enum TopHandle
        {
            Idle,
            Top_Handle_Up_Check,
            Top_Handle_Pickup_Position_Check,
            Top_Clamp_In_Check,
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
            Top_Tray_Sensor_Check,
            Out_Handle_Y_Pickup_Pos_Check,
            Y_Move_Wait,
            Y_Move_Start,
            Out_Handle_Z_Down_Done,
            Out_Handle_Grip_Check,
            Out_Handle_Grip_Wait,
            Out_Handle_Z_Pickup_Up_Done,
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
        public enum Ready_Step
        {
            Idle,
            Out_Z_Ready_Move,
            Out_Z_Ready_Wait,
            In_Handler_Ready_Move,
            In_Handler_Ready_Wait,
            Y_Position_Move,
            Y_Move_Wait,
            X_Z_In_Ready_Wait

        }
        public InCvSequence In_Cv_Step = InCvSequence.Idle;
        public OutCvSequence Out_Cv_Step = OutCvSequence.Idle;
        public BottomHandle Bottom_Step = BottomHandle.Idle;
        public TopHandle Top_Handle_Step = TopHandle.Idle;
        public OutHandle Out_Handle_Step = OutHandle.Idle;
        public Aging_CV_Step AgingCVStep = Aging_CV_Step.Idle;

        public Rtn_Top_CV_1 RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
        public Rtn_BTM_CV RetBtmCV_Step = Rtn_BTM_CV.Idle;
        public Top_NG TopNgStep = Top_NG.Idle;
        public Top_CV TopCvStep = Top_CV.Idle;

        public Ready_Step ReadyStep = Ready_Step.Idle;
        public Aging_CV_Pass_Step AgingCvPassStep = Aging_CV_Pass_Step.Idle;
        public void Loop()
        {
            // Task.Delay를 사용하는경우 Loop 동작 확인후 리턴. 중복호출 방지
            if (_isLoopRunning)
                return;

            switch (UnitGroup)
            {
                case MotionUnit_List.Top_X:
                    if (SingletonManager.instance.EquipmentMode != EquipmentMode.AgingPass)
                    {
                        Top_Handel_Logic();
                        Bottom_Handel_Logic();
                    }
                    break;
                case MotionUnit_List.Out_Y:
                    if (SingletonManager.instance.EquipmentMode != EquipmentMode.AgingPass)
                        Out_Handle_Y_Logic();
                    break;
                case MotionUnit_List.Lift_1:
                    if (SingletonManager.instance.EquipmentMode != EquipmentMode.AgingPass)
                    {
                        //Aging_CV_Logic();
                        Aging_CV_StepRun_Logic();
                    }
                    else
                    {
                        AgingConveyorPass();
                    }
                    break;
                case MotionUnit_List.In_CV:
                    if (SingletonManager.instance.EquipmentMode != EquipmentMode.AgingPass)
                        In_CV_Logic();

                    Top_Cv();
                    Return_Bottom_CV_Logic();
                    Return_Top_CV_1_Logic();
                    break;
                case MotionUnit_List.Out_CV:
                    if (SingletonManager.instance.EquipmentMode != EquipmentMode.AgingPass)
                    {
                        Bottom_Out_CV_Logic();
                        Top_NG_CV_Logic();
                    }
                    break;
            }
        }
        private void In_CV_Logic()
        {
            switch (In_Cv_Step)
            {
                case InCvSequence.Idle:
                    In_Cv_Step = InCvSequence.In_Sensor_Check;
                    break;
                case InCvSequence.In_Sensor_Check:
                    // 제품 투입 sensor 와 도착 위치에 제품이 없을때 cv on
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.INTERFACE_FRONT_MC_SAFETY] == true)
                    {
                        _TimeDelay.Restart();
                        In_Cv_Step = InCvSequence.CV_Run_Wait;
                    }
                    break;
                case InCvSequence.CV_Run_Wait:
                    if (_TimeDelay.ElapsedMilliseconds >1000)
                    {
                        Dio_Output(DO_MAP.INPUT_SET_CV_RUN, true);
                        In_Cv_Step = InCvSequence.CV_Off_Check;
                    }
                    break;
                case InCvSequence.CV_Off_Check:
                    // cv out 센서 감지 됬을때 cv off 하고 centering 전진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == true)
                    {
                        _TimeDelay.Restart();

                        In_Cv_Step = InCvSequence.CV_Off_Wait;
                        Global.instance.InputCountPlus();
                    }
                    break;
                case InCvSequence.CV_Off_Wait:
                    if (_TimeDelay.ElapsedMilliseconds >1000)
                    {
                        // cv off
                        Dio_Output(DO_MAP.INPUT_SET_CV_RUN, false);
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
                            if (SingletonManager.instance.SetNfcResult == true)
                            {
                                // OK 일때
                                Out_Cv_Step = OutCvSequence.Out_CV_Tray_OK_Out;
                            }
                            else
                            {
                                // NG 일때
                                Out_Cv_Step = OutCvSequence.Out_CV_Tray_NG_Out;
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
                    }
                    break;
                case OutCvSequence.Out_CV_Tray_NG_Out:
                    // ng 이면 도착 센서 감지 후 스토퍼 하강
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] != true)
                    {
                        // cv On
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, true);
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, true);
                        // 스토퍼 하강
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, false);
                        Out_Cv_Step = OutCvSequence.Out_CV_Tray_NG_Check;
                    }
                    break;
                case OutCvSequence.Out_Clamp_CV_Stop:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] != true
                        && (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_1] == true || Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == true))
                    {
                        // cv On
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                        Out_Cv_Step = OutCvSequence.Out_Clamp_CV_StopperUp_Wait;
                        _TimeDelay.Restart();
                    }
                    break;
                case OutCvSequence.Out_Clamp_CV_StopperUp_Wait:
                    if (_TimeDelay.ElapsedMilliseconds > 1000)
                    {
                        // 스토퍼 상승
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        Out_Cv_Step = OutCvSequence.Out_CV_Tray_NG_Check;
                    }
                    break;
                case OutCvSequence.Out_CV_Tray_NG_Check:
                    // Tray 도착 감지 센서 꺼질때까지 대기 후 Cv Off 
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] == true)
                    {
                        // 스토퍼 상승되여 있지 않으면 다시 상승
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] != true)
                            Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        // cv off
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, false);
                        Out_Cv_Step = OutCvSequence.Idle;
                    }
                    break;
                case OutCvSequence.Out_CV_Off_Check:
                    // top 조립 위치 도착 센서 받으면 CV Off 센터링 전징
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true)
                    {
                        _TimeDelay.Restart();
                        // cv Off
                        Out_Cv_Step = OutCvSequence.Out_CV_Off_Wait;
                    }
                    break;
                case OutCvSequence.Out_CV_Off_Wait:
                    if (_TimeDelay.ElapsedMilliseconds > 500)
                    {
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                        Out_Cv_Step = OutCvSequence.Idle;
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
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == false)
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
                    if (_TimeDelay.ElapsedMilliseconds > 2000)
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
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == false
                        || Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_SS_2] == false)
                    {
                        // cv on
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, true);
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, true);
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
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_Unclalmp_IF_Send;
                        _TopCvTimeDelay.Restart();
                    }
                    else if (_TopCvTimeDelay.ElapsedMilliseconds > 2000)
                    {
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, false);
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_Unclalmp_IF_Send;
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
                case Rtn_Top_CV_1.Top_Unclalmp_IF_Send:
                    // unclamp에서 I/F신호다 들오와있고 Top Return C/V투입에 제품이 없으면 I/F ON
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_2_1] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == true)
                    {
                        // Unclamp Top Return Interface On
                        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, true);
                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_Unclamp_IF_Off;
                    }
                    else if (_TopCvTimeDelay.ElapsedMilliseconds > 1000)
                    {
                        // not Receive for Unclamp
                        RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
                    }
                    break;
                case Rtn_Top_CV_1.Top_Unclamp_IF_Off:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == false)
                    {
                        // Unclamp Top Return Interface On
                        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, false);
                        RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
                    }
                    break;
            }
        }
        public void Return_Bottom_CV_Logic()
        {
            switch(RetBtmCV_Step)
            {
                case Rtn_BTM_CV.Idle:
                    // Return Botton CV 도착위치에 clamp가 없으면  CV run
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == false)
                    {
                        if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.BTM_RETURN_CV_RUN] == false)
                            Dio_Output(DO_MAP.BTM_RETURN_CV_RUN, true);
                        if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.BTM_RETURN_CV_RUN_2] == false)
                            Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, true);
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
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, false);
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Send;
                        _BottomCvTimeDelay.Restart();
                    }
                    break;
                case Rtn_BTM_CV.Rtn_BTM_CV_Stop_Wait:

                    if (_BottomCvTimeDelay.ElapsedMilliseconds > 300)
                    {
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN, false);
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, false);
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Send;
                        _BottomCvTimeDelay.Restart();
                    }
                    break;
                case Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Send:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_2_1] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == true)
                    {
                        Dio_Output(DO_MAP.BOTTOM_RETURN_CV_INTERFACE, true);
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Off;
                    }
                    else if (_BottomCvTimeDelay.ElapsedMilliseconds > 1000)
                    {
                        RetBtmCV_Step = Rtn_BTM_CV.Idle;
                    }
                    break;
                case Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Off:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == false)
                    {
                        Dio_Output(DO_MAP.BOTTOM_RETURN_CV_INTERFACE, false);
                        RetBtmCV_Step = Rtn_BTM_CV.Idle;
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
                    int time = (int)_TimeDelay.ElapsedMilliseconds;
                    Bottom_Step = BottomHandle.Out_Position_Tray_Check;

                    Global.Mlog.Info($"Bottom_Step => Next Step : Out_Position_Tray_Check");
                    break;
                case BottomHandle.Out_Position_Tray_Check:
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
                    Bottom_Step = BottomHandle.TrayInSecsorCheck;
                    Global.Mlog.Info($"Bottom_Step => Next Step : TrayInSecsorCheck");
                    break;
                case BottomHandle.TrayInSecsorCheck:
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
                        /************************************************************************/
                        // Bottom Clamp가 있는지 확인한다.
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] == true)
                            || _NoneSetTest == true
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            // Bottom clamp가 놓여져 있으면 Put Down 동작을 한다.
                            Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, true);
                            Bottom_Step = BottomHandle.Set_PutDown;

                            Global.Mlog.Info($"Bottom_Step => Left Z Down");
                            Global.Mlog.Info($"Bottom_Step => Next Step : Set_PutDown");
                        }
                        else
                        {
                            // Bottom Clamp가 없으면 Clamp Pickup 으로 이동한다.
                            Bottom_Step = BottomHandle.Bottom_Clmap_Pickup;
                            Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Clmap_Pickup");
                            _BottomHandlerTimeDelay.Restart();
                        }
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
                        Bottom_Step = BottomHandle.Set_Handler_Up;

                        Global.Mlog.Info($"Bottom_Step => Vacuum Off, Bolw On");
                        Global.Mlog.Info($"Bottom_Step => Left Z Up");
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
                        Bottom_Step = BottomHandle.Set_PutDown_Done;

                        Global.Mlog.Info($"Bottom_Step => Bolw Off");
                        Global.Mlog.Info($"Bottom_Step => CV Side Centering BWD");
                        Global.Mlog.Info($"Bottom_Step => CV Up Centering BWD");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_PutDown_Done");
                    }
                    break;
                case BottomHandle.Set_PutDown_Done:
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_2_BWD] == true
                       && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DOWN_CYL_SS] == true)
                       || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        _BottomHandlerTimeDelay.Restart();
                        Bottom_Step = BottomHandle.Bottom_Clmap_Pickup;
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Clmap_Pickup");
                        // 여기서 CV Move 한다.
                        // 스토퍼 상승
                        SingletonManager.instance.BottomClampDone = true;
                        Global.Mlog.Info($"Bottom_Step => BottomClampDone : true");
                        //// 불량이면
                        //if (SingletonManager.instance.EquipmentMode != EquipmentMode.Dry)
                        //    SingletonManager.instance.BottomClampSetResult = true;
                        //// 아니면 
                        //SingletonManager.instance.BottomClampSetResult = false;
                    }
                    break;
                case BottomHandle.Bottom_Clmap_Pickup:
                    // Return CV에 bottom clamp가 있으면 
                    if ( Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        _BottomHandlerTimeDelay.Restart();
                        Bottom_Step = BottomHandle.Bottom_RZ_Down_Wait;

                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_RZ_Down_Wait");
                    }
                    // Y Pickup 필요할때 바텀지그가 3초안에 들어오지 않으면 left이동했다가 다시 픽업으로온다
                    else if (SingletonManager.instance.IsY_PickupColl == true
                        && _BottomHandlerTimeDelay.ElapsedMilliseconds > 3000)
                    {
                        // Bottom Pickup Move , false:Right  true:Left
                        Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, true);

                        Global.Mlog.Info($"Bottom_Step => Left Move");
                        Global.Mlog.Info($"Bottom_Step => Next Step : TrayInSecsorCheck");
                        Bottom_Step = BottomHandle.TrayInSecsorCheck;
                    }
                    break;
                case BottomHandle.Bottom_RZ_Down_Wait:
                    if (_BottomHandlerTimeDelay.ElapsedMilliseconds > 500)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                        Bottom_Step = BottomHandle.Bottom_Clamp_Grip;

                        Global.Mlog.Info($"Bottom_Step => Right Z Down");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Clamp_Grip");
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
                    break;
                case BottomHandle.Bottom_PicUp_Done:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true)
                    {
                        Bottom_Step = BottomHandle.Bottom_Handler_Forward;

                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Handler_Forward");
                    }
                    break;
                
                case BottomHandle.Bottom_Handler_Forward:
                    // Panel Grip Return
                    Dio_Output(DO_MAP.TRANSFER_LZ_TURN_SOL, false);
                    // Panel Pickup Move
                    Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, true);
                    Bottom_Step = BottomHandle.Set_PickUp_Down;

                    Global.Mlog.Info($"Bottom_Step => Left Z Turn Off");
                    Global.Mlog.Info($"Bottom_Step => X Left Move");
                    Global.Mlog.Info($"Bottom_Step => Next Step : Set_PickUp_Down");
                    break;
                case BottomHandle.Set_PickUp_Down:
                    // Set Pickup으로 Turn하고 전진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_TURN_CYL_SS] == true)
                    {
                        // Input Stop 일때
                        if (SingletonManager.instance.IsInspectionInputStop == true)
                            break;
                        // Set 진입 되있으면
                        /**************************************************/
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == true
                            || _NoneSetTest == true
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            _BottomHandlerTimeDelay.Restart();
                            Bottom_Step = BottomHandle.Set_PickUp_Down_Wait;

                            Global.Mlog.Info($"Bottom_Step => Next Step : Set_PickUp_Down_Wait");
                        }
                    }
                    break;
                case BottomHandle.Set_PickUp_Down_Wait:
                    if (_BottomHandlerTimeDelay.ElapsedMilliseconds >1000)
                    {
                        Dio_Output(DO_MAP.IN_SET_CV_CENTERING, true);
                        Bottom_Step = BottomHandle.Set_Centering_Fwd_Check;

                        Global.Mlog.Info($"Bottom_Step => In SET Centering FWD");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_Centering_Fwd_Check");
                    }
                    break;
                case BottomHandle.Set_Centering_Fwd_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_UNALIGN_CYL_SS] == true)
                    {
                        // Set Handler Down
                        Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, true);
                        Bottom_Step = BottomHandle.Set_Vacuum_On;

                        Global.Mlog.Info($"Bottom_Step => Left Z Down");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_Vacuum_On");
                    }
                    break;
                case BottomHandle.Set_Vacuum_On:
                    // Vacuum On
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_DOWN_CYL_SS] == true)
                    {
                        Global.Mlog.Info($"Bottom_Step => NFC UseNotuse : {SingletonManager.instance.SystemModel.NfcUseNotUse.ToString()}");
                        if (SingletonManager.instance.SystemModel.NfcUseNotUse == "Use")
                        {
                            string nfc = SingletonManager.instance.SerialModel[1].NfcData;
                            Global.Mlog.Info($"Bottom_Step => NFC Data : {nfc}");
                            if (!string.IsNullOrEmpty(nfc))
                            {
                                SingletonManager.instance.HttpJsonModel.SendRequest("getPrevInspInfo", nfc, "");
                                Bottom_Step = BottomHandle.Http_Receive_Check;
                            }
                            else
                            {
                                SingletonManager.instance.SetNfcResult = false;
                                Bottom_Step = BottomHandle.Set_Centering_Bwd;

                                Global.Mlog.Info($"Bottom_Step => NFC Result = FAIL");
                                Global.Mlog.Info($"Bottom_Step => Vacuum On");
                                Global.Mlog.Info($"Bottom_Step => Next Step : Set_Centering_Bwd");
                            }
                        }
                        else
                        {
                            SingletonManager.instance.SetNfcResult = true;
                            Bottom_Step = BottomHandle.Set_Centering_Bwd;
                            Global.Mlog.Info($"Bottom_Step => NFC Result = SKIP");
                            Global.Mlog.Info($"Bottom_Step => Vacuum On");
                            Global.Mlog.Info($"Bottom_Step => Next Step : Set_Centering_Bwd");
                        }

                        if (SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            Bottom_Step = BottomHandle.Set_Centering_Bwd;
                            SingletonManager.instance.SetNfcResult = true;
                        }
                        Dio_Output(DO_MAP.TRANSFER_LZ_VACUUM_SOL, true);
                    }
                    break;
                case BottomHandle.Http_Receive_Check:
                    if (SingletonManager.instance.HttpJsonModel.DataSendFlag == true)
                    {
                        Global.Mlog.Info($"Bottom_Step => rsltCode : {SingletonManager.instance.HttpJsonModel.ResultCode}");
                        if (SingletonManager.instance.HttpJsonModel.ResultCode == "PASS")
                        {
                            SingletonManager.instance.SetNfcResult = true;
                            Global.Mlog.Info($"Bottom_Step => NFC Result : PASS");
                            Bottom_Step = BottomHandle.Set_Centering_Bwd;
                        }
                        else
                        {
                            Global.Mlog.Info($"Bottom_Step => NFC Result : FAIL");
                            SingletonManager.instance.SetNfcResult = false;
                        }
                        Bottom_Step = BottomHandle.Set_Centering_Bwd;
                    }
                    break;
                case BottomHandle.Set_Centering_Bwd:
                    // Set CV Out centering Backward
                    /***********************************************************/
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] == true
                        || _NoneSetTest == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.IN_SET_CV_CENTERING, false);
                        Bottom_Step = BottomHandle.Set_PickUp_Up;

                        Global.Mlog.Info($"Bottom_Step => In SET Centering BWD");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_PickUp_Up");
                    }
                    break;
                case BottomHandle.Set_PickUp_Up:
                    // centerring 후진 센서가 뭔지 모르겠다.???
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_UNALIGN_CYL_SS] == false)
                    {
                        // Set Handler Up
                        Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                        Bottom_Step = BottomHandle.Set_PickUp_Done;

                        Global.Mlog.Info($"Bottom_Step => Left Z Up");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Set_PickUp_Done");
                    }
                    break;
                case BottomHandle.Set_PickUp_Done:
                    // Set Handler Up 확인하고 Bottom PutDown 동작
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true)
                    {
                        Bottom_Step = BottomHandle.Bottom_PutDown_Down;
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_PutDown_Down");
                    }
                    break;
                case BottomHandle.Bottom_PutDown_Down:
                    // Bottom putdown위치에 clamp가 없고 clamp cv 가 멈춘상태이면 Down 동작
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_RUN] == false)
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.CLAMPING_CV_UP_SOL, false);
                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_2, false);

                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                        Bottom_Step = BottomHandle.Bottom_UnGrip;

                        Global.Mlog.Info($"Bottom_Step => Right Z Down");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_UnGrip");
                    }
                    break;
                case BottomHandle.Bottom_UnGrip:
                    // Bottom PutDown Cylinder Down 확인 후 ungrip
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_DOWN_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_GRIP_SOL, false);
                        Bottom_Step = BottomHandle.Bottom_PutDown_Up;

                        Global.Mlog.Info($"Bottom_Step => Ungrip");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_PutDown_Up");
                    }
                    break;
                case BottomHandle.Bottom_PutDown_Up:
                    // Bottom Handler Up
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UNGRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                        Bottom_Step = BottomHandle.Bottom_Centering_Fwd;

                        Global.Mlog.Info($"Bottom_Step => Right Z Up");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_Centering_Fwd");
                    }
                    break;
                case BottomHandle.Bottom_Centering_Fwd:
                    // Bottom sentering fwd
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.CLAMPING_CV_UP_SOL, true);
                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_2, true);
                        Bottom_Step = BottomHandle.Bottom_PutDown_Done;

                        Global.Mlog.Info($"Bottom_Step => CV Up Centering Up");
                        Global.Mlog.Info($"Bottom_Step => CV Side Centering FWD");
                        Global.Mlog.Info($"Bottom_Step => Next Step : Bottom_PutDown_Done");
                    }
                    break;
                case BottomHandle.Bottom_PutDown_Done:
                    // Bottom sentering fwd
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLMAPING_CV_UP_CYL_SS] == true
                        && (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_2_FWD] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry))
                    {
                        // Out X Handler 동작하지 않을때를 까지 대기한다.

                        if (SingletonManager.instance.IsY_PickupColl == false
                            && Ez_Model.IsOutHandlerSaftyInterlockY() == true
                            && SingletonManager.instance.BottomClampDone == false)
                        {
                            Global.Mlog.Info($"Bottom_Step => BottomClampDone : false");
                            Global.Mlog.Info($"Bottom_Step => IsY_PickupColl : false");
                            Global.Mlog.Info($"Bottom_Step => Next Step : Idle");
                            Bottom_Step = BottomHandle.Idle;
                        }
                    }
                    break;
            }
            int step = (int)Bottom_Step;
            Global.instance.Write_Sequence_Log("BOTTOM_STEP", step.ToString());
            if (Dio.DO_RAW_DATA[(int)DO_MAP.TRANSFER_FORWARD_SOL] == true)
                Global.instance.Write_Sequence_Log("BOTTOM_HANDLER_POS", "LEFT");
            else
                Global.instance.Write_Sequence_Log("BOTTOM_HANDLER_POS", "RIGHT");

            if (Dio.DO_RAW_DATA[(int)DO_MAP.TRANSFER_RZ_DOWN_SOL] == false)
                Global.instance.Write_Sequence_Log("BOTTOM_RZ", "UP");
            else
                Global.instance.Write_Sequence_Log("BOTTOM_RZ", "DOWN");
            if (Dio.DO_RAW_DATA[(int)DO_MAP.TRANSFER_LZ_DOWN_SOL] == false)
                Global.instance.Write_Sequence_Log("BOTTOM_LZ", "UP");
            else
                Global.instance.Write_Sequence_Log("BOTTOM_LZ", "DOWN");
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
                        // Input Stop 상태일때
                        if (SingletonManager.instance.IsInspectionInputStop == true)
                            break;
                        // Top Clamping 완료한 상태이면 X Handle Coll Flag ON
                        //Global.Mlog.Info($"TopHandle =>  TopClampingDone : {TopClampingDone.ToString()}");
                        // Top Clamp가 조립되여 있으면
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                        {
                            // X Handle Pickup완료 후 Clamping 상태 변경
                            TopClampingDone = false;
                            SingletonManager.instance.IsY_PickupColl = true;
                            Global.Mlog.Info($"TopHandle =>  IsY_PickupColl : true");
                        }
                       Top_Handle_Step = TopHandle.Top_Clamp_In_Check;
                       Global.Mlog.Info($"TopHandle =>  Next Step : Top_Clamp_In_Check");
                    }
                    break;
                case TopHandle.Top_Clamp_In_Check:
                    // Top Tray in 센서 확인 후 tryp가있으면 Down 한다.
                    if(Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        _TopHandlerTimeDelay.Restart();
                        Top_Handle_Step = TopHandle.Top_PickUp_Time_Wait;
                        Global.Mlog.Info($"TopHandle =>  Next Step : Top_PickUp_Time_Wait");
                    }
                    break;
                case TopHandle.Top_PickUp_Time_Wait:
                    if (_TopHandlerTimeDelay.ElapsedMilliseconds > 2000)
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
                            myIni.Write(SingletonManager.instance.SerialModel[0].Barcode, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), "AGING");
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
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Top_Handle_Step = TopHandle.Top_Handle_Tray_Out_Wait;
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_Tray_Out_Wait");
                    }
                    break;
                case TopHandle.Top_Handle_Tray_Out_Wait:
                    // Top Cmap put down 위치에 Bottom이 들어와 있고  Out Y 축이 Pickup위치에 있지않으면
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == true
                        && SingletonManager.instance.IsY_PickupColl == false)
                    {
                        // Tray NG이면 NG 포지션으로 이동
                        /***************************************************************/
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
                            Top_Handle_Step = TopHandle.Buttom_Clamp_Arrival_Check;
                            Global.Mlog.Info($"TopHandle => Next Step : Buttom_Clamp_Arrival_Check");
                        }


                        if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                           && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                        {
                            // X Handle Pickup완료 후 Clamping 상태 변경
                            TopClampingDone = false;
                            SingletonManager.instance.IsY_PickupColl = true;

                            //Global.Mlog.Info($"TopHandle => TopClampingDone : true");
                            Global.Mlog.Info($"TopHandle => IsY_PickupColl : true");
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
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == false
                        && SingletonManager.instance.IsY_PickupColl == false)
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
                    // ok일경우 안착위치도착 확인 후 Centering 전진
                    if (Ez_Model.IsTopHandlerPutDownPos() == true )
                    {
                        Global.Mlog.Info($"TopHandle => IsTopHandlerPutDownPos Done");
                        Global.Mlog.Info($"TopHandle => CV Centering FWD");

                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, true);
                        Top_Handle_Step = TopHandle.Top_Handle_Centering_FWD_Check;

                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_Centering_FWD_Check");
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
                    }
                    break;
                
                case TopHandle.Top_Handle_PutDown_Check:
                    // PutDown 완료 후 grip UnLock
                    //if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true
                    //    && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_2] == true)
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                        Top_Handle_Step = TopHandle.Top_Handle_Grip_Unlock_Check;

                        Global.Mlog.Info($"TopHandle => Ungrip");
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_Grip_Unlock_Check");
                    }
                    break;
                case TopHandle.Top_Handle_Grip_Unlock_Check:
                    // grip UnLock 
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        //Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                        //Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, false); // stopper down

                        Global.Mlog.Info($"TopHandle => TR Z Up");
                        Global.Mlog.Info($"TopHandle => Next Step : Top_Handle_PutDown_Up_Check");

                        Top_Handle_Step = TopHandle.Top_Handle_PutDown_Up_Check;
                    }
                    break;
                case TopHandle.Top_Handle_PutDown_Up_Check:
                    // centering 후진 센서 확인 후 PutDown 한다
                    //if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_1_BWD] == true
                    //    && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                    //    && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                    && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                    {
                        // Top Clamping 완료 상태 변경
                        TopClampingDone = true;
                        Top_Handle_Step = TopHandle.Idle;

                        Global.Mlog.Info($"TopHandle => TopClampingDone : true");
                        Global.Mlog.Info($"TopHandle => Next Step : Idle");
                    }
                    break;
            }
            int step = (int)Top_Handle_Step;
            Global.instance.Write_Sequence_Log("TOP_STEP", step.ToString());
            Global.instance.Write_Sequence_Log("Y_PICKUP_COLL_FLAG", SingletonManager.instance.IsY_PickupColl.ToString());

            if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1] == false)
                Global.instance.Write_Sequence_Log("TOP_JIG_1", "UP");
            else
                Global.instance.Write_Sequence_Log("TOP_JIG_1", "DOWN");
            if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2] == false)
                Global.instance.Write_Sequence_Log("TOP_JIG_2", "UP");
            else
                Global.instance.Write_Sequence_Log("TOP_JIG_2", "DOWN");
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
                    break;
                case OutHandle.Out_Handle_Z_Up_Done:
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true)
                    {
                        // Out Handle Z up 완료 후 Tray Sensor 확인
                        Out_Handle_Step = OutHandle.Top_Tray_Sensor_Check;
                        Global.instance.TactTimeStart = false;
                        _TimeDelay.Restart();
                    }
                    break;
                case OutHandle.Top_Tray_Sensor_Check:
                    // Tray Sensor 들어오고 & Top Handle이 PIckup위치에 있을때
                    // Bottom Handle  Set Pickup위치에서 Down된 상태이면 
                    // Out Handle Pickup위치로 이동한다.
                    if (SingletonManager.instance.IsY_PickupColl == true
                        && Ez_Model.IsTopHandlerPickUpPos() == true                                 // Top Handle Pickup위치
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true     // Out CV 배출 Bottom Tray Sensor
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true     // Out CV 배출 Top Tray Sensor
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true)  // Bottom Handle Left 위치 도착
                    {
                        // Out Handle Y축 Pickup위치로 이동
                        if (Ez_Model.MoveOutHandlerPickUpY() == true)
                        {
                            Out_Handle_Step = OutHandle.Out_Handle_Y_Pickup_Pos_Check;
                            Global.instance.LoadingTactTimeStart();
                            Global.instance.TactTimeStart = true;
                        }
                    }
                    else
                    {
                        // Y PickUp Time out 발생시 현제 싸여져있는 만큼 배출
                        if (SingletonManager.instance.EquipmentMode == EquipmentMode.Auto)
                        {
                            if (SingletonManager.instance.SystemModel.PickUpWaitTimeOutY > 0
                                && SingletonManager.instance.SystemModel.PickUpWaitTimeOutY < _TimeDelay.ElapsedMilliseconds / 1000)
                            {
                                Global.Mlog.Info($"OutHandle => Y PickUp Time Out : {SingletonManager.instance.SystemModel.PickUpWaitTimeOutY.ToString()}");
                                Global.Mlog.Info($"OutHandle => Lift No : {SingletonManager.instance.LoadStageNo.ToString()}");
                                // Clamp 배출한다.
                                SingletonManager.instance.LoadComplete[SingletonManager.instance.LoadStageNo] = true;
                                SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = 0;
                                SingletonManager.instance.LoadStageNo += 1;
                                if (SingletonManager.instance.LoadStageNo >= (int)Lift_Index.Max)
                                    SingletonManager.instance.LoadStageNo = 0;
                                Out_Handle_Step = OutHandle.Idle;
                            }
                            else if (_TimeDelay.ElapsedMilliseconds == 0)
                                _TimeDelay.Restart();
                        }
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
                        // Top Clamping 완료 위치 제품있는지 확인 
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            // Out Handle Z down
                            Ez_Model.MoveOutHandlerPickUpZ();
                            Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                            Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, false); // stopper down
                            
                            Out_Handle_Step = OutHandle.Out_Handle_Z_Down_Done;
                        }
                    }
                    // Y 축 충돌 방지용
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] != true)
                    {
                        Ez_Model.ServoStop((int)ServoSlave_List.Out_Y_Handler_Y);
                        Out_Handle_Step = OutHandle.Y_Move_Wait;
                    }
                    break;
                case OutHandle.Y_Move_Wait:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true)
                    {
                        if (Ez_Model.MoveOutHandlerPickUpY() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_Y_Pickup_Pos_Check;
                    }
                    break;
                case OutHandle.Out_Handle_Z_Down_Done:
                    // Out Handle Z down 완료하면 Grip lock
                    if (Ez_Model.IsOutHandlerPickUpZ() == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_1_BWD] == true) 
                    {
                        // Out Handle Grip Lock
                        Dio_Output(DO_MAP.CLAMPING_LD_Z_GRIP_SOL, true);
                        Out_Handle_Step = OutHandle.Out_Handle_Grip_Check;
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
                    if (_TimeDelay.ElapsedMilliseconds >1000)
                    {
                        // Out Handle Z up
                        Ez_Model.MoveOutHandlerRadyZ();
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        Out_Handle_Step = OutHandle.Out_Handle_Z_Pickup_Up_Done;
                    }
                    break;
                case OutHandle.Out_Handle_Z_Pickup_Up_Done:
                    // Z up완료 후 X PutDown위치 이동
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true 
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == true)
                    {
                        // Dry run 일때는 Put Down 하기전에 이전 데이터를 초기화
                        if (SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            int floorMax = 7;
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
                            Out_Handle_Step = OutHandle.Out_Handle_Y_PutDown_Pos_Check;
                    }
                    // stopper가 없이 되여 있지 않으면 다시 up을 한다.
                    else if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] != true)
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                    break;
                case OutHandle.Out_Handle_Y_PutDown_Pos_Check:
                    // X Putdown위치 도착하면 기록한 층수위에 안착한다.
                    // 안착 포지션은 1,2,3 순으로 놓는다
                    if (Ez_Model.IsOutHandlerYPutDownPos() == true)
                    {
                        SingletonManager.instance.IsY_PickupColl = false;
                        // Out Handle Z down
                        if (Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) != true)
                        {
                            Ez_Model.MoveLiftLoding(SingletonManager.instance.LoadStageNo);
                        }
                        Out_Handle_Step = OutHandle.Lift_Loding_Move_Done;
                    }
                    break;
                case OutHandle.Lift_Loding_Move_Done:
                    if (Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true)
                    {
                        // Out Handle Z down
                        if (Ez_Model.MoveOutHandlerPutDownZ() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_Z_PutDown_Done;
                    }
                    break;
                case OutHandle.Out_Handle_Z_PutDown_Done:
                    // Z Down완료 후 Ungrip
                    if (Ez_Model.IsOutHandlerPutDownDoneZ() == true)
                    {
                        // Out Handle UnGrip 
                        Dio_Output(DO_MAP.CLAMPING_LD_Z_GRIP_SOL, false);
                        Out_Handle_Step = OutHandle.Out_Handle_UnGrip_Check;
                    }
                    break;
                case OutHandle.Out_Handle_UnGrip_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_UNGRIP_CYL_SS] == true)
                    {
                        Ez_Model.MoveOutHandlerRadyZ();
                        Out_Handle_Step = OutHandle.Out_Handle_Z_Ready_Check;
                    }
                    break;
                case OutHandle.Out_Handle_Z_Ready_Check:
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true)
                    {
                        // 적제 단수 증가
                        int floor = SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo];
                        SingletonManager.instance.Display_Lift[SingletonManager.instance.LoadStageNo].Floor[floor] = true;

                        SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] += 1;

                        int floorMax = 7;
                        if (SingletonManager.instance.SystemModel.LoadFloorCount > 0)
                            floorMax = SingletonManager.instance.SystemModel.LoadFloorCount;

                        if (SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] >= floorMax)
                        {
                            // 적제 완료 했으면 Complete 상태 변경한다. false 원복은 Aging C/V에 배출 후 변경한다.
                            SingletonManager.instance.LoadComplete[SingletonManager.instance.LoadStageNo] = true;

                            _TimeDelay.Restart();
                            Out_Handle_Step = OutHandle.Lift_Out_Wait;
                        }
                        else
                            Out_Handle_Step = OutHandle.Idle;

                        Global.instance.LoadCountPlus();
                    }
                    break;
                case OutHandle.Lift_Out_Wait:
                    bool LiftWait = false;
                    if (_TimeDelay.ElapsedMilliseconds < 1000)
                        break;
                    if (SingletonManager.instance.LoadStageNo == 0)
                    {
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_IN_SS_1] == false
                            || (SingletonManager.instance.AgingCvIndex < 3 && Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true)
                            || SingletonManager.instance.AgingCvIndex >= 3)
                        {
                            LiftWait = true;
                        }
                    }
                    else if(SingletonManager.instance.LoadStageNo == 1)
                    {
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_IN_SS_1] == false
                            || (SingletonManager.instance.AgingCvIndex < 3 && Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true)
                            || SingletonManager.instance.AgingCvIndex >= 3)
                        {
                            LiftWait = true;
                        }
                    }
                    else if (SingletonManager.instance.LoadStageNo == 2)
                    {
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_IN_SS_1] == false
                            || (SingletonManager.instance.AgingCvIndex < 3 && Ez_Model.IsMoveLiftLodingDone(SingletonManager.instance.LoadStageNo) == true)
                            || SingletonManager.instance.AgingCvIndex >= 3)
                        {
                            LiftWait = true;
                        }
                    }
                    if (LiftWait == true )//&& SingletonManager.instance.LoadComplete[SingletonManager.instance.LoadStageNo] == false)
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

                        SingletonManager.instance.LoadFloor[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = 0;
                        for (int i = 0; i < (int)Floor_Index.Max; i++)
                            SingletonManager.instance.Display_Lift[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].Floor[i] = false;

                        // Interfase 신호를 on했으면 Off 조건확인 시퀀스로 이동한다.
                        if (GetInterfaseSendStatus(SingletonManager.instance.AgingCvIndex) == true)
                            AgingCVStep = Aging_CV_Step.Unclamping_IF_Set_Off;
                        else
                            AgingCVStep = Aging_CV_Step.Low_Lift_Up_Start;
                    }
                    break;
                #region  //시작 종료 센서 전부 확인하는 로직
                // aging CV start End Sensor 체크
                //GetAgingCVStartEndSS(SingletonManager.instance.AgingCvIndex);

                //// Clamp 한번이라도 감지않되면 
                //if (AgingCvFull[SingletonManager.instance.AgingCvIndex] == false)
                //    AgingCvEndStopCondition[SingletonManager.instance.AgingCvIndex] = true;
                //else
                //    AgingCvEndStopCondition[SingletonManager.instance.AgingCvIndex] = false;
                //if (AgingCvStart[SingletonManager.instance.AgingCvIndex] == false)
                //    AgingCvInStopCondition[SingletonManager.instance.AgingCvIndex] = true;
                //else
                //    AgingCvInStopCondition[SingletonManager.instance.AgingCvIndex] = false;

                //// Aging CV 배출 끝단 센서가 한번꺼졌다가 다시 들어오면
                //// **** CV 끝단 센서를 최대한 끝으로 달아야한다.*****
                //if (AgingCvFull[SingletonManager.instance.AgingCvIndex] == true
                //    && AgingCvEndStopCondition[SingletonManager.instance.AgingCvIndex] == true)
                //{
                //    // Aging CV Stop
                //    Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false);

                //    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                //        Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                //    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                //        Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                //    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                //        Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                //    AgingCVStep = Aging_CV_Step.Unclamping_IF_Set_Off;

                //    SingletonManager.instance.LoadFloor[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = 0;
                //    for (int i = 0; i < (int)Floor_Index.Max; i++)
                //        SingletonManager.instance.Display_Lift[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].Floor[i] = false;
                //}
                //// Aging CV 진입구가 센서가 한번꺼졌다가 다시 들어오면
                //else if (AgingCvStart[SingletonManager.instance.AgingCvIndex] == true
                //    && AgingCvInStopCondition[SingletonManager.instance.AgingCvIndex] == true)
                //{
                //    // Aging CV Stop
                //    Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false);

                //    if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 0)
                //        Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                //    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 1)
                //        Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                //    else if (GetLiftNomber(SingletonManager.instance.AgingCvIndex) == 2)
                //        Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                //    AgingCVStep = Aging_CV_Step.Unclamping_IF_Set_Off;

                //    SingletonManager.instance.LoadFloor[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = 0;
                //    for (int i = 0; i < (int)Floor_Index.Max; i++)
                //        SingletonManager.instance.Display_Lift[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].Floor[i] = false;
                //}
                //break;
                #endregion
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
            Global.instance.Write_Sequence_Log("LOAD_COMPLETE_FLAG", SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].ToString());
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
                    AgingCVStep = Aging_CV_Step.CV_On_Condition_Wait;
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
                    // 적제 complete true 이지만 Lift에 Clamp가 없으면 Step Time으로 전진
                    else if (SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] == true
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

                        SingletonManager.instance.LoadFloor[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = 0;
                        for (int i = 0; i < (int)Floor_Index.Max; i++)
                            SingletonManager.instance.Display_Lift[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].Floor[i] = false;

                        // Interfase 신호를 on했으면 Off 조건확인 시퀀스로 이동한다.
                        if (GetInterfaseSendStatus(SingletonManager.instance.AgingCvIndex) == true)
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
                        SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = false;
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

                        SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = false;
                        // 다음 에이징 컨베아 Index로  변수증가 Upper:0,1,2 Low:3,4,5
                        SingletonManager.instance.AgingCvIndex += 1;
                        if (SingletonManager.instance.AgingCvIndex >= 6)
                            SingletonManager.instance.AgingCvIndex = 0;

                        AgingCVStep = Aging_CV_Step.Idle;
                     }
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
                    // Aging CV Start
                    Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, true);
                    
                    AgingCVStep = Aging_CV_Step.Cv_Step_IF_Clamp_OutWait;
                    break;
                case Aging_CV_Step.Cv_Step_IF_Clamp_OutWait:
                    // Clamp가 aging end seneor에 감지되지않을때 cv stop 체크들어간다
                    if (GetAgingCVEndSS(SingletonManager.instance.AgingCvIndex) == false)
                    {
                        AgingCVStep = Aging_CV_Step.Cv_Step_IF_Stop;
                        _TimeDelay.Restart();
                    }                    
                    break;
                case Aging_CV_Step.Cv_Step_IF_Stop:
                    // 다음 clamp가 감지되든가 Step 이동 시간 초과시 aging cv stop
                    if (GetAgingCVEndSS(SingletonManager.instance.AgingCvIndex) == true
                        || _TimeDelay.ElapsedMilliseconds > (SingletonManager.instance.SystemModel.AgingCvStepTime + 3000))
                    {
                        // Aging CV Start
                        Dio_Aging_CV_Control(SingletonManager.instance.AgingCvIndex, false);
                        AgingCVStep = Aging_CV_Step.Cv_Step_IF_Off;
                    }
                    break;
                case Aging_CV_Step.Cv_Step_IF_Off:
                    if (UnclampInterfaseReturnOn(SingletonManager.instance.AgingCvIndex) == false)
                    {
                        SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = false;
                        SetUnclampInterfase(SingletonManager.instance.AgingCvIndex, false);
                        AgingCVStep = Aging_CV_Step.Idle;

                        SingletonManager.instance.AgingCvIndex += 1;
                        if (SingletonManager.instance.AgingCvIndex >= 6)
                            SingletonManager.instance.AgingCvIndex = 0;
                    }
                    break;

            }
            int step = (int)AgingCVStep;
            Global.instance.Write_Sequence_Log("AGING_CV_STEP", step.ToString());
            Global.instance.Write_Sequence_Log("AGING_CV_INDEX", SingletonManager.instance.AgingCvIndex.ToString());
            Global.instance.Write_Sequence_Log("LOAD_COMPLETE_FLAG", SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)].ToString());
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
        #region // Motion Control

        #endregion

        #region // Top Handler

        #endregion
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
        private bool GetInterfaseSendStatus(int index)
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
        private void LiftNextIndex(int CvIndex)
        {
            // i<= 6 Aging Conveyor 하나만 켰을때 현재 진행했던 index를 확인할수 있도록 7번 반복한다.
            for (int i = 0; i <= 6; i++)
            {
                CvIndex++; // 다음 인덱스
                if (CvIndex >= 6)
                    CvIndex =0; // 0~5 까지 인덱스
                if (SingletonManager.instance.SystemModel.AgingCvNotUse[CvIndex] == "Use")
                {
                    if (i == 0 || i == 3)
                    {
                        SingletonManager.instance.LoadStageNo = 0;
                    }
                    else if (i == 1 || i == 4)
                    {
                        SingletonManager.instance.LoadStageNo = 1;
                    }
                    else if (i == 2 || i == 5)
                    {
                        SingletonManager.instance.LoadStageNo = 2;
                    }
                    break;
                }
            }
        }
        private void AgingCvNextIndex(int CvIndex)
        {
            // i<= 6 Aging Conveyor 하나만 켰을때 현재 진행했던 index를 확인할수 있도록 7번 반복한다.
            for (int i = 0; i <= 6; i++)
            {
                CvIndex++; // 다음 인덱스
                if (CvIndex >= 6)
                    CvIndex = 0; // 0~5 까지 인덱스
                if (SingletonManager.instance.SystemModel.AgingCvNotUse[CvIndex] == "Use")
                {
                    SingletonManager.instance.AgingCvIndex = CvIndex;
                    break;
                }
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
                SingletonManager.instance.AgingCvIndex = 0;
            else
                SingletonManager.instance.AgingCvIndex = Convert.ToInt32(value);
            value = myIni.Read("LOAD_COMPLETE_FLAG", "SEQUENCE");
            if(value == "")
                SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = false;
            else
                SingletonManager.instance.LoadComplete[GetLiftNomber(SingletonManager.instance.AgingCvIndex)] = bool.Parse(value);

            // 모터 마지막 위치 원복은 위험함으로 생각 좀...
            /*
            switch (ReadyStep)
            {
                case Ready_Step.Idle:
                    
                    ReadyStep = Ready_Step.Out_Z_Ready_Move;
                    Dio.Set_HandlerUpDown(true);
                    break;
                case Ready_Step.Out_Z_Ready_Move:
                    Ez_Model.MoveOutHandlerRadyZ();
                    ReadyStep = Ready_Step.Out_Z_Ready_Wait;
                    break;
                case Ready_Step.Out_Z_Ready_Wait:
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true)
                    {
                        ReadyStep = Ready_Step.In_Handler_Ready_Move;
                    }
                    break;
                case Ready_Step.In_Handler_Ready_Move:
                    Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, true);
                    ReadyStep = Ready_Step.In_Handler_Ready_Wait;
                    break;
                case Ready_Step.In_Handler_Ready_Wait:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_RIGHT_CYL_SS] == true)
                        ReadyStep = Ready_Step.Y_Position_Move;
                    break;
                case Ready_Step.Y_Position_Move:
                    value = myIni.Read("OUT_Y_SERVO_POS", "SEQUENCE");
                    pos = Math.Round(Convert.ToDouble(value), 2);
                    SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Out_Y_Handler_Y), pos);
                    ReadyStep = Ready_Step.Y_Move_Wait;
                    break;
                case Ready_Step.Y_Move_Wait:
                    value = myIni.Read("OUT_Y_SERVO_POS", "SEQUENCE");
                    pos = Math.Round(Convert.ToDouble(value), 2);
                    double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
                    if (GetPos == pos)
                    {
                        if (Ez_Model.IsOutHandlerPickupPosY() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_Y_Pickup_Pos_Check;
                        else if (Ez_Model.IsOutHandlerYPutDownPos() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_Y_PutDown_Pos_Check;

                        value = myIni.Read("TOP_SERVO_POS", "SEQUENCE");
                        pos = Math.Round(Convert.ToDouble(value), 2);
                        SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Top_X_Handler_X), pos);

                        value = myIni.Read("BOTTOM_HANDLER_POS", "SEQUENCE");
                        if (value == "LEFT")
                            Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, true);
                        else
                            Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, false);
                        ReadyStep = Ready_Step.X_Z_In_Ready_Wait;
                    }
                    break;
                case Ready_Step.X_Z_In_Ready_Wait:
                    value = myIni.Read("BOTTOM_HANDLER_POS", "SEQUENCE");
                    bool InHanderLR = false;
                    if (value == "LEFT")
                    {
                        InHanderLR = Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_RIGHT_CYL_SS];
                    }
                    else
                    {
                        InHanderLR = Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_RIGHT_CYL_SS];
                    }

                    value = myIni.Read("TOP_SERVO_POS", "SEQUENCE");
                    pos = Math.Round(Convert.ToDouble(value), 2);
                    GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Top_X_Handler_X)), 2);

                    if (InHanderLR == true
                    && GetPos == pos)
                    {
                        value = myIni.Read("TOP_JIG_1", "SEQUENCE");
                        if (value == "UP")
                            Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        else
                            Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, true);
                        value = myIni.Read("TOP_JIG_2", "SEQUENCE");
                        if (value == "UP")
                            Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        else
                            Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, true);

                        value = myIni.Read("BOTTOM_RZ", "SEQUENCE");
                        if (value == "UP")
                            Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                        else
                            Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                        value = myIni.Read("BOTTOM_LZ", "SEQUENCE");
                        if (value == "UP")
                            Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                        else
                            Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, true);

                        ReadyStep = Ready_Step.Idle;
                        SingletonManager.instance.UnitLastPositionSet = false;
                    }
                    break;
            }
            */
        }
    }
}
