using Common.Mvvm;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YJ_AutoClamp.Utils.EzMotion_E;
using YJ_AutoClamp.Utils.EzMotion_R;
using YJ_AutoClamp.ViewModels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace YJ_AutoClamp.Models
{
    public class EzMotion_Model_E : BindableAndDisposable
    {
        // 10000 / (지름 * Pi) * 감속기 : 파슨택
        // 파라소닉 기어비 필요
        // Scale * (속도 / 기어비) - Move, Scale * (속도 * 기어비) - 정지 : 파라소닉
        // 기어비 : 리드 / 83886080
        public double[] ServoScales { get; set; } = new double[]
        {
            (10000 / (41.38 * Math.PI)*3),    // Out_Handler_Y
            (10000 / (31.83 * Math.PI)*3),    // Out_Handler_Z
            (10000 / (41.38 * Math.PI)*3),    // Top_Handler_X
            (10000 / (39.79 * Math.PI)*3),    // Top Return In CV
            (10000 / 10 * 3),    // Lift_Z_1
            (10000 / 10 * 3),    // Lift_Z_2
            (10000 / 10 * 3)     // Lift_Z_3
        };

        public IPAddress IpAddress ;
        public EzMotion_Model_E() { }
        public bool Connect(int iSlaveNo)
        {
            try
            {
                // Already Connected
                if (SingletonManager.instance.Servo_Model[iSlaveNo].IsEzConnected == true)
                {
                    SingletonManager.instance.Servo_Model[iSlaveNo].IsEzConnected = true;
                    return true;
                }

                if (iSlaveNo == (int)ServoSlave_List.Out_Y_Handler_Y)   IpAddress = IPAddress.Parse("192.168.0.2");
                if (iSlaveNo == (int)ServoSlave_List.Out_Z_Handler_Z)   IpAddress = IPAddress.Parse("192.168.0.3");
                if (iSlaveNo == (int)ServoSlave_List.Top_X_Handler_X)     IpAddress = IPAddress.Parse("192.168.0.4");
                if (iSlaveNo == (int)ServoSlave_List.Top_CV_X) IpAddress = IPAddress.Parse("192.168.0.5");
                if (iSlaveNo == (int)ServoSlave_List.Lift_1_Z) IpAddress = IPAddress.Parse("192.168.0.6");
                if (iSlaveNo == (int)ServoSlave_List.Lift_2_Z) IpAddress = IPAddress.Parse("192.168.0.7");
                if (iSlaveNo == (int)ServoSlave_List.Lift_3_Z) IpAddress = IPAddress.Parse("192.168.0.8");
                // Is not 0 == Connect Success
                if (EziMOTIONPlusELib.FAS_ConnectTCP(IpAddress, iSlaveNo) == true)
                {
                    SingletonManager.instance.Servo_Model[iSlaveNo].IsEzConnected = true;
                    Global.Mlog.Info($"EziMOTION Plus E Connect Success. IP Address : {IpAddress.ToString()}, Slave : {iSlaveNo}");
                    return true;
                }
                // Is 0 == Connect Fail
                else
                {
                    SingletonManager.instance.Servo_Model[iSlaveNo].IsEzConnected = false;
                    Global.Mlog.Info($"EziMOTION Plus E Connect Fail. IP Address : {IpAddress.ToString()}, Slave : {iSlaveNo}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                return false;
            }
        }
        public void Close(int iSlaveNo)
        {
            try
            {
                if (SingletonManager.instance.Servo_Model[iSlaveNo].IsEzConnected == true)
                {
                    EziMOTIONPlusELib.FAS_Close(iSlaveNo);
                    SingletonManager.instance.Servo_Model[iSlaveNo].IsEzConnected = false;
                    Global.Mlog.Info($"EziMOTION Plus E Disconnect Success. IP Address : {IpAddress.ToString()}, Slave : {iSlaveNo}");
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
            }
        }
        public bool IsServoOn(int iSlaveNo)
        {
            // Check Drive's Servo Status
            uint AxisStatus = 0;
            bool flagServOn = false;

            // if ServoOnFlagBit is OFF('0'), switch to ON('1')
            if (EziMOTIONPlusELib.FAS_GetAxisStatus(iSlaveNo, ref AxisStatus) != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(FAS_GetAxisStatus) was failed.");
                return false;
            }
            /*FFLAG_SERVOON*/
            flagServOn = (AxisStatus & 0x00100000) != 0 ? true : false;
            if (flagServOn == true)
                Global.Mlog.Info($"Servo On. Slave No : {iSlaveNo}");
            else
                Global.Mlog.Info($"Servo Off. Slave No : {iSlaveNo}");

            return flagServOn;
        }
        public bool SetServoOn(int iSlaveNo, bool OnOff)
        {
            int cmd = OnOff == true ? 1 : 0;
            // 0 : OFF, 1 : ON
            if (EziMOTIONPlusELib.FAS_ServoEnable(iSlaveNo, cmd) != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_ServoEnable() was failed : " + EziMOTIONPlusELib.FMM_OK.ToString();
                Global.Mlog.Info(strMsg);
                return false;
            }
            else
            {
                if (OnOff == true)
                {
                    SingletonManager.instance.Servo_Model[iSlaveNo].IsServoOn = OnOff;
                    Global.Mlog.Info($"Servo On. Slave No : {iSlaveNo}");
                }
                else
                    Global.Mlog.Info($"Servo Off. Slave No : {iSlaveNo}");

                return true;
            }
        }
        public bool ServoStop(int iSlaveNo)
        {
            int nRtn = EziMOTIONPlusELib.FAS_MoveStop(iSlaveNo);
            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_MoveStop() was failed : " + nRtn.ToString();
                Global.Mlog.Info($"{strMsg}. Slave No : {iSlaveNo}");
                return false;
            }
            else
            {
                Global.Mlog.Info($"Servo Stop. Slave No : {iSlaveNo}");
                return true;
            }
        }
        public bool IsServoAlarm(int iSlaveNo)
        {
            uint status = 0;
            int nRtn = EziMOTIONPlusELib.FAS_GetAxisStatus(iSlaveNo, ref status);
            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_GetAxisStatus() was failed : " + nRtn.ToString();
                Global.Mlog.Info($"{strMsg}. Slave No : {iSlaveNo}");
                return false;
            }
            // 0x00000001 is Error All Flag
            return (status & 0x00000001) != 0 ? true : false;
        }
        public bool IsMoveDone(int iSlaveNo)
        {
            if (SingletonManager.instance.Servo_Model[iSlaveNo].IsEzConnected == false)
                return true;

            uint status = 0;
            int nRtn = EziMOTIONPlusELib.FAS_GetAxisStatus(iSlaveNo, ref status);
            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_GetAxisStatus() was failed : " + nRtn.ToString();
                Global.Mlog.Info($"{strMsg}. Slave No : {iSlaveNo}");
                return false;
            }
            // 0x00080000 is Inposition Flag
            bool inposition = (status & 0x00080000) != 0 ? true : false;
            // 0x08000000 is Moving Flag
            bool moving = (status & 0x08000000) != 0 ? true : false;

            if (inposition == true && moving == false)
                return true;
            else
                return false;
        }
        public bool SetClearPosition(int iSlaveNo)
        {
            if (EziMOTIONPlusELib.FAS_ClearPosition(iSlaveNo) != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_ClearPosition() was failed : " + EziMOTIONPlusELib.FMM_OK.ToString();
                Global.Mlog.Info(strMsg);
                return false;
            }
            else
            {
                Global.Mlog.Info($"Servo Position Clear. Slave No : {iSlaveNo}");
                return true;
            }
        }
        public bool EmergencyServoStop(int iSlaveNo)
        {
            int nRtn = EziMOTIONPlusELib.FAS_EmergencyStop(iSlaveNo);
            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_EmergencyStop() was failed : " + nRtn.ToString();
                Global.Mlog.Info($"{strMsg}. Slave No : {iSlaveNo}");
                return false;
            }
            else
            {
                Global.Mlog.Info($"Emergency Servo Stop. Slave No : {iSlaveNo}");
                return true;
            }
        }
        public bool ServoAlarmReset(int iSlaveNo)
        {
            int nRtn = EziMOTIONPlusELib.FAS_ServoAlarmReset(iSlaveNo);
            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_AlarmReset() was failed : " + nRtn.ToString();
                Global.Mlog.Info($"{strMsg}. Slave No : {iSlaveNo}");
                return false;
            }
            else
            {
                Global.Mlog.Info($"Servo Alarm Reset. Slave No : {iSlaveNo}");
                return true;
            }
        }
        public double GetActualPos(int iSlaveNo)
        {
            int lActualPos = 0;
            if (EziMOTIONPlusELib.FAS_GetActualPos(iSlaveNo, ref lActualPos) != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(FAS_GetActualPos) was failed.");
            }
            return (double)(lActualPos/ServoScales[iSlaveNo]);
        }
        
        // Todo : 작업 이어서 해야함
        public bool MoveJog(int iSlaveNo, int direct, int speedFlag=0)
        {
            var cServoModel = SingletonManager.instance.Servo_Model[iSlaveNo];
            if (cServoModel.IsEzConnected == false)
                return false;

            uint vel = (uint)(ServoScales[iSlaveNo] * cServoModel.JogVelocity[speedFlag]);//(uint)(10 * ServoScales[iSlaveNo]);

            if (EziMOTIONPlusELib.FAS_MoveVelocity(iSlaveNo, vel, direct) != EziMOTIONPlusELib.FMM_OK)
            {
                Console.WriteLine("Function(FAS_MoveVelocity) was failed.");
                return false;
            }

            return true;
        }
        public bool MoveABS(int iSlaveNo, double IPosition)
        {
            var cServoModel = SingletonManager.instance.Servo_Model[iSlaveNo];
            if (cServoModel.IsEzConnected == false)
                return false;
            IPosition = Math.Round(IPosition, 2);
            int pos = (int)(IPosition * ServoScales[iSlaveNo]);
            uint vel = (uint)(cServoModel.Velocity * ServoScales[iSlaveNo]);//(25 * ServoScales[iSlaveNo]);
            int nRtn = EziMOTIONPlusELib.FAS_MoveSingleAxisAbsPos
                (
                    iSlaveNo,
                    pos,
                    vel
                );

            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_MoveSingleAxisAbsPos() \nReturned: " + nRtn.ToString();
                return false;
            }
            else
                return true;
        }
        public string GetAlarmMessage(int iSlaveNo)
        {
            byte dwAlarm = new byte();
            int nRtn = EziMOTIONPlusELib.FAS_GetAlarmType(iSlaveNo, ref dwAlarm);
            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(FAS_GetAlarm) was failed.");
                return "Get Alarm Fail";
            }
            string returnMsg ="";
            return returnMsg;
        }
        public bool SetOrigion(int iSlaveNo)
        {
            int nRtn = EziMOTIONPlusELib.FAS_MoveOriginSingleAxis(iSlaveNo);
            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(FAS_SetOrigin) was failed.");
                return false;
            }
            else
            {
                Global.Mlog.Info($"Set Origin. Slave No : {iSlaveNo}");
                return true;
            }
        }
        private bool GetAmpFault(int iSlaveNo)
        {
            int result = 0;
            uint AxisStatus = 0;
            bool flag1, flag2, flag3;

            result = EziMOTIONPlusELib.FAS_GetAxisStatus(iSlaveNo, ref AxisStatus);
            if (result != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(FAS_GetAxisStatus) was failed.");
                return false;
            }
            flag1 = (AxisStatus & 0x00000001) != 0 ? true : false;//ERRORALL
            flag2 = (AxisStatus & 0x00000040) != 0 ? true : false;//ERROVERCURRENT
            flag3 = (AxisStatus & 0x00000200) != 0 ? true : false;//ERROVERLOAD

            if (flag1 || flag2 || flag3)
            {
                Global.Mlog.Info("Function(FAS_GetAxisStatus) was Success.");
                return true;
            }
            else
            {
                Global.Mlog.Info("Function(FAS_GetAxisStatus) Status Check failed.");
                return false;
            }
        }
        public bool SetAmpEnable(int iSlaveNo, bool bEnable)
        {
            /** AMP */
            if ((EziMOTIONPlusELib.FAS_ServoEnable(iSlaveNo, bEnable ? 1 : 0) != EziMOTIONPlusRLib.FMM_OK))
            {
                return false;
            }
            return true;
        }
        public bool IsOriginOK(int iSlaveNo)
        {
            uint status = 0;
            if (EziMOTIONPlusELib.FAS_GetAxisStatus(iSlaveNo, ref status) != EziMOTIONPlusELib.FMM_OK)
            {
                return false;
            }

            bool flag = (status & 0x02000000) != 0 ? true : false;//ERRORALL
            return flag;
        }
        public async Task<bool> ServoOrigin(int iSlaveNo)
        {
            // Status Check
            //if (GetAmpFault(iSlaveNo) == false)
            //{
            //    if (ServoAlarmReset(iSlaveNo)==false)
            //    {
            //        EmergencyServoStop(iSlaveNo);
            //        return false; 
            //    }
            //}
            // Servo Off
            if (SetAmpEnable(iSlaveNo, false) == false)
            {
                EmergencyServoStop(iSlaveNo);
                return false;
            }
            // Alarm Reset
            if (ServoAlarmReset(iSlaveNo) == false)
            {
                EmergencyServoStop(iSlaveNo);
                return false;
            }
            await Task.Delay(200);
            // Status Check
            //if (GetAmpFault(iSlaveNo) == false)
            //{
            //    EmergencyServoStop(iSlaveNo);
            //    return false;
            //}
            // Servo On
            if (SetAmpEnable(iSlaveNo, true) == false)
            {
                EmergencyServoStop(iSlaveNo);
                return false;
            }
            await Task.Delay(2000);
            // Origin Start
            if (SetOrigion(iSlaveNo) == false)
            {
                EmergencyServoStop(iSlaveNo);
                return false;
            }
            await Task.Delay(200);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int waitTime = 20000;
            if (iSlaveNo == (int)ServoSlave_List.Out_Y_Handler_Y)
                waitTime = 50000;
            while (true)
            {
                if (IsOriginOK(iSlaveNo) == true)
                {
                    break;
                }
                // 20초 Time Out Check
                if (sw.ElapsedMilliseconds > waitTime)
                {
                    sw.Stop();
                    EmergencyServoStop(iSlaveNo);
                    return false;
                }
                await Task.Delay(10);
            }
            // Z up
            if (iSlaveNo == (int)ServoSlave_List.Out_Z_Handler_Z)
            {
                if (IsOriginOK(iSlaveNo) == true)
                {
                    double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Home).ToString()];
                    pos = Math.Round(pos, 2);
                    MoveABS(iSlaveNo, pos);
                    while(true)
                    {
                        double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Z_Handler_Z)), 2);
                        if (GetPos == pos)
                            break ;
                        Thread.Sleep(10);
                    }
                }
            }
            if (iSlaveNo == (int)ServoSlave_List.Top_X_Handler_X)
            {
                if (IsOriginOK(iSlaveNo) == true)
                {
                    double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Pick_Up).ToString()];
                    pos = Math.Round(pos, 2);
                    MoveABS(iSlaveNo, pos);
                    while (true)
                    {
                        double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Top_X_Handler_X)), 2);
                        if (GetPos == pos)
                            break;
                        Thread.Sleep(10);
                    }
                }
            }
            return true;
        }
        public async Task<bool> SetHomePositionWithTimeout(int slave, int timeoutMilliseconds = 30000, int pollingIntervalMilliseconds = 100)
        {
            Thread.Sleep(500);
            // Set Servo Home Position
            if (await ServoOrigin(slave))
            {
                Global.Mlog.Error($"Failed to set home position for slave {slave}.");
                ServoStop(slave);
                return false;
            }

            // Check Home position with timeout
            
            return true;
        }
        public int IsOverSWLimit(int slave, ref bool plusOver, ref bool minusOver)
        {
            int iResult;
            bool flag1, flag2;
            uint status =0;

            if (EziMOTIONPlusELib.FMM_OK != (iResult = EziMOTIONPlusELib.FAS_GetAxisStatus(slave, ref status)))
            {
                return iResult;
            }

            flag1 = (status & 0x00000008) != 0 ? true : false;//+ sw limit over
            flag2 = (status & 0x00000010) != 0 ? true : false;//- sw limit over

            plusOver = flag1;   //true이면 over이다.
            minusOver = flag2;  //true이면 over이다.

            return EziMOTIONPlusRLib.FMM_OK;
        }
        #region  //DIO Control
        public bool GetIO_InputData(int iSlaveNo, int target)
        {
            uint dwInput = 0;
            int nRtn = EziMOTIONPlusELib.FAS_GetIOInput(iSlaveNo, ref dwInput);

            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(GetIO_InputData) was failed.");
                return false;
            }

            return (dwInput & (1u << target)) != 0;
        }
        public bool GetIO_OutputData(int iSlaveNo, int target)
        {
            uint dwInput = 0;
            int nRtn = EziMOTIONPlusELib.FAS_GetIOOutput(iSlaveNo, ref dwInput);

            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(GetIO_InputData) was failed.");
                return false;
            }

            return (dwInput & (1u << target)) != 0;
        }
        public bool SetIO_InputData(int iSlaveNo, uint on, uint off)
        {
            int nRtn = EziMOTIONPlusELib.FAS_SetIOInput(iSlaveNo, on, off);

            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(SetIO_InputData) was failed.");
                return false;
            }

            return true;
        }
        public bool SetIO_OutputData(int iSlaveNo, uint on, uint off)
        {
            int nRtn = EziMOTIONPlusELib.FAS_SetIOOutput(iSlaveNo, on, off);

            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                Global.Mlog.Info("Function(SetIO_InputData) was failed.");
                return false;
            }

            return true;
        }
        #endregion
    }
}
