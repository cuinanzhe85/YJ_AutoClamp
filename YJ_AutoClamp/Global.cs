using Common.Managers;
using Common.Mvvm;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Telerik.Windows.Controls.FileDialogs;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp
{
    public delegate void UiLogSignal(string content, Global.UiLogType type = Global.UiLogType.Info);
    public class Global : BindableAndDisposable
    {
        public event UiLogSignal UiLogSignal;
        static public Global instance = new Global();

        public enum UiLogType
        {
            Info,
            Error,
            Clear
        }
        public enum TowerLampType
        {
            Init,
            Start,
            Stop,
            Error,
            Operator,
            InputStop,
            OutputStop
        }
        // Log Set
        public static ILog Mlog;
        public static ILog ExceptionLog;

        private readonly ConcurrentQueue<Tuple<string, string>> SequenceLogQueue = new ConcurrentQueue<Tuple<string, string>>();
        private Task SequenceLogWorker;
        private bool _IsInspectionBusy = false;
        // Date Timer
        private DispatcherTimer ClockTimer { get; set; } = new DispatcherTimer();
        private CultureInfo cultureinfo { get; set; } = new CultureInfo("en-US");

        private string _NowDate;
        public string NowDate
        {
            get { return _NowDate; }
            set { SetValue(ref _NowDate, value); }
        }
        private string _Safety_NowDate;
        public string Safety_NowDate
        {
            get { return _Safety_NowDate; }
            set { SetValue(ref _Safety_NowDate, value); }
        }
        private string _Safety_NowTime;
        public string Safety_NowTime
        {
            get { return _Safety_NowTime; }
            set { SetValue(ref _Safety_NowTime, value); }
        }
        // Etc
        public string IniConfigPath { get; set; } = Environment.CurrentDirectory + @"\Config";
        public string IniSystemPath { get; set; } = Environment.CurrentDirectory + @"\Config\System.ini";
        public string IniVelocityPath { get; set; } = Environment.CurrentDirectory + @"\Config\Velocity.ini";
        public string IniTeachPath { get; set; } = Environment.CurrentDirectory + @"\Config\Teach\Teaching.ini";
        public string IniModelPath { get; set; } = Environment.CurrentDirectory + @"\Config\Model";
        public string IniSpecPath { get; set; } = Environment.CurrentDirectory + @"\Config\Spec";
        public string IniAgingPath { get; set; } = Environment.CurrentDirectory + @"\Aging";
        public string IniSequencePath { get; set; } = Environment.CurrentDirectory + @"\Config\Sequence.ini";

        private bool _BusyStatus = true;
        public bool BusyStatus
        {
            get { return _BusyStatus; }
            set { SetValue(ref _BusyStatus, value); }
        }
        private string _BusyContent = string.Empty;
        public string BusyContent
        {
            get { return _BusyContent; }
            set { SetValue(ref _BusyContent, value); }
        }
        private string _SafetyErrorMessage = string.Empty;
        public string SafetyErrorMessage
        {
            get { return _SafetyErrorMessage; }
            set { SetValue(ref _SafetyErrorMessage, value); }
        }
        Stopwatch TactTimeSw = new Stopwatch();
        private bool _TactTimeStart = false;
        public bool TactTimeStart
        {
            get { return _TactTimeStart; }
            set { SetValue(ref _TactTimeStart, value); }
        }
        private Global()
        {
            // Date Timer
            ClockTimer.Interval = TimeSpan.FromSeconds(1);
            ClockTimer.Tick += new EventHandler(ClockTimer_Tick);
            ClockTimer.Start();
            // Log Set
            Mlog = CreateLog4NetLogger("Mlog");
            ExceptionLog = CreateLog4NetLogger("Exception");
            // Sequence Log Worker Start
            StartSequenceLogWorker();
            Mlog.Info($"---------- Software Start ----------");
        }
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            NowDate = DateTime.Now.ToString("yyyy - MM - dd. HH : mm : ss");

            if(SingletonManager.instance.IsSafetyInterLock == true)
            {
                Safety_NowTime = DateTime.Now.ToString("HH:mm:ss");
                Safety_NowDate = DateTime.Now.ToString("ddd yyyy-MM-dd", cultureinfo);
            }
        }
        public void StartSequenceLogWorker()
        {
            if (SequenceLogWorker != null && !SequenceLogWorker.IsCompleted)
                return;

            SequenceLogWorker = Task.Run(() => SequenceLogThreadWorker());
        }
        public void Write_Sequence_Log(string key, string value)
        {
            SequenceLogQueue.Enqueue(Tuple.Create(key, value));
        }
        public void SequenceLogThreadWorker()
        {
            while (true)
            {
                if (SequenceLogQueue.TryDequeue(out var item))
                {
                    try
                    {
                        var key = item.Item1;
                        var value = item.Item2;

                        var myIni = new IniFile(Global.instance.IniSequencePath);
                        myIni.Write(key, value, "SEQUENCE");
                    }
                    catch (Exception ex)
                    {
                        ExceptionLog?.Error("Error writing Sequence log", ex);
                    }
                }
                else
                {
                    Thread.Sleep(100); // 큐가 비었으면 잠시 대기
                }
            }
        }
        public ILog CreateLog4NetLogger(string logname)
        {
            var hierarchy = new Hierarchy();

            var rollingFileAppender = new RollingFileAppender()
            {
                Name = logname,
                AppendToFile = true,
                File = string.Format(@"Logs\"),
                DatePattern = string.Format($"yyyyMMdd\\\\yyyyMMdd'_{logname}.log'"),
                StaticLogFileName = false,
                RollingStyle = RollingFileAppender.RollingMode.Date,
                Layout = new PatternLayout("%d %-5p - %m%n")
            };
            rollingFileAppender.ActivateOptions();

            hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.AddAppender(rollingFileAppender);
            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            ILoggerRepository repository = LogManager.CreateRepository(logname);
            BasicConfigurator.Configure(repository, rollingFileAppender);

            return LogManager.GetLogger(rollingFileAppender.Name, logname);
        }
        public void InputCountPlus()
        {
            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "SYSTEM";
            string valus = myIni.Read("INPUT_COUNT", section);
            int count=0;
            if (!string.IsNullOrEmpty(valus)) 
            {
                count = Convert.ToInt32(valus);
            }
            count += 1;
            SingletonManager.instance.Channel_Model[0].InputCount = count.ToString();
            myIni.Write("INPUT_COUNT", count.ToString(), section);
        }
        public void LoadCountPlus()
        {
            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "SYSTEM";
            string valus = myIni.Read("LOAD_COUNT", section);
            int count = 0;
            if (!string.IsNullOrEmpty(valus))
            {
                count = Convert.ToInt32(valus);
            }
            count += 1;
            SingletonManager.instance.Channel_Model[0].LoadCount = count.ToString();
            myIni.Write("LOAD_COUNT", count.ToString(), section);
        }
        public void LoadingTactTimeStart()
        {
            TactTimeSw.Restart();
        }
        public void LoadingTactTimeEnd()
        {
            if (TactTimeStart == true)
            {
                long elapsedMs = TactTimeSw.ElapsedMilliseconds;
                long minutes = elapsedMs / 60000;
                long seconds = (elapsedMs % 60000) / 1000;
                long milliseconds = elapsedMs % 1000;
                SingletonManager.instance.Channel_Model[0].TactTime = $"{minutes:D2}:{seconds:D2}:{milliseconds:D1}";
            }
        }
        public void ShowMessagebox(string message, bool isError = true)
        {
            try
            {
                // UI 쓰레드에서 동작하도록 보장
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var view = new MessageBox_View(message, isError);
                    view.Show();
                });
            }
            catch (Exception ex)
            {
                // 예외 처리
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
        public void SendMainUiLog(string content, UiLogType type = UiLogType.Info)
        {
            Application.Current.Dispatcher.BeginInvoke(
            (ThreadStart)(() =>
            {
                UiLogSignal?.Invoke(content, type);
            }), DispatcherPriority.Send);
        }
        public void Set_TowerLamp(TowerLampType type)
        {
            switch (type)
            {
                case TowerLampType.Start:
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, false);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, true);
                    break;
                case TowerLampType.Init:
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, true);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, true);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, true);
                    break;
                case TowerLampType.Stop:
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, true);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, false);
                    break;
                case TowerLampType.Error:
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, true);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, false);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, false);
                    break;
                case TowerLampType.Operator:
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, true);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, false);
                    break;
                case TowerLampType.InputStop:
                case TowerLampType.OutputStop:
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, true);
                    SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, true);
                    break;
            }
        }
        public async Task<bool> InspectionStart()
        {
            // 중복 호출 방지
            if (_IsInspectionBusy)
                return false;

            _IsInspectionBusy = true;

            try
            {
                // Set BusyStatus
                BusyContent = "Inspection Starting...";
                BusyStatus = true;

                // Tower Lamp Start
                Set_TowerLamp(TowerLampType.Start);


                // Inspection Thread Start
                SendMainUiLog($"Inspection Start [ {SingletonManager.instance.EquipmentMode} Mode ]");
                Mlog.Info($"{SingletonManager.instance.EquipmentMode.ToString()} Run Inspection Start.");

                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_STOP, false);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_START, true);

                SingletonManager.instance.IsInspectionStart = true;

                // Set BusyStatus
                BusyStatus = false;
                BusyContent = string.Empty;

                return true;
            }
            finally
            {
                BusyStatus = false;
                BusyContent = string.Empty;
                _IsInspectionBusy = false;
            }
        }
        public async void InspectionStop()
        {
            // 중복 호출 방지
            if (_IsInspectionBusy)
                return;

            _IsInspectionBusy = true;

            try
            {
                // 검사중지
                SendMainUiLog($"Inspection Stop [ {SingletonManager.instance.EquipmentMode} Mode ]");
                Mlog.Info($"{SingletonManager.instance.EquipmentMode.ToString()} Run Inspection Stop.");
                SingletonManager.instance.IsInspectionStart = false;

                // Tower Lamp Stop
                Set_TowerLamp(TowerLampType.Stop);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_STOP, true);
                SingletonManager.instance.Ez_Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_START, false);

            }
            finally
            {
                _IsInspectionBusy = false;
            }
        }
        #region // override
        protected override void DisposeManaged()
        {
            // ClockTimer 정지 및 해제
            if (ClockTimer != null)
            {
                ClockTimer.Stop();
                ClockTimer.Tick -= ClockTimer_Tick;
                ClockTimer = null;
            }

            // LogManager Shutdown
            LogManager.Shutdown();

            base.DisposeManaged();
        }
        #endregion
    }
}
