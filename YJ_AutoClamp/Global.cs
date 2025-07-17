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
using System.Windows.Threading;
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
        public static ILog TTlog;
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
        public string IniAgingPath { get; set; } = Environment.CurrentDirectory + @"\Config\AGING";
        public string IniMesLogPath { get; set; } = Environment.CurrentDirectory + @"\MES";
        public string AlarmLogPath { get; set; } = Environment.CurrentDirectory + @"\Alarm";
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
        private double[] _AverageTactTime = new double[10];
        public double[] AverageTactTime
        {
            get { return _AverageTactTime; }
            set { SetValue(ref _AverageTactTime, value); }
        }

        private Global()
        {
            AverageTactTime = new double[10];
            // Date Timer
            ClockTimer.Interval = TimeSpan.FromSeconds(1);
            ClockTimer.Tick += new EventHandler(ClockTimer_Tick);
            ClockTimer.Start();
            // Log Set
            Mlog = CreateLog4NetLogger("Mlog");
            TTlog = CreateLog4NetLogger("TACTTIME");
            ExceptionLog = CreateLog4NetLogger("Exception");
            // Sequence Log Worker Start
            StartSequenceLogWorker();
            Mlog.Info($"---------- Software Start ----------");
        }
        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            NowDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

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
                        Global.ExceptionLog.Error($"SequenceLogThreadWorker - {ex.ToString()}");
                    }
                }
                else
                {
                    Thread.Sleep(100); // 큐가 비었으면 잠시 대기
                }
                Thread.Sleep(2);
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
            
            //if (!string.IsNullOrEmpty(SingletonManager.instance.Channel_Model[0].AgingCvTotalCount))
            //{
            //    count = Convert.ToInt32(SingletonManager.instance.Channel_Model[0].AgingCvTotalCount);
            //}
            //count += 1;
            //SingletonManager.instance.Channel_Model[0].AgingCvTotalCount = count.ToString();
        }
        public void AgingConveyerTotalCount()
        {
            int total = SingletonManager.instance.Aging_Model[0].TotalCount
                + SingletonManager.instance.Aging_Model[1].TotalCount
                + SingletonManager.instance.Aging_Model[2].TotalCount
                + SingletonManager.instance.Aging_Model[3].TotalCount
                + SingletonManager.instance.Aging_Model[4].TotalCount
                + SingletonManager.instance.Aging_Model[5].TotalCount;

            SingletonManager.instance.Channel_Model[0].AgingCvTotalCount = total.ToString();
            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "SYSTEM";
            myIni.Write("AGING_CV_COUNT", total.ToString(), section);
        }
        public void MES_LOG(string cn, string Result)
        {
            string Path = Global.instance.IniMesLogPath + $@"\{DateTime.Now.ToString("yyyyMMdd")}.ini";
            var myIni = new IniFile(Path);
            string section = "MES";
            string writedata = cn + " = " + Result;
            myIni.Write(DateTime.Now.ToString("HH:mm:ss:fff"), writedata, section);
        }
        public void LoadingTactTimeReset()
        {
            TactTimeStart = false;
            TactTimeSw.Reset();
            SingletonManager.instance.Channel_Model[0].TactTime = "0.0"; // 초기화 시 TactTime을 0.0으로 설정
        }
        public void LoadingTactTimeStart()
        {
            TactTimeSw.Restart();
            TactTimeStart = true;
        }
        public void LoadingTactTimeEnd()
        {
            if (TactTimeStart == true)
            {
                long elapsedMs = TactTimeSw.ElapsedMilliseconds;
                long minutes = elapsedMs / 60000;
                long seconds = (elapsedMs % 60000) / 1000;
                long milliseconds = elapsedMs % 1000;
                double tt = TactTimeSw.ElapsedMilliseconds / 1000.0;
                tt = Math.Round(tt, 1);
                SingletonManager.instance.Channel_Model[0].TactTime = $"{tt.ToString()}";//"{seconds:D2}:{milliseconds:D1}";
                AverageTacttimeUpdate(tt);
            }
        }
        public void AverageTacttimeUpdate()
        {
            // 배열에서 가장 오래된 값을 제거하고 새 값을 추가
            for (int i = AverageTactTime.Length - 1; i > 0; i--)
            {
                AverageTactTime[i] = AverageTactTime[i - 1];
            }
            AverageTactTime[0] = Convert.ToDouble(SingletonManager.instance.Channel_Model[0].TactTime);
            // 평균 계산
            double sum = 0;
            for (int i = 0; i < AverageTactTime.Length; i++)
            {
                sum += AverageTactTime[i];
            }
            double average = sum / AverageTactTime.Length;
            SingletonManager.instance.Channel_Model[0].AverageTactTime = average.ToString("F1");
        }
        public void WriteAlarmLog(string message, string section = "ALARM")
        {
            try
            {
                message.Replace("\r\n", " ");
                string logFile = Path.Combine(AlarmLogPath, $"{DateTime.Now:yyyyMMdd}.txt");

                string time = DateTime.Now.ToString("yyyyMMdd HH:mm:ss:fff");
                string logLine = $"{time},{message}";

                // 파일에 append
                File.AppendAllText(logFile, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Global.ExceptionLog.Error($"WriteAlarmLog - {ex.ToString()}");
            }
        }
        public void ShowMessagebox(string message, bool isError = true, bool buzzOn = false, bool Alarm = false)
        {
            try
            {
                // UI 쓰레드에서 동작하도록 보장
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (buzzOn == true)
                    {
                        SendMainUiLog(message, UiLogType.Error);
                        Mlog.Info($"Error Message : {message}");
                        Global.instance.Set_TowerLamp(Global.TowerLampType.Error);
                        SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.BUZZER, true);
                        var view = new MessageBox_View(message, isError);
                        view.ShowDialog();
                        Global.instance.Set_TowerLamp(Global.TowerLampType.Stop);
                        SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.BUZZER, false);
                    }
                    else if (Alarm == true)
                    {
                        SendMainUiLog(message, UiLogType.Error);
                        Mlog.Info($"Error Message : {message}");
                        //SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, true);
                        SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.BUZZER, true);
                        Thread.Sleep(1200);
                        SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.BUZZER, false);
                        var view = new MessageBox_View(message, isError);
                        view.ShowDialog();
                        //SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    }
                    else
                    {
                        var view = new MessageBox_View(message, isError);
                        view.Show();
                        
                    }
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
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, false);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, true);
                    break;
                case TowerLampType.Init:
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, true);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, true);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, true);
                    break;
                case TowerLampType.Stop:
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, true);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, false);
                    break;
                case TowerLampType.Error:
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, true);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, false);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, false);
                    break;
                case TowerLampType.Operator:
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, true);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, false);
                    break;
                case TowerLampType.InputStop:
                case TowerLampType.OutputStop:
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_RED, false);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW, true);
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, true);
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

                SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_STOP, false);
                SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_START, true);

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
        public void InspectionStop()
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
                SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_STOP, true);
                SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_START, false);

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
