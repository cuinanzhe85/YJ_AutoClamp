using Common.Commands;
using Common.Managers;
using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Telerik.Windows.Data;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels
{
    public class AgingStep_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand Reset_Command { get; private set; }
        public ICommand Select_Command { get; private set; }
        #endregion
        public RadObservableCollection<Channel_Model> Channels
        {
            get { return SingletonManager.instance.Channel_Model; }
        }
        
        private Aging_Model _DisplayAgingCvList = new Aging_Model(0);
        public Aging_Model DisplayAgingCvList
        {
            get { return _DisplayAgingCvList; }
            set { SetValue(ref _DisplayAgingCvList, value); }
        }
        private string _TotalCount = "0";
        public string TotalCount
            {
            get { return _TotalCount; }
            set { SetValue(ref _TotalCount, value); }
        }
        private RadObservableCollection<string> _CvCount;
        public RadObservableCollection<string> CvCount
        {
            get { return _CvCount; }
            set { SetValue(ref _CvCount, value); }
        }
        private int _testStep = 0;
        private int _testCount = 0;
        private DispatcherTimer AgingTimer { get; set; } = new DispatcherTimer();
        public AgingStep_ViewModel()
        {
            CvCount = new RadObservableCollection<string>();
            for (int i=0; i<6; i++)
            {
                CvCount.Add("0");
            }

            AgingTimer.Interval = TimeSpan.FromSeconds(1);
            AgingTimer.Tick += new EventHandler(AgingTimer_Tick);
            AgingTimer.Start();

            DisplayAgingCvList = SingletonManager.instance.Aging_Model[0];
            InitializeCommands();
        }
        private void AgingTimer_Tick(object sender, EventArgs e)
        {
            for (int i=0; i<28; i++)
            {
                try
                {
                    if (DisplayAgingCvList.AgingList[i].AgingStartTimet != "")
                    {
                        DateTime AgingStartTime = DateTime.ParseExact(DisplayAgingCvList.AgingList[i].AgingStartTimet, "yyyyMMddHHmmss", null);
                        string now = DateTime.Now.ToString("yyyyMMddHHmmss");
                        DateTime nowTime = DateTime.ParseExact(now, "yyyyMMddHHmmss", null);
                        TimeSpan diff = nowTime - AgingStartTime;
                        DisplayAgingCvList.AgingList[i].AgingTime = diff.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Global.ExceptionLog.Info(ex.ToString());
                }
            }

            
            #region // test code
#if false
            switch (_testStep)
            {
                case 0:
                    SingletonManager.instance.Aging_Model[0].SetFirstLoadingFloor(1);
                    //DisplayAgingCvList = SingletonManager.instance.Aging_Model[0];
                    SingletonManager.instance.Aging_Model[1].SetFirstLoadingFloor(2);
                    SingletonManager.instance.Aging_Model[2].SetFirstLoadingFloor(3);
                    SingletonManager.instance.Aging_Model[3].SetFirstLoadingFloor(4); 
                    SingletonManager.instance.Aging_Model[4].SetFirstLoadingFloor(5);
                    SingletonManager.instance.Aging_Model[5].SetFirstLoadingFloor(3);
                    _testStep = 1;
                    break;
                case 1:
                    //DisplayAgingCvList = SingletonManager.instance.Aging_Model[0];
                    SingletonManager.instance.Aging_Model[0].AgingStepShift();
                    SingletonManager.instance.Aging_Model[1].AgingStepShift();
                    SingletonManager.instance.Aging_Model[2].AgingStepShift();
                    SingletonManager.instance.Aging_Model[3].AgingStepShift();
                    SingletonManager.instance.Aging_Model[4].AgingStepShift();
                    SingletonManager.instance.Aging_Model[5].AgingStepShift();
                    _testStep = 2;
                    break;
                case 2:
                    //DisplayAgingCvList = SingletonManager.instance.Aging_Model[0];
                    SingletonManager.instance.Aging_Model[0].AgingStepShift();
                    SingletonManager.instance.Aging_Model[1].AgingStepShift();
                    SingletonManager.instance.Aging_Model[2].AgingStepShift();
                    SingletonManager.instance.Aging_Model[3].AgingStepShift();
                    SingletonManager.instance.Aging_Model[4].AgingStepShift();
                    SingletonManager.instance.Aging_Model[5].AgingStepShift();
                    _testStep = 3;
                    break;
                case 3:
                    //DisplayAgingCvList = SingletonManager.instance.Aging_Model[0];
                    _testCount+=2;
                    _testStep = 0;
                    if (_testCount>10)
                    {
                        //_testCount = 0;
                        SingletonManager.instance.Aging_Model[0].AgingLastDelete();
                        SingletonManager.instance.Aging_Model[1].AgingLastDelete();
                        SingletonManager.instance.Aging_Model[2].AgingLastDelete();
                        SingletonManager.instance.Aging_Model[3].AgingLastDelete();
                        SingletonManager.instance.Aging_Model[4].AgingLastDelete();
                        SingletonManager.instance.Aging_Model[5].AgingLastDelete();

                        SingletonManager.instance.Aging_Model[0].AgingStepShift();
                        SingletonManager.instance.Aging_Model[1].AgingStepShift();
                        SingletonManager.instance.Aging_Model[2].AgingStepShift();
                        SingletonManager.instance.Aging_Model[3].AgingStepShift();
                        SingletonManager.instance.Aging_Model[4].AgingStepShift();
                        SingletonManager.instance.Aging_Model[5].AgingStepShift();
                    }
                    break; //1282
            }
#endif
            #endregion
            Global.instance.AgingConveyerTotalCount();
            TotalCount = SingletonManager.instance.Channel_Model[0].AgingCvTotalCount;
            CvCount[0] = SingletonManager.instance.Aging_Model[0].TotalCount.ToString();
            CvCount[1] = SingletonManager.instance.Aging_Model[1].TotalCount.ToString();
            CvCount[2] = SingletonManager.instance.Aging_Model[2].TotalCount.ToString();
            CvCount[3] = SingletonManager.instance.Aging_Model[3].TotalCount.ToString();
            CvCount[4] = SingletonManager.instance.Aging_Model[4].TotalCount.ToString();
            CvCount[5] = SingletonManager.instance.Aging_Model[5].TotalCount.ToString();
        }
        private void OnReset_Command(object obj)
        {
            if (MessageBox.Show($"Do you want to reset the production quantity?", "Product", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
            {
                return;
            }
            Channels[0].AgingCvTotalCount = "0";

            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "SYSTEM";
            myIni.Write("AGING_CV_COUNT", Channels[0].AgingCvTotalCount, section);

            SingletonManager.instance.Aging_Model[0].AgingCountReset();
            SingletonManager.instance.Aging_Model[1].AgingCountReset();
            SingletonManager.instance.Aging_Model[2].AgingCountReset();
            SingletonManager.instance.Aging_Model[3].AgingCountReset();
            SingletonManager.instance.Aging_Model[4].AgingCountReset();
            SingletonManager.instance.Aging_Model[5].AgingCountReset();
        }
        private void OnSelect_Command(object obj)
        {
            switch(obj.ToString())
            {
                case "Upper1":
                    DisplayAgingCvList = SingletonManager.instance.Aging_Model[0];
                    break;
                case "Upper2":
                    DisplayAgingCvList = SingletonManager.instance.Aging_Model[1];
                    break;
                case "Upper3":
                    DisplayAgingCvList = SingletonManager.instance.Aging_Model[2];
                    break;
                case "Low1":
                    DisplayAgingCvList = SingletonManager.instance.Aging_Model[3];
                    break;
                case "Low2":
                    DisplayAgingCvList = SingletonManager.instance.Aging_Model[4];
                    break;
                case "Low3":
                    DisplayAgingCvList = SingletonManager.instance.Aging_Model[5];
                    break;
            }
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            Reset_Command = new RelayCommand(OnReset_Command);
            Select_Command = new RelayCommand(OnSelect_Command);
        }
        protected override void DisposeManaged()
        {
            Reset_Command = null;
            Select_Command = null;
            if (AgingTimer != null)
            {
                AgingTimer.Stop();
                AgingTimer.Tick -= AgingTimer_Tick;
                AgingTimer = null;
            }
            
            base.DisposeManaged();
        }
        #endregion
    }
}
