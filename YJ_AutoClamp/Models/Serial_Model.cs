using Common.Mvvm;
using System;
using System.IO.Ports;
using System.Security.Cryptography;
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

        public enum BcrIndex
        {
            bcr1,
            bcr2,
            bcr3,
            bcr4,
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
        public bool IsBcrReceived { get; set; } = false;
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

                if (PortName == "Label Print")
                    SerialPort.BaudRate = 9600;
                else
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
            IsBcrReceived = false;

            if (SerialPort.IsOpen == false)
                return;

            SerialPort.Write("+");
        }
        
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //시리얼 버터에 수신된 데이타를 ReceiveData 읽어오기
            int ReceiveData = SerialPort.ReadByte();
            if (ReceiveData > 0)
            {
                string Data = SerialPort.ReadExisting();
                string.Format("{0:X2}", ReceiveData);

                int index = Data.IndexOf("\r\n");
                if (index < 8)
                    return;

                Barcode = Data.Remove(index);
                IsBcrReceived = true;
            }
        }
        public void LabelPrint()
        {
            /*
            
             A<x>,<y>,<rotation>,<font>,<h-multiplier>,<v-multiplier>,<reverse>,<data>
             •	<x>: X 좌표 (0부터 시작)
             •	<y>: Y 좌표 (0부터 시작)
             •	<rotation>: 텍스트 회전 (0=0도, 1=90도, 2=180도, 3=270도)
             •	<font>: 글꼴 (1~5)
             •	<h-multiplier>: 텍스트 가로 확대 (1~8)
             •	<v-multiplier>: 텍스트 세로 확대 (1~8)
             •	<reverse>: 반전 여부 (N=정상, R=반전)
             •	<data>: 출력할 텍스트
              B<x>,<y>,<rotation>,<barcode-type>,<narrow-bar-width>,<wide-bar-width>,<height>,<human-readable>,<data>
             •	< x >: X 좌표
             •	< y >: Y 좌표
             •	< rotation >: 바코드 회전(0 = 0도, 1 = 90도, 2 = 180도, 3 = 270도)
             •	< barcode - type >: 바코드 유형(e.g., 1 = Code 39, 2 = Code 128)
             •	< narrow - bar - width >: 좁은 바의 너비
             •	< wide - bar - width >: 넓은 바의 너비
             •	< height >: 바코드 높이
             •	< human - readable >: 텍스트 표시 여부(B = 표시, N = 미표시)
             •	< data >: 바코드 데이터

            N
            A50,50,0,4,1,1,N,"Hello, Zebra!"
            B50,150,0,1,2,2,100,B,"1234567890"
            P1

             -> A50,50,0,4,1,1,N,"Hello, Zebra!"
             •	X=50, Y=50 위치에 텍스트 출력
             •	회전 없음 (0도)
             •	글꼴 4 사용
             •	가로/세로 확대 1배
             •	반전 없음
             •	출력 텍스트: "Hello, Zebra!"

             -> B50,150,0,1,2,2,100,B,"1234567890"
             •	X=50, Y=150 위치에 바코드 출력
             •	회전 없음 (0도)
             •	바코드 유형: Code 39 (1)
             •	좁은 바 너비: 2
             •	넓은 바 너비: 2
             •	바코드 높이: 100
             •	텍스트 표시 (B)
             •	바코드 데이터: "1234567890"
            */

            if (SerialPort.IsOpen == false)
                return;

            string model = "Test Model";                // 모델명
            string barcode = "Test Label Print JK";     // 바코드
            string process = "ScrewInsp";               // 공정명
            string jig = "1 JIG";                       // JIG
            string errorPoint = "None";                 // Error Point


            string Contents = string.Empty;
            Contents += "N\r\n";
            Contents += $"A100,10,0,1,1,1,N,\r{model}\r\r\n";
            Contents += $"A100,30,0,1,1,1,N,\r{barcode}\r\r\n";
            Contents += $"A100,60,0,1,1,1,N,\r{process}\r\r\n";
            Contents += $"A100,90,0,1,1,1,N,\r{jig}\r\r\n";
            Contents += $"A100,110,0,1,1,1,N,\r{DateTime.Now.ToString("%Y/%m/%d")}\r\r\n";
            Contents += $"A100,140,0,1,1,1,N,\r{DateTime.Now.ToString("%H:%M:%S")}\r\r\n";
            Contents += $"A100,170,0,1,1,1,N,\r{errorPoint}\r\r\n";
            Contents += "P1\r\n";

            SerialPort.WriteLine(Contents); // Ensure the correct variable is written to the serial port.
        }
    }
}
