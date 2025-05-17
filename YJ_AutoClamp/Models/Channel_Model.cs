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
        private Image _ImageSource;
        public Image ImageSource
        {
            get { return _ImageSource; }
            set { SetValue(ref _ImageSource, value); }
        }
        private string _TactTime;
        public string TactTime
        {
            get { return _TactTime; }
            set { SetValue(ref _TactTime, value); }
        }
        private string _Barcode;
        public string Barcode
        {
            get { return _Barcode; }
            set { SetValue(ref _Barcode, value); }
        }
        public bool IsJudgeOk { get; set; }
        public bool IsOutWait { get; set; }
        public bool IsSkip { get; set; }
        // Stopwatch 및 DispatcherTimer 추가
        private Stopwatch _Stopwatch;
        public Channel_Model(ChannelList channel)
        {
            this.Index = (int)channel;
            this.Channel = channel;
            this.Status = ChannelStatus.EMPTY;
            this.ImageSource = null;
            this.TactTime = string.Empty;
            this.Barcode = string.Empty;
            this.IsJudgeOk = false;
            this.IsOutWait = false;
            this.IsSkip = false;

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
