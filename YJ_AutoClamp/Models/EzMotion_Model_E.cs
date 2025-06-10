using Common.Mvvm;
using System;
using System.Collections.Generic;
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
        //private EziDio_Model Dio = SingletonManager.instance.Ez_Dio;
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
            await Task.Delay(1000);
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
            int waitTime = 60000;
            if (iSlaveNo == (int)ServoSlave_List.Out_Y_Handler_Y)
                waitTime = 90000;
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
            sw.Restart();
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
                        if (sw.ElapsedMilliseconds > 5000)
                            return false;
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
                        if (sw.ElapsedMilliseconds > 5000)
                            return false;
                        Thread.Sleep(10);
                    }
                }
            }
            if (iSlaveNo == (int)ServoSlave_List.Out_Y_Handler_Y)
            {
                if (IsOriginOK(iSlaveNo) == true)
                {
                    double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Home).ToString()];
                    pos = Math.Round(pos, 2);
                    MoveABS(iSlaveNo, pos);
                    while (true)
                    {
                        double GetPos = Math.Round(SingletonManager.instance.Ez_Model.GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
                        if (GetPos == pos)
                            break;
                        if (sw.ElapsedMilliseconds > 10000)
                            return false;
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
        public bool MoveTopHandlerNGPort()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_NG_Port).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("TOP_SERVO_POS", pos.ToString());
            return MoveABS((int)(ServoSlave_List.Top_X_Handler_X), pos);
        }
        public bool IsMoveTopNGPortDone()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_NG_Port).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Top_X_Handler_X)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool MoveTopHandlerPutDownPos()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Put_Down).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("TOP_SERVO_POS", pos.ToString());
            return MoveABS((int)(ServoSlave_List.Top_X_Handler_X), pos);
        }
        public bool MoveTopHandlerPickUpPos()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Pick_Up).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("TOP_SERVO_POS", pos.ToString());
            return MoveABS((int)(ServoSlave_List.Top_X_Handler_X), pos);
        }
        public bool IsTopHandlerPickUpPos()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Pick_Up).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Top_X_Handler_X)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool MoveTopReturnCvRun()
        {
            return MoveJog((int)(ServoSlave_List.Top_CV_X), (int)Direction.CCW, 2);
        }
        public bool MoveTopReturnCvStop()
        {
            return ServoStop((int)(ServoSlave_List.Top_CV_X));
        }
        public bool IsTopHandlerPutDownPos()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Top_X_Handler_Put_Down).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Top_X_Handler_X)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool MoveLiftDown(int Index)
        {
            double pos;
            bool ret = false;
            if (Index == 3)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_1).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_1_Z), pos);
            }
            if (Index == 4)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_2).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_2_Z), pos);
            }
            if (Index == 5)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_3).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_3_Z), pos);
            }
            return ret;
        }
        public bool IsMoveLiftDownDone(int Index)
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
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_1_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (Index == 4)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_2).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_2_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (Index == 5)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Low_3).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_3_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            return false;
        }
        public bool MoveLiftUp(int Index)
        {
            double pos;
            bool ret = false;
            if (Index == 0 || Index == 3)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_1).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_1_Z), pos);
            }
            if (Index == 1 || Index == 4)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_2).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_2_Z), pos);
            }
            if (Index == 2 || Index == 5)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_3).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_3_Z), pos);
            }
            return ret;
        }
        public bool IsMoveLiftUpDone(int Index)
        {
            double pos;
            double GetPos;
            if (Index == 0 || Index == 3)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_1).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_1_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (Index == 1 || Index == 4)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_2).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_2_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (Index == 2 || Index == 5)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Upper_3).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_3_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            return false;
        }
        public bool MoveLiftLoding(int LiftIndex)
        {
            double pos;
            bool ret = false;
            if (LiftIndex == 0)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_1).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_1_Z), pos);
            }
            if (LiftIndex == 1)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_2).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_2_Z), pos);
            }
            if (LiftIndex == 2)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_3).ToString()];
                pos = Math.Round(pos, 2);
                ret = MoveABS((int)(ServoSlave_List.Lift_3_Z), pos);
            }
            return ret;
        }
        public bool IsMoveLiftLodingDone(int LiftIndex)
        {
            double pos;
            double GetPos;
            if (LiftIndex == 0)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_1).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_1_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (LiftIndex == 1)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_2).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_2_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            if (LiftIndex == 2)
            {
                pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_3).ToString()];
                // 소수점아래 2자리까지비교
                pos = Math.Round(pos, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_3_Z)), 2);
                if (GetPos == pos)
                    return true;
            }
            return false;
        }
       
        public bool MoveOutHandlerPickUpY()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Pick_Up).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Y_SERVO_POS", pos.ToString());
            return MoveABS((int)(ServoSlave_List.Out_Y_Handler_Y), pos);
        }
        public bool IsOutHandlerPickupPosY()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Pick_Up).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool MoveOutHandlerPutDownY()
        {
            double pos = GetOutY_PutDownMovePosition();
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Y_SERVO_POS", pos.ToString());
            return MoveABS((int)(ServoSlave_List.Out_Y_Handler_Y), pos);
        }
        public bool IsOutHandlerYPutDownPos()
        {
            double pos = GetOutY_PutDownMovePosition();
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool IsOutHandlerYSafetyPos()
        {
            double pos = 0.0;
            pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_1).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
            if (GetPos <= pos)
                return true;
            return false;
        }
        public bool MoveOutHandlerReadyY()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Home).ToString()];
            pos = Math.Round(pos, 2);
            return MoveABS((int)(ServoSlave_List.Out_Y_Handler_Y), pos);
        }
        public bool IsMoveOutHandlerReadyY()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Home).ToString()];
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool MoveOutHandlerPickUpZ()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Pick_Up).ToString()];
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Z_SERVO_POS", pos.ToString());
            return MoveABS((int)(ServoSlave_List.Out_Z_Handler_Z), pos);
        }
        public bool IsOutHandlerPickUpZ()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Pick_Up).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Out_Z_Handler_Z)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool MoveOutHandlerRadyZ()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Home).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Z_SERVO_POS", pos.ToString());
            return MoveABS((int)(ServoSlave_List.Out_Z_Handler_Z), pos);
        }
        public bool IsOutHandlerReadyDoneZ()
        {
            double pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Home).ToString()];
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Out_Z_Handler_Z)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool MoveOutHandlerPutDownZ()
        {
            double pos = GetOutZ_PutDownFloorPos();
            pos = Math.Round(pos, 2);
            Global.instance.Write_Sequence_Log("OUT_Y_SERVO_POS", pos.ToString());
            return MoveABS((int)(ServoSlave_List.Out_Z_Handler_Z), pos);
        }
        public bool IsOutHandlerPutDownDoneZ()
        {
            double pos = GetOutZ_PutDownFloorPos();
            // 소수점아래 2자리까지비교
            pos = Math.Round(pos, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Out_Z_Handler_Z)), 2);
            if (GetPos == pos)
                return true;
            return false;
        }
        public bool IsOutHandlerSaftyInterlockY()
        {
            double pos1, pos2, pos3, pos4;
            pos1 = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_1).ToString()];
            pos1 = Math.Round(pos1, 2);
            pos2 = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_2).ToString()];
            pos2 = Math.Round(pos2, 2);
            pos3 = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_3).ToString()];
            pos3 = Math.Round(pos3, 2);
            pos4 = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Home).ToString()];
            pos4 = Math.Round(pos4, 2);
            double GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Out_Y_Handler_Y)), 2);

            // Y 축 Lift 1,2,3 , Ready 일때
            if (GetPos == pos1 || GetPos == pos2 || GetPos == pos3 || GetPos == pos4 || GetPos ==0)
                return true;
            return false;
        }
        public bool IsYPickupMoveLiftSaftyPos(int Index)
        {
            double pos1=0;
            double GetPos = 0;
            if (Index == 0)
            {
                pos1 = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_1).ToString()];
                pos1 = Math.Round(pos1, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_1_Z)), 2);

                if (GetPos >= pos1)
                    return true;
            }
            else if (Index == 1)
            {
                pos1 = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_2).ToString()];
                pos1 = Math.Round(pos1, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_2_Z)), 2);

                if (GetPos >= pos1)
                    return true;
            }
            else if (Index == 2)
            {
                pos1 = SingletonManager.instance.Teaching_Data[(Teaching_List.Lift_Load_3).ToString()];
                pos1 = Math.Round(pos1, 2);
                GetPos = Math.Round(GetActualPos((int)(ServoSlave_List.Lift_3_Z)), 2);

                if (GetPos >= pos1)
                    return true;
            }
            
            return false;
        }
        public bool ServoMovePause(int Slave, int StartStop)
        {
            int nRtn = EziMOTIONPlusELib.FAS_MovePause(Slave, StartStop);
            if (nRtn != EziMOTIONPlusELib.FMM_OK)
            {
                string strMsg = "FAS_MovePause() \nReturned: " + nRtn.ToString();
                return false;
            }
            else
                return true;
        }
        public double GetOutZ_PutDownFloorPos()
        {
            double pos = 0.0;
            pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Z_Handler_Put_Down_1 + SingletonManager.instance.LoadFloor[SingletonManager.instance.LoadStageNo]).ToString()];
            pos = Math.Round(pos, 2);
            return pos;
        }
        public double GetOutY_PutDownMovePosition()
        {
            double pos = 0.0;
            // Lift 1,2,3 순서로 Load 한다.
            pos = SingletonManager.instance.Teaching_Data[(Teaching_List.Out_Y_Handler_Put_Down_1 + SingletonManager.instance.LoadStageNo).ToString()];
            pos = Math.Round(pos, 2);
            return pos;
        }
        public bool ServoSlaveOriginStatus()
        {
            if (IsOriginOK((int)ServoSlave_List.Out_Y_Handler_Y) == false
                || IsOriginOK((int)ServoSlave_List.Out_Z_Handler_Z) == false
                || IsOriginOK((int)ServoSlave_List.Top_X_Handler_X) == false
                || IsOriginOK((int)ServoSlave_List.Lift_1_Z) == false
                || IsOriginOK((int)ServoSlave_List.Lift_2_Z) == false
                || IsOriginOK((int)ServoSlave_List.Lift_3_Z) == false)
            {
                string message = IsOriginOK((int)ServoSlave_List.Out_Y_Handler_Y) == false ? "Servo Y is not Origin." : "Servo Y Origin OK.";
                if (!string.IsNullOrEmpty(message))
                    message += "\r\n";
                message = IsOriginOK((int)ServoSlave_List.Out_Z_Handler_Z) == false ? "Servo Z is not Origin." : "Servo Z Origin OK.";
                if (!string.IsNullOrEmpty(message))
                    message += "\r\n";
                message = IsOriginOK((int)ServoSlave_List.Top_X_Handler_X) == false ? "Servo X is not Origin." : "Servo X Origin OK.";
                if (!string.IsNullOrEmpty(message))
                    message += "\r\n";
                message = IsOriginOK((int)ServoSlave_List.Lift_1_Z) == false ? "Lift 1 is not Origin." : "Lift 1 Origin OK.";
                if (!string.IsNullOrEmpty(message))
                    message += "\r\n";
                message = IsOriginOK((int)ServoSlave_List.Lift_2_Z) == false ? "Lift 2 is not Origin." : "Lift 2 Origin OK.";
                if (!string.IsNullOrEmpty(message))
                    message += "\r\n";
                message = IsOriginOK((int)ServoSlave_List.Lift_3_Z) == false ? "Lift 3 is not Origin." : "Lift 3 Origin OK.";
                Global.instance.Set_TowerLamp(Global.TowerLampType.Error);
                Global.instance.ShowMessagebox(message);
                return false;
            }
            return true;
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
