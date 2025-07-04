using Common.Mvvm;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Threading;

namespace YJ_AutoClamp.Models
{
    public enum ChannelList
    {
        J1,
        J2,
        J3,
        J4,
        Max
    }
    public enum ChannelStatus
    {
        EMPTY,
        READY,
        RUNNING,
        OK,
        NG,
        Max
    }
    public class Channel_Model : BindableAndDisposable
    {
        private int _Index;
        public int Index
        {
            get { return _Index; }
            set { SetValue(ref _Index, value); }
        }
        private ChannelList _Channel;
        public ChannelList Channel
        {
            get { return _Channel; }
            set { SetValue(ref _Channel, value); }
        }
        private ChannelStatus _Status;
        public ChannelStatus Status
        {
            get { return _Status; }
            set { SetValue(ref _Status, value); }
        }
        private string _TactTime = "0.0";
        public string TactTime
        {
            get { return _TactTime; }
            set { SetValue(ref _TactTime, value); }
        }
        private string _AverageTactTime = "0.0";
        public string AverageTactTime
        {
            get { return _AverageTactTime; }
            set { SetValue(ref _AverageTactTime, value); }
        }
        private string _Barcode;
        public string Barcode
        {
            get { return _Barcode; }
            set { SetValue(ref _Barcode, value); }
        }
        private string _InputCount;
        public string InputCount
        {
            get { return _InputCount; }
            set { SetValue(ref _InputCount, value); }
        }
        private string _LoadCount;
        public string LoadCount
        {
            get { return _LoadCount; }
            set { SetValue(ref _LoadCount, value); }
        }
        private string _CnNomber;
        public string CnNomber
        {
            get { return _CnNomber; }
            set { SetValue(ref _CnNomber, value); }
        }
        private string _MesResult;
        public string MesResult
        {
            get { return _MesResult; }
            set { SetValue(ref _MesResult, value); }
        }
        private string _AgingCvTotalCount;
        public string AgingCvTotalCount
        {
            get { return _AgingCvTotalCount; }
            set { SetValue(ref _AgingCvTotalCount, value); }
        }
        // Stopwatch 및 DispatcherTimer 추가
        private Stopwatch _Stopwatch;
        public Channel_Model(ChannelList channel)
        {
            this.Index = (int)channel;
            this.Status = ChannelStatus.EMPTY;
            this.TactTime = "0";
            this.AgingCvTotalCount = "0";
            this.Barcode = string.Empty;
            this.InputCount = "0";
            this.LoadCount = "0";
            this.AgingCvTotalCount = "0";

            // Stop watch 초기화
            _Stopwatch = new Stopwatch();
        }
        // Stopwatch 시작
        public void StartTactTime()
        {
            _Stopwatch.Restart();         // 기존 시간 초기화 후 시작
        }
        // Stopwatch 정지 및 TactTime 업데이트
        public void StopTactTime()
        {
            _Stopwatch.Stop();
        }
        public void GetTactTime()
        {
            TactTime = $"{_Stopwatch.Elapsed.TotalSeconds:F2} sec";
        }
        // Dispose에서 Timer 해제
        protected override void DisposeManaged()
        {
            // Stopwatch 정리 (필요 시)
            if (_Stopwatch != null)
            {
                _Stopwatch.Stop(); // 실행 중인 상태 멈춤
                _Stopwatch = null; // 참조 해제
            }

            base.DisposeManaged();
        }
    }
}
