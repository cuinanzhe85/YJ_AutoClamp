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
        #endregion

        private ObservableCollection<string> _UseNotUse = new ObservableCollection<string>();
        public ObservableCollection<string> UseNotUse
        {
            get { return _UseNotUse; }
            set { SetValue(ref _UseNotUse, value); }
        }
        private ObservableCollection<string> _AgingCvUseNotUseList = new ObservableCollection<string>();
        public ObservableCollection<string> AgingCvUseNotUseList
        {
            get { return _AgingCvUseNotUseList; }
            set { SetValue(ref _AgingCvUseNotUseList, value); }
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
        private string _AgingCvNotUse;
        public string AgingCvNotUse
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
            UseNotUse.Add("NotUse");
            UseNotUse.Add("Use");
            AgingCvList.Add("Upper 1");
            AgingCvList.Add("Upper 2");
            AgingCvList.Add("Upper 3");
            AgingCvList.Add("Low 1");
            AgingCvList.Add("Low 2");
            AgingCvList.Add("Low 3");
            AgingCvUseNotUseList.Add("All");
            AgingCvUseNotUseList.Add("Upper");
            AgingCvUseNotUseList.Add("Low");

            BcrUseNotuse = SingletonManager.instance.SystemModel.BcrUseNotUse;
            NfcUseNotuse = SingletonManager.instance.SystemModel.NfcUseNotUse;
            PickUpTimeOut = SingletonManager.instance.SystemModel.PickUpWaitTimeOutY.ToString();
            LoadFloorCount = SingletonManager.instance.SystemModel.LoadFloorCount.ToString();
            AgingCvStepTime = SingletonManager.instance.SystemModel.AgingCvStepTime.ToString();
            AgingCvNotUse = SingletonManager.instance.SystemModel.AgingCvNotUse;
        }
        private void OnSave_Command(object obj)
        {
            try
            {
                if (MessageBox.Show("Do you want to save the modification data?.", "System Manager", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                {
                    return;
                }
                Global.Mlog.Info($"[USER] SerialPort 'Save' Button Click");
                var myIni = new IniFile(Global.instance.IniSystemPath);
                // Gocator Front,Rear Ip Set
                string Section = "SYSTEM";
                // Serial : Bacorde, Label
                //myIni.Write("BARCODE_USE", BcrUseNotuse, Section);
                //Global.Mlog.Info(" BARCODE_USE = " + BcrUseNotuse);
                //SingletonManager.instance.SystemModel.BcrUseNotUse = BcrUseNotuse;

                myIni.Write("NFC_USE", NfcUseNotuse, Section);
                Global.Mlog.Info(" NFC_USE = " + NfcUseNotuse);
                SingletonManager.instance.SystemModel.NfcUseNotUse = NfcUseNotuse;
                SingletonManager.instance.Channel_Model[0].MesResult = NfcUseNotuse;

                myIni.Write("PICKUP_TIMEOUT", PickUpTimeOut, Section);
                Global.Mlog.Info(" PICKUP_TIMEOUT = " + PickUpTimeOut);
                SingletonManager.instance.SystemModel.PickUpWaitTimeOutY = Convert.ToInt32(PickUpTimeOut);

                int floorcount = Convert.ToInt32(LoadFloorCount);
                if (floorcount > 5)
                    LoadFloorCount = "5";
                myIni.Write("LOAD_FLOOR_COUNT", LoadFloorCount, Section);
                Global.Mlog.Info(" LOAD_FLOOR_COUNT = " + LoadFloorCount);
                SingletonManager.instance.SystemModel.LoadFloorCount = Convert.ToInt32(LoadFloorCount);

                myIni.Write("AGING_CV_STEP_TIME", AgingCvStepTime, Section);
                Global.Mlog.Info(" AGING_CV_STEP_TIME = " + AgingCvStepTime);
                SingletonManager.instance.SystemModel.AgingCvStepTime = Convert.ToInt32(AgingCvStepTime);

                myIni.Write($"AGING_CV_USE", AgingCvNotUse, Section);
                Global.Mlog.Info($" AGING_CV_USE = " + AgingCvNotUse);
                SingletonManager.instance.SystemModel.AgingCvNotUse = AgingCvNotUse;
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Info(e.ToString());
                Global.instance.ShowMessagebox("Save Fail.");
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
