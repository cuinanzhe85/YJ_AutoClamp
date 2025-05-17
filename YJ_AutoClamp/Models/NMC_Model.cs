using Common.Mvvm;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YJ_AutoClamp.Utils;
using YJ_AutoClamp.ViewModels;

namespace YJ_AutoClamp.Models
{
    public class NMC_Model : BindableAndDisposable
    {
        private readonly StringBuilder rtnMessage = new StringBuilder(128);
        private NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_STATUS.MC_OK;

        public bool IsConnected { get; set; } = false;
        public ushort BoardID { get; set; } = 0;
        public NMC_Model() { }
        public bool Connect()
        {
            const int TimeoutMilliseconds = 30000; // 타임아웃 상수 정의
            const int PollingIntervalMilliseconds = 500; // 폴링 간격 상수 정의

            // NMC 초기화
            if (!InitializeNMC())
                return false;

            // MasterRun 실행
            if (!RunMaster())
                return false;

            // 상태 확인
            if (!WaitForMasterRun(TimeoutMilliseconds, PollingIntervalMilliseconds))
            {
                Global.Mlog.Error("NMC Motion MasterRun Timeout.");
                return false;
            }

            Global.Mlog.Info("NMC Motion Connection Success.");
            IsConnected = true;
            return true;
        }

        private bool InitializeNMC()
        {
            mc = NMCSDKLib.MC_Init(); // mc를 클래스 멤버 변수로 사용
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
            {
                HandleError(mc, "NMC Motion Init Fail");
                return false;
            }

            Global.Mlog.Info("NMC Motion Init Success.");
            return true;
        }
        private bool RunMaster()
        {
            mc = NMCSDKLib.MC_MasterRUN(BoardID); // mc를 클래스 멤버 변수로 사용
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
            {
                HandleError(mc, "NMC Motion MasterRun Fail");
                return false;
            }

            Global.Mlog.Info("NMC Motion MasterRun Success.");
            return true;
        }
        private bool WaitForMasterRun(int timeoutMilliseconds, int pollingIntervalMilliseconds)
        {
            Stopwatch sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds <= timeoutMilliseconds)
            {
                byte mode = 0;
                mc = NMCSDKLib.MasterGetCurMode(0, ref mode); // mc를 클래스 멤버 변수로 사용

                if (mc != NMCSDKLib.MC_STATUS.MC_OK)
                {
                    HandleError(mc, "Failed to get current mode");
                    return false;
                }

                if (mode == (byte)NMCSDKLib.EcMstMode.eMM_RUN)
                    return true;

                if (mode == (byte)NMCSDKLib.EcMstMode.eMM_ERR || mode == (byte)NMCSDKLib.EcMstMode.eMM_LINKBROKEN)
                {
                    Global.Mlog.Error("NMC Motion encountered an error or link broken.");
                    return false;
                }

                Thread.Sleep(pollingIntervalMilliseconds); // 비동기로 전환 가능
            }

            return false;
        }
        private void HandleError(NMCSDKLib.MC_STATUS mc, string errorMessage)
        {
            NMCSDKLib.MC_GetErrorMessage((uint)mc, (uint)128, rtnMessage);
            Global.Mlog.Error($"{errorMessage}. {rtnMessage}");
        }
        public void Close()
        {
            mc = NMCSDKLib.MC_MasterSTOP(BoardID); // mc를 클래스 멤버 변수로 사용
            if (mc == NMCSDKLib.MC_STATUS.MC_OK)
            {
                Global.Mlog.Info("Master Stop OK!");
            }
            else
            {
                NMCSDKLib.MC_GetErrorMessage((uint)mc, (uint)128, rtnMessage);
                Global.Mlog.Error($"Master Stop Fail. {rtnMessage}");
            }
        }
        public async Task<bool> ServoOrigin(int slave)
        {
            // Origin Start
            Global.Mlog.Info($"Servo origin start for slave {slave}.");
            // 서보 상태 확인 및 리셋
            if (!GetServoFault(slave))
            {
                if (!SetServoReset(slave))
                {
                    Global.Mlog.Error($"Servo reset failed for slave {slave}.");
                    return false;
                }
                await Task.Delay(100);
            }
            // 서보 활성화
            if (!GetServoStatus(slave))
            {
                if (!SetServoOnOff(slave, false))
                {
                    Global.Mlog.Error($"Failed to turn off servo for slave {slave}.");
                    return false;
                }
                await Task.Delay(100);
                if (SetServoOnOff(slave, true))
                {
                    Global.Mlog.Info($"Servo turn on for slave {slave}.");
                }
                else
                {
                    Global.Mlog.Error($"Failed to turn on servo for slave {slave}.");
                    return false;
                }
            }
            // Set Servo Home Position
            if (!await SetHomePositionWithTimeout(slave))
            {
                return false;
            }
            // In_Handler Pickup Position
            if (slave == (int)ServoSlave_List.Top_X_Handler_X)
            {
                if (SingletonManager.instance.Dio.DI_RAW_DATA[(int)NmcDio_Model.DI_MAP.IN_HANDLER_UP_SENSOR])
                {
                    MoveABS(slave, SingletonManager.instance.Teaching_Data[Teaching_List.Top_X_Handler_Pick_Up.ToString()]);
                    while (true)
                    {
                        await Task.Delay(100);

                        if (IsMoveDone(slave))
                        {
                            break;
                        }
                    }
                }
                // 실린더 Down상태에서 움직이면 핸드파손됨
                else
                    return false;
            }
            else if(slave == (int)ServoSlave_List.Out_Y_Handler_Y)
            {
                if (SingletonManager.instance.Dio.DI_RAW_DATA[(int)NmcDio_Model.DI_MAP.OUT_HANDLER_UP_SENSOR])
                {
                    MoveABS(slave, SingletonManager.instance.Teaching_Data[Teaching_List.Out_Y_Handler_Put_Down_1.ToString()]);
                    while (true)
                    {
                        await Task.Delay(100);

                        if (IsMoveDone(slave))
                        {
                            break;
                        }
                    }
                }
                // 실린더 Down상태에서 움직이면 핸드파손됨
                else
                    return false;
            }

            return true;
        }
        public async Task<bool> SetHomePositionWithTimeout(int slave, int timeoutMilliseconds = 30000, int pollingIntervalMilliseconds = 100)
        {
            // Set Servo Home Position
            if (!SetHomePosition(slave))
            {
                Global.Mlog.Error($"Failed to set home position for slave {slave}.");
                return false;
            }

            // Check Home position with timeout
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (stopwatch.ElapsedMilliseconds <= timeoutMilliseconds)
            {
                await Task.Delay(pollingIntervalMilliseconds);

                if (IsHomeOK(slave) && IsMoveDone(slave))
                {
                    Global.Mlog.Info($"Home position finish for slave {slave}.");
                    return true;
                }
            }

            // 타임아웃 발생
            Global.Mlog.Error($"Servo origin operation timed out for slave {slave}.");
            ServoStop(slave);
            return false;
        }
        public bool GetServoFault(int slave)
        {
            uint status = 0;
            NMCSDKLib.MC_ReadAxisStatus(0, (ushort)slave, ref status);
            if ((status & (uint)NMCSDKLib.MC_AXISSTATUS.mcDriveFault) != 0 || (status & (uint)NMCSDKLib.MC_AXISSTATUS.mcErrorStop) != 0)
                return false;
            else
                return true;
        }
        public bool GetServoStatus(int slave)
        {
            bool isServoOn = false;
            uint status = 0;
            NMCSDKLib.MC_ReadAxisStatus(0, (ushort)slave, ref status);
            isServoOn = (status & (uint)NMCSDKLib.MC_AXISSTATUS.mcPowerOn) != 0;

            return isServoOn;
        }
        public bool SetServoReset(int slave)
        {
            mc = NMCSDKLib.MC_Reset(BoardID, (ushort)slave);
            if (mc == NMCSDKLib.MC_STATUS.MC_OK)
            {
                Global.Mlog.Info($"Servo Reset OK. Slave ID : {slave}");
                return true;
            }
            else
            {
                mc = NMCSDKLib.MC_GetErrorMessage((uint)mc, (uint)128, rtnMessage);
                Global.Mlog.Error($"Servo Reset Fail. Slave ID : {slave}. rtnMessage : {rtnMessage}");
                return false;
            }
        }
        public bool SetServoOnOff(int slave, bool onoff)
        {
            string action = onoff ? "On" : "Off";

            mc = NMCSDKLib.MC_Power(BoardID, (ushort)slave, onoff);
            if (mc == NMCSDKLib.MC_STATUS.MC_OK)
            {
                SingletonManager.instance.Servo_Model[slave].IsServoOn = onoff;
                Global.Mlog.Info($"Servo {action} OK. Slave ID : {slave}");
                return true;
            }
            else
            {
                mc = NMCSDKLib.MC_GetErrorMessage((uint)mc, (uint)128, rtnMessage);
                Global.Mlog.Error($"Servo {action} Fail. Slave ID : {slave}. rtnMessage : {rtnMessage}");
                return false;
            }
        }
        public bool IsPlusLimit(int slave)
        {
            uint status = 0;
            NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_ReadAxisStatus(0, (ushort)slave, ref status);
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
                return false;

            return (status & (uint)NMCSDKLib.MC_AXISSTATUS.mcLimitSwitchPos) != 0;
        }
        public bool IsMinusLimit(int slave)
        {
            uint status = 0;
            NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_ReadAxisStatus(0, (ushort)slave, ref status);
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
                return false;

            return (status & (uint)NMCSDKLib.MC_AXISSTATUS.mcLimitSwitchNeg) != 0;
        }
        public bool IsHomeLimit(int slave)
        {
            uint status = 0;
            NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_ReadAxisStatus(0, (ushort)slave, ref status);
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
                return false;

            return (status & (uint)NMCSDKLib.MC_AXISSTATUS.mcHomeAbsSwitch) != 0;
        }
        public int ServoStop(int slave)
        {
            var cServoModel = SingletonManager.instance.Servo_Model[slave];
            double vel = cServoModel.ServoScale * cServoModel.Velocity;
            double decel = (vel / cServoModel.Decelerate) * 1000.0;
            return (int)NMCSDKLib.MC_Halt(0, (ushort)slave, decel, 0, NMCSDKLib.MC_BUFFER_MODE.mcAborting);
        }
        public int MoveJog(int slave, int direction, int speed = 0)
        {
            var cServoModel = SingletonManager.instance.Servo_Model[slave];

            NMCSDKLib.MC_DIRECTION md = direction == 1 ? NMCSDKLib.MC_DIRECTION.mcPositiveDirection : NMCSDKLib.MC_DIRECTION.mcNegativeDirection;
            double vel = cServoModel.ServoScale * cServoModel.JogVelocity[speed];
            double acc = vel * 1000.0 / cServoModel.Accelerate;
            double dec = vel * 1000.0 / cServoModel.Decelerate;
            mc = NMCSDKLib.MC_MoveVelocity(BoardID, (ushort)slave, vel, acc, dec, 0, md, NMCSDKLib.MC_BUFFER_MODE.mcAborting);
            return (int)mc;
        }
        public bool MoveABS(int slave, double position, double? velocity = null)
        {
            var cServoModel = SingletonManager.instance.Servo_Model[slave];
            double vel = velocity.HasValue ? velocity.Value * cServoModel.ServoScale : cServoModel.Velocity * cServoModel.ServoScale;

            double pos = cServoModel.ServoScale * position;
            double acc = (vel / cServoModel.Accelerate) * 1000;
            double dec = (vel / cServoModel.Decelerate) * 1000;

            mc = NMCSDKLib.MC_MoveAbsolute(0, (ushort)slave, pos, vel, acc, dec, 0, NMCSDKLib.MC_DIRECTION.mcPositiveDirection, NMCSDKLib.MC_BUFFER_MODE.mcAborting);
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
                return false;
            else
                return true;
        }
        public bool IsMoveDone(int slave)
        {
            uint status = 0;
            if (NMCSDKLib.MC_STATUS.MC_OK != NMCSDKLib.MC_ReadAxisStatus(0, (ushort)slave, ref status))
                return false;

            bool flag1 = (status & (uint)NMCSDKLib.MC_AXISSTATUS.mcMotionComplete) != 0;
            bool flag2 = (status & (uint)NMCSDKLib.MC_AXISSTATUS.mcContinuousMotion) != 0;

            if (flag1 == true && flag2 == false)
                return true;
            else
                return false;
        }
        public bool SetHomePosition(int slave)
        {
            mc = NMCSDKLib.MC_Home(0, (ushort)slave, 0, NMCSDKLib.MC_BUFFER_MODE.mcAborting);
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
                return false;
            else
                return true;
        }
        public bool IsHomeOK(int slave)
        {
            bool isHome = false;
            uint status = 0;
            NMCSDKLib.MC_STATUS mc = NMCSDKLib.MC_ReadAxisStatus(0, (ushort)slave, ref status);
            if (mc != NMCSDKLib.MC_STATUS.MC_OK)
                return false;

            isHome = (status & (uint)NMCSDKLib.MC_AXISSTATUS.mcIsHomed) != 0;

            return isHome;
        }
        public double GetActualPos(int slave)
        {
            var cServoModel = SingletonManager.instance.Servo_Model[slave];

            double currentPos = 0;
            NMCSDKLib.MC_ReadActualPosition(0, (ushort)slave, ref currentPos);

            return currentPos / cServoModel.ServoScale;
        }

    }
}
