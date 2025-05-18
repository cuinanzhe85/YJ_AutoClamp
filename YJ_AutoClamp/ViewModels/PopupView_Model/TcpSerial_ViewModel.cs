using Common.Commands;
using Common.Managers;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

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
        private string _NfcPort;
        public string NfcPort
        {
            get { return _NfcPort; }
            set { SetValue(ref _NfcPort, value); }
        }
        private string _Port;
        public string Port
        {
            get { return _Port; }
            set { SetValue(ref _Port, value); }
        }
        public TcpSerial_ViewModel()
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string port in ports)
                PortNames.Add(port);

            BarCodePort = SingletonManager.instance.Barcode_Model.Port;
            NfcPort = SingletonManager.instance.Nfc_Model.Port;
        }
        private void OnSave_Command(object obj)
        {
            try
            {
                if (MessageBox.Show("Save Success.", "Save Success", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                {
                    return;
                }
                Global.Mlog.Info($"[USER] SerialPort 'Save' Button Click");
                var myIni = new IniFile(Global.instance.IniSystemPath);
                // Gocator Front,Rear Ip Set
                string Section = "SERIAL";
                // Serial : Bacorde, Label
                myIni.Write("BARCODE_PORT", BarCodePort, Section);
                Global.Mlog.Info(" BARCODE_PORT = " + BarCodePort);
                myIni.Write("NFC_PORT", NfcPort, Section);
                Global.Mlog.Info(" NFC_PORT = " + NfcPort);

                SingletonManager.instance.Barcode_Model.Port = BarCodePort;
                SingletonManager.instance.Nfc_Model.Port = NfcPort;
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
