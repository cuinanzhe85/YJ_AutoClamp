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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp
{
    public class Global : BindableAndDisposable
    {
        static public Global instance = new Global();

        // Log Set
        public static ILog Mlog;
        public static ILog ExceptionLog;

        private readonly ConcurrentQueue<Tuple<string, string>> SequenceLogQueue = new ConcurrentQueue<Tuple<string, string>>();
        private Task SequenceLogWorker;
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
        public string IniSystemPath { get; set; } = Environment.CurrentDirectory + @"\Config\SystemOperation.ini";
        public string IniVelocityPath { get; set; } = Environment.CurrentDirectory + @"\Config\Velocity.ini";
        public string IniTeachPath { get; set; } = Environment.CurrentDirectory + @"\Config\Teach";
        public string IniModelPath { get; set; } = Environment.CurrentDirectory + @"\Config\Model";
        public string IniSpecPath { get; set; } = Environment.CurrentDirectory + @"\Config\Spec";

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
