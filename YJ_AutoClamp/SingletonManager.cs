using Common.Managers;
using Common.Mvvm;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Telerik.Windows.Data;
using YJ_AutoClamp.Models;
using YJ_AutoClamp.Utils;
using YJ_AutoClamp.ViewModels;
using static YJ_AutoClamp.Models.EziDio_Model;
using static YJ_AutoClamp.Models.Serial_Model;

namespace YJ_AutoClamp
{
    public enum EquipmentMode
    {
        Auto,
        Dry
    }
    public class SingletonManager : BindableAndDisposable
    {
        static public SingletonManager instance = new SingletonManager();
        // Background Thread 관련
        private BackgroundWorker UnitsProcThread;
        public bool IsWorkingUnitsProcThread = true;
        public bool IsSafetyInterLock = false;
        private bool _IsInspectionStart = false;
        public bool IsInspectionStart
        {
            get {  return _IsInspectionStart; }
            set { SetValue(ref _IsInspectionStart, value); }
        }
        public bool IsInspectionInputStop = false;
        private msgQueue SequenceQueue; // Sequence 관련 Q

        #region // 나중에 정리해야할 변수들
        // Out Y축 이동시 사용되는 변수
        private bool _IsY_PickupColl = false;
        public bool IsY_PickupColl
        {
            get { return _IsY_PickupColl; }
            set { SetValue(ref _IsY_PickupColl, value); }
        }


        private bool isServerThreadRun = false;
        private bool isClientThreadRun = false;
        private List<TcpClient> tcpClientList;
        private List<ItemClient> clientItemList;
        private bool _IsServerOpen = false;
        public bool IsServerOpen
        {
            get { return _IsServerOpen; }
            set { SetValue(ref _IsServerOpen, value); }
        }
        // Interface Client 속성
        private bool _IsIF_Connected_1 = false;
        public bool IsIF_Connected_1
        {
            get { return _IsIF_Connected_1; }
            set { SetValue(ref _IsIF_Connected_1, value); }
        }

        public ObservableCollection<int> LoadFloor { get; set; }
        public ObservableCollection<Lift_Model> Display_Lift { get; set; }
        public ObservableCollection<Aging_Model> Aging_Model { get; set; }
        public int LoadStageNo = 0;
        public int LoadAgingCvIndex = 0;
        public int AgingCvIndex = 0;
        public bool BottomClampDone = false;
        public bool ClampResult = false;
        public bool UnitLastPositionSet = false;
        
        // 7단 Loading완료 변수 
        public bool[] LoadComplete = { false, false, false, false, false, false };
        public string Nfc_Data = string.Empty;
        #endregion

        // Default Infomation
        public EquipmentMode EquipmentMode { get; set; } = EquipmentMode.Auto;

        #region // Properties
        public EziDio_Model Dio { get; set; }
        public EzMotion_Model_E Motion { get; set; }
        public Serial_Model[] SerialModel { get; set; }
        public RadObservableCollection<Unit_Model> Unit_Model { get; set; }
        public RadObservableCollection<Servo_Model> Servo_Model { get; set; }
        public RadObservableCollection<Channel_Model> Channel_Model { get; set; }
        public Dictionary<string, double> Teaching_Data { get; set; }
        public System_Model SystemModel { get; set; }

        #endregion
        #region // UI Properties
        public RadObservableCollection<bool> DisplayUI_Dio { get; set; }

        #endregion
        private SingletonManager()
        {
            UnitsProcThread = new BackgroundWorker();
            SequenceQueue = new msgQueue();
            Unit_Model = new RadObservableCollection<Unit_Model>();
            Servo_Model = new RadObservableCollection<Servo_Model>();
            Motion = new EzMotion_Model_E();
            Dio = new EziDio_Model();
            DisplayUI_Dio = new RadObservableCollection<bool>();
            Channel_Model = new RadObservableCollection<Channel_Model>();
            Teaching_Data = new Dictionary<string, double>();
            SerialModel = new Serial_Model[(int)SerialIndex.Max];
            SystemModel = new System_Model();
            tcpClientList = new List<TcpClient>();
            clientItemList = new List<ItemClient>();

            // Lift Data
            LoadFloor = new ObservableCollection<int>();
            for (int i = 0; i < 3; i++)
                LoadFloor.Add(0);
            // Lift Status Display
            Display_Lift = new ObservableCollection<Lift_Model>();
            for (int i = 0; i < 3; i++)
                Display_Lift.Add(new Lift_Model("LIFT " + (i + 1)));

            Aging_Model = new ObservableCollection<Aging_Model>();
            for (int i = 0; i < 6; i++)
                Aging_Model.Add(new Aging_Model(i));

            for (int i = 0; i < (int)SerialIndex.Max; i++)
            {
                SerialModel[i] = new Serial_Model();
            }

            // Channel Model 초기화
            for (int i = 0; i < (int)ChannelList.Max; i++)
            {
                Channel_Model.Add(new Channel_Model((ChannelList)i));
            }
        }
        public void Run()
        {
            // Load System Files.
            LoadSystemFiles();
            // Unit & Servo Init 
            Unit_Init();
            // Motion Init
            Motion_Init();
            // Dio Init
            DioBoard_Init();
            // Serial Port Init : Barcode, Label Print
            SerialPort_init();
            // Load Teaching Data
            LoadTeachFile();
            // Load Velocity Data
            LoadVelocityFiles();
            // Background Thread Start
            BackgroundThread_Init();
            //TCP Server Start
            InterfaceServerStart();
        }
        private void LoadSystemFiles()
        {
            //BusyContent
            Global.instance.BusyContent = "System Operation Loading...";

            // Config 폴더 경로 설정
            string configPath = Global.instance.IniConfigPath;

            // Teach 폴더 경로 설정
            string teachFolder = Path.Combine(configPath, "Teach");
            string agingFolder = Global.instance.IniAgingPath;
            string mesLogFolder = Global.instance.IniMesLogPath;
            string AlarmLogFolder = Global.instance.AlarmLogPath;
            // Config 폴더가 없으면 생성
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);
            // Teach 폴더가 없으면 생성
            if (!Directory.Exists(teachFolder))
                Directory.CreateDirectory(teachFolder);
            //Aging 폴거가 없으면 생성
            if (!Directory.Exists(agingFolder))
                Directory.CreateDirectory(agingFolder);
            //MES LOG 폴거가 없으면 생성
            if (!Directory.Exists(mesLogFolder))
                Directory.CreateDirectory(mesLogFolder);
            //Alarm LOG 폴거가 없으면 생성
            if (!Directory.Exists(AlarmLogFolder))
                Directory.CreateDirectory(AlarmLogFolder);

            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "SYSTEM";
            string valus = myIni.Read("BARCODE_USE", section);
            SystemModel.BcrUseNotUse = valus;

            valus = myIni.Read("NFC_USE", section);
            SystemModel.NfcUseNotUse = valus;
            Channel_Model[0].MesResult = valus;

            valus = myIni.Read("PICKUP_TIMEOUT", section);
            if (!string.IsNullOrEmpty(valus))
                SystemModel.PickUpWaitTimeOutY = Convert.ToInt32(valus);
            else
                SystemModel.PickUpWaitTimeOutY = 0;

            valus = myIni.Read("LOAD_FLOOR_COUNT", section);
            if (!string.IsNullOrEmpty(valus))
                SystemModel.LoadFloorCount = Convert.ToInt32(valus);
            else
                SystemModel.LoadFloorCount = 0;

                valus = myIni.Read("LOAD_COUNT", section);
            if (string.IsNullOrEmpty(valus))
                Channel_Model[0].LoadCount = "0";
            else
                Channel_Model[0].LoadCount = valus;

            valus = myIni.Read("AGING_CV_STEP_TIME", section);
            if (!string.IsNullOrEmpty(valus))
                SystemModel.AgingCvStepTime = Convert.ToInt32(valus);
            else
                SystemModel.AgingCvStepTime = 0;

            valus = myIni.Read("AGING_CV_USE", section);
            SystemModel.AgingCvNotUse = valus;

            valus = myIni.Read("AGING_COUNT", "SYSTEM");
            Channel_Model[0].AgingCvTotalCount = valus;
        }
       
        public void LoadTeachFile()
        {
            // Teaching Data 섹션 데이터 로드
            string teachFilePath = Path.Combine(Global.instance.IniTeachPath);
            var iniTeachFile = new IniFile(teachFilePath);

            string[] teachSection = { "Top_X_Handler", "Out_Y_Handler", "Out_Z_Handler", "Lift" };
            Teaching_Data.Clear();
            foreach (var sectionName in teachSection)
            {
                foreach (Teaching_List teachingItem in Enum.GetValues(typeof(Teaching_List)))
                {
                    if (teachingItem == Teaching_List.Max) // Max는 제외
                        continue;

                    // 섹션 이름이 항목 이름의 시작 부분에 포함된 경우만 처리
                    if (teachingItem.ToString().IndexOf(sectionName, StringComparison.Ordinal) != 0)
                        continue;

                    string value = iniTeachFile.Read(teachingItem.ToString(), sectionName);
                    if (double.TryParse(value, out double parsedValue))
                    {
                        Teaching_Data[teachingItem.ToString()] = parsedValue; // Teaching_Data에 저장
                    }
                }
            }
        }
        public void LoadVelocityFiles()
        {
            // INI 파일 경로 설정
            var iniVelocityFile = new IniFile(Global.instance.IniVelocityPath);

            // Motor_Velocity 섹션 데이터 로드
            string section = "Motor_Velocity";
            foreach (var servo in Servo_Model)
            {
                string servoName = servo.ServoName.ToString();

                // INI 파일에서 각 속성 값을 읽어옴
                string velocity = iniVelocityFile.Read($"{servoName}_Velocity", section);
                string accelerate = iniVelocityFile.Read($"{servoName}_Accelerate", section);
                string decelerate = iniVelocityFile.Read($"{servoName}_Decelerate", section);
                string measurementVel = iniVelocityFile.Read($"{servoName}_Measurement_Vel", section);
                string barcodeVel = iniVelocityFile.Read($"{servoName}_Barcode_Vel", section);

                // 읽어온 값을 Double로 변환하여 Servo_Model에 반영
                if (double.TryParse(velocity, out double parsedVelocity))
                    servo.Velocity = parsedVelocity;

                if (int.TryParse(accelerate, out int parsedAccelerate))
                    servo.Accelerate = parsedAccelerate;

                if (int.TryParse(decelerate, out int parsedDecelerate))
                    servo.Decelerate = parsedDecelerate;

                if (double.TryParse(measurementVel, out double parsedMeasurementVel))
                    servo.Measurement_Vel = parsedMeasurementVel;

                if (double.TryParse(barcodeVel, out double parsedBarcodeVel))
                    servo.Barcode_Vel = parsedBarcodeVel;
            }
            // Jog_Velocity 섹션 데이터 로드
            string jogSection = "Jog_Velocity";
            string[] jogLabels = { "LOW", "MIDDLE", "HIGH" }; // Jog 속도 레이블
            foreach (var servo in Servo_Model)
            {
                string servoName = servo.ServoName.ToString();

                // INI 파일에서 JogVelocity 값을 읽어와 리스트의 특정 인덱스에 직접 할당
                for (int i = 0; i < jogLabels.Length; i++)
                {
                    string jogValue = iniVelocityFile.Read($"{servoName}_JogVelocity_{jogLabels[i]}", jogSection);
                    if (double.TryParse(jogValue, out double parsedJogValue))
                    {
                        servo.JogVelocity[i] = parsedJogValue; // 특정 인덱스에 값 할당
                    }
                }
            }
        }
        private void SerialPort_init()
        {
            //BusyContent
            Global.instance.BusyContent = "Serial Port Connecting...";

            // Serial 설정 파일 경로
            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "SERIAL";

            string _Port;
            // Barcode Serial Port Init
            //_Port = myIni.Read("BARCODE_PORT", section);
            //SerialModel[0].PortName = "BARCODE_PORT";
            //SerialModel[0].Port = _Port;

            //if(SerialModel[0].Open() == false)
            //    Global.instance.ShowMessagebox($"BCR {SerialModel[0].Port} open fail.");

            _Port = myIni.Read("NFC_PORT", section);
            SerialModel[(int)SerialIndex.Nfc].PortName = "NFC";
            SerialModel[(int)SerialIndex.Nfc].Port = _Port;

            if (SerialModel[(int)SerialIndex.Nfc].Open() == false)
                Global.instance.ShowMessagebox($"NFC {SerialModel[(int)SerialIndex.Nfc].Port} open fail.");

            _Port = myIni.Read("MES_PORT", section);
            SerialModel[(int)SerialIndex.Mes].PortName = "MES";
            SerialModel[(int)SerialIndex.Mes].Port = _Port;

            if (SerialModel[(int)SerialIndex.Mes].Open() == false)
                Global.instance.ShowMessagebox($"MES {SerialModel[(int)SerialIndex.Mes].Port} open fail.");


        }
        private void DioBoard_Init()
        {
            //BusyContent
            Global.instance.BusyStatus = true;
            Global.instance.BusyContent = "Dio Board Connecting...";
            string error = string.Empty;
            for (int i =0; i < (int)DI_MAP.DI_MAX / 16; i++)
            {
                if (Dio.Connect(i) == false)
                {
                    if (string.IsNullOrEmpty(error) == false)
                        error += ", ";
                    error += $"DIO Slave {i} Connect fail";
                }
            }
            if (string.IsNullOrEmpty(error) == false)
            {
                Global.instance.ShowMessagebox(error);
            }
            else
            {
                Dio.DioThreadStart(); // Dio Thread Start
            }
            for (int i = 0; i < (Dio.DisplayDio_List.Count + (int)EziDio_Model.DisplayExist_List.Max); i++)
                DisplayUI_Dio.Add(false);

            Global.instance.BusyStatus = false;
            Global.instance.BusyContent = string.Empty;
        }
        private void Motion_Init()
        {
            //BusyContent
            Global.instance.BusyContent = "Ez Motion Connecting...";
            string error = "";
            for (int i=0; i<(int)ServoSlave_List.Max; i++)
            {
                if (Motion.Connect(i) == false)
                {
                    if (string.IsNullOrEmpty(error) == false)
                        error += ", ";
                    error += (ServoSlave_List.Out_Y_Handler_Y + i).ToString();
                }
            }
            Global.instance.BusyStatus = false;
            Global.instance.BusyContent = string.Empty;
            if (string.IsNullOrEmpty(error) == false)
            {
                error += " Ez Motion Connect Fail";
                Global.instance.ShowMessagebox(error);
            }
        }
        private void Unit_Init()
        {
            //BusyContent
            Global.instance.BusyContent = "Units Initializing ...";

            // Unit Model 초기화
            for (int i = 0; i < (int)MotionUnit_List.Max; i++)
            {
                // 유닛 그룹 추가
                Unit_Model.Add(new Unit_Model((MotionUnit_List)i));
                string _UnitGroup = ((MotionUnit_List)i).ToString();

                for (int j = 0; j < (int)ServoSlave_List.Max; j++)
                {
                    // 유닛그룹에 있는 서보를 찾았을 경우 카운트 증가
                    string _ServoName = ((ServoSlave_List)j).ToString();
                    if (_ServoName.Contains(_UnitGroup))
                    {
                        Unit_Model[i].ServoNames.Add((ServoSlave_List)j);
                    }
                    // 프로그램 시작할때 마지막 step을 불러온다.
                    Unit_Model[i].StartReady();
                }
            }

            // Servo Model 초기화
            for(int i = 0; i< (int)ServoSlave_List.Max; i++)
            {
                Servo_Model.Add(new Servo_Model((ServoSlave_List)i));
            }
            
        }
        private void InterfaceServerStart()
        {
            //var myIni = new IniFile(Global.instance.IniSystemPath);
            isServerThreadRun = true;
            isClientThreadRun = true;

            Task.Factory.StartNew(AsyncServerListen);
        }
        public void InterfaceServerStop()
        {
            isServerThreadRun = false;
            isClientThreadRun = false;
            IsServerOpen = false;

            // 모든 클라이언트 연결 종료
            foreach (var client in tcpClientList.ToArray())
            {
                try
                {
                    client.Close();
                }
                catch { }
            }
            tcpClientList.Clear();

        }
        async Task AsyncServerListen()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 8000);
            listener.Start();
            listener.Server.ReceiveTimeout = 200;
            IsServerOpen = true;

            while (isServerThreadRun)
            {
                try
                {
                    TcpClient tc = await listener.AcceptTcpClientAsync().ConfigureAwait(false);

                    // 클라이언트 IP 확인
                    IPEndPoint rmtIpep = tc.Client.RemoteEndPoint as IPEndPoint;
                    string clientIp = rmtIpep.Address.ToString();

                    if (isServerThreadRun)
                    {
                        Thread ClientThread = new Thread(() => workClient(tc));
                        ClientThread.Start();
                    }
                }
                catch (Exception e)
                {
                    Global.ExceptionLog.Error($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {e}");
                }
                Thread.Sleep(1);
            }
        }
        void workClient(TcpClient tc)
        {
            IPEndPoint rmtIpep = tc.Client.RemoteEndPoint as IPEndPoint;                                    // IP주소 저장
                                                                                                            // 연결 시 상태 변경
            IsIF_Connected_1 = true;
            Global.Mlog.Info($"[{rmtIpep.Address.ToString()}-C]  {"{STATUS=CONNECT},"}");                   // Client 연결을 로그에 저장

            try
            {
                tcpClientList.Add(tc);                                                                      // TCP Client List에 넘겨받은 TCP Client 연결 추가
                ItemClient clientItem = new ItemClient(rmtIpep.Address.ToString());                         // 검사기 Item 생성
                AddClientItem(clientItem);                                                                  // 검사기 Item 전역 관리 List에 추가
                NetworkStream stream = tc.GetStream();                                                      // 통신 스트림 가져옴
                clientItem.stream = stream;                                                                 // 통신 스트림을 검사기 Item에 할당
                clientItem.SendStream($"{rmtIpep.Address.ToString()}=CONNECTED");                           // 통신 연결 후 제일 먼저 검사기에 보내는 데이터
                stream.ReadTimeout = 3600000;

                while ((isClientThreadRun) && (tc.Connected) && (tc.GetStream() != null) && (clientItem.mIdString != ""))
                {
                    try
                    {
                        StringBuilder myCompleteMessage = new StringBuilder();
                        if (stream.CanRead)
                        {
                            byte[] myReadBuffer = new byte[1024];
                            int numberOfBytesRead = 0;

                            // Incoming message may be larger than the buffer size.
                            do
                            {
                                numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                                myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));
                            }
                            while (stream.DataAvailable);

                            // 클라이언트가 연결을 종료함
                            if (numberOfBytesRead == 0)
                            {
                                IsIF_Connected_1 = false;
                                break;
                            }
                        }

                        // 검사기로부터 받은 데이터가 있으면
                        string rcvString = myCompleteMessage.ToString();

                        // 프로토콜의 끝은 줄바꿈이므로 줄바꿈이 있는 경우 여러개의 메시지로 구분
                        if (!string.IsNullOrEmpty(rcvString) &&
                            rcvString[0] == '\x02' && rcvString[rcvString.Length - 1] == '\x03')
                        {
                            // 메시지 마다 동작 한번씩 하도록 루프
                            Global.Mlog.Info($"[{rmtIpep.Address.ToString()}-R] : {rcvString}");

                            // STX/ETX 제거 후 내용만 추출
                            rcvString = rcvString.Substring(1, rcvString.Length - 2);
                            string rtnMsg = clientItem.receiveMsgProc(rcvString);  // 메시지에 대한 처리함수 (처리 후 보내야 하는 데이터가 있으면 반환됨) ** 주요 동작 함수 **

                            if (string.IsNullOrEmpty(rtnMsg) == false)
                            {
                                IsIF_Connected_1 = true;
                                clientItem.SendStream(rtnMsg);
                            }
                        }

                        stream.ReadTimeout = 3600000;

                    }
                    catch (IOException)
                    {
                        Thread.Sleep(1000);
                    }
                    catch (Exception ee)
                    {
                        Global.ExceptionLog.Error($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {ee}");
                    }
                }

                // 연결이 종료되므로 Client 항목에서 제거
                RemoveClientItem(clientItem);

                if (stream != null)
                {
                    stream.Close();
                }
                tc.Close();

                // 연결 종료됨을 로그와 DB에 저장
                Global.Mlog.Info($"[{rmtIpep.Address.ToString()}-C] : {"{STATUS=DISCONNECT},"}");
                tcpClientList.Remove(tc);
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Error($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {e}");
            }
            finally
            {
                // 연결 해제 시 상태 변경
                IsIF_Connected_1 = false;
            }
        }
        void AddClientItem(ItemClient item)
        {
            try
            {
                if (item != null)
                {
                    for (int i = 0; i < clientItemList.Count; i++)
                    {
                        if (clientItemList[i] != null)
                        {
                            if (clientItemList[i].mIdString == item.mIdString)
                            {
                                clientItemList[i].Close();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Error($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {e}");
            }
            clientItemList.Add(item);
        }
        void RemoveClientItem(ItemClient item)
        {
            try
            {
                if (item != null)
                {
                    clientItemList.Remove(item);
                }

                CleanClientItem();
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Error($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {e}");
            }
        }
        void CleanClientItem()
        {
            try
            {
                int idx = clientItemList.Count - 1;
                while (idx >= 0)
                {
                    if (clientItemList[idx] == null)
                    {
                        clientItemList.RemoveAt(idx);
                    }
                    idx--;
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Error($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {e}");
            }
        }
        // Background Worker
        private void BackgroundThread_Init()
        {
            // 작업 쓰레드 ( Station )
            UnitsProcThread.WorkerReportsProgress = false;        // 진행률 전송 여부
            UnitsProcThread.WorkerSupportsCancellation = true;    // 작업 취소 여부
            UnitsProcThread.DoWork += new DoWorkEventHandler(UnitsProc_DoWork);
            UnitsProcThread.RunWorkerAsync();
        }
        public void BackgroundThread_Stop()
        {
            // UnitsProcThread 작업 취소
            if (UnitsProcThread != null)
            {
                UnitsProcThread.CancelAsync(); // 작업 취소 요청
                while (UnitsProcThread.IsBusy) // 작업이 완료될 때까지 대기
                {
                    Thread.Sleep(10);
                }

                // 이벤트 핸들러 제거
                UnitsProcThread.DoWork -= UnitsProc_DoWork;

                // BackgroundWorker 정리
                UnitsProcThread.Dispose();
                UnitsProcThread = null;
            }
            // SequenceQueue 정리
            if (SequenceQueue != null)
            {
                SequenceQueue.Clear(); // 메시지 큐 비우기
                SequenceQueue = null;
            }
        }
        private void UnitsProc_DoWork(object sender, DoWorkEventArgs e)
        {
            // 프로그램이 종료될 때까지 아래의 루프가 반복됨
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                try
                {
                    if (UnitsProcThread.CancellationPending == true)
                        break;

                    
                    // Tact Time Display
                    for (int i = 0; i < (int)ChannelList.Max; i++)
                    {
                        if (Channel_Model[i].Status == ChannelStatus.RUNNING)
                            Channel_Model[i].GetTactTime();
                    }

                    // 데이터 송신 외에는 아래의 상태 루프를 반복적으로 수행
                    if (IsWorkingUnitsProcThread == true)
                    {
                        // Emergency 상시 체크
                        if (!Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.FRONT_OP_EMERGENCY_FEEDBACK]
                            || !Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.REAR_OP_EMERGENCY_FEEDBACK])
                        {
                            Global.instance.SafetyErrorMessage = "EMERGENCY Button Operation! ";
                            IsSafetyInterLock = true;
                        }

                        if (IsSafetyInterLock == true)
                        {
                            IsWorkingUnitsProcThread = false;

                            Application.Current.Dispatcher.BeginInvoke(
                                (ThreadStart)(() =>
                                {
                                    // Todo : Interlock Loop Stop. 진행중인 작업 모두 정지
                                    Global.instance.InspectionStop();
                                    // Safety Popup
                                    Window window = new Safety_View();
                                    Safety_ViewModel safety_ViewModel = new Safety_ViewModel();
                                    window.DataContext = safety_ViewModel;
                                    window.ShowDialog();
                                    // Close
                                    safety_ViewModel.Dispose();
                                    safety_ViewModel = null;
                                    window.Close();
                                    window = null;

                                    IsSafetyInterLock = false;
                                    IsWorkingUnitsProcThread = true;
                                }), DispatcherPriority.Send);
                        }
                        else
                        {
                            // 시작 신호가 들어오면 검사 Loop 반복
                            if (IsInspectionStart == true)
                            {
                                //Safety 먼저 체크
                                if (!Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.FRONT_DOOR_SS]
                                || !Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.REAR_DOOR_SS]
                                || !Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.LEFT_L_DOOR_SS]
                                || !Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.LEFT_R_DOOR_SS])
                                {
                                    Global.instance.SafetyErrorMessage = "DOOR IS OPEN ! ";
                                    IsSafetyInterLock = true;
                                }
                                else if (Motion.ServoSlaveOriginStatus() == false)
                                {
                                    Global.instance.InspectionStop();
                                }
                                else
                                {
                                    for (int i = 0; i < (int)MotionUnit_List.Max; i++)
                                    {
                                        if (i == (int)MotionUnit_List.Top_X
                                            || i == (int)MotionUnit_List.Out_Y
                                            || i == (int)MotionUnit_List.Lift_1
                                            || i == (int)MotionUnit_List.In_CV
                                            || i == (int)MotionUnit_List.Out_CV)

                                            Unit_Model[i].Loop();
                                        // clamp Mes Error
                                        if (!string.IsNullOrEmpty(Unit_Model[i].ClampFailMassage))
                                        {
                                            string message = Unit_Model[i].ClampFailMassage;
                                            Unit_Model[i].ClampFailMassage = string.Empty;
                                            Application.Current.Dispatcher.BeginInvoke(
                                               (ThreadStart)(() =>
                                               {
                                                   Global.instance.InspectionStop();
                                                   Global.instance.WriteAlarmLog(message);
                                                   Global.instance.ShowMessagebox(message, true,true);
                                               }), DispatcherPriority.Send);
                                        }
                                        Thread.Sleep(5);
                                    }
                                    Global.instance.LoadingTactTimeEnd();
                                }
                                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.OP_BOX_STOP])
                                {
                                    // 삼성담당자 요청으로 버튼 누를시 바로 동작
                                    Global.instance.InspectionStop();
                                }
                            }
                            // 검사 시작 신호가 들어오지 않으면 스위치 체크
                            else
                            {
                                if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.OP_BOX_START])
                                {
                                    // 삼성담당자 요청으로 버튼 누를시 바로 동작
                                    _ = Global.instance.InspectionStart();                                                                                                                                    
                                }
                                Unit_Model[(int)MotionUnit_List.In_CV].Return_Bottom_CV_1_Logic();
                                Unit_Model[(int)MotionUnit_List.In_CV].Return_Top_CV_1_Logic();
                                Unit_Model[(int)MotionUnit_List.In_CV].Return_Bottom_CV_2_Logic();
                                Unit_Model[(int)MotionUnit_List.In_CV].Return_Top_CV_2_Logic();
                                Unit_Model[(int)MotionUnit_List.In_CV].Top_Cv();
                                Unit_Model[(int)MotionUnit_List.In_CV].In_CV_Logic();
                                Unit_Model[(int)MotionUnit_List.Lift_1].Aging_CV_StepRun_Logic();
                                Unit_Model[(int)MotionUnit_List.Out_CV].Set_Ng_CV_Logic();
                                Thread.Sleep(5);
                            }
                        }
                    }
                    else
                    {
                        Unit_Model[(int)MotionUnit_List.In_CV].Return_Bottom_CV_1_Logic();
                        Unit_Model[(int)MotionUnit_List.In_CV].Return_Top_CV_1_Logic();
                        Unit_Model[(int)MotionUnit_List.In_CV].Return_Bottom_CV_2_Logic();
                        Unit_Model[(int)MotionUnit_List.In_CV].Return_Top_CV_2_Logic();
                        Unit_Model[(int)MotionUnit_List.In_CV].Top_Cv();
                        Unit_Model[(int)MotionUnit_List.In_CV].In_CV_Logic();
                        Unit_Model[(int)MotionUnit_List.Lift_1].Aging_CV_StepRun_Logic();
                        Thread.Sleep(5);
                    }
                }
                catch (Exception ee)
                {
                    Global.ExceptionLog.ErrorFormat($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {ee}");
                }
            }
        }
        private DateTime? _StartPressedTime = null;
        private DateTime? _StopPressedTime = null;

        #region // override
        protected override void DisposeManaged()
        {
            // Motion 해제
            if (Motion != null)
            {
                Motion.Dispose();
                Motion = null;
            }
            // Dio 정리
            if (Dio != null)
            {
                Dio = null;
            }
            // Channel_Model 정리
            if (Channel_Model != null)
            {
                foreach (var channel in Channel_Model)
                {
                    channel.Dispose();
                }
                Channel_Model.Clear();
                Channel_Model = null;
            }
            // Unit_Model 정리
            if (Unit_Model != null)
            {
                Unit_Model.Clear();
                Unit_Model = null;
            }

            // Servo_Model 정리
            if (Servo_Model != null)
            {
                Servo_Model.Clear();
                Servo_Model = null;
            }
            // DisplayUI_Dio 정리
            if (DisplayUI_Dio != null)
            {
                DisplayUI_Dio.Clear();
                DisplayUI_Dio = null;
            }
            base.DisposeManaged();
        }
        #endregion
    }
}
