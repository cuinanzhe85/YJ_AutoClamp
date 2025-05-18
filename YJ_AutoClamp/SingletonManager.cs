using Common.Managers;
using Common.Mvvm;
using Lmi3d.GoSdk;
using Lmi3d.Zen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Telerik.Windows.Data;
using YJ_AutoClamp.Models;
using YJ_AutoClamp.Utils;
using YJ_AutoClamp.ViewModels;
using static YJ_AutoClamp.Models.EziDio_Model;

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
        public bool IsInspectionStart = false;
        private msgQueue SequenceQueue; // Sequence 관련 Q

        // Out Y축 이동시 사용되는 변수
        private bool _IsY_PickupColl = false;
        public bool IsY_PickupColl
        {
            get { return _IsY_PickupColl; }
            set { SetValue(ref _IsY_PickupColl, value); }
        }
        public ObservableCollection<int> LoadFloor { get; set; }
        public ObservableCollection<Lift_Model> Display_Lift { get; set; }
        public int LoadStageNo = 0;
        public bool BottomClampDone = false;
        public bool BottomClampNG = false;
        public bool UnitLastPositionSet = false;
        // 7단 Loading완료 변수 
        public bool[] LoadComplete = { false, false, false };
        // Default Infomation
        public EquipmentMode EquipmentMode { get; set; } = EquipmentMode.Dry;

        #region // Properties
        public NmcDio_Model Dio { get; set; }
        public NMC_Model NMC_Model { get; set; }
        public EziDio_Model Ez_Dio { get; set; }
        public EzMotion_Model_E Ez_Model { get; set; }
        public Serial_Model Barcode_Model { get; set; }
        public Serial_Model Nfc_Model { get; set; }
        public RadObservableCollection<Unit_Model> Unit_Model { get; set; }
        public RadObservableCollection<Servo_Model> Servo_Model { get; set; }
        public RadObservableCollection<Channel_Model> Channel_Model { get; set; }
        public Dictionary<string, double> Teaching_Data { get; set; }
        public ObservableCollection<SpecData_Model> Spec_Data { get; set; }
        public ModelData_Model Current_Model { get; set; }


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
            NMC_Model = new NMC_Model();
            Dio = new NmcDio_Model();
            Ez_Model = new EzMotion_Model_E();
            Ez_Dio = new EziDio_Model();
            DisplayUI_Dio = new RadObservableCollection<bool>();
            Channel_Model = new RadObservableCollection<Channel_Model>();
            Teaching_Data = new Dictionary<string, double>();
            Barcode_Model = new Serial_Model();
            Nfc_Model = new Serial_Model();
            Current_Model = new ModelData_Model();
            Spec_Data = new ObservableCollection<SpecData_Model>();

            // Lift Data
            LoadFloor = new ObservableCollection<int>();
            for (int i = 0; i < 3; i++)
                LoadFloor.Add(0);
            // Lift Status Display
            Display_Lift = new ObservableCollection<Lift_Model>();
            for (int i = 0; i < 3; i++)
                Display_Lift.Add(new Lift_Model("LIFT " + (i + 1)));
            
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
            // Load Model Data Init. CurrentModel, Teaching, Velocity
            ModelData_Init();
            // Gocator Init
            Gocator_Init();
            // Background Thread Start
            BackgroundThread_Init();
        }
        private void LoadSystemFiles()
        {
            //BusyContent
            Global.instance.BusyContent = "System Operation Loading...";

            // Config 폴더 경로 설정
            string configPath = Global.instance.IniConfigPath;

            // Model, Job, Teach 폴더 경로 설정
            string modelFolder = Path.Combine(configPath, "Model");
            string specFolder = Path.Combine(configPath, "Spec");
            string jobFolder = Path.Combine(configPath, "Job");
            string teachFolder = Path.Combine(configPath, "Teach");

            // Config 폴더가 없으면 생성
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);
            // Model 폴더가 없으면 생성
            if (!Directory.Exists(modelFolder))
                Directory.CreateDirectory(modelFolder);
            // Spec 폴더가 없으면 생성
            if (!Directory.Exists(specFolder))
                Directory.CreateDirectory(specFolder);
            // Job 폴더가 없으면 생성
            if (!Directory.Exists(jobFolder))
                Directory.CreateDirectory(jobFolder);
            // Teach 폴더가 없으면 생성
            if (!Directory.Exists(teachFolder))
                Directory.CreateDirectory(teachFolder);
        }
        public void ModelData_Init()
        {
            // 현재 모델 초기화. Spec, Job, Teach File Load
            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "SYSTEM";
            string currentModel = myIni.Read("CURRENT_MODEL", section);
            
            // Current Model Data Load
            string modelFilePath = Path.Combine(Global.instance.IniModelPath, currentModel);
            section = "ModelData";
            var iniModelFile = new IniFile(modelFilePath);

            Current_Model.ModelFileName = currentModel;
            Current_Model.SpecFileName = iniModelFile.Read("SpecFileName", section);
            Current_Model.TeachFileName = iniModelFile.Read("TeachFileName", section);
            Current_Model.JobFileName = iniModelFile.Read("JobFileName", section);

            // Load Teaching Data
            LoadTeachFile();

            // Load Velocity Data
            LoadVelocityFiles();
        }
       
        public void LoadTeachFile()
        {
            // Teaching Data 섹션 데이터 로드
            string teachFilePath = Path.Combine(Global.instance.IniTeachPath, "Teaching.ini");
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

                if (double.TryParse(accelerate, out double parsedAccelerate))
                    servo.Accelerate = parsedAccelerate;

                if (double.TryParse(decelerate, out double parsedDecelerate))
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

            // Barcode Serial Port Init
            string bcrPort = myIni.Read("BARCODE_PORT", section);
            Barcode_Model.PortName = "BarCode";
            Barcode_Model.Port = bcrPort;

            if(Barcode_Model.Open() == false)
                MessageBox.Show("BCR Port open fail.", "BCR Open Fail", MessageBoxButton.OK, MessageBoxImage.Error);

            
        }
        private void Gocator_Init()
        {
            //BusyContent
            Global.instance.BusyContent = "Gocator Connecting...";

            KApiLib.Construct();
            GoSdkLib.Construct();

            // Gocator 설정 파일 경로
            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "TCP";

            // Gocator IP 읽기
            string gocator_front = myIni.Read("GOCATOR_FRONT_IP", section);
            string gocator_rear = myIni.Read("GOCATOR_REAR_IP", section);

            // 기본 IP 주소 설정
            if (string.IsNullOrEmpty(gocator_front))
            {
                gocator_front = "192.168.1.5";
            }

            if (string.IsNullOrEmpty(gocator_rear))
            {
                gocator_rear = "192.168.1.6";
            }
        }
        private void DioBoard_Init()
        {
            //BusyContent
            Global.instance.BusyContent = "Dio Board Connecting...";
            
            for (int i =0; i < (int)DI_MAP.DI_MAX / 16; i++)
            {
                Ez_Dio.Connect(i);
            }

            for (int i = 0; i < Ez_Dio.DisplayDio_List.Count + (int)EziDio_Model.DisplayExist_List.Max; i++)
                DisplayUI_Dio.Add(false);
        }
        private void Motion_Init()
        {
            //BusyContent
            Global.instance.BusyContent = "Ez Motion Connecting...";
            string error = "";
            for (int i=0; i<(int)ServoSlave_List.Max; i++)
            {
                if (Ez_Model.Connect(i) == false)
                {
                    if (string.IsNullOrEmpty(error) == false)
                        error += ", ";
                    error += (ServoSlave_List.Top_X_Handler_X + i).ToString();
                }
            }
            Global.instance.BusyStatus = false;
            Global.instance.BusyContent = string.Empty;
            if (string.IsNullOrEmpty(error) == false)
            {
                error += "Ez Motion Connect Fail";
                MessageBox.Show(error, "Ez Motion", MessageBoxButton.OK, MessageBoxImage.Error);
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
                }
            }
            // Servo Model 초기화
            for(int i = 0; i< (int)ServoSlave_List.Max; i++)
            {
                Servo_Model.Add(new Servo_Model((ServoSlave_List)i));
            }
            // Channel Model 초기화
            for(int i = 0; i < (int)ChannelList.Max; i++)
            {
                Channel_Model.Add(new Channel_Model((ChannelList)i));
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
            while (true)
            {
                try
                {
                    if (UnitsProcThread.CancellationPending == true)
                        break;

                    // 메시지 큐에 메시지가 있다면 (전역으로 데이터를 송신하기 위한 목적으로 사용)
                    if (SequenceQueue.GetCount() > 0)
                    {
                        msgItem msg = SequenceQueue.GetItem(0); //  첫번째 메시지를 가져온다.

                        // 메시지의 evt(이벤트)에 따라 역할을 수행
                        switch (msg.evt)
                        {
                            case msgItem.Event.SendVisionMsg:
                                {
                                    // Vision 데이터 송신. 정상적으로 연결되어있다면
                                    break;
                                }
                        }
                    }

                    // Tact Time Display
                    for (int i = 0; i < (int)ChannelList.Max; i++)
                    {
                        if (Channel_Model[i].Status == ChannelStatus.RUNNING)
                            Channel_Model[i].GetTactTime();
                    }

                    // 데이터 송신 외에는 아래의 상태 루프를 반복적으로 수행
                    if (IsWorkingUnitsProcThread == true)
                    {
                        if (IsSafetyInterLock == true)
                        {
                            IsWorkingUnitsProcThread = false;

                            Application.Current.Dispatcher.BeginInvoke(
                                (ThreadStart)(() =>
                                {
                                    // Todo : Interlock Loop Stop. 진행중인 작업 모두 정지
                                    
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
                            if(IsInspectionStart == true)
                            {
                                if (UnitLastPositionSet == true)
                                {
                                    Unit_Model[0].StartReady();
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
                                        Thread.Sleep(5);
                                    }
                                }
                            }
                            else
                            {
                                if (UnitLastPositionSet == false)
                                    UnitLastPositionSet = true;
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    Global.ExceptionLog.ErrorFormat($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {ee}");
                }
            }
        }


        #region // override
        protected override void DisposeManaged()
        {
            // NMC Motion 해제
            if (Ez_Model != null)
            {
                Ez_Model.Dispose();
                Ez_Model = null;
            }
            // Dio 정리
            if (Ez_Dio != null)
            {
                Ez_Dio = null;
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
