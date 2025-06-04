using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YJ_AutoClamp.Utils.EzMotion_E;

namespace YJ_AutoClamp.Models
{
    public class EziDio_Model : BindableAndDisposable
    {
        public enum DI_MAP
        {
            // X000 ~ X00F
            FRONT_OP_EMERGENCY_FEEDBACK,    // X000
            OP_BOX_START,                   // X001
            OP_BOX_STOP,                    // X002
            OP_BOX_RESET,                   // X003
            DOOR_FEEDBACK,                  // X004
            REAR_OP_EMERGENCY_FEEDBACK,     // X005
            X006,X007,
            INTERFACE_FRONT_MC_SAFETY,      // X008
            INTERFACE_REAR_MC_SAFETY,       // X009
            X00A, X00B,
            FRONT_DOOR_SS,                  // X00C
            REAR_DOOR_SS,                   // X00D
            LEFT_L_DOOR_SS,                 // X00E
            LEFT_R_DOOR_SS,                 // X00F
            // X010 ~ X01F
            IN_CV_DETECT_IN_SS_1,           // X010
            IN_CV_DETECT_OUT_SS_2,          // X011
            IN_CV_DETECT_SS_3,              // X012
            IN_CV_UNALIGN_CYL_SS,           // X013
            X014,
            NG_TOP_CV_DETECT_SS_1,          // X015
            NG_TOP_CV_DETECT_SS_2,          // X016
            NG_BOTTOM_CV_DETECT_SS_1,       // X017
            NG_BOTTOM_CV_DETECT_SS_2,       // X018
            TRANSFER_X_RIGHT_CYL_SS,      // X019
            TRANSFER_X_LEFT_CYL_SS,     // X01A
            TRANSFER_LZ_UP_CYL_SS,          // X01B
            TRANSFER_LZ_DOWN_CYL_SS,        // X01C
            TRANSFER_LZ_TURN_CYL_SS,        // X01D 90도
            TRANSFER_LZ_RETURN_CYL_SS,      // X01E 0도
            TRANSFER_LZ_VACUUM_SS,          // X01F
            // X020~X02F
            TRANSFER_RZ_GRIP_CYL_SS,        // X020
            TRANSFER_RZ_UNGRIP_CYL_SS,      // X021
            CLAMPING_CV_DETECT_SS_1,        // X022
            CLAMPING_CV_DETECT_SS_2,        // X023
            CLAMPING_CV_DETECT_SS_3,        // X024
            CLAMPING_CV_DETECT_SS_4,        // X025
            CLAMPING_CV_DETECT_SS_5,        // X026
            CLAMPING_CV_DETECT_SS_6,        // X027
            CLAMPING_CV_CENTERING_CYL_SS_1_FWD, // X028
            CLAMPING_CV_CENTERING_CYL_SS_1_BWD, // X029
            CLMAPING_CV_UP_CYL_SS,          // X02A
            CLAMPING_CV_DOWN_CYL_SS,        // X02B
            CLAMPING_CV_STOPER_UP_CYL_SS,   // X02C
            TOP_JIG_CV_DETECT_SS,           // X02D
            TOP_JIG_TR_Z_UP_CYL_SS_1,       // X02E
            TOP_JIG_RT_Z_DOWN_CYL_SS_1,     // X02F
            // X030~X03F
            TOP_JIG_TR_Z_UP_CYL_SS_2,       // X030
            TOP_JIG_RT_Z_DOWN_CYL_SS_2,     // X031
            TOP_JIG_TR_Z_GRIP_CYL_SS,       // X032
            TOP_JIG_RT_Z_UNGRIP_CYL_SS,     // X033
            TRANSFER_RZ_UP_CYL_SS,          // X034
            TRANSFER_RZ_DOWN_CYL_SS,        // X035
            CLAMP_LD_Z_GRIP_CYL_SS,         // X036
            CLAMP_LD_Z_UNGRIP_CYL_SS,       // X037
            LIFT_1_CV_DETECT_IN_SS_1,       // X038
            LIFT_1_CV_DETECT_OUT_SS_2,      // X039
            LIFT_2_CV_DETECT_IN_SS_1,       // X03A
            LIFT_2_CV_DETECT_OUT_SS_2,      // X03B
            LIFT_3_CV_DETECT_IN_SS_1,       // X03C
            LIFT_3_CV_DETECT_OUT_SS_2,      // X03D
            CLAMPING_CV_CENTERING_CYL_SS_2_FWD, // X03E
            CLAMPING_CV_CENTERING_CYL_SS_2_BWD, // X03F
            // X040~X04F
            AGING_CV_1_1_UPPER_DETECT_SS_1, // X040
            AGING_CV_1_1_UPPER_DETECT_SS_2, // X041
            AGING_CV_1_1_UPPER_DETECT_SS_3, // X042
            AGING_CV_1_1_LOW_DETECT_SS_1,   // X043
            AGING_CV_1_1_LOW_DETECT_SS_2,   // X044
            AGING_CV_1_1_LOW_DETECT_SS_3,   // X045
            AGING_CV_2_1_UPPER_DETECT_SS_1, // X046
            AGING_CV_2_1_UPPER_DETECT_SS_2, // X047
            AGING_CV_2_1_UPPER_DETECT_SS_3, // X048
            AGING_CV_2_1_LOW_DETECT_SS_1,   // X049
            AGING_CV_2_1_LOW_DETECT_SS_2,   // X04A
            AGING_CV_2_1_LOW_DETECT_SS_3,   // X04B
            AGING_CV_3_1_UPPER_DETECT_SS_1, // X04C
            AGING_CV_3_1_UPPER_DETECT_SS_2, // X04D
            AGING_CV_3_1_UPPER_DETECT_SS_3, // X04E
            AGING_CV_3_1_LOW_DETECT_SS_1,   // X04F
            // X050~ X05F
            AGING_CV_3_1_LOW_DETECT_SS_2,   // X050
            AGING_CV_3_1_LOW_DETECT_SS_3,   // X051
            RETURN_TOP_CV_DETECT_SS_1,      // X052
            RETURN_TOP_CV_DETECT_SS_2,      // X053
            RETURN_BOTTOM_CV_DETECT_SS_1,   // X054
            RETURN_BOTTOM_CV_DETECT_SS_2,   // X055
            X056, X057,
            AGING_CV_UPPER_INTERFACE_1,     // X058
            AGING_CV_LOW_INTERFACE_1,       // X059
            AGING_CV_UPPER_INTERFACE_2,     // X05A
            AGING_CV_LOW_INTERFACE_2,       // X05B
            AGING_CV_UPPER_INTERFACE_3,     // X05C
            AGING_CV_LOW_INTERFACE_3,       // X05D
            BOTTOM_RETURN_CV_INTERFACE,     // X05E
            TOP_RETURN_CV_INTERFACE,        // X05F
            // Y060 ~ Y06F
            AGING_CV_1_2_UPPER_DETECT_SS_1, // Y060
            AGING_CV_1_2_UPPER_DETECT_SS_2, // Y061
            AGING_CV_1_2_LOW_DETECT_SS_1,   // Y062
            AGING_CV_1_2_LOW_DETECT_SS_2,   // Y063
            AGING_CV_2_2_UPPER_DETECT_SS_1, // Y064
            AGING_CV_2_2_UPPER_DETECT_SS_2, // Y065
            AGING_CV_2_2_LOW_DETECT_SS_1,   // Y066
            AGING_CV_2_2_LOW_DETECT_SS_2,   // Y067
            AGING_CV_3_2_UPPER_DETECT_SS_1, // Y068
            AGING_CV_3_2_UPPER_DETECT_SS_2, // Y069
            AGING_CV_3_2_LOW_DETECT_SS_1,   // Y06A
            AGING_CV_3_2_LOW_DETECT_SS_2,   // Y06B
            RETURN_TOP_CV_DETECT_2_1,       // Y06C
            RETURN_TOP_CV_DETECT_2_2,       // Y06D
            RETURN_BOTTOM_CV_DETECT_2_1,    // Y06E
            RETURN_BOTTOM_CV_DETECT_2_2,    // Y06F

            //X070, X07F,
            AGING_CV_1_2_UPPER_DETECT_SS_3, // Y070
            AGING_CV_2_2_UPPER_DETECT_SS_3, // Y071
            AGING_CV_3_2_UPPER_DETECT_SS_3, // Y072
            AGING_CV_1_2_LOW_DETECT_SS_3,   // Y073
            AGING_CV_2_2_LOW_DETECT_SS_3,   // Y074
            AGING_CV_3_2_LOW_DETECT_SS_3,   // Y075
            X076, X077,X078, X079,X07A, 
            X07B, X07C, X07D, X07E, X07F,   

            DI_MAX
        }
        public enum DO_MAP
        {
            // Y000 ~ Y00F
            Y000,                   // Y000
            OP_BOX_START,           // Y001
            OP_BOX_STOP,            // Y002
            OP_BOX_RESET,           // Y003
            TOWER_LAMP_GREEN,       // Y004
            TOWER_LAMP_RED,         // Y005
            TOWER_LAMP_YELLOW,      // Y006
            BUZZER,                 // Y007
            INTERFACE_FRONT_MC_SEND,// Y008
            INTERFACE_REAR_MC_SEND, // Y009
            Y00A, Y00B, Y00C, Y00D, Y00E, Y00F,
            // Y010 ~ Y01F
            INPUT_SET_CV_RUN,       // Y010
            CLAMPING_CV_RUN,        // Y011
            NG_BOTTOM_JIG_CV_RUN,   // Y012
            NG_TOP_JIG_CV_RUN,      // Y013
            LIFT_CV_RUN_1,          // Y014
            LIFT_CV_RUN_2,          // Y015
            LIFT_CV_RUN_3,          // Y016
            BTM_RETURN_CV_RUN,      // Y017
            TOP_RETURN_CV_RUN,      // Y018
            Y019,
            CLAMPING_LD_Z_GRIP_SOL, // Y01A
            TRANSFER_LZ_DOWN_SOL,   // Y01b // 1b,1c는 배선이 꼬인건지 확인이 필요하다
            IN_SET_CV_CENTERING,    // Y01c
            TRANSFER_FORWARD_SOL,   // Y01D
            TRANSFER_RZ_DOWN_SOL,   // Y01E
            TRANSFER_LZ_TURN_SOL,   // Y01F
            // Y020 ~ Y02F
            TRANSFER_RZ_GRIP_SOL,           // Y020
            TRANSFER_LZ_VACUUM_SOL,         // Y021
            CLAMPING_CV_CENTERING_SOL_1,    // Y022
            CLAMPING_CV_CENTERING_SOL_2,    // Y023
            CLAMPING_CV_UP_SOL,             // Y024
            CLAMPING_CV_STOPER_UP_SOL,      // Y025
            TOP_JIG_TR_Z_DOWN_SOL_1,        // Y026
            TOP_JIG_TR_Z_DOWN_SOL_2,        // Y027
            TOP_JIG_TR_Z_GRIP_SOL,          // Y028
            TRANSFER_LZ_BOLW_SOL,           // Y029
            Y02A, Y02B, Y02C, Y02D,
            BTM_RETURN_CV_RUN_2,            // Y02E
            TOP_RETURN_CV_RUN_2,            // Y02F
            // Y030 ~ Y03F
            AGING_INVERT_CV_UPPER_RUN_1_1,  // Y030
            AGING_INVERT_CV_UPPER_RUN_2_1,  // Y031
            AGING_INVERT_CV_UPPER_RUN_3_1,  // Y032
            AGING_INVERT_CV_LOW_RUN_1_1,    // Y033
            AGING_INVERT_CV_LOW_RUN_2_1,    // Y034
            AGING_INVERT_CV_LOW_RUN_3_1,    // Y035
            AGING_INVERT_CV_UPPER_RUN_1_2,  // Y036
            AGING_INVERT_CV_UPPER_RUN_2_2,  // Y037
            AGING_INVERT_CV_UPPER_RUN_3_2,  // Y038
            AGING_INVERT_CV_LOW_RUN_1_2,    // Y039
            AGING_INVERT_CV_LOW_RUN_2_2,    // Y03A
            AGING_INVERT_CV_LOW_RUN_3_2,    // Y03B
            Y03C, Y03D, Y03E, Y03F,
            // Y040 ~ Y04F
            AGING_CV_UPPER_INTERFACE_1, // Y040
            AGING_CV_LOW_INTERFACE_1,   // Y041
            AGING_CV_UPPER_INTERFACE_2, // Y042
            AGING_CV_LOW_INTERFACE_2,   // Y043
            AGING_CV_UPPER_INTERFACE_3, // Y044
            AGING_CV_LOW_INTERFACE_3,   // Y045
            BOTTOM_RETURN_CV_INTERFACE, // Y046
            TOP_RETURN_CV_INTERFACE,    // Y047
            Y048, Y049, Y04A, Y04B, Y04C, Y04D, Y04E, Y04F,
            
            DO_MAX
        }
        public enum DisplayExist_List
        {
            IN_CV_EXIST1,
            NG_EXIST1,
            NG_EXIST2,
            IN_SET_EXIST,
            IN_BOTTOM_EXIST,
            IN_TOP_EXIST,
            OUT_HAND_EXIST,
            InterFace_1,
            InterFace_2,
            InterFace_3,
            InterFace_4,
            InterFace_5,
            InterFace_6,
            InterFace_7,
            InterFace_8,
            Max
        }
        public List<int> DisplayDio_List = new List<int>
        {
            (int)DI_MAP.IN_CV_DETECT_IN_SS_1,       //0
            (int)DI_MAP.IN_CV_DETECT_OUT_SS_2,      //1
            (int)DI_MAP.NG_TOP_CV_DETECT_SS_2,      //2
            (int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2,   //3
            (int)DI_MAP.TRANSFER_LZ_VACUUM_SS,      //4
            (int)DI_MAP.TRANSFER_LZ_UP_CYL_SS,      //5
            (int)DI_MAP.TRANSFER_LZ_DOWN_CYL_SS,    //6
            (int)DI_MAP.TRANSFER_RZ_UP_CYL_SS,      //7
            (int)DI_MAP.TRANSFER_RZ_DOWN_CYL_SS,    //8
            (int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1,    //9
            (int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_1,  //10
            (int)DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2,    //11
            (int)DI_MAP.TOP_JIG_RT_Z_DOWN_CYL_SS_2,  //12
            (int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS,    //13
            (int)DI_MAP.TRANSFER_RZ_UNGRIP_CYL_SS,  //14
            (int)DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS,   //15
            (int)DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS, //16
            (int)DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS,     //17
            (int)DI_MAP.CLAMP_LD_Z_UNGRIP_CYL_SS,   //18
            (int)DI_MAP.RETURN_BOTTOM_CV_DETECT_SS_2,//19
            (int)DI_MAP.TOP_JIG_CV_DETECT_SS,  //20
            (int)DI_MAP.LIFT_1_CV_DETECT_IN_SS_1,   //21
            (int)DI_MAP.LIFT_2_CV_DETECT_IN_SS_1,   //22
            (int)DI_MAP.LIFT_3_CV_DETECT_IN_SS_1    //23
        };
        public enum Dio_Slave
        {
            DIO_1 = 9
        }
        private Thread DioThread;
        private bool IsDioThreadRunning { get; set; } = false;
        private volatile bool _shouldStop = false;
        private readonly Dictionary<DisplayExist_List, Func<bool>> DisplayExistMapping;
        public IPAddress IpAddress { get; set; }
        public bool[] IsConnected { get; set; } = 
            { false ,false , false, false, false, false, false, false};
        public int DioSlave { get; set; } = 1;
        public int Dio_InputCount { get; set; } = (int)DI_MAP.DI_MAX - 1;
        public int Dio_OutputCount { get; set; } = (int)DO_MAP.DO_MAX - 1;
        // Input Label
        public List<string> Input_Label { get; set; } = new List<string>();
        // Output Label
        public List<string> Output_Label { get; set; } = new List<string>();
        // Input Address
        public List<string> Input_Address { get; set; } = new List<string>();
        // Output Address
        public List<string> Output_Address { get; set; } = new List<string>();

        private ObservableCollection<bool> _DI_RAW_DATA = new ObservableCollection<bool>();
        public ObservableCollection<bool> DI_RAW_DATA
        {
            get
            {
                return _DI_RAW_DATA;
            }
        }
        private ObservableCollection<bool> _DO_RAW_DATA = new ObservableCollection<bool>();
        public ObservableCollection<bool> DO_RAW_DATA
        {
            get
            {
                return _DO_RAW_DATA;
            }
        }
        private ObservableCollection<bool> _DO_OPER_DATA = new ObservableCollection<bool>();
        public ObservableCollection<bool> DO_OPER_DATA
        {
            get { return _DO_OPER_DATA; }
        }
        
        public EziDio_Model()
        {
            // Data Init
            string label = string.Empty;
            for (int i = 0; i < (int)DI_MAP.DI_MAX; i++)
            {
                Input_Address.Add(string.Format("X{0:X2}", i));
                DI_RAW_DATA.Add(false);
                // set input label name
                label = ((DI_MAP)i).ToString().Replace('_', ' ');
                Input_Label.Add(label);
            }
            for (int i = 0; i < (int)DO_MAP.DO_MAX; i++)
            {
                Output_Address.Add(string.Format("Y{0:X2}", i));
                DO_RAW_DATA.Add(false);
                DO_OPER_DATA.Add(false);
                // set output label name
                label = ((DO_MAP)i).ToString().Replace('_', ' ');
                Output_Label.Add(label);
            }
            // Auto UI Display Dio
            DisplayExistMapping = new Dictionary<DisplayExist_List, Func<bool>>
            {
                { DisplayExist_List.IN_CV_EXIST1, () => DI_RAW_DATA[(int)DI_MAP.IN_CV_DETECT_OUT_SS_2] }, // 24
                { DisplayExist_List.NG_EXIST1, () => DI_RAW_DATA[(int)DI_MAP.NG_TOP_CV_DETECT_SS_2] },    // 25
                { DisplayExist_List.NG_EXIST2, () => DI_RAW_DATA[(int)DI_MAP.NG_BOTTOM_CV_DETECT_SS_2] }, // 26
                { DisplayExist_List.IN_SET_EXIST, () => DI_RAW_DATA[(int)DI_MAP.TRANSFER_LZ_VACUUM_SS] }, // 27
                {//28
                    DisplayExist_List.IN_BOTTOM_EXIST,
                    () =>
                        SingletonManager.instance.EquipmentMode == EquipmentMode.Dry
                            ? DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS]
                            :!DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_GRIP_CYL_SS] && !DI_RAW_DATA[(int)DI_MAP.TRANSFER_RZ_UNGRIP_CYL_SS]
                },
                {//29
                    DisplayExist_List.IN_TOP_EXIST,
                    () =>
                        SingletonManager.instance.EquipmentMode == EquipmentMode.Dry
                            ? DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS]
                            :!DI_RAW_DATA[(int)DI_MAP.TOP_JIG_TR_Z_GRIP_CYL_SS] && !DI_RAW_DATA[(int)DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS]
                },
                {//30
                    DisplayExist_List.OUT_HAND_EXIST,
                    () =>
                        SingletonManager.instance.EquipmentMode == EquipmentMode.Dry
                            ? DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS]
                            :!DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_GRIP_CYL_SS] && !DI_RAW_DATA[(int)DI_MAP.CLAMP_LD_Z_UNGRIP_CYL_SS]
                },
                { DisplayExist_List.InterFace_1, () => DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_1] },   // 31
                { DisplayExist_List.InterFace_2, () => DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_1] },     // 32
                { DisplayExist_List.InterFace_3, () => DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_2] },   // 33
                { DisplayExist_List.InterFace_4, () => DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_2] },     // 34
                { DisplayExist_List.InterFace_5, () => DI_RAW_DATA[(int)DI_MAP.AGING_CV_UPPER_INTERFACE_3] },   // 35
                { DisplayExist_List.InterFace_6, () => DI_RAW_DATA[(int)DI_MAP.AGING_CV_LOW_INTERFACE_3] },     // 36
                { DisplayExist_List.InterFace_7, () => DI_RAW_DATA[(int)DI_MAP.BOTTOM_RETURN_CV_INTERFACE] },   // 37
                { DisplayExist_List.InterFace_8, () => DI_RAW_DATA[(int)DI_MAP.TOP_RETURN_CV_INTERFACE] }       // 38
            };
        }
        public bool Connect(int iSlaveNo)
        {
            try
            {
                int slave = (int)Dio_Slave.DIO_1 + iSlaveNo;
                // Already Connected
                if (IsConnected[iSlaveNo] == true)
                {
                    return true;
                }
                if (iSlaveNo == 0) 
                    IpAddress = IPAddress.Parse("192.168.0.9");
                if (iSlaveNo == 1) 
                    IpAddress = IPAddress.Parse("192.168.0.10");
                if (iSlaveNo == 2)
                    IpAddress = IPAddress.Parse("192.168.0.11");
                if (iSlaveNo == 3)
                    IpAddress = IPAddress.Parse("192.168.0.12");
                if (iSlaveNo == 4)
                    IpAddress = IPAddress.Parse("192.168.0.13");
                if (iSlaveNo == 5)
                    IpAddress = IPAddress.Parse("192.168.0.14");
                if (iSlaveNo == 6)
                    IpAddress = IPAddress.Parse("192.168.0.15");
                if (iSlaveNo == 7)
                    IpAddress = IPAddress.Parse("192.168.0.16");

                // Is not 0 == Connect Success
                if (EziMOTIONPlusELib.FAS_ConnectTCP(IpAddress, slave) == true)
                {
                    IsConnected[iSlaveNo] = true;
                    Global.Mlog.Info($"EziMotion DIO Board Connect Success. IP Address : {IpAddress.ToString()}, Slave : {slave}");

                    // Thread Start
                    DioThread = new Thread(ThreadReceive);
                    DioThread.Start();
                    IsDioThreadRunning = true;
                    return true;
                }
                // Is 0 == Connect Fail
                else
                {
                    IsConnected[iSlaveNo] = false;
                    Global.Mlog.Info($"EziMotion DIO Board Connect Fail. IP Address : {IpAddress.ToString()}, Slave : {slave}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                return false;
            }
        }
        public void Close(int iSlave)
        {
            try
            {
                int slave = (int)Dio_Slave.DIO_1 + iSlave;
                if (IsConnected[iSlave])
                {
                    EziMOTIONPlusELib.FAS_Close(slave);
                    IsConnected[iSlave] = false;
                    Global.Mlog.Info($"EziMotion DIO Board Disconnect Success. IP Address : {IpAddress}, Slave : {slave}");
                }

                if (DioThread != null)
                {
                    _shouldStop = true;  // 스레드 종료 신호 설정
                    if (!DioThread.Join(5000))  // 스레드가 종료될 때까지 최대 5초 대기
                    {
                        Global.Mlog.Info("DioThread did not terminate in a timely fashion.");
                    }
                    DioThread = null;
                    IsDioThreadRunning = false;
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
            }
        }

        public void GetIO_InputData(int slave)
        {
            uint dwInput = 0;
            uint dwLatch = 0;
            int dioSlave = (int)Dio_Slave.DIO_1+ slave;
            int nRtn = EziMOTIONPlusELib.FAS_GetInput(dioSlave, ref dwInput, ref dwLatch);

            if (nRtn == EziMOTIONPlusELib.FMM_OK)
            {
                UpdateRawData(slave,DI_RAW_DATA, dwInput,false);
                UpdateUiDio();
            }
        }
        public uint GetIO_OutputData(int slave)
        {
            uint uOutput = 0;
            uint uStatus = 0;
            int dioSlave = (int)Dio_Slave.DIO_1 + slave;
            int nRtn = EziMOTIONPlusELib.FAS_GetOutput(dioSlave, ref uOutput, ref uStatus);

            if (nRtn == EziMOTIONPlusELib.FMM_OK)
            {
                UpdateRawData(slave, DO_RAW_DATA, uOutput, true);
                return uOutput;
            }
            return 0;
        }
        
        public void UpdateIO_OperData()
        {
            // DO_RAW_DATA와 DO_OPER_DATA를 비교하여 변경된 항목만 업데이트
            for (int i = 0; i < DO_RAW_DATA.Count; i++)
            {
                if (DO_OPER_DATA[i] != DO_RAW_DATA[i])
                {
                    DO_OPER_DATA[i] = DO_RAW_DATA[i];
                }
            }
        }
        public bool SetIO_OutputData(int index, bool OnOff)
        {
            UpdateIO_OperData();
            // IO Bit를 통해 Slave NO를 계산한다.
            int slave = (int)Dio_Slave.DIO_1 + (index / 16);
            // 해당 Slave 의 제어해야할 bit No를 계산한다.
            int uOotBit = (int)index % 16;

            uint OnBit=0, OffBit=0;
            // 제어해야할 Bit를 기존 출력 중 IO정보와 합쳐서 Send buf에 담는다
            if (OnOff == true)
                OnBit = (uint)(1u << uOotBit);
            else if (OnOff == false)
                OffBit = (uint)(1u << uOotBit);

            uint onShift = OnBit << 16;
            uint offShift = OffBit << 16;
            int nRtn = EziMOTIONPlusELib.FAS_SetOutput(slave, onShift, offShift);

            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(SetIO_OutputData) was failed.");
                return false;
            }
            
            return true;
        }
        public void Set_HandlerUpDown(bool OnOff)
        {
            
            if (OnOff == true)
            {
                SetIO_OutputData((int)DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                SetIO_OutputData((int)DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                SetIO_OutputData((int)DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                SetIO_OutputData((int)DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
            }
            else
            {
                SetIO_OutputData((int)DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, true);
                SetIO_OutputData((int)DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, true);
                SetIO_OutputData((int)DO_MAP.TRANSFER_RZ_DOWN_SOL, true);
                SetIO_OutputData((int)DO_MAP.TRANSFER_LZ_DOWN_SOL, true);
            }
            Thread.Sleep(1000);
            SetIO_OutputData((int)DO_MAP.TRANSFER_FORWARD_SOL, true);

        }
        public void ThreadReceive()
        {
            UpdateIO_OperData();
            while (!_shouldStop)
            {
                try
                {
                    for (int i = 0; i < ((int)DI_MAP.DI_MAX / 16); i++)
                    {
                        // Get Input Data
                        GetIO_InputData(i);
                        Thread.Sleep(2);

                    }
                    for (int j = 0; j < (int)DO_MAP.DO_MAX / 16; j++)
                    {
                        // Get Output Data
                        GetIO_OutputData(j);
                        Thread.Sleep(2);
                    }
                    

                }
                catch (Exception e)
                {
                    //IsConnected[iSlave] = false;
                    string error = e.ToString();
                    Global.ExceptionLog.ErrorFormat($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {error}");
                }

                Thread.Sleep(5);
            }
            // 스레드 종료 후 상태 업데이트
            IsDioThreadRunning = false;
            Global.Mlog.Info("DioThread has stopped.");
        }
        public void DioThreadStop()
        {
            // 스레드 종료 플래그 설정
            _shouldStop = true;

            // DioThread가 실행 중인지 확인
            if (DioThread != null && DioThread.IsAlive)
            {
                try
                {
                    // 스레드가 종료될 때까지 대기
                    DioThread.Join(500); // 최대 500ms 대기
                }
                catch (ThreadStateException ex)
                {
                    Global.ExceptionLog.ErrorFormat($"DioThreadStop - ThreadStateException: {ex}");
                }
                catch (ThreadInterruptedException ex)
                {
                    Global.ExceptionLog.ErrorFormat($"DioThreadStop - ThreadInterruptedException: {ex}");
                }
                finally
                {
                    // DioThread 객체를 null로 설정하여 참조 해제
                    DioThread = null;
                    IsDioThreadRunning = false;
                }
            }
            else
            {
                IsDioThreadRunning = false;
            }
        }
        private void UpdateRawData(int slave ,ObservableCollection<bool> rawData, uint data , bool InOutFlag)
        {
            int count = rawData.Count;
            int StartIndex = slave * 16;
            for (int i = 0; i < 16; i++)
            {
                bool newValue;
                if (InOutFlag == true)
                    newValue = ((data >> (i+16)) & 1u) == 1u;
                else
                    newValue = ((data >> i) & 1u) == 1u;
                if (rawData[StartIndex + i] != newValue)
                {
                    rawData[StartIndex + i] = newValue;
                }
            }
        }
        private void UpdateUiDio()
        {
            // LINQ를 사용하여 필요한 값만 추출
            for (int i = 0; i < DisplayDio_List.Count; i++)
            {
                if (SingletonManager.instance.DisplayUI_Dio[i] != DI_RAW_DATA[DisplayDio_List[i]])
                    SingletonManager.instance.DisplayUI_Dio[i] = DI_RAW_DATA[DisplayDio_List[i]];
            }

            // DisplayExistMapping을 사용하여 UI 업데이트
            foreach (var mapping in DisplayExistMapping)
            {
                int targetIndex = DisplayDio_List.Count + (int)mapping.Key;
                bool newValue = mapping.Value();
                if (SingletonManager.instance.DisplayUI_Dio[targetIndex] != newValue)
                {
                    SingletonManager.instance.DisplayUI_Dio[targetIndex] = newValue;
                }
            }
        }
    }
}
