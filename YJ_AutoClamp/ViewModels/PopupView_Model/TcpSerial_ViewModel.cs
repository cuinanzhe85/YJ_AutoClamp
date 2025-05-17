using Common.Commands;
using Common.Managers;
using Lmi3d.Zen.Io;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels
{
    public class TcpSerial_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand Save_Command { get; private set; }
        #endregion

        private ObservableCollection<string> _PortNames = new ObservableCollection<string>();
        public ObservableCollection<string> PortNames
        {
            get { return _PortNames; }
            set { SetValue(ref _PortNames, value); }
        }
        private string _BarCodePort;
        public string BarCodePort
        {
            get { return _BarCodePort; }
            set { SetValue(ref _BarCodePort, value); }
        }
        private string _LabelPrintPort;
        public string LabelPrintPort
        {
            get { return _LabelPrintPort; }
            set { SetValue(ref _LabelPrintPort, value); }
        }
        private string[] _MotionIP = new string[(int)ServoSlave_List.Max];
        public string[] MotionIP
        {
            get { return _MotionIP; }
            set { SetValue(ref _MotionIP, value); }
        }
        public TcpSerial_ViewModel()
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string port in ports)
                PortNames.Add(port);

            MotionIP = new string[(int)ServoSlave_List.Max];
        }
        private void OnSave_Command(object obj)
        {
            try
            {
                Global.Mlog.Info($"[USER] Tcp / SerialPort 'Save' Button Click");
                var myIni = new IniFile(Global.instance.IniSystemPath);
                // Gocator Front,Rear Ip Set
                string Section = "TCP";
                //myIni.Write("GOCATOR_FRONT_IP", Gocator_Front, Section);
                //myIni.Write("GOCATOR_REAR_IP", Gocator_Rear, Section);
                //Global.Mlog.Info(" Gocator Front Ip Address = " + Gocator_Front);
                //Global.Mlog.Info(" Gocator REar Ip Address = " + Gocator_Rear);
                // Serial : Bacorde, Label
                Section = "SERIAL";
                myIni.Write("BARCODE_PORT", BarCodePort, Section);
                Global.Mlog.Info(" Barcode Port = " + BarCodePort);
                myIni.Write("LABELPRINT_PORT", LabelPrintPort, Section);
                Global.Mlog.Info(" Label Print_PORT = " + LabelPrintPort);
                // Set Singleton Data
                //SingletonManager.instance.Gocator_Model[0].IpAddress = KIpAddress.Parse(Gocator_Front);
                //SingletonManager.instance.Gocator_Model[1].IpAddress = KIpAddress.Parse(Gocator_Rear);
                SingletonManager.instance.Barcode_Model.Port = BarCodePort;
                SingletonManager.instance.LabelPrint_Model.Port = LabelPrintPort;
                //for(int i=0; i<(int)ServoSlave_List.Max; i++)
                //{
                //    SingletonManager.instance.Ez_Model.IpAddress[i]= IPAddress.Parse(MotionIP[i]);
                //}
                MessageBox.Show("Save Success.", "Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                MessageBox.Show("Save Fail.", "Save Fail", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            Save_Command = new RelayCommand(OnSave_Command);
        }
        protected override void DisposeManaged()
        {
            Save_Command = null;
            base.DisposeManaged();
        }
        #endregion
    }
}
