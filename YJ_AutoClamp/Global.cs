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
        public enum MesLogType
        {
            DGS,
            EDM
        }
        public enum EdmLogList
        {
            None,
            InspectionStart = 9000,
            InspectionStop,
            JudePass_Wait = 9002,
            JudePass = 9002,
            JudgeNg,
            InspectionInput,
            ChangeLamp = 9009,
            JigStatus = 9020,
            SetCount = 9200,
            // EQP EDM ERROR
            E_Stop = 1000,
            Door_Open = 1100,
        }
        // Log Set
        public static ILog Mlog;
        public static ILog TTlog;
        public static ILog ExceptionLog;

        private readonly ConcurrentQueue<Tuple<string, string>> SequenceLogQueue = new ConcurrentQueue<Tuple<string, string>>();
        private Task SequenceLogWorker;
        private bool _IsInspectionBusy = false;
        private Task MesLogWorker;
        private readonly ConcurrentQueue<Tuple<Channel_Model, DateTime, MesLogType, EdmLogList, string>> MesLogQueue;
        public TowerLampType towerLampOld = TowerLampType.Init;
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
        private string _DepartmentName = "SmartFactory Group(MX)";
        public string DepartmentName
        {
            get { return _DepartmentName; }
            set { SetValue(ref _DepartmentName, value); }
        }
        private string _SoftwareName = "AUTO CLAMP";
        public string SoftwareName
        {
            get { return _SoftwareName; }
            set { SetValue(ref _SoftwareName, value); }
        }
        private string _SoftwareVersion = "260723.2";
        public string SoftwareVersion
        {
            get { return _SoftwareVersion; }
            set { SetValue(ref _SoftwareVersion, value); }
        }
        // Etc
        public string IniConfigPath { get; set; } = Environment.CurrentDirectory + @"\Config";
        public string IniSystemPath { get; set; } = Environment.CurrentDirectory + @"\Config\System.ini";
        public string IniVelocityPath { get; set; } = Environment.CurrentDirectory + @"\Config\Velocity.ini";
        public string IniTeachPath { get; set; } = Environment.CurrentDirectory + @"\Config\Teach";
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
            MesLogQueue = new ConcurrentQueue<Tuple<Channel_Model, DateTime, MesLogType, EdmLogList, string>>();
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

            // GMES Log Worker Start
            StartMesLogWorker();
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
            Write_Mes_Log(null,MesLogType.EDM, EdmLogList.InspectionInput);
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
                AverageTacttimeUpdate();
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
        public bool ShowMessagebox(string message, bool isError = true, bool buzzOn = false, bool Alarm = false,bool IsYesNo = false)
        {
            try
            {
                if (IsYesNo == false)
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
                            SingletonManager.instance.Dio.BuzzerOnOff(3500);
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
                else
                {
                    bool? result = Application.Current.Dispatcher.Invoke(() =>
                    {
                        var msgBox = new MessageBoxYesNo_View(message);
                        return msgBox.ShowDialog();

                    });
                    if (result == true)
                    {
                        // Yes 선택 시
                        return true;
                    }
                    else
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                // 예외 처리
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return false;
            }
        }
        public void StartMesLogWorker()
        {
            if (MesLogWorker != null && !MesLogWorker.IsCompleted)
                return;

            MesLogWorker = Task.Run(() => MesLogThreadWorker());
        }
        public void MesLogThreadWorker()
        {
            while (true)
            {
                if (MesLogQueue.TryDequeue(out var item))
                {
                    var channel = item.Item1;
                    var logTime = item.Item2;
                    var logType = item.Item3;
                    var errorcode = item.Item4;
                    var extra = item.Item5;

                    if (logType == MesLogType.EDM)
                        WriteEdmLog(channel, logTime, errorcode, extra);
                }
                else
                {
                    Thread.Sleep(100); // 큐가 비었으면 잠시 대기
                }
            }
        }
        public void Write_Mes_Log(Channel_Model channel, MesLogType logType, EdmLogList errorcode)
        {
            string extra = string.Empty;

            // EDM Log
            if (logType == MesLogType.EDM)
            {
                if (SingletonManager.instance.SystemModel.IsEdmUse == false)
                    return;
                // Auto Mode 일때만 저장
                if (SingletonManager.instance.EquipmentMode != EquipmentMode.Auto)
                    return;

                // Set Extra Data
                extra = Set_EdmExtra(channel, errorcode);
            }

            MesLogQueue.Enqueue(Tuple.Create(channel, DateTime.Now, logType, errorcode, extra));
        }
        private void WriteEdmLog(Channel_Model channel, DateTime logTime, EdmLogList errorcode, string extra)
        {
            try
            {
                string filedata = string.Empty;
                string ngname = string.Empty;
                int notuse = 0;
                int jig = 0;

                // Set Jig Number
                if (channel != null)
                {
                    jig = channel.Index;
                }

                // Set Not Use Count
                for (int i = 0; i < (int)ChannelList.Max; i++)
                {
                    
                }
                if (errorcode == EdmLogList.JudePass)
                {
                    //C:\FA\LOG\,20230522180725321,9002,04051428,RAD12-F-V3_1.230522.27,,,,,.txt
                    filedata += $",";                                        // 1. 설비 Station 구분자
                    filedata += $"{logTime.ToString("yyyyMMddHHmmssfff")},";// 2. 날짜시간 17자리 yyyyMMddHHmmfff
                    filedata += $"{(int)errorcode},";                       // 3. Event Code
                    filedata += $"00000{notuse}02,";                        // 4. Port Status 8자리. 12 : Pack Block Count, 34 Block Count, 56 Not Use, Total Port
                    filedata += $"{SoftwareName}_{SoftwareVersion},";       // 5. Software Version
                    filedata += $"0,";                                       // 6. 0: Pass, 1: 대기
                    filedata += $",";                                       // 7. Extra Value
                    filedata += $",";                                       // 8. 예비필드
                    filedata += $",";                                       // 9. 기종구분
                }
                if (errorcode == EdmLogList.JudePass_Wait)
                {
                    filedata += $",";                                         // 1. 설비 Station 구분자
                    filedata += $"{logTime.ToString("yyyyMMddHHmmssfff")},"; // 2. 날짜시간 17자리 yyyyMMddHHmmfff
                    filedata += $"{(int)errorcode},";                        // 3. Event Code
                    filedata += $"00000{notuse}02,";                         // 4. Port Status 8자리. 12 : Pack Block Count, 34 Block Count, 56 Not Use, Total Port
                    filedata += $"{SoftwareName}_{SoftwareVersion},";        // 5. Software Version
                    filedata += $"1,";                                        // 6. 0: Pass, 1: 대기
                    filedata += $",";                                        // 7. Extra Value
                    filedata += $",";                                        // 8. 예비필드
                    filedata += $",";                                        // 9. 기종구분
                }
                else if (errorcode == EdmLogList.JudgeNg || errorcode == EdmLogList.JigStatus)
                {
                    filedata += $",";                                      // 설비 Station 구분자
                    filedata += $"{logTime.ToString("yyyyMMddHHmmssfff")},"; // 날짜시간 15자리 yyyyMMddHHmmfff
                    filedata += $"{(int)errorcode},";                      // Error Code
                    filedata += $"00000{notuse}02,";                       // Port Status 8자리. 12 : Pack Block Count, 34 Block Count, 56 Not Use, Total Port
                    filedata += $"{SoftwareName}_{SoftwareVersion},";      // Software Version
                    filedata += $"{jig.ToString("D2")},";                  // Jig 번호 ※ 이벤트 코드9003 인 경우 해당 정의 참조
                    filedata += $"{extra},";                               // Extra Value
                    filedata += $"TOPxx,";    // 예비필드 ※ 이벤트 코드9003, 9020 인 경우 해당 정의 참조
                    filedata += $",";                                       // 기종구분 ※ 이벤트 코드9003, 9020 인 경우 해당 정의 참조 
                }
                else
                {
                    filedata += $",";                                      // 설비 Station 구분자
                    filedata += $"{logTime.ToString("yyyyMMddHHmmssfff")},"; // 날짜시간 15자리 yyyyMMddHHmmfff
                    filedata += $"{(int)errorcode},";                      // Error Code
                    filedata += $"00000{notuse}02,";                       // Port Status 8자리. 12 : Pack Block Count, 34 Block Count, 56 Not Use, Total Port
                    filedata += $"{SoftwareName}_{SoftwareVersion},";      // Software Version
                    filedata += $",";                                       // 예비필드 ※ 이벤트 코드9003, 9020 인 경우 해당 정의 참조
                    filedata += $"{extra},";                               // Extra Value
                    filedata += $",";                                       // 예비필드 ※ 이벤트 코드9003, 9020 인 경우 해당 정의 참조
                    filedata += $",";                                       // 기종구분 ※ 이벤트 코드9003, 9020 인 경우 해당 정의 참조 
                }

                string directoryPath = $@"C:\FA\LOG";
                string fileName = string.Format(
                    @"{0}\{1}.txt",
                    directoryPath,
                    filedata
                );
                // 폴더가 없으면 생성
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                // 파일생성
                File.WriteAllText(fileName, string.Empty);
            }
            catch (Exception ex)
            {
                ExceptionLog?.Error("Error writing EDM log", ex);
            }
        }
        public string Set_EdmExtra(Channel_Model channel, EdmLogList errorcode)
        {
            string extra = string.Empty;

            if (errorcode == EdmLogList.ChangeLamp)
            {
                // Lamp Change Delay
                Thread.Sleep(100);

                extra = SingletonManager.instance.Dio.DI_RAW_DATA[(int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN] == false ? "0" : "1";
                extra += SingletonManager.instance.Dio.DI_RAW_DATA[(int)EziDio_Model.DO_MAP.TOWER_LAMP_YELLOW] == false ? "0" : "1";
                extra += SingletonManager.instance.Dio.DI_RAW_DATA[(int)EziDio_Model.DO_MAP.TOWER_LAMP_RED] == false ? "0" : "1";
            }
            else if (errorcode == EdmLogList.JigStatus)
            {
                if (channel != null)
                {
                    extra = "";
                }
            }
            else if (errorcode == EdmLogList.SetCount)
            {
                int totalcount = 0;
                extra = totalcount.ToString();
            }
            else if (errorcode == EdmLogList.InspectionStart)
            {
                extra = ((int)SingletonManager.instance.EquipmentMode).ToString();
            }
            else if (errorcode < EdmLogList.InspectionStart || errorcode == EdmLogList.JudgeNg)
            {
                extra = ((EdmLogList)errorcode).ToString();
            }
            return extra;
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
                    SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOWER_LAMP_GREEN, false);
                    break;
            }
            if (towerLampOld != type)
            {
                towerLampOld = type;
                Write_Mes_Log(null, MesLogType.EDM, EdmLogList.ChangeLamp);
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
                Global.instance.Write_Mes_Log(null, MesLogType.EDM, EdmLogList.InspectionStart);

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

                Global.instance.Write_Mes_Log(null, MesLogType.EDM, EdmLogList.InspectionStop);
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
