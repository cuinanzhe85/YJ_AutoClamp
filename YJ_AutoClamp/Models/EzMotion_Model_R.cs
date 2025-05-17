using Common.Mvvm;
using System;
using System.Collections.ObjectModel;
using YJ_AutoClamp.Utils.EzMotion_R;

namespace YJ_AutoClamp.Models
{
    public class EzMotion_Model_R : BindableAndDisposable
    {
        public byte PortNo { get; set; }
        private uint BaudRate { get; set; } = 115200;
        private int Pulse { get; set; } = 1000;
        private int Ratio { get; set; } = 3;      // 1:3 감속기 사용하므로 * 3 해줘야 함

        private bool _IsConnected = false;
        public bool IsConnected
        {
            get { return _IsConnected; }
            set { SetValue(ref _IsConnected, false); }
        }
        public ObservableCollection<Unit_Model> Unit_Model { get; set; }
        public ObservableCollection<Servo_Model> Servo_Model { get; set; }

        public EzMotion_Model_R() { }
        public bool Connect()
        {
            try
            {
                // Already Connected
                if (IsConnected == true)
                {
                    EziMOTIONPlusRLib.FAS_Close(PortNo);
                    IsConnected = false;
                }
                // Is not 0 == Connect Success
                if (EziMOTIONPlusRLib.FAS_Connect(PortNo, BaudRate) != 0)
                {
                    IsConnected = true;
                    Global.Mlog.Info($"EziMOTION Plus R Connect Success. Port No : COM{PortNo} BaudRate : {BaudRate}");

                    for (byte i = 0; i < EziMOTIONPlusRLib.MAX_SLAVE_NUMS; i++)
                    {
                        if (EziMOTIONPlusRLib.FAS_IsSlaveExist(PortNo, i) != 0)
                        {
                            // Slave Check
                            string _SlaveNumber = i.ToString();
                            Global.Mlog.Info($"EziMOTION Plus R Slave Check Success. Slave No : {_SlaveNumber}");
                            break;
                        }
                    }
                    return true;
                }
                // Is 0 == Connect Fail
                else
                {
                    IsConnected = false;
                    Global.Mlog.Info($"EziMOTION Plus R Connect Fail. Port No : COM{PortNo} BaudRate : {BaudRate}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                return false;
            }
        }
        public void Close()
        {
            try
            {
                if (IsConnected == true)
                {
                    EziMOTIONPlusRLib.FAS_Close(PortNo);
                    IsConnected = false;
                    Global.Mlog.Info($"EziMOTION Plus R Disconnect Success. Port No : COM{PortNo}");
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
            }
        }
        public bool IsServoOn(int iSlaveNo)
        {
            try
            {
                // Check Drive's Servo Status
                uint AxisStatus = 0;
                bool flagServOn = false;

                // if ServoOnFlagBit is OFF('0'), switch to ON('1')
                if (EziMOTIONPlusRLib.FAS_GetAxisStatus(PortNo, (byte)iSlaveNo, ref AxisStatus) != EziMOTIONPlusRLib.FMM_OK)
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
            catch(Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                return false;
            }
        }
        public bool SetServoOn(int iSlaveNo, int OnOff)
        {
            try
            {
                // 0 : OFF, 1 : ON
                if (EziMOTIONPlusRLib.FAS_ServoEnable(PortNo, (byte)iSlaveNo, OnOff) != EziMOTIONPlusRLib.FMM_OK)
                {
                    string strMsg = "FAS_ServoEnable() was failed : " + EziMOTIONPlusRLib.FMM_OK.ToString();
                    Global.Mlog.Info(strMsg);
                    return false;
                }
                else
                {
                    if (OnOff == 1)
                        Global.Mlog.Info($"Servo On. Slave No : {iSlaveNo}");
                    else
                        Global.Mlog.Info($"Servo Off. Slave No : {iSlaveNo}");

                    return true;
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                return false;
            }
        }
        public bool Stop(int iSlaveNo)
        {
            try
            {
                int nRtn = EziMOTIONPlusRLib.FAS_MoveStop(PortNo, (byte)iSlaveNo);
                if (nRtn != EziMOTIONPlusRLib.FMM_OK)
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
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                return false;
            }

        }
        public bool EmergencyStop(int iSlaveNo)
        {
            int nRtn = EziMOTIONPlusRLib.FAS_EmergencyStop(PortNo, (byte)iSlaveNo);
            if (nRtn != EziMOTIONPlusRLib.FMM_OK)
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
        public bool AlarmReset(int iSlaveNo)
        {
            int nRtn = EziMOTIONPlusRLib.FAS_ServoAlarmReset(PortNo, (byte)iSlaveNo);
            if (nRtn != EziMOTIONPlusRLib.FMM_OK)
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
        // Todo : 작업 이어서 해야함
        public bool MoveABS(byte Slave, double IPosition, uint Speed)
        {
            if (IsConnected == false)
                return false;

            int npos = Convert.ToInt32(IPosition * Pulse);

            int nRtn = EziMOTIONPlusRLib.FAS_MoveSingleAxisAbsPos
                (
                    PortNo,
                    Slave,
                    npos * Ratio,
                    (Speed * Convert.ToUInt16(Pulse))
                );

            if (nRtn != EziMOTIONPlusRLib.FMM_OK)
            {
                string strMsg = "FAS_MoveSingleAxisAbsPos() \nReturned: " + nRtn.ToString();
                return false;
            }
            else
                return true;
        }
    }
}
