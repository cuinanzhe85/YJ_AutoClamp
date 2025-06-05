using Common.Commands;
using Common.Managers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YJ_AutoClamp.Models;
using static YJ_AutoClamp.Models.Serial_Model;

namespace YJ_AutoClamp.ViewModels
{
    public class SystemData_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand Save_Command { get; private set; }
        public ICommand Manual_Command { get; private set; }
        #endregion

        private ObservableCollection<string> _UseNotUse = new ObservableCollection<string>();
        public ObservableCollection<string> UseNotUse
        {
            get { return _UseNotUse; }
            set { SetValue(ref _UseNotUse, value); }
        }
        private ObservableCollection<string> _AgingCvList = new ObservableCollection<string>();
        public ObservableCollection<string> AgingCvList
        {
            get { return _AgingCvList; }
            set { SetValue(ref _AgingCvList, value); }
        }
        private string _BcrUseNotuse;
        public string BcrUseNotuse
        {
            get { return _BcrUseNotuse; }
            set { SetValue(ref _BcrUseNotuse, value); }
        }
        private string _NfcUseNotuse;
        public string NfcUseNotuse
        {
            get { return _NfcUseNotuse; }
            set { SetValue(ref _NfcUseNotuse, value); }
        }
        private string[] _AgingCvNotUse;
        public string[] AgingCvNotUse
        {
            get { return _AgingCvNotUse; }
            set { SetValue(ref _AgingCvNotUse, value); }
        }
        private string _AgingCvSelected;
        public string AgingCvSelected
        {
            get { return _AgingCvSelected; }
            set { SetValue(ref _AgingCvSelected, value); }
        }
        private string _PickUpTimeOut;
        public string PickUpTimeOut
        {
            get { return _PickUpTimeOut; }
            set { SetValue(ref _PickUpTimeOut, value); }
        }
        private string _HttpSendData;
        public string HttpSendData
        {
            get { return _HttpSendData; }
            set { SetValue(ref _HttpSendData, value); }
        }
        private string _LoadFloorCount;
        public string LoadFloorCount
        {
            get { return _LoadFloorCount; }
            set { SetValue(ref _LoadFloorCount, value); }
        }
        private string _AgingCvStepTime;
        public string AgingCvStepTime
        {
            get { return _AgingCvStepTime; }
            set { SetValue(ref _AgingCvStepTime, value); }
        }
        public SystemData_ViewModel()
        {
            AgingCvNotUse = new string[6];
            UseNotUse.Add("NotUse");
            UseNotUse.Add("Use");
            AgingCvList.Add("Upper 1");
            AgingCvList.Add("Upper 2");
            AgingCvList.Add("Upper 3");
            AgingCvList.Add("Low 1");
            AgingCvList.Add("Low 2");
            AgingCvList.Add("Low 3");

            BcrUseNotuse = SingletonManager.instance.SystemModel.BcrUseNotUse;
            NfcUseNotuse = SingletonManager.instance.SystemModel.NfcUseNotUse;
            PickUpTimeOut = SingletonManager.instance.SystemModel.PickUpWaitTimeOutY.ToString();
            LoadFloorCount = SingletonManager.instance.SystemModel.LoadFloorCount.ToString();
            AgingCvStepTime = SingletonManager.instance.SystemModel.AgingCvStepTime.ToString();

            for (int i =0; i<6; i++)
            {
                AgingCvNotUse[i] = SingletonManager.instance.SystemModel.AgingCvNotUse[i];
            }
        }
        private void OnSave_Command(object obj)
        {
            try
            {
                if (MessageBox.Show("Do you want to save the modification data?.", "System Data", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                {
                    return;
                }
                Global.Mlog.Info($"[USER] SerialPort 'Save' Button Click");
                var myIni = new IniFile(Global.instance.IniSystemPath);
                // Gocator Front,Rear Ip Set
                string Section = "SYSTEM";
                // Serial : Bacorde, Label
                myIni.Write("BARCODE_USE", BcrUseNotuse, Section);
                Global.Mlog.Info(" BARCODE_USE = " + BcrUseNotuse);
                SingletonManager.instance.SystemModel.BcrUseNotUse = BcrUseNotuse;

                myIni.Write("NFC_USE", NfcUseNotuse, Section);
                Global.Mlog.Info(" NFC_USE = " + NfcUseNotuse);
                SingletonManager.instance.SystemModel.NfcUseNotUse = NfcUseNotuse;

                myIni.Write("PICKUP_TIMEOUT", PickUpTimeOut, Section);
                Global.Mlog.Info(" PICKUP_TIMEOUT = " + PickUpTimeOut);
                SingletonManager.instance.SystemModel.PickUpWaitTimeOutY = Convert.ToInt32(PickUpTimeOut);

                myIni.Write("LOAD_FLOOR_COUNT", LoadFloorCount, Section);
                Global.Mlog.Info(" LOAD_FLOOR_COUNT = " + LoadFloorCount);
                SingletonManager.instance.SystemModel.LoadFloorCount = Convert.ToInt32(LoadFloorCount);

                myIni.Write("AGING_CV_STEP_TIME", AgingCvStepTime, Section);
                Global.Mlog.Info(" AGING_CV_STEP_TIME = " + AgingCvStepTime);
                SingletonManager.instance.SystemModel.AgingCvStepTime = Convert.ToInt32(AgingCvStepTime);

                for (int i = 0; i < 6; i++)
                {
                    myIni.Write($"AGING_CV_NOTUSE_{i + 1}", AgingCvNotUse[i], Section);
                    Global.Mlog.Info($" AGING_CV_NOT_USE_{i + 1} = " + AgingCvNotUse[i]);
                    SingletonManager.instance.SystemModel.AgingCvNotUse[i] = AgingCvNotUse[i];
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                Global.instance.ShowMessagebox("Save Fail.");
            }
        }
        private async void OnManual_Command(object obj)
        {
            switch(obj.ToString())
            {
                case "StepRun":
                    AgingStepRun();
                    break;
                case "HTTP_TEST":
                    if (string.IsNullOrEmpty(HttpSendData))
                    {
                        HttpSendData = SingletonManager.instance.SerialModel[(int)SerialIndex.Nfc].NfcData;
                    }
                    if (!string.IsNullOrEmpty(HttpSendData))
                    {
                        SingletonManager.instance.HttpJsonModel.SendRequest("getPrevInspInfo", HttpSendData);
                        //SingletonManager.instance.HttpModel.GetprocCodeData("A5102289AG188");
                        Stopwatch sw = new Stopwatch();
                        await Task.Run(() =>
                        {
                            while (true)
                            {
                                if (SingletonManager.instance.HttpJsonModel.DataSendFlag == true)
                                {
                                    Global.instance.ShowMessagebox($"HTTP Response ResultCode: {SingletonManager.instance.HttpJsonModel.ResultCode}");
                                    break;
                                }
                                if (sw.ElapsedMilliseconds > 5000) // 5초
                                {
                                    Global.instance.ShowMessagebox($"HTTP Data Receive Timeout.");
                                    break;
                                }
                                Thread.Sleep(100); // 100ms
                            }
                        });
                    }
                    break;
            }
        }
        private void AgingStepRun()
        {
            Stopwatch sw = Stopwatch.StartNew();

            AgingCvRunStop(true);
            sw.Restart();
            while (true)
            {
                if (sw.ElapsedMilliseconds > SingletonManager.instance.SystemModel.AgingCvStepTime) // ms
                {
                    AgingCvRunStop(false);
                    break;
                }
            }
        }
        void AgingCvRunStop(bool runStop)
        {
            if (AgingCvSelected == "Upper 1")
            {
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_1, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_2, runStop);
            }
            else if (AgingCvSelected == "Upper 2")
            {
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_2, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_2, runStop);
            }
            else if (AgingCvSelected == "Upper 3")
            {
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_3, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_2, runStop);
            }
            else if (AgingCvSelected == "Low 1")
            {
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_1, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_2, runStop);
            }
            else if (AgingCvSelected == "Low 2")
            {
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_2, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_2, runStop);
            }
            else if (AgingCvSelected == "Low 3")
            {
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.LIFT_CV_RUN_3, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1, runStop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_2, runStop);
            }
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            Save_Command = new RelayCommand(OnSave_Command);
            Manual_Command = new RelayCommand(OnManual_Command);
        }
        protected override void DisposeManaged()
        {
            Save_Command = null;
            Manual_Command = null;
            base.DisposeManaged();
        }
        #endregion
    }
}
