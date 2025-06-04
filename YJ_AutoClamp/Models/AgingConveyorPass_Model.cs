using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static YJ_AutoClamp.Models.EziDio_Model;

namespace YJ_AutoClamp.Models
{
    public class AgingConveyorPass_Model : BindableAndDisposable
    {
        private EziDio_Model Dio = SingletonManager.instance.Ez_Dio;
        private EzMotion_Model_E Ez_Model = SingletonManager.instance.Ez_Model;
        private int _AgingCvIndex = 0;
        public int AgingCvIndex
        {
            get { return _AgingCvIndex; }
            set { SetValue(ref _AgingCvIndex , value); }
        }

        Stopwatch StopwatchTimer = new Stopwatch();
        public enum Aging_CV_Pass_Step
        {
            Idle,
            CV_End_Sensor_Check,
            CV_Run,
            CV_End_Sensor_Wait,
            Interfase_Send,
            Unclamp_Interfase_Wait,
            Clamp_Out_Start,
            CV_Off_Wait,
            CV_Off,
            Next_CV_Start
        }
        public Aging_CV_Pass_Step AgingCVStep = Aging_CV_Pass_Step.Idle;
        public AgingConveyorPass_Model()
        {

        }
        public void AgingConveyorPass()
        {
            switch (AgingCVStep)
            {
                case Aging_CV_Pass_Step.Idle:
                    AgingCVStep = Aging_CV_Pass_Step.CV_End_Sensor_Check;
                    break;
                case Aging_CV_Pass_Step.CV_End_Sensor_Check:
                    // Conveyor End Sensor에 제품이 있는지 확인한다. 
                    if (GetAgingCVStartEndSS(AgingCvIndex) == true)
                    {
                        // 제품이 있으면 UnClamp Interfase를 보낸다.
                        AgingCVStep = Aging_CV_Pass_Step.Interfase_Send;
                    }
                    else
                    {
                        // 제품이 없으면 CV를 실행한다. 
                        AgingCVStep = Aging_CV_Pass_Step.CV_Run;
                    }
                    break;
                case Aging_CV_Pass_Step.CV_Run:
                    // Aging Conveyour를 실행한다.
                    Dio_Aging_CV_Control(AgingCvIndex, true);
                    StopwatchTimer.Restart();
                    AgingCVStep = Aging_CV_Pass_Step.CV_End_Sensor_Wait;
                    break;
                case Aging_CV_Pass_Step.CV_End_Sensor_Wait:
                    // Aging Conveyour의 End Sensor가 들어오면 conveyor를 정지한다.
                    if (GetAgingCVStartEndSS(AgingCvIndex) == true)
                    {
                        Dio_Aging_CV_Control(AgingCvIndex, false);
                        AgingCVStep = Aging_CV_Pass_Step.Interfase_Send;
                    }
                    // 60초 동안 구동하고 clamp가 감지되지 않으면 clamp 없는걸로 인식하고  다음 conveyour를 구동한다.
                    if (StopwatchTimer.ElapsedMilliseconds > 60000)
                    {
                        AgingCvIndex++;

                        AgingCVStep = Aging_CV_Pass_Step.Idle;
                    }
                    break;
                case Aging_CV_Pass_Step.Interfase_Send:
                    // Unclamp Interfase를 보낸다.
                    if (SetUnclampInterfase(AgingCvIndex, true) == true)
                    {
                        // 인터페이스가 정상 전달되면 다음으로 이동한다.
                        AgingCVStep = Aging_CV_Pass_Step.Unclamp_Interfase_Wait;
                    }
                    break;
                case Aging_CV_Pass_Step.Unclamp_Interfase_Wait:
                    // Unclamp Interfase Return을 확인한다.
                    if (UnclampInterfaseReturnOn(AgingCvIndex) == true)
                    {
                        AgingCVStep = Aging_CV_Pass_Step.Clamp_Out_Start;
                    }
                    break;
                case Aging_CV_Pass_Step.Clamp_Out_Start:
                    // Unclamp가 Clamp 받을 준비가 되여있으면 conveyou를 전진시킨다.
                    Dio_Aging_CV_Control(AgingCvIndex, true);
                    AgingCVStep = Aging_CV_Pass_Step.CV_Off_Wait;
                    break;
                case Aging_CV_Pass_Step.CV_Off_Wait:
                    // 끝단에 있는 clamp 하나가 나가는것을 감지한다.
                    if (GetAgingCVStartEndSS(AgingCvIndex) == false)
                        AgingCVStep = Aging_CV_Pass_Step.CV_Off;
                    break;
                case Aging_CV_Pass_Step.CV_Off:
                    // 다음 clmap가 sensor까지 오면 conveyou 정지한다.
                    if (GetAgingCVStartEndSS(AgingCvIndex) == true)
                    {
                        Dio_Aging_CV_Control(AgingCvIndex, false);
                        AgingCVStep = Aging_CV_Pass_Step.Next_CV_Start;

                    }
                    break;
                case Aging_CV_Pass_Step.Next_CV_Start:
                    // Unclamp Interfase가 꺼지면 send Interface Off 하고 다음 conveyou를 구동한다.
                    if (UnclampInterfaseReturnOn(AgingCvIndex) == false)
                    {
                        SetUnclampInterfase(AgingCvIndex, false);
                        AgingCvIndex++;
                        AgingCVStep = Aging_CV_Pass_Step.Idle;
                    }
                    break;
                
            }
        }
        private bool GetAgingCVStartEndSS(int Index)
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
        private void Dio_Aging_CV_Control(int Index, bool OnOff)
        {
            if (Index == 0)
            {
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, OnOff);
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, OnOff);
            }
            if (Index == 1)
            {
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, OnOff);
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, OnOff);
            }
            if (Index == 2)
            {
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, OnOff);
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, OnOff);
            }
            if (Index == 3)
            {
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, OnOff);
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, OnOff);
            }

            if (Index == 4)
            {
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, OnOff);
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, OnOff);
            }

            if (Index == 5)
            {
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, OnOff);
                Dio.SetIO_OutputData((int)DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2, OnOff);
            }
        }
        private bool SetUnclampInterfase(int index, bool OnOff)
        {
            if (index == 0)
                return Dio.SetIO_OutputData((int)DO_MAP.AGING_CV_UPPER_INTERFACE_1, OnOff);
            else if (index == 1)
                return Dio.SetIO_OutputData((int)DO_MAP.AGING_CV_UPPER_INTERFACE_2, OnOff);
            else if (index == 2)
                return Dio.SetIO_OutputData((int)DO_MAP.AGING_CV_UPPER_INTERFACE_3, OnOff);
            else if (index == 3)
                return Dio.SetIO_OutputData((int)DO_MAP.AGING_CV_LOW_INTERFACE_1, OnOff);
            else if (index == 4)
                return Dio.SetIO_OutputData((int)DO_MAP.AGING_CV_LOW_INTERFACE_2, OnOff);
            else if (index == 5)
                return Dio.SetIO_OutputData((int)DO_MAP.AGING_CV_LOW_INTERFACE_3, OnOff);

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
    }
}
