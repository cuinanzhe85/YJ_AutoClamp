using Common.Mvvm;

namespace YJ_AutoClamp.Models
{
    public class System_Model : BindableAndDisposable
    {
        public string _BcrUseNotUse;
        public string BcrUseNotUse
        {
            get { return _BcrUseNotUse; }
            set { SetValue(ref _BcrUseNotUse, value); }
        }
        public string _NfcUseNotUse;
        public string NfcUseNotUse
        {
            get { return _NfcUseNotUse; }
            set { SetValue(ref _NfcUseNotUse, value); }
        }
        public int _PickUpWaitTimeOutY =0;
        public int PickUpWaitTimeOutY
        {
            get { return _PickUpWaitTimeOutY; }
            set { SetValue(ref _PickUpWaitTimeOutY, value); }
        }
        public int _LoadFloorCount = 0;
        public int LoadFloorCount
        {
            get { return _LoadFloorCount; }
            set { SetValue(ref _LoadFloorCount, value); }
        }
        public int _AgingCvStepTime = 0;
        public int AgingCvStepTime
        {
            get { return _AgingCvStepTime; }
            set { SetValue(ref _AgingCvStepTime, value); }
        }
        public string _AgingCvNotUse;
        public string AgingCvNotUse
        {
            get { return _AgingCvNotUse; }
            set { SetValue(ref _AgingCvNotUse, value); }
        }
        public bool _IsEdmUse;
        public bool IsEdmUse
        {
            get { return _IsEdmUse; }
            set { SetValue(ref _IsEdmUse, value); }
        }
        public bool _IsNoneSetTest;
        public bool IsNoneSetTest
        {
            get { return _IsNoneSetTest; }
            set { SetValue(ref _IsNoneSetTest, value); }
        }
        public int _ClampCvStopDelay;
        public int ClampCvStopDelay
        {
            get { return _ClampCvStopDelay; }
            set { SetValue(ref _ClampCvStopDelay, value); }
        }
        public System_Model()
        {
        }
       
    }
}
