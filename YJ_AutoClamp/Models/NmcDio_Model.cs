using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using YJ_AutoClamp.Utils;

namespace YJ_AutoClamp.Models
{

    public class NmcDio_Model
    {
        public enum DI_MAP
        {
            // X00 ~ X0F
            FRONT_REAR_OP_START_SW, 
            FRONT_REAR_OP_STOP_SW, 
            FRONT_REAR_OP_RESET_SW,
            X03, X04, X05, X06, X07, X08,
            DOOR_OPEN_SAFETY_CHECK_FEEDBACK, 
            FRONT_DOOR_OPEN_DETECT, 
            REAR_LEFT_DOOR_OPEN_DETECT, 
            REAR_RIGHT_DOOR_OPEN_DETECT,
            X0D, X0E, 
            FRONT_OP_EMERGENCY_SW,
            // X10 ~ X1F
            REAR_OP_EMERGENCY_SW, 
            X11, X12, X13,
            JIG_DETECT_SENSOR_1, 
            JIG_DETECT_SENSOR_2, 
            JIG_DETECT_SENSOR_3, 
            JIG_DETECT_SENSOR_4,
            X18, X19, X1A, X1B, X1C, X1D, X1E, X1F,
            // X20 ~ X2F
            X20, X21,
            IN_CV_1_DETECT_SENSOR_1, 
            IN_CV_1_DETECT_SENSOR_2,
            X24, X25,
            IN_CV_2_DETECT_SENSOR_1, 
            IN_CV_2_DETECT_SENSOR_2, 
            IN_CV_2_UP_SENSOR, 
            IN_CV_2_DOWN_SENSOR, 
            OUT_CV_DETECT_SENSOR_1, 
            OUT_CV_DETECT_SENSOR_2,
            X2C, X2D,
            NG_SHIFT_DETECT_SENSOR_1, 
            NG_SHIFT_DETECT_SENSOR_2,
            // X30 ~ X3F
            NG_SHIFT_DETECT_SENSOR_3, 
            NG_SHIFT_UP_1_SENSOR, 
            NG_SHIFT_UP_2_SENSOR, 
            NG_SHIFT_FWD_SENSOR, 
            NG_SHIFT_BWD_SENSOR,
            X35,
            IN_HANDLER_UP_SENSOR, 
            IN_HANDLER_DOWN_SENSOR, 
            IN_HANDLER_GRIP_SENSOR, 
            IN_HANDLER_UNGRIP_SENSOR,
            X3A,X3B,
            OUT_HANDLER_UP_SENSOR, 
            OUT_HANDLER_DOWN_SENSOR, 
            OUT_HANDLER_GRIP_SENSOR, 
            OUT_HANDLER_UNGRIP_SENSOR,
            // X40 ~ X4F
            X40, X41, X42, X43, X44, X45, X46, X47, X48, X49, X4A, X4B, X4C,
            FRONT_IF_3, 
            REAR_IF_1, 
            REAR_IF_2,
            DI_MAX
        }
        public enum DO_MAP
        {
            // Y00 ~ Y0F
            FRONT_REAR_OP_START_SW_LAMP, 
            FRONT_REAR_OP_STOP_SW_LAMP, 
            FRONT_REAR_OP_RESET_SW_LAMP, 
            Y03, Y04, Y05, Y06, Y07, Y08,
            TOWER_LAMP_GREEN, 
            TOWER_LAMP_YELLOW, 
            TOWER_LAMP_RED, 
            MACHINE_BUZZER,
            Y0D, Y0E,
            GOCATOR_FRONT_TRIGGER_ON,
            // Y10 ~ Y1F
            GOCATOR_REAR_TRIGGER_ON, 
            GOCATOR_ENCODER_FRONT_JIG_1_SELECT, 
            GOCATOR_ENCODER_FRONT_JIG_2_SELECT, 
            GOCATOR_ENCODER_REAR_JIG_1_SELECT, 
            GOCATOR_ENCODER_REAR_JIG_2_SELECT, 
            Y15, Y16,
            GOCATOR_FRONT_LASER_SAFETY_POWER_ON_OFF,
            GOCATOR_REAR_LASER_SAFETY_POWER_ON_OFF,
            Y19, Y1A, Y1B, Y1C, Y1D, Y1E, Y1F,
            // Y20 ~ Y2F
            SAFETY_PLC_RESET,
            Y21,
            IN_CV_1_RUN_STOP, 
            IN_CV_2_RUN_STOP, 
            OUT_CV_RUN_STOP, 
            IN_CV_2_UP_DOWN_S_SOL,
            Y26,
            NG_SHIFT_UP_DOWN_1_S_SOL, 
            NG_SHIFT_UP_DOWN_2_S_SOL, 
            NG_SHIFT_FWD_D_SOL, 
            NG_SHIFT_BWD_D_SOL,
            Y2B, Y2C,
            IN_HANDLER_UP_S_SOL,
            IN_HANDLER_DOWN_S_SOL,
            IN_HANDLER_GRIP_D_SOL,
            // Y30 ~ Y3F
            IN_HANDLER_UNGRIP_D_SOL,
            Y31, Y32,
            OUT_HANDLER_UP_S_SOL,
            OUT_HANDLER_DOWN_S_SOL,
            OUT_HANDLER_GRIP_D_SOL, 
            OUT_HANDLER_UNGRIP_D_SOL,
            Y37, Y38, Y39, Y3A, Y3B, Y3C, Y3D, Y3E, Y3F,
            DO_MAX
        }
        public enum DisplayExist_List
        {
            IN_CV_EXIST1,
            IN_CV_EXIST2,
            OUT_CV_EXIST1,
            NG_EXIST1,
            NG_EXIST2,
            NG_EXIST3,
            IN_HAND_EXIST,
            OUT_HAND_EXIST,
            Max
        }

        public List<int> DisplayDio_List = new List<int>
        {
            (int)DI_MAP.IN_CV_1_DETECT_SENSOR_1,
            (int)DI_MAP.IN_CV_1_DETECT_SENSOR_2,
            (int)DI_MAP.IN_CV_2_DETECT_SENSOR_1,
            (int)DI_MAP.IN_CV_2_DETECT_SENSOR_2,
            (int)DI_MAP.OUT_CV_DETECT_SENSOR_1,
            (int)DI_MAP.OUT_CV_DETECT_SENSOR_2,
            (int)DI_MAP.NG_SHIFT_DETECT_SENSOR_1,
            (int)DI_MAP.NG_SHIFT_DETECT_SENSOR_2,
            (int)DI_MAP.NG_SHIFT_DETECT_SENSOR_3,
            (int)DI_MAP.IN_HANDLER_UP_SENSOR,
            (int)DI_MAP.IN_HANDLER_DOWN_SENSOR,
            (int)DI_MAP.IN_HANDLER_GRIP_SENSOR,
            (int)DI_MAP.IN_HANDLER_UNGRIP_SENSOR,
            (int)DI_MAP.OUT_HANDLER_UP_SENSOR,
            (int)DI_MAP.OUT_HANDLER_DOWN_SENSOR,
            (int)DI_MAP.OUT_HANDLER_GRIP_SENSOR,
            (int)DI_MAP.OUT_HANDLER_UNGRIP_SENSOR
        };

        private Thread DioThread;
        private bool IsDioThreadRunning { get; set; } = false;
        private volatile bool _ShouldStop = false;
        private const ushort _EcatAddress  = 9;
        private readonly Dictionary<DisplayExist_List, Func<bool>> DisplayExistMapping;
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
            get { return _DI_RAW_DATA; }
        }
        private ObservableCollection<bool> _DO_RAW_DATA = new ObservableCollection<bool>();
        public ObservableCollection<bool> DO_RAW_DATA
        {
            get { return _DO_RAW_DATA; }
        }
        private ObservableCollection<bool> _DO_OPER_DATA = new ObservableCollection<bool>();
        public ObservableCollection<bool> DO_OPER_DATA
        {
            get { return _DO_OPER_DATA; }
        }

        private byte[] m_uInBuffer = new byte[(int)DI_MAP.DI_MAX / 8];
        private byte[] m_uOutBuffer = new byte[(int)DO_MAP.DO_MAX / 8];
        public NmcDio_Model()
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
                { DisplayExist_List.IN_CV_EXIST1, () => DI_RAW_DATA[(int)DI_MAP.IN_CV_1_DETECT_SENSOR_1] && DI_RAW_DATA[(int)DI_MAP.IN_CV_1_DETECT_SENSOR_2] },
                { DisplayExist_List.IN_CV_EXIST2, () => DI_RAW_DATA[(int)DI_MAP.IN_CV_2_DETECT_SENSOR_1] && DI_RAW_DATA[(int)DI_MAP.IN_CV_2_DETECT_SENSOR_2] },
                { DisplayExist_List.OUT_CV_EXIST1, () => DI_RAW_DATA[(int)DI_MAP.OUT_CV_DETECT_SENSOR_1] && DI_RAW_DATA[(int)DI_MAP.OUT_CV_DETECT_SENSOR_2] },
                { DisplayExist_List.NG_EXIST1, () => DI_RAW_DATA[(int)DI_MAP.NG_SHIFT_DETECT_SENSOR_1] },
                { DisplayExist_List.NG_EXIST2, () => DI_RAW_DATA[(int)DI_MAP.NG_SHIFT_DETECT_SENSOR_2] },
                { DisplayExist_List.NG_EXIST3, () => DI_RAW_DATA[(int)DI_MAP.NG_SHIFT_DETECT_SENSOR_3] },
                { 
                    DisplayExist_List.IN_HAND_EXIST, 
                    () =>
                        SingletonManager.instance.EquipmentMode == EquipmentMode.Dry
                            ? DI_RAW_DATA[(int)DI_MAP.IN_HANDLER_GRIP_SENSOR]
                            :!DI_RAW_DATA[(int)DI_MAP.IN_HANDLER_GRIP_SENSOR] && !DI_RAW_DATA[(int)DI_MAP.IN_HANDLER_UNGRIP_SENSOR]
                },
                { 
                    DisplayExist_List.OUT_HAND_EXIST, 
                    () =>
                        SingletonManager.instance.EquipmentMode == EquipmentMode.Dry
                            ? DI_RAW_DATA[(int)DI_MAP.OUT_HANDLER_GRIP_SENSOR]
                            :!DI_RAW_DATA[(int)DI_MAP.OUT_HANDLER_GRIP_SENSOR] && !DI_RAW_DATA[(int)DI_MAP.OUT_HANDLER_UNGRIP_SENSOR]
                }
            };
        }
        public void Get_InputData()
        {
            Array.Clear(m_uInBuffer, 0, m_uInBuffer.Length);
            NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_IO_READ(0, _EcatAddress, 1, 4, 10, m_uInBuffer);
            if (mc == NMCSDKLib.MC_STATUS.MC_OK)
            {
                // Raw Data Update
                UpdateRawData(DI_RAW_DATA, m_uInBuffer);
                // Auto View Display Dio
                UpdateUiDio();
            }
        }
        public void Get_OutputData()
        {
            Array.Clear(m_uOutBuffer, 0, m_uOutBuffer.Length);
            NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_IO_READ(0, _EcatAddress, 0, 0, 8, m_uOutBuffer);
            if (mc == NMCSDKLib.MC_STATUS.MC_OK)
            {
                UpdateRawData(DO_RAW_DATA, m_uOutBuffer);
            }
        }
        public void UpdateOperData()
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
        public bool Set_OperData()
        {
            // 연결되지 않으면 Return
            if (SingletonManager.instance.NMC_Model.IsConnected == false)
                return false;

            // 복제된 데이터를 바탕으로 출력 버퍼 생성
            byte[] outBuffer = new byte[m_uOutBuffer.Length];
            for (int i = 0; i < DO_OPER_DATA.Count; i++)
            {
                if (DO_OPER_DATA[i])
                {
                    outBuffer[i / 8] |= (byte)(1 << (i % 8)); // 비트 설정
                }
            }

            // EtherCAT 출력 데이터 쓰기
            NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_IO_WRITE(0, _EcatAddress, 0, 8, outBuffer);

            // 쓰기 성공 여부 반환
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
            {
                Global.ExceptionLog.ErrorFormat($"Set_OutputData - MC_IO_WRITE failed with status: {mc}");
                return false;
            }

            return true;
        }
        public bool Set_OutputData(int index, bool onoff)
        {
            Get_OutputData();
            UpdateOperData();

            // 클론 데이터 수정
            DO_OPER_DATA[index] = onoff;

            return Set_OperData();
        }
        public bool Set_Handler_Grip(int inout, bool grip)
        {
            Get_OutputData();
            UpdateOperData();

            // In Handler
            if(inout == 0)
            {
                DO_OPER_DATA[(int)DO_MAP.IN_HANDLER_UNGRIP_D_SOL] = !grip;
                DO_OPER_DATA[(int)DO_MAP.IN_HANDLER_GRIP_D_SOL] = grip;
            }
            else
            {
                DO_OPER_DATA[(int)DO_MAP.OUT_HANDLER_UNGRIP_D_SOL] = !grip;
                DO_OPER_DATA[(int)DO_MAP.OUT_HANDLER_GRIP_D_SOL] = grip;
            }


            // 복제된 데이터를 바탕으로 출력 버퍼 생성
            byte[] outBuffer = new byte[m_uOutBuffer.Length];
            for (int i = 0; i < DO_OPER_DATA.Count; i++)
            {
                if (DO_OPER_DATA[i])
                {
                    outBuffer[i / 8] |= (byte)(1 << (i % 8)); // 비트 설정
                }
            }

            // EtherCAT 출력 데이터 쓰기
            NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_IO_WRITE(0, _EcatAddress, 0, 8, outBuffer);

            // 쓰기 성공 여부 반환
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
            {
                Global.ExceptionLog.ErrorFormat($"Set_OutputData - MC_IO_WRITE failed with status: {mc}");
                return false;
            }

            return true;
        }
        public bool Set_Handler_UpDwon(int inout, bool updown)
        {
            Get_OutputData();
            UpdateOperData();

            // In Handler
            if (inout == 0)
            {
                DO_OPER_DATA[(int)DO_MAP.IN_HANDLER_DOWN_S_SOL] = !updown;
                DO_OPER_DATA[(int)DO_MAP.IN_HANDLER_UP_S_SOL] = updown;
            }
            else
            {
                DO_OPER_DATA[(int)DO_MAP.OUT_HANDLER_DOWN_S_SOL] = !updown;
                DO_OPER_DATA[(int)DO_MAP.OUT_HANDLER_UP_S_SOL] = updown;
            }

            return Set_OperData();
        }
        public bool Set_Gocator_Encoder (int gocator, int channel)
        {
            Get_OutputData();
            UpdateOperData();

            
            return Set_OperData();
        }
        public void DioThreadStart()
        {
            if (DioThread == null)
            {
                _ShouldStop = false;
                DioThread = new Thread(ThreadReceive);
                DioThread.IsBackground = true;
                DioThread.Start();

                IsDioThreadRunning = true;
                Global.Mlog.Info("DioThread has started.");
            }
        }
        public void DioThreadStop()
        {
            // 스레드 종료 플래그 설정
            _ShouldStop = true;

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
        public void ThreadReceive()
        {
            Get_OutputData();
            UpdateOperData();

            while (!_ShouldStop)
            {
                try
                {
                    Get_InputData();
                    Get_OutputData();
                }
                catch (Exception e)
                {
                    string error = e.ToString();
                    Global.ExceptionLog.ErrorFormat($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {error}");
                }

                Thread.Sleep(100);
            }

            // 스레드 종료 후 상태 업데이트
            IsDioThreadRunning = false;
            Global.Mlog.Info("DioThread has stopped.");
        }
        private void UpdateRawData(ObservableCollection<bool> targetCollection, byte[] inputBytes)
        {
            if (targetCollection == null)
                throw new ArgumentNullException(nameof(targetCollection));
            if (inputBytes == null)
                throw new ArgumentNullException(nameof(inputBytes));

            int bitIndex = 0;

            // targetCollection의 크기를 초과하지 않도록 제한
            for (int i = 0; i < inputBytes.Length; i++)
            {
                for (int bit = 0; bit < 8; bit++) // 1바이트는 8비트
                {
                    if (bitIndex >= targetCollection.Count)
                        return;

                    // 비트 값을 추출하여 bool로 변환
                    bool bitValue = (inputBytes[i] & (1 << bit)) != 0;

                    // targetCollection 업데이트
                    if (targetCollection[bitIndex] != bitValue)
                    {
                        targetCollection[bitIndex] = bitValue;
                    }

                    bitIndex++;
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
