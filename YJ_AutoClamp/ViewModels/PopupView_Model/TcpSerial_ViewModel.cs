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
        public ICommand Comport_Command { get; private set; }
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
        private string _MesPort;
        public string MesPort
        {
            get { return _MesPort; }
            set { SetValue(ref _MesPort, value); }
        }
        private string _MesData;
        public string MesData
        {
            get { return _MesData; }
            set { SetValue(ref _MesData, value); }
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
            MesPort = SingletonManager.instance.SerialModel[2].Port;

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

                myIni.Write("MES_PORT", MesPort, Section);
                Global.Mlog.Info(" MES_PORT = " + MesPort);
                SingletonManager.instance.SerialModel[2].Port = MesPort;

            }
            catch(Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                Global.instance.ShowMessagebox("Save Fail.");
            }
        }
        private async void OnComport_Command(object obj)
        {
            switch(obj.ToString())
            {
                case "BcrPortOpen":
                    SingletonManager.instance.SerialModel[(int)SerialIndex.bcr1].PortName = "BARCODE_PORT";
                    if (SingletonManager.instance.SerialModel[(int)SerialIndex.bcr1].Open() == true)
                        MessageBox.Show("BCR Port Open Success.");
                    else
                        MessageBox.Show("BCR Port Open Fail.");
                    break;
                case "BcrTest":
                    BcrTest();
                    break;
                case "NfcPortOpen":

                    SingletonManager.instance.SerialModel[(int)SerialIndex.Nfc].PortName = "NFC_PORT";
                    if (SingletonManager.instance.SerialModel[(int)SerialIndex.Nfc].Open() == true)
                        MessageBox.Show("NFC Port Open Success.");
                    else
                        MessageBox.Show("NFC Port Open Fail.");
                    break;

                case "NfcTest":
                    await NFC_DataRead();
                    break;
                case "MesPortOpen":
                    SingletonManager.instance.SerialModel[(int)SerialIndex.Mes].PortName = "MES_PORT";
                    if (SingletonManager.instance.SerialModel[(int)SerialIndex.Mes].Open() == true)
                        MessageBox.Show("MES Port Open Success.");
                    else
                        MessageBox.Show("MES Port Open Fail.");
                    break;
                case "MesTest":
                    MesTest();
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
        private async void BcrTest()
        {
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
        }
        private async void MesTest()
        {
            if (SingletonManager.instance.SerialModel[(int)SerialIndex.Mes].IsConnected != true)
                return;

            SingletonManager.instance.SerialModel[(int)SerialIndex.Mes].SendMes(MesData);
            await Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Restart();
                while (true)
                {
                    if (SingletonManager.instance.SerialModel[(int)SerialIndex.Mes].IsReceived == true)
                    {
                        Global.instance.ShowMessagebox($"MES Receive Data : {SingletonManager.instance.SerialModel[(int)SerialIndex.Mes].MesResult}");
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
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            Save_Command = new RelayCommand(OnSave_Command);
            Comport_Command = new RelayCommand(OnComport_Command);
        }
        protected override void DisposeManaged()
        {
            Save_Command = null;
            Comport_Command = null;
            base.DisposeManaged();
        }
        #endregion
    }
}
