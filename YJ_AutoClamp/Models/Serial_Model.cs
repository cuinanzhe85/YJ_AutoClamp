using Common.Managers;
using Common.Mvvm;
using System;
using System.IO.Ports;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace YJ_AutoClamp.Models
{
    public class Serial_Model : BindableAndDisposable
    {

        private string STX = string.Format("{0}", Convert.ToChar(0x02));
        private string ETX = string.Format("{0}", Convert.ToChar(0x03));
        private bool _IsConnected = false;

        public enum SerialIndex
        {
            bcr1,
            Nfc,
            Max
        }
        public bool IsConnected
        {
            get { return _IsConnected; }
            set { SetValue(ref _IsConnected, value); }
        }
        private string _PortName = string.Empty;
        public string PortName
        {
            get { return _PortName; }
            set { SetValue(ref _PortName, value); }
        }
        private string _Port = string.Empty;
        public string Port
        {
            get { return _Port; }
            set { SetValue(ref _Port, value); }
        }
        private string _Barcode = string.Empty;
        public string Barcode
        {
            get { return _Barcode; }
            set { SetValue(ref _Barcode, value); }
        }
        private string _NfcData = string.Empty;
        public string NfcData
        {
            get { return _NfcData; }
            set { SetValue(ref _NfcData, value); }
        }
        public bool IsReceived { get; set; } = false;
        public SerialPort SerialPort { get; set; } = null;
        public Serial_Model()
        {
            SerialPort = new SerialPort();
        }
        public bool Open()
        {
            try
            {
                if (SerialPort.IsOpen == true)
                {
                    SerialPort.Close();
                }
                SerialPort.PortName = Port;
                SerialPort.BaudRate = 115200;

                SerialPort.DataBits = 8;
                SerialPort.StopBits = StopBits.One;
                SerialPort.Parity = Parity.None;

                SerialPort.Open();  //시리얼포트 열기

                if (SerialPort.IsOpen == true)
                {
                    SerialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
                    Global.Mlog.Info($"{PortName} : {Port} Open Success - Connected");
                    IsConnected = true;
                }
            }
            catch
            {
                Global.Mlog.Info($"{PortName} : {Port} Open Fail - Disconnected");
                IsConnected = false;
                return false;
            }

            return true;
        }
        public void Close()
        {
            try
            {
                if (SerialPort != null && SerialPort.IsOpen == true)
                {
                    SerialPort.DataReceived -= new SerialDataReceivedEventHandler(SerialPort_DataReceived);
                    SerialPort.Close();
                    Global.Mlog.Info($"{PortName} : {Port} Close Success - Disconnected");
                    IsConnected = false;
                }
            }
            catch
            {
                Global.Mlog.Info($"{PortName} : {Port} Close Fail");
            }
        }
        public void SendBcrTrig()
        {
            Barcode = string.Empty;
            IsReceived = false;

            if (SerialPort.IsOpen == false)
                return;

            SerialPort.Write("+");
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //시리얼 버터에 수신된 데이타를 ReceiveData 읽어오기

            Thread.Sleep(20);
            string Data = SerialPort.ReadExisting();
            if (PortName.Contains("BARCODE"))
            {
                Barcode = Data.Trim();
                if (!string.IsNullOrEmpty(Barcode))
                {
                    IsReceived = true;
                }
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(Data))
                    {
                        string[] parts = Data.Split('=');
                        NfcData = parts[1].Trim();
                        IsReceived = true;
                    }
                }
                catch
                {
                    IsReceived = false;
                }
            }
        }
    }
}
