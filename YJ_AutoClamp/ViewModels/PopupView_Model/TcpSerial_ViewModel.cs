using Common.Commands;
using Common.Managers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using YJ_AutoClamp.Models;
using static YJ_AutoClamp.Models.Serial_Model;

namespace YJ_AutoClamp.ViewModels
{
    public class TcpSerial_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand Save_Command { get; private set; }
        public ICommand BarCodeTest_Command { get; private set; }
        public ICommand Bcr_Command { get; private set; }
        #endregion

        private ObservableCollection<string> _PortNames = new ObservableCollection<string>();
        public ObservableCollection<string> PortNames
        {
            get { return _PortNames; }
            set { SetValue(ref _PortNames, value); }
        }
        private ObservableCollection<string> _bcrData = new ObservableCollection<string>();
        public ObservableCollection<string> bcrData
        {
            get { return _bcrData; }
            set { SetValue(ref _bcrData, value); }
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
        private string _NfcData;
        public string NfcData
        {
            get { return _NfcData; }
            set { SetValue(ref _NfcData, value); }
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

            bcrData.Add("Empty");
            BarCodePort = SingletonManager.instance.SerialModel[0].Port;
            NfcPort = SingletonManager.instance.SerialModel[1].Port;

        }
        private void OnSave_Command(object obj)
        {
            try
            {
                if (MessageBox.Show("Do you want to save the modification data?.", "Serial Setting Data", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
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

                SingletonManager.instance.SerialModel[0].Port = BarCodePort;

                myIni.Write("NFC_PORT", NfcPort, Section);
                Global.Mlog.Info(" NFC_PORT = " + NfcPort);

                SingletonManager.instance.SerialModel[1].Port = NfcPort;
            
            }
            catch(Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                Global.instance.ShowMessagebox("Save Fail.");
            }
        }
        private void OnBarCodeTest_Command(object obj)
        {
            SingletonManager.instance.SerialModel[0].SendBcrTrig();
        }
        private async void OnBcr_Command(object obj)
        {
            switch(obj.ToString())
            {
                case "BcrPortOpen":
                    SingletonManager.instance.SerialModel[0].PortName = "BARCODE_PORT";
                    SingletonManager.instance.SerialModel[0].Open();
                    break;
                case "BcrTest":
                    if (SingletonManager.instance.SerialModel[0].IsConnected != true)
                        return;
                    SingletonManager.instance.SerialModel[0].SendBcrTrig();
                    bcrData[0] = "";
                    await Task.Run(() =>
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Restart();
                        while (true)
                        {
                            if (SingletonManager.instance.SerialModel[0].IsReceived == true)
                            {
                                bcrData[0] = SingletonManager.instance.SerialModel[0].Barcode;
                                break;
                            }
                            if (sw.ElapsedMilliseconds > 1500)
                            {
                                Global.instance.ShowMessagebox("BCR Barcode read fail.");
                                break;
                            }
                        }

                        return Task.CompletedTask;
                    });
                    break;
                case "NfcPortOpen":

                    SingletonManager.instance.SerialModel[1].PortName = "NFC_PORT";
                    SingletonManager.instance.SerialModel[1].Open();
                    break;

                case "NfcTest":
                    await NFC_DataRead();
                    break;
                    
            }
            
        }
        private async Task NFC_DataRead()
        {
            if (SingletonManager.instance.SerialModel[(int)SerialIndex.Nfc].IsConnected != true)
                return;
            NfcData = "";
            await Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Restart();
                while (true)
                {
                    if (SingletonManager.instance.SerialModel[(int)SerialIndex.Nfc].IsReceived == true)
                    {
                        NfcData = SingletonManager.instance.SerialModel[(int)SerialIndex.Nfc].NfcData;
                        break;
                    }
                    if (sw.ElapsedMilliseconds > 10000)
                    {
                        MessageBox.Show("NFC read fail.", "NFC");
                        break;
                    }
                }
            });
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            Save_Command = new RelayCommand(OnSave_Command);
            BarCodeTest_Command = new RelayCommand(OnBarCodeTest_Command);
            Bcr_Command = new RelayCommand(OnBcr_Command);
        }
        protected override void DisposeManaged()
        {
            Save_Command = null;
            BarCodeTest_Command = null;
            base.DisposeManaged();
        }
        #endregion
    }
}
