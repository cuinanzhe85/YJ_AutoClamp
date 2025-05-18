using Common.Managers;
using HelixToolkit.Wpf;
using Lmi3d.Zen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Windows.Data;
using YJ_AutoClamp.ViewModels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
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
        private bool[] TopReturnDone = { false, false,};
        private bool[] AgingCvFull = { false, false, false, false, false, false };
        private bool[] AgingCvStart = { false, false, false, false, false, false };
        private bool[] AgingCvEndStopCondition = { false, false, false, false, false, false };
        private bool[] AgingCvInStopCondition = { false, false, false, false, false, false };
        public Unit_Model(MotionUnit_List unit)
        {
            UnitGroup = unit;
            UnitID = (int)unit;
            ServoNames = new List<ServoSlave_List>();
        }
        private RadObservableCollection<Channel_Model> Channel_Model = SingletonManager.instance.Channel_Model;
        private Dictionary<string,double> Teaching_Data = SingletonManager.instance.Teaching_Data;
        private EziDio_Model Dio = SingletonManager.instance.Ez_Dio;
        private EzMotion_Model_E Ez = SingletonManager.instance.Ez_Model;

        private bool _isLoopRunning = false;
        // Steps
        public enum InCvSequence
        {
            Idle,
            In_Sensor_Check,
            CV_Off_Check,
            Centering_forward_Check,
            Centering_Backward_Check
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
            Bottom_Clamp_Grip,
            Bottom_Handler_Up,
            Bottom_PicUp_Done,
            Set_Handler_Down,
            Bottom_Handler_Forward,
            Set_PickUp_Down,
            Set_Vacuum_On,
            Set_Centering_Bwd,
            Set_PickUp_Up,
            Set_PickUp_Done,
            Bottom_PutDown_Down,
            Bottom_UnGrip,
            Bottom_PutDown_Up,
            Bottom_Centering_Fwd,
            Bottom_PutDown_Done,

            HandleLeftMoveCheck,
            PutDownSensorCheck,
            GripSensorCheck,
            HandleUpSensorCheck,
            HandleRightMoveCheck,
            PanelInputSensorCheck,
            NgBottomTrayPutDownCheck,
            NgBottomTrayUnGripCheck,
            PanelPutdownCheck,
            VacuumOnGribUnlockCheck,
            Input_Centering_BWD_Check,
            HandleUpCheck,
            Out_CV_On
        }
        public enum TopHandle
        {
            Idle,
            Top_Handle_Up_Check,
            Top_Handle_Pickup_Position_Check,
            Top_Tray_In_Check,
            Top_Handle_Down_Check,
            Top_Handle_Grip_Lock_Check,
            Top_Handle_PutUp_Check,
            Top_Handle_Tray_Out_Wait,
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
            Out_Handle_X_Pickup_Pos_Check,
            Out_Handle_Z_Down_Done,
            Out_Handle_Grip_Check,
            Out_Handle_Z_Pickup_Up_Done,
            Out_Handle_X_PutDown_Pos_Check,
            Out_Handle_Z_PutDown_Done,
            Out_Handle_UnGrip_Check,
            Out_Handle_Z_Ready_Check
        }
        public enum OutCvSequence
        {
            Idle,
            Out_CV_On_Wait,
            Out_CV_Tray_OK_Out,
            Out_CV_Tray_NG_Out,
            Out_CV_Tray_NG_Check,
            Out_CV_Off_Wait,
            Out_CV_Centering_FWD_Check,
            Out_CV_Centering_BWD_Check
        }
        public enum Aging_CV_Step
        {
            Idle,
            CV_On_Condition_Wait,
            Low_Lift_Down,
            Unclamping_IF_Send,
            Unclamping_IF_Receive,
            Lift_CV_Forward,
            Aging_CV_Forward,
            CV_Stop,
            Unclamping_IF_Set_Off,
            Low_Lift_Up_Start,
            Low_Lift_Up_Wait
            
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
        public enum Rtn_BTM_CV
        {
            Idle,
            Rtn_BTM_CV_Stop,
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
        public Aging_CV_Step[] AgingCVStep = { Aging_CV_Step.Idle, 
                                            Aging_CV_Step.Idle, 
                                            Aging_CV_Step.Idle,
                                            Aging_CV_Step.Idle, 
                                            Aging_CV_Step.Idle, 
                                            Aging_CV_Step.Idle };

        public Rtn_Top_CV_1 RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
        public Rtn_BTM_CV RetBtmCV_Step = Rtn_BTM_CV.Idle;
        public Top_NG TopNgStep = Top_NG.Idle;

        public Ready_Step ReadyStep = Ready_Step.Idle;
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
                    // Aging C/V 개수만큼 돌린다.
                    //Aging_CV_Logic(0);
                    //Aging_CV_Logic(2);
                    //Aging_CV_Logic(4);
                    //Aging_CV_Logic(1);
                    //Aging_CV_Logic(3);
                    //Aging_CV_Logic(5);
                    break;
                case MotionUnit_List.In_CV:
                    In_CV_Logic();
                    Return_Bottom_CV_Logic();
                    Return_Top_CV_1_Logic();
                    break;
                case MotionUnit_List.Out_CV:
                    Bottom_Out_CV_Logic();
                    //Top_NG_CV_Logic();
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
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_IN_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == false)
                    {
                        Dio_Output(DO_MAP.INPUT_SET_CV_RUN, true);
                        In_Cv_Step = InCvSequence.CV_Off_Check;
                    }
                    break;
                case InCvSequence.CV_Off_Check:
                    // cv out 센서 감지 됬을때 cv off 하고 centering 전진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == true)
                    {
                        // cv off
                        Dio_Output(DO_MAP.INPUT_SET_CV_RUN, false);
                        // centering 전진
                        Dio_Output(DO_MAP.IN_SET_CV_CENTERING, true);

                        In_Cv_Step = InCvSequence.Centering_forward_Check;
                    }
                    break;
                case InCvSequence.Centering_forward_Check:
                    // 센터링 전진센서 확인 후 후진 sentering 후진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_UNALIGN_CYL_SS] == true) // IN_CV_UNALIGN_CYL_SS 센서 확인 필요 ????
                    {
                        //Dio_Output(DO_MAP.IN_SET_CV_CENTERING, false);

                        //In_Cv_Step = InCvSequence.Centering_Backward_Check;
                        In_Cv_Step = InCvSequence.Idle; // 센터링 후진은 SET 흡착할후 하는걸로.
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
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] != true 
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // 스토퍼 상승
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_STOPER_UP_SOL] == false)
                            Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        Out_Cv_Step = OutCvSequence.Out_CV_On_Wait;
                    }
                    break;
                case OutCvSequence.Out_CV_On_Wait:
                    // bottom tray ok/ng 확인 & Pannel 장착 완료 상태 확인 후 cv on
                    if (SingletonManager.instance.BottomClampDone == true)
                    {
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == true         // Bottom clamp 센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_5] == true  // Bottom clamp 센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_6] == true  // Bottom clamp  센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == false // Top clamp  센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == false // Top clamp  센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_3] == false // Top clamp  센서
                                && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true)    // Set grap handler up
                                || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)   
                        {
                            if (SingletonManager.instance.BottomClampNG == true)
                            {
                                // NG 일때
                                Out_Cv_Step = OutCvSequence.Out_CV_Tray_NG_Out;
                            }
                            else
                            {
                                // OK 일때
                                Out_Cv_Step = OutCvSequence.Out_CV_Tray_OK_Out;
                            }
                        }
                    }
                    break;
                case OutCvSequence.Out_CV_Tray_OK_Out:
                    // Clmap 끝단 제품이 있는지 확인 한다.제품 없을때 cv on
                    SingletonManager.instance.BottomClampDone = false;
                    // 스토퍼 상승
                    if (Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_STOPER_UP_SOL] == false)
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                    //cv on
                    Dio_Output(DO_MAP.CLAMPING_CV_RUN, true);
                    Out_Cv_Step = OutCvSequence.Out_CV_Off_Wait;
                    break;
                case OutCvSequence.Out_CV_Tray_NG_Out:
                    // ng 이면 도착 센서 감지 후 스토퍼 하강
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] != true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] != true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_1] != true)
                    {
                        // NG 변수 Flag 초기화
                        SingletonManager.instance.BottomClampNG = false;
                        // cv On
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, true);
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, true);
                        // 스토퍼 하강
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, false);
                        Out_Cv_Step = OutCvSequence.Out_CV_Tray_NG_Check;
                    }
                    break;
                case OutCvSequence.Out_CV_Tray_NG_Check:
                    // Tray 도착 감지 센서 꺼질때까지 대기 후 Cv Off 
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_1] == true)
                    {
                        // 스토퍼 상승
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        // cv off
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                        Dio_Output(DO_MAP.NG_BOTTOM_JIG_CV_RUN, false);
                        Out_Cv_Step = OutCvSequence.Idle;
                    }
                    break;
                case OutCvSequence.Out_CV_Off_Wait:
                    // 도착 센서 받으면 CV Off 센터링 전징
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true)
                        //&& Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                    {
                        // cv Off
                        Dio_Output(DO_MAP.CLAMPING_CV_RUN, false);
                        
                        Out_Cv_Step = OutCvSequence.Idle;
                    }
                    break;
            }
        }
        private void Return_Top_CV_1_Logic()
        {
            switch(RetTopCV_1_Step)
            {
                case Rtn_Top_CV_1.Idle:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == false)
                    {
                        // cv on
                        MoveTopReturnCvRun();
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, true);
                        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, true);
                        TopReturnDone[0] = false;
                        TopReturnDone[1] = false;
                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_CV_Stop;
                    }
                    else
                    {
                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_Unclalmp_IF_Send;
                    }
                    break;
                case Rtn_Top_CV_1.Top_CV_Stop:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == true
                        || Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == true)
                    {
                        if (TopReturnDone[0] == false)
                        {
                            // cv off
                            MoveTopReturnCvStop();
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, false);
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                            TopReturnDone[0] = true;
                            RetTopCV_1_Step = Rtn_Top_CV_1.Top_Unclalmp_IF_Send;
                        }
                    }
                    //if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_SS_2] == true
                    //    || Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == true)
                    //{
                    //    if (TopReturnDone[1] == false)
                    //    {
                    //        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, false);
                    //        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                    //        TopReturnDone[1] = true;
                    //    }
                    //}
                    //if (TopReturnDone[0] == true && TopReturnDone[1] == true)
                    //    RetTopCV_1_Step = Rtn_Top_CV_1.Top_Unclalmp_IF_Send;

                    // Top In에 제품이 없으면
                    /*
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] != true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] != true)
                    {
                        // Unclamp Top Return Interface Off
                        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, false);
                        // cv on
                        MoveTopReturnCvRun();
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN] != true)
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, true);
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN_2] != true)
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, true);

                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_CV_Stop;
                        break;
                    }
                    // Return Top CV 에 Tray가 있으면 cv stop
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_SS_2] == true)
                    {
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN] == true)
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, false);
                        if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN_2] == true)
                            Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                    }
                    */
                    break;
                    /*
                case Rtn_Top_CV_1.Top_CV_Stop:
                    // Top Pickup위치에 Tray가 진입되면 Top In CV Stop
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == true)
                    {
                        // cv on
                        if (TopReturnDone[0] != true)
                        {
                            MoveTopReturnCvStop();
                            TopReturnDone[0] = true;
                        }
                    }
                    // Return Top CV에 Tray 
                    //if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_SS_2] != true)
                    //{
                    //    TopReturnDone[1] = true;
                    //}
                    // 
                    // Top Pickup에 Tray가 있고 Top Return CV 도착에 Tray가 있으면
                    //if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_SS_2] == true 
                    //    && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == true)
                    //{
                    //    if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN] == true)
                    //        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN, false);
                    //    if (Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN_2] == true)
                    //        Dio_Output(DO_MAP.TOP_RETURN_CV_RUN_2, false);
                    //}
                    //if (TopReturnDone[0] == true 
                    //    && Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN] == false
                    //    && Dio.DO_RAW_DATA[(int)DO_MAP.TOP_RETURN_CV_RUN_2] == false)
                    //{
                    //    TopReturnDone[0] = false;
                    //    TopReturnDone[1] = false;
                    //    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_2_1] == false)
                    //    {
                    //        // Unclamp Top Return Interface On
                    //        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, true);
                    //    }
                        
                    //    RetTopCV_1_Step = Rtn_Top_CV_1.Idle;
                    //}
                    break;
                    */
                case Rtn_Top_CV_1.Top_Unclalmp_IF_Send:
                    // unclamp에서 I/F신호다 들오와있고 Top Return C/V투입에 제품이 없으면 I/F ON
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_TOP_CV_DETECT_2_1] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] == true)
                    {
                        // Unclamp Top Return Interface On
                        Dio_Output(DO_MAP.TOP_RETURN_CV_INTERFACE, true);
                        RetTopCV_1_Step = Rtn_Top_CV_1.Top_Unclamp_IF_Off;
                    }
                    else
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
        private void Return_Bottom_CV_Logic()
        {
            switch(RetBtmCV_Step)
            {
                case Rtn_BTM_CV.Idle:
                    // Return Botton CV 도착위치에 clamp가 없으면  CV run
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == false)
                    {
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN, true);
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, true);
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_CV_Stop;
                    }
                    else
                    {
                        // 이미 clamp가 있으면 Unclamp Interface 확인 한다.
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Send;
                    }
                    break;
                case Rtn_BTM_CV.Rtn_BTM_CV_Stop:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == true
                        || Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == true)
                    {
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN, false);
                        Dio_Output(DO_MAP.BTM_RETURN_CV_RUN_2, false);
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Send;
                    }
                    break;
                case Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Send:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_2_1] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] == true)
                    {
                        Dio_Output(DO_MAP.BOTTOM_RETURN_CV_INTERFACE, true);
                        RetBtmCV_Step = Rtn_BTM_CV.Rtn_BTM_Unclmap_IF_Off;
                    }
                    else
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
        private void Top_NG_CV_Logic()
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
                        && IsMoveTopNGPortDone() != true)
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.NG_TOP_JIG_CV_RUN, true);
                        TopNgStep = Top_NG.Top_CV_Stop;
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
                    Bottom_Step = BottomHandle.Out_Position_Tray_Check;
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
                    break;
                case BottomHandle.TrayInSecsorCheck:
                    // Bottom Handler Up,Ungrip상태 확인
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UNGRIP_CYL_SS] == true)
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // Set handler 90도 Turn
                        Dio_Output(DO_MAP.TRANSFER_LZ_TURN_SOL, true);
                        // Bottom Pickup Move , false: tray pickup true: set pickup
                        Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, false);
                        
                        Bottom_Step = BottomHandle.Set_Handler_Down;
                    }
                    break;
                case BottomHandle.Set_Handler_Down:
                    // Left 위치 도착 확인
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_FORWARD_CYL_SS] == true)
                    {
                        // Bottom Clamp가 있는지 확인한다.
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_5] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_TURN_CYL_SS] == true)
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            // Bottom clamp가 놓여져 있으면 Put Down 동작을 한다.
                            Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, true);
                            Bottom_Step = BottomHandle.Set_PutDown;
                        }
                        else
                        {
                            // Bottom Clamp가 없으면 Clamp Pickup 으로 이동한다.
                            Bottom_Step = BottomHandle.Bottom_Clmap_Pickup;
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
                    }
                    break;
                case BottomHandle.Set_PutDown_Done:
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_2_BWD] == true
                       && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DOWN_CYL_SS] == true)
                       || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Bottom_Step = BottomHandle.Bottom_Clmap_Pickup;
                        /****************************************************/
                        // 여기서 CV Move 한다.
                        // 스토퍼 상승
                        SingletonManager.instance.BottomClampDone = true;
                        // 불량이면
                        if (SingletonManager.instance.EquipmentMode != EquipmentMode.Dry)
                            SingletonManager.instance.BottomClampNG = true;
                        // 아니면 
                        SingletonManager.instance.BottomClampNG = false;
                    }
                    break;
                case BottomHandle.Bottom_Clmap_Pickup:
                    // Return CV에 bottom clamp가 있으면 
                    if ( Dio.DI_RAW_DATA[(int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                        Bottom_Step = BottomHandle.Bottom_Clamp_Grip;
                    }
                    break;
                case BottomHandle.Bottom_Clamp_Grip:
                    // Bottom clamp Grip
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_DOWN_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_GRIP_SOL, true);
                        Bottom_Step = BottomHandle.Bottom_Handler_Up;
                    }
                    break;
                case BottomHandle.Bottom_Handler_Up:
                    // Bottom handler Up
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                        Bottom_Step = BottomHandle.Bottom_PicUp_Done;
                    }
                    break;
                case BottomHandle.Bottom_PicUp_Done:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true)
                    {
                        Bottom_Step = BottomHandle.Bottom_Handler_Forward;
                    }
                    break;
                
                case BottomHandle.Bottom_Handler_Forward:
                    // Panel Grip Return
                    Dio_Output(DO_MAP.TRANSFER_LZ_TURN_SOL, false);
                    // Panel Pickup Move
                    Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, true);
                    Bottom_Step = BottomHandle.Set_PickUp_Down;
                    break;
                case BottomHandle.Set_PickUp_Down:
                    // Set Pickup으로 Turn하고 전진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_BACKWARD_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_RETURN_CYL_SS] == true)
                    {
                        // Set 진입 되있으면
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] == true
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            // Set Handler Down
                            Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, true);
                            Bottom_Step = BottomHandle.Set_Vacuum_On;
                        }
                        //else
                        //{
                        //    In_Grip_Step = InBottomHandle.Bottom_PutDown_Down;
                        //}
                    }
                    break;
                case BottomHandle.Set_Vacuum_On:
                    // Vacuum On
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_DOWN_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_LZ_VACUUM_SOL , true);
                        Bottom_Step = BottomHandle.Set_Centering_Bwd;
                    }
                    break;
                case BottomHandle.Set_Centering_Bwd:
                    // Set CV Out centering Backward
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.IN_SET_CV_CENTERING, false);
                        Bottom_Step = BottomHandle.Set_PickUp_Up;
                    }
                    break;
                case BottomHandle.Set_PickUp_Up:
                    // centerring 후진 센서가 뭔지 모르겠다.???
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.IN_CV_UNALIGN_CYL_SS] == false)
                    {
                        // Set Handler Up
                        Dio_Output(DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                        Bottom_Step = BottomHandle.Set_PickUp_Done;
                    }
                    break;
                case BottomHandle.Set_PickUp_Done:
                    
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true)
                    {
                        Bottom_Step = BottomHandle.Bottom_PutDown_Down;
                    }
                    break;
                case BottomHandle.Bottom_PutDown_Down:
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_4] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_5] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_6] == false
                        && Dio.DO_RAW_DATA[(int)DO_MAP.CLAMPING_CV_RUN] == false)
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                        Bottom_Step = BottomHandle.Bottom_UnGrip;
                    }
                    break;
                case BottomHandle.Bottom_UnGrip:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_DOWN_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_GRIP_SOL, false);
                        Bottom_Step = BottomHandle.Bottom_PutDown_Up;
                    }
                    break;
                case BottomHandle.Bottom_PutDown_Up:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UNGRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                        Bottom_Step = BottomHandle.Bottom_Centering_Fwd;
                    }
                    break;
                case BottomHandle.Bottom_Centering_Fwd:
                    // Bottom sentering fwd
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_2, true);
                        Dio_Output(DO_MAP.CLAMPING_CV_UP_SOL, true);
                        Bottom_Step = BottomHandle.Bottom_PutDown_Done;
                    }
                    break;
                case BottomHandle.Bottom_PutDown_Done:
                    // Bottom sentering fwd
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLMAPING_CV_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_2_FWD] == true)
                    {
                        // Out X Handler 동작하지 않을때를 까지 대기한다.
                        if (SingletonManager.instance.IsY_PickupColl == false
                            && IsOutHandlerSaftyInterlockY() == true)
                            Bottom_Step = BottomHandle.Idle;
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
                    // Up sensor Check 하여 하강 상태이면 Up하고 Grip unlock한다
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_2] == true)
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        Top_Handle_Step = TopHandle.Top_Handle_Up_Check;
                    }
                    break;
                case TopHandle.Top_Handle_Up_Check:
                    // up 상태이면 Pickup위치로 이동하여 대기한다.
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS] == true
                        && IsOutHandlerPickupPosY() != true)
                    {
                        if (MoveTopHandlerPickUpPos() == true)
                            Top_Handle_Step = TopHandle.Top_Handle_Pickup_Position_Check;
                    }
                    
                    break;
                case TopHandle.Top_Handle_Pickup_Position_Check:
                    // pickup위치에 도착확인
                    if (IsTopHandlerPickUpPos() == true)
                    {
                        // X022,X023 센서 용도 확인 후 사용
                        //if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        //    && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)

                        // Top Clamping 완료한 상태이면 X Handle Coll Flag ON
                        if (TopClampingDone == true)
                        {
                            // X Handle Pickup완료 후 Clamping 상태 변경
                            TopClampingDone = false;
                            SingletonManager.instance.IsY_PickupColl = true;
                        }
                        Top_Handle_Step = TopHandle.Top_Tray_In_Check;
                    }
                    break;
                case TopHandle.Top_Tray_In_Check:
                    // Top Tray in 센서 확인 후 tryp가있으면 Down 한다.
                    if(Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_CV_DETECT_SS] == true
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, true);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        Top_Handle_Step = TopHandle.Top_Handle_Down_Check;
                    }
                    break;
                case TopHandle.Top_Handle_Down_Check:
                    // Down완료 후 Grip Lock
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, true);
                        Top_Handle_Step = TopHandle.Top_Handle_Grip_Lock_Check;
                    }
                    break;
                case TopHandle.Top_Handle_Grip_Lock_Check:
                    // Grip완료 후 up
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        Top_Handle_Step = TopHandle.Top_Handle_PutUp_Check;
                    }
                    break;
                case TopHandle.Top_Handle_PutUp_Check:
                    // Up 완료 하면 tray 배출위치에 제품이 있는지 확인 한다
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        Top_Handle_Step = TopHandle.Top_Handle_Tray_Out_Wait;
                    }
                    break;
                case TopHandle.Top_Handle_Tray_Out_Wait:
                    // Tray Out위치에 Bottom이 들어와 있고  Out X 축이 Pickup위치에 있지않으면
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == true
                        && SingletonManager.instance.IsY_PickupColl == false)
                    {
                        // Tray OK이면 안착위치로 이동
                        Top_Handle_Step = TopHandle.Buttom_Clamp_Arrival_Check;

                        // Tray NG이면 NG 포지션으로 이동
                        /***************************************************************/
                        //if (MoveTopHandlerNGPort() == true)
                        //    Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Move_Check;
                    }
                    if (SingletonManager.instance.IsY_PickupColl == false && SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // Tray OK이면 안착위치로 이동
                        Top_Handle_Step = TopHandle.Buttom_Clamp_Arrival_Check;

                        // Tray NG이면 NG 포지션으로 이동
                        /***************************************************************/
                        //if (MoveTopHandlerNGPort() == true)
                        //    Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Move_Check;
                    }
                    break;
                
                case TopHandle.Top_Handle_NG_Port_Move_Check:
                    // NG 안착위치에 도착했는지 확인 후 Down 한다
                    if (IsMoveTopNGPortDone() == true)
                    {
                        // NG Port에 Clmap가 없으면 
                        if (Dio.DI_RAW_DATA[(int)DI_MAP.NG_TOP_CV_DETECT_SS_1] != true)
                        {
                            Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, true);
                            Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, true);
                            Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Down_check;
                        }
                    }
                    break;
                case TopHandle.Top_Handle_NG_Port_Down_check:
                    // NG Port Down 완료 후 Grip Unlock
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_2] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                        Top_Handle_Step = TopHandle.Top_Handle_NG_Port_UnGrip_check;
                    }
                    break;
                case TopHandle.Top_Handle_NG_Port_UnGrip_check:
                    // NG Port Down 완료 후 Grip Unlock
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                        Top_Handle_Step = TopHandle.Top_Handle_NG_Port_Up_check;
                    }
                    break;
                case TopHandle.Top_Handle_NG_Port_Up_check:
                    // NG Port Down 완료 후 Grip Unlock
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true)
                    {
                        Top_Handle_Step = TopHandle.Idle;
                    }
                    break;
                case TopHandle.Buttom_Clamp_Arrival_Check:
                    // Bottom Clamp 도착 확인 후 PutDown 한다
                    // Stoper가 Down되있는 상태는 Bottom Clamp NG 배출이기때문에 
                    if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == false)
                        || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        if (MoveTopHandlerPutDownPos() == true)
                            Top_Handle_Step = TopHandle.Top_Handle_PutDown_Move_Check;
                    }
                    break;
                case TopHandle.Top_Handle_PutDown_Move_Check:
                    // ok일경우 안착위치도착 확인 후 Centering 전진
                    if (IsTopHandlerPutDownPos() == true )
                    {
                        Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, true);
                        Top_Handle_Step = TopHandle.Top_Handle_Centering_FWD_Check;
                    }
                    break;
                case TopHandle.Top_Handle_Centering_FWD_Check:
                    // centering 전진 센서 확인 후 후진
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_1_FWD] == true)
                    {
                        //Dio_Output(DO_MAP.TOP_JIG_TR_Z_UP_SOL_1, true);
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, true);

                        Top_Handle_Step = TopHandle.Top_Handle_PutDown_Check;
                    }
                    break;
                
                case TopHandle.Top_Handle_PutDown_Check:
                    // PutDown 완료 후 grip UnLock
                    //if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1] == true
                    //    && Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_2] == true)
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_2] == true)
                    {
                        Dio_Output(DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                        Top_Handle_Step = TopHandle.Top_Handle_Grip_Unlock_Check;
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
                    if (IsOutHandlerReadyDoneZ() != true)
                        MoveOutHandlerRadyZ();
                    Out_Handle_Step = OutHandle.Out_Handle_Z_Up_Done;
                    break;
                case OutHandle.Out_Handle_Z_Up_Done:
                    if (IsOutHandlerReadyDoneZ() == true)
                    {
                        // Out Handle Z up 완료 후 Tray Sensor 확인
                        Out_Handle_Step = OutHandle.Top_Tray_Sensor_Check;
                    }
                    break;
                case OutHandle.Top_Tray_Sensor_Check:
                    // Tray Sensor 들어오고 & Top Handle이 PIckup위치에 있을때
                    // Bottom Handle  Set Pickup위치에서 Down된 상태이면 
                    // Out Handle Pickup위치로 이동한다.
                    if (SingletonManager.instance.IsY_PickupColl == true
                        && IsTopHandlerPickUpPos() == true                                 // Top Handle Pickup위치
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true     // Out CV 배출 Bottom Tray Sensor
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true     // Out CV 배출 Top Tray Sensor
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_BACKWARD_CYL_SS] == true)  // Bottom Handle Left 위치 도착
                    {
                        // Out Handle X축 Pickup위치로 이동
                        if (MoveOutHandlerPickUpY() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_X_Pickup_Pos_Check;
                    }
                    if (SingletonManager.instance.IsY_PickupColl == true
                        && IsTopHandlerPickUpPos() == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_BACKWARD_CYL_SS] == true
                        && SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                    {
                        // Out Handle X축 Pickup위치로 이동
                        if (MoveOutHandlerPickUpY() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_X_Pickup_Pos_Check;
                    }
                    break;
                case OutHandle.Out_Handle_X_Pickup_Pos_Check:
                    // Out Handle Pickup위치 도착완료하면 z Down 한다.
                    if (IsOutHandlerPickupPosY() == true)
                    {
                        // Top Clamping 완료 위치 제품있는지 확인 
                        if ((Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_1] == true
                            && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_DETECT_SS_2] == true)
                            || SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                        {
                            // Out Handle Z down
                            MoveOutHandlerPickUpZ();
                            Out_Handle_Step = OutHandle.Out_Handle_Z_Down_Done;

                            Dio_Output(DO_MAP.CLAMPING_CV_CENTERING_SOL_1, false);
                            Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, false); // stopper down
                        }
                    }
                    break;
                case OutHandle.Out_Handle_Z_Down_Done:
                    // Out Handle Z down 완료하면 Grip lock
                    if (IsOutHandlerPickUpZ() == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == false
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_CENTERING_CYL_SS_1_BWD] == true) 
                    {
                        // Out Handle Grip Lock
                        Dio_Output(DO_MAP.CLAMPING_LD_Z_GRIP_SOL, true);
                        Out_Handle_Step = OutHandle.Out_Handle_Grip_Check;
                    }
                    break;
                case OutHandle.Out_Handle_Grip_Check:
                    // Grip lock완료하면 Z up
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)DI_MAP.CLAMPING_CV_STOPER_UP_CYL_SS] == false)
                    {
                        // Out Handle Z up
                        MoveOutHandlerRadyZ();
                        Dio_Output(DO_MAP.CLAMPING_CV_STOPER_UP_SOL, true);
                        Out_Handle_Step = OutHandle.Out_Handle_Z_Pickup_Up_Done;
                    }
                    break;
                case OutHandle.Out_Handle_Z_Pickup_Up_Done:
                    // Z up완료 후 X PutDown위치 이동
                    if (IsOutHandlerReadyDoneZ() == true)
                    {
                        // Out Handle X PutDown위치로 이동
                        if (MoveOutHandlerPutDownX() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_X_PutDown_Pos_Check;
                    }
                    break;
                case OutHandle.Out_Handle_X_PutDown_Pos_Check:
                    // X Putdown위치 도착하면 기록한 층수위에 안착한다.
                    // 안착 포지션은 1,2,3 순으로 놓는다
                    if (IsOutHandlerXPutDownPos() == true)
                    {
                        SingletonManager.instance.IsY_PickupColl = false;
                        // Out Handle Z down
                        if (MoveOutHandlerPutDownZ() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_Z_PutDown_Done;
                    }
                    break;
                case OutHandle.Out_Handle_Z_PutDown_Done:
                    // Z Down완료 후 Ungrip
                    if (IsOutHandlerPutDownDoneZ() == true)
                    {
                        // Out Handle UnGrip 
                        Dio_Output(DO_MAP.CLAMPING_LD_Z_GRIP_SOL, false);
                        Out_Handle_Step = OutHandle.Out_Handle_UnGrip_Check;
                    }
                    break;
                case OutHandle.Out_Handle_UnGrip_Check:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_UNGRIP_CYL_SS] == true)
                    {
                        MoveOutHandlerRadyZ();
                        Out_Handle_Step = OutHandle.Out_Handle_Z_Ready_Check;
                    }
                    break;
                case OutHandle.Out_Handle_Z_Ready_Check:
                    if (IsOutHandlerReadyDoneZ() == true)
                    {
                        // 적제 단수 증가
                        SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] += 1;
                        if (SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] >= (int)Floor_Index.Max)
                        {
                            // 7단 적제 완료 후 초기화
                            if (SingletonManager.instance.EquipmentMode == EquipmentMode.Dry)
                                SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = 0;

                            // 적제 완료 했으면 Complete 상태 변경한다. false 원복은 Aging C/V에 배출 후 변경한다.
                            SingletonManager.instance.LoadComplete[SingletonManager.instance.LoadStageNo] = true;

                            // 7단 적제 완료하면 다음 Load 위치로 설정
                            SingletonManager.instance.LoadStageNo += 1;
                            if (SingletonManager.instance.LoadStageNo >= (int)Lift_Index.Max)
                                SingletonManager.instance.LoadStageNo = 0;
                            
                        }
                        Out_Handle_Step = OutHandle.Idle;
                    }
                    break;
            }
            int step = (int)Out_Handle_Step;
            Global.instance.Write_Sequence_Log("OUT_HANDLER_STEP", step.ToString());
            Global.instance.Write_Sequence_Log("OUT_LOAD_FLOOR", SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo].ToString());
            Global.instance.Write_Sequence_Log("OUT_LOAD_STAGE", SingletonManager.instance.LoadStageNo.ToString());
        }
        private void Aging_CV_Logic(int Index)
        {
            switch(AgingCVStep[Index])
            {
                case Aging_CV_Step.Idle:
                    // 변수 초기화
                    AgingCvFull[Index] = false;
                    AgingCvStart[Index] = false;
                    AgingCvEndStopCondition[Index] = false;
                    AgingCvInStopCondition[Index] = false;

                    AgingCVStep[Index] = Aging_CV_Step.CV_On_Condition_Wait;
                    break;
                case Aging_CV_Step.CV_On_Condition_Wait:
                    int LiftNO = 0;
                    if (Index == 0 || Index == 3) LiftNO = 0;
                    if (Index == 1 || Index == 4) LiftNO = 1;
                    if (Index == 2 || Index == 5) LiftNO = 2;
                    if (SingletonManager.instance.LoadComplete[LiftNO] == true)
                    {
                        SingletonManager.instance.LoadComplete[LiftNO] = false;
                        GetAgingCVStartEndSS(Index);
                        // Upper
                        if (Index < 3)
                        {
                            if (AgingCvFull[Index] == true)
                            {
                                // Unclamping IF on step 이동
                                AgingCVStep[Index] = Aging_CV_Step.Unclamping_IF_Send;
                            }
                            else
                            {
                                // CV 전진 step 이동
                                AgingCVStep[Index] = Aging_CV_Step.Lift_CV_Forward;
                            }
                        }
                        // Low
                        else
                        {
                            // Lift Down Step으로 이동
                            if (LiftLowMoveConditon(Index) == true)
                            {
                                MoveLiftDown(Index);
                                AgingCVStep[Index] = Aging_CV_Step.Low_Lift_Down;
                            }
                        }
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Down:
                    if (IsMoveLiftDownDone(Index) == true)
                    {
                        if (AgingCvFull[Index] == true)
                        {
                            AgingCVStep[Index] = Aging_CV_Step.Unclamping_IF_Send;
                        }
                        else
                        {
                            AgingCVStep[Index] = Aging_CV_Step.Lift_CV_Forward;
                        }
                    }

                    break;
                case Aging_CV_Step.Unclamping_IF_Send:

                    SetUnclampInterfase(Index, true);

                    AgingCVStep[Index] = Aging_CV_Step.Unclamping_IF_Receive;
                    break;
                case Aging_CV_Step.Unclamping_IF_Receive:
                    
                    if (UnclampInterfaseReturnOn(Index) == true)
                        AgingCVStep[Index] = Aging_CV_Step.Lift_CV_Forward;

                    break;
                case Aging_CV_Step.Lift_CV_Forward:
                    if (Index == 0 || Index == 3)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_1, true);
                    if (Index == 1 || Index == 4)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_2, true);
                    if (Index == 2 || Index == 5)
                        Dio_Output(DO_MAP.LIFT_CV_RUN_3, true);

                    AgingCVStep[Index] = Aging_CV_Step.Aging_CV_Forward;
                    break;
                case Aging_CV_Step.Aging_CV_Forward:
                    /*
                    //int Lift_CV_IO = 0;
                    //if (Index == 0 || Index == 3)
                    //    Lift_CV_IO = (int)DO_MAP.LIFT_CV_RUN_1;
                    //if (Index == 1 || Index == 4)
                    //    Lift_CV_IO = (int)DO_MAP.LIFT_CV_RUN_2;
                    //if (Index == 2 || Index == 5)
                    //    Lift_CV_IO = (int)DO_MAP.LIFT_CV_RUN_3;

                    //if (Dio.DO_RAW_DATA[Lift_CV_IO] == false)
                    //    AgingCVStep[Index] = Aging_CV_Step.Lift_CV_Forward;
                    */
                    int Lift_CV_Out_IO = 0;
                    if (Index == 0 || Index == 3)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_1_CV_DETECT_OUT_SS_2;
                    if (Index == 1 || Index == 4)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_2_CV_DETECT_OUT_SS_2;
                    if (Index == 2 || Index == 5)
                        Lift_CV_Out_IO = (int)DI_MAP.LIFT_3_CV_DETECT_OUT_SS_2;
                    // Lift CV out sensor check
                    if (Dio.DI_RAW_DATA[Lift_CV_Out_IO] == true)
                    {
                        // Aging CV Run
                        Dio_Aging_CV_Control(Index, true);
                        AgingCVStep[Index] = Aging_CV_Step.CV_Stop;
                    }
                    break;
                case Aging_CV_Step.CV_Stop:

                    // aging CV start End Sensor 체크
                    GetAgingCVStartEndSS(Index);

                    if (Index == 0 || Index == 3) LiftNO = 0;
                    if (Index == 1 || Index == 4) LiftNO = 1;
                    if (Index == 2 || Index == 5) LiftNO = 2;

                    // Clamp 한번이라도 감지않되면 
                    if (AgingCvFull[Index] == false)
                        AgingCvEndStopCondition[Index] = true;
                    if (AgingCvStart[Index] == false)
                        AgingCvInStopCondition[Index] = true;

                    // Aging CV 배출 끝단 센서가 한번꺼졌다가 다시 들어오면
                    // **** CV 끝단 센서를 최대한 끝으로 달아야한다.*****
                    if (AgingCvFull[Index] == true && AgingCvEndStopCondition[Index] == true)
                    {
                        // Aging CV Stop
                        Dio_Aging_CV_Control(Index, false);

                        if (Index == 0 || Index == 1)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                        if (Index == 2 || Index == 3)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                        if (Index == 4 || Index == 5)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                        AgingCVStep[Index] = Aging_CV_Step.Unclamping_IF_Set_Off;

                        int Lift_NO = 0;
                        if (Index == 0 || Index == 3) Lift_NO = 0;
                        if (Index == 1 || Index == 4) Lift_NO = 1;
                        if (Index == 2 || Index == 5) Lift_NO = 2;
                        SingletonManager.instance.LoadFloor[Lift_NO] = 0;
                    }
                    // Aging CV 진입구가 센서가 한번꺼졌다가 다시 들어오면
                    else if (AgingCvStart[Index] == true && AgingCvInStopCondition[Index] == true)
                    {
                        // Aging CV Stop
                        Dio_Aging_CV_Control(Index, false);

                        if (Index == 0 || Index == 3)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_1, false);
                        if (Index == 1 || Index == 4)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_2, false);
                        if (Index == 2 || Index == 5)
                            Dio_Output(DO_MAP.LIFT_CV_RUN_3, false);
                        AgingCVStep[Index] = Aging_CV_Step.Unclamping_IF_Set_Off;

                        int Lift_NO = 0;
                        if (Index == 0 || Index == 3) Lift_NO = 0;
                        if (Index == 1 || Index == 4) Lift_NO = 1;
                        if (Index == 2 || Index == 5) Lift_NO = 2;
                        SingletonManager.instance.LoadFloor[Lift_NO] = 0;
                    }
                    break;
                case Aging_CV_Step.Unclamping_IF_Set_Off:
                    if (GetUnclampInterfaseOff(Index) == true)
                    {
                        SetUnclampInterfase(Index, false);
                        // 1층이면 Lift Up을 한다.
                        if (Index == 3 || Index == 4 || Index == 5)
                            AgingCVStep[Index] = Aging_CV_Step.Low_Lift_Up_Start;
                        else
                            AgingCVStep[Index] = Aging_CV_Step.Idle;
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Up_Start:
                    if (LiftLowMoveConditon(Index) == true)
                    {
                        MoveLiftUp(Index);
                        AgingCVStep[Index] = Aging_CV_Step.Low_Lift_Up_Wait;
                    }
                    break;
                case Aging_CV_Step.Low_Lift_Up_Wait:
                    if (IsMoveLiftUpDone(Index)==true)
                        AgingCVStep[Index] = Aging_CV_Step.Idle;
                    break;
                
            }
        }
        #region // Motion Control
        private bool MoveOutHandlerPickUpY()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Pick_Up).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Y_SERVO_POS", pos.ToString());
            return SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Out_Y_Handler_Y), pos);
        }
        private bool IsOutHandlerPickupPosY()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Pick_Up).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        private bool MoveOutHandlerPutDownX()
        {
            double pos = GetOutX_PutDownMovePosition();
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Y_SERVO_POS", pos.ToString());
            return SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Out_Y_Handler_Y), pos);
        }
        private bool IsOutHandlerXPutDownPos()
        {
            double pos = GetOutX_PutDownMovePosition();
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        private bool MoveOutHandlerPickUpZ()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Pick_Up).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Z_SERVO_POS", pos.ToString());
            return SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Out_Z_Handler_Z), pos);
        }
        private bool IsOutHandlerPickUpZ()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Pick_Up).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Z_Handler_Z)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        private bool MoveOutHandlerRadyZ()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Home).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Z_SERVO_POS", pos.ToString());
            return SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Out_Z_Handler_Z), pos);
        }
        private bool IsOutHandlerReadyDoneZ()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Home).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Z_Handler_Z)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        private bool MoveOutHandlerPutDownZ()
        {
            double pos = GetOutZ_PutDownFloorPos();
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Y_SERVO_POS", pos.ToString());
            return SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Out_Z_Handler_Z), pos);
        }
        private bool IsOutHandlerPutDownDoneZ()
        {
            double pos = GetOutZ_PutDownFloorPos();
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos,2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Z_Handler_Z)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        private bool IsOutHandlerSaftyInterlockY()
        {
            double pos1, pos2, pos3;
            pos1 = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_1).ToString()];
            pos1 = Math.Round(pos1, 2);
            pos2 = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_2).ToString()];
            pos2 = Math.Round(pos2, 2);
            pos3 = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_3).ToString()];
            pos3 = Math.Round(pos3, 2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
            
            if (GetPos == pos1 || GetPos == pos2 || GetPos == pos3)
                return true;
            return false;
        }
        private double GetOutZ_PutDownFloorPos()
        {
            double pos = 0.0;
            //double NowPos = SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Handler_X));
            //if (SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_X_Put_Down_1).ToString()] == NowPos)
            //{
            //    pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_Z_Put_Down_1 + LoadFloor[(int)Lift_Index.Lift_1]).ToString()];
            //}
            //else if (SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_X_Put_Down_2).ToString()] == NowPos)
            //{
            //    pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_Z_Put_Down_2 + LoadFloor[(int)Lift_Index.Lift_2]).ToString()];
            //}
            //else if (SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_X_Put_Down_3).ToString()] == NowPos)
            //{
            //    pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_Z_Put_Down_3 + LoadFloor[(int)Lift_Index.Lift_3]).ToString()];
            //}
            pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Put_Down_1 + SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo]).ToString()];
            pos = Math.Round(pos, 2);
            return pos;
        }
        private double GetOutX_PutDownMovePosition()
        {
            double pos = 0.0;
            // Lift 1,2,3 순서로 Load 한다.
            pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_1 + SingletonManager.instance.LoadStageNo).ToString()];
            pos = Math.Round(pos, 2);
            // 1번 Lift: tray가 있으면 아직 max층이 아니면 이어서 올린다.
            // 또는 비었있으면 1층부터 시작한다
            //if ((Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_IN_SS_1] == true
            //    && LoadFloor[(int)Lift_Index.Lift_1] > (int)Floor_Index.Max)
            //    || Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_IN_SS_1] != true)
            //{
            //    pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_X_Put_Down_1).ToString()];
            //}
            //else if ((Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_IN_SS_1] == true
            //    && LoadFloor[(int)Lift_Index.Lift_2] > (int)Floor_Index.Max)
            //    || Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_IN_SS_1] != true)
            //{
            //    pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_X_Put_Down_2).ToString()];
            //}
            //else if ((Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_IN_SS_1] == true
            //    && LoadFloor[(int)Lift_Index.Lift_3] > (int)Floor_Index.Max)
            //    || Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_3_CV_DETECT_IN_SS_1] != true)
            //{
            //    pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_X_Put_Down_3).ToString()];
            //}
            //else
            //{
            //    // Lift 3곳에 Tray가 Max층까지 다 싸여 있으면 1번 List에서 기다린다.
            //    pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Handler_X_Put_Down_1).ToString()];
            //}

            return pos;
        }
        #endregion
        #region // Top Handler
        private bool MoveTopHandlerNGPort()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_NG_Port).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("TOP_SERVO_POS", pos.ToString());
            return SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Top_X_Handler_X), pos);
        }
        private bool IsMoveTopNGPortDone()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_NG_Port).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Top_X_Handler_X)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        private bool MoveTopHandlerPutDownPos()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Put_Down).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("TOP_SERVO_POS", pos.ToString());
            return SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Top_X_Handler_X), pos);
        }
        private bool MoveTopHandlerPickUpPos()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Pick_Up).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("TOP_SERVO_POS", pos.ToString());
            return SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Top_X_Handler_X), pos);
        }
        private bool IsTopHandlerPickUpPos()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Pick_Up).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Top_X_Handler_X)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        private bool MoveTopReturnCvRun()
        {
            return SingletonManager.instance.Ez_Model.MoveJog((int)(ServoSlave_List.Top_CV_X),(int)Direction.CCW, 2);
        }
        private bool MoveTopReturnCvStop()
        {
            return SingletonManager.instance.Ez_Model.ServoStop((int)(ServoSlave_List.Top_CV_X));
        }
        private bool IsTopHandlerPutDownPos()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Put_Down).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Top_X_Handler_X)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        private bool MoveLiftDown(int Index)
        {
            double pos;
            bool ret = false;
            if (Index == 3)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_1).ToString()];
                pos = Math.Round(pos, 2);
                ret = SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Lift_1_Z), pos);
            }
            if (Index == 4)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_2).ToString()];
                pos = Math.Round(pos, 2);
                ret = SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Lift_2_Z), pos);
            }
            if (Index == 5)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_3).ToString()];
                pos = Math.Round(pos, 2);
                ret = SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Lift_3_Z), pos);
            }
            return ret;
        }
        private bool IsMoveLiftDownDone(int Index)
        {
            double pos;
            double GetPos;
            if (Index == 0 || Index == 1 || Index == 2)
                return true;
            if (Index == 3)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_1).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Lift_1_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (Index == 4)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_2).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Lift_2_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (Index == 5)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_3).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Lift_3_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            return false;
        }
        private bool MoveLiftUp(int Index)
        {
            double pos;
            bool ret = false;
            if (Index == 3)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_1).ToString()];
                pos = Math.Round(pos, 2);
                ret = SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Lift_1_Z), pos);
            }
            if (Index == 4)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_2).ToString()];
                pos = Math.Round(pos, 2);
                ret = SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Lift_2_Z), pos);
            }
            if (Index == 5)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_3).ToString()];
                pos = Math.Round(pos, 2);
                ret = SingletonManager.instance.Ez_Model.MoveABS((int)(ServoSlave_List.Lift_3_Z), pos);
            }
            return ret;
        }
        private bool IsMoveLiftUpDone(int Index)
        {
            double pos;
            double GetPos;
            if (Index == 3)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_1).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Lift_1_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (Index == 4)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_2).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Lift_2_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (Index == 5)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_3).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Lift_3_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            return false;
        }
        #endregion
        private bool Dio_Output(DO_MAP io, bool OnOff)
        {
            bool result = false;
            result = Dio.SetIO_OutputData((int)io, OnOff);
            
            Thread.Sleep(5);
            return result;
        }
        private void Dio_Aging_CV_Control(int Index, bool OnOff)
        {
            if (Index == 0)
            {
                Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, OnOff);
                Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, OnOff);
            }
            if (Index == 1)
            {
                Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, OnOff);
                Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, OnOff);
            }
            if (Index == 2)
            {
                Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, OnOff);
                Dio_Output(DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, OnOff);
            }
            if (Index == 3)
            {
                Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, OnOff);
                Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, OnOff);
            }
            
            if (Index == 4)
            {
                Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, OnOff);
                Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, OnOff);
            }
            
            if (Index == 5)
            {
                Dio_Output(DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, OnOff);
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
        private bool LiftLowMoveConditon(int index)
        {
            // Lift Donw 하는데 간섭이 있는지 확인
            if (index == 3)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_1_CV_DETECT_OUT_SS_2] == false)
                    return true;
            }
            if (index == 4)
            {
                if (Dio.DI_RAW_DATA[(int)DI_MAP.LIFT_2_CV_DETECT_OUT_SS_2] == false)
                    return true;
            }
            if (index == 5)
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
        private void SetUnclampInterfase(int index, bool OnOff)
        {
            if (index == 0)
                Dio_Output(DO_MAP.AGING_CV_UPPER_INTERFACE_1, OnOff);
            if (index == 1)
                Dio_Output(DO_MAP.AGING_CV_UPPER_INTERFACE_2, OnOff);
            if (index == 2)
                Dio_Output(DO_MAP.AGING_CV_UPPER_INTERFACE_3, OnOff);
            if (index == 3)
                Dio_Output(DO_MAP.AGING_CV_LOW_INTERFACE_1, OnOff);
            if (index == 4)
                Dio_Output(DO_MAP.AGING_CV_LOW_INTERFACE_2, OnOff);
            if (index == 5)
                Dio_Output(DO_MAP.AGING_CV_LOW_INTERFACE_3, OnOff);
        }
        public void StartReady()
        {
            var myIni = new IniFile(Global.instance.IniSequencePath);

            double pos=0;
            string value="";
            //value = myIni.Read("OUT_HANDLER_STEP", "SEQUENCE");
            //Out_Handle_Step = (OutHandle)Convert.ToInt16(value);
            value = myIni.Read("TOP_STEP", "SEQUENCE");
            if (string.IsNullOrEmpty(value) == true)
            {
                Out_Handle_Step = OutHandle.Idle;
                Top_Handle_Step = TopHandle.Idle;
                Bottom_Step = BottomHandle.Idle;
                SingletonManager.instance.LoadStageNo = 0;
                SingletonManager.instance.IsY_PickupColl = false;
                SingletonManager.instance.UnitLastPositionSet = false;
                SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = 0;
                return;
            }
            Top_Handle_Step = (TopHandle)Convert.ToInt16(value);
            value = myIni.Read("BOTTOM_STEP", "SEQUENCE");
            Bottom_Step = (BottomHandle)Convert.ToInt16(value);
            value = myIni.Read("Y_PICKUP_COLL_FLAG", "SEQUENCE");
            SingletonManager.instance.IsY_PickupColl =bool.Parse(value);
            value = myIni.Read("OUT_LOAD_STAGE", "SEQUENCE");
            SingletonManager.instance.LoadStageNo = Convert.ToInt32(value);
            value = myIni.Read("OUT_LOAD_FLOOR", "SEQUENCE");
            SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo] = Convert.ToInt32(value);
            
            
            switch (ReadyStep)
            {
                case Ready_Step.Idle:
                    
                    ReadyStep = Ready_Step.Out_Z_Ready_Move;
                    Dio.Set_HandlerUpDown(true);
                    break;
                case Ready_Step.Out_Z_Ready_Move:
                    MoveOutHandlerRadyZ();
                    ReadyStep = Ready_Step.Out_Z_Ready_Wait;
                    break;
                case Ready_Step.Out_Z_Ready_Wait:
                    if (IsOutHandlerReadyDoneZ() == true)
                    {
                        ReadyStep = Ready_Step.In_Handler_Ready_Move;
                    }
                    break;
                case Ready_Step.In_Handler_Ready_Move:
                    Dio_Output(DO_MAP.TRANSFER_FORWARD_SOL, true);
                    ReadyStep = Ready_Step.In_Handler_Ready_Wait;
                    break;
                case Ready_Step.In_Handler_Ready_Wait:
                    if (Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_BACKWARD_CYL_SS] == true)
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
                        if (IsOutHandlerPickupPosY() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_X_Pickup_Pos_Check;
                        else if (IsOutHandlerXPutDownPos() == true)
                            Out_Handle_Step = OutHandle.Out_Handle_X_PutDown_Pos_Check;

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
                        InHanderLR = Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_BACKWARD_CYL_SS];
                    }
                    else
                    {
                        InHanderLR = Dio.DI_RAW_DATA[(int)DI_MAP.TRANSFER_X_FORWARD_CYL_SS];
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
        }
    }
}
