using Common.Commands;
using Common.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Telerik.Windows.Data;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels
{
    public enum Teaching_List
    {
        Top_X_Handler_Home,
        Top_X_Handler_Pick_Up,
        Top_X_Handler_Put_Down,
        Top_X_Handler_NG_Port,
        Out_Y_Handler_Home,
        Out_Y_Handler_Pick_Up,
        Out_Y_Handler_Put_Down_1,
        Out_Y_Handler_Put_Down_2,
        Out_Y_Handler_Put_Down_3,
        Out_Z_Handler_Home,
        Out_Z_Handler_Pick_Up,
        Out_Z_Handler_Put_Down_1,
        Out_Z_Handler_Put_Down_2,
        Out_Z_Handler_Put_Down_3,
        Out_Z_Handler_Put_Down_4,
        Out_Z_Handler_Put_Down_5,
        Out_Z_Handler_Put_Down_6,
        Out_Z_Handler_Put_Down_7,
        Lift_Load_1,
        Lift_Upper_1,
        Lift_Low_1,
        Lift_Load_2,
        Lift_Upper_2,
        Lift_Low_2,
        Lift_Load_3,
        Lift_Upper_3,
        Lift_Low_3,

        Max
    }
    // Initial -> Flow 초기화 홈포지션
    // Origin -> Servo On 등 프로그램 처음에 한번만
    public class Teach_ViewModel : Child_ViewModel
    {
        enum ServoTarget
        {
            X,
            Y,
            Z,
            Max
        }
        enum JogType
        {
            X_CW,
            X_CCW,
            Y_CW,
            Y_CCW,
            Z_CW,
            Z_CCW,
            Stop,
            Max
        }
        enum ServoLimitType
        {
            X_CW_Limit,
            X_CCW_Limit,
            Y_CW_Limit,
            Y_CCW_Limit,
            Z_CW_Limit,
            Z_CCW_Limit,
            Max
        }
        enum TeachingSection
        {
            Top_Handler_X,
            Out_Handler_Y,
            Out_Handler_Z,
            Lift,
            Max
        }

        #region // ICommands
        public ICommand ServoMove_Command { get; private set; }
        public ICommand TeachingMove_Command { get; private set; }
        public ICommand TeachingSave_Command { get; private set; }

        #endregion

        private readonly EzMotion_Model_E EzModel = SingletonManager.instance.Motion;
        private readonly RadObservableCollection<Servo_Model> ServoModel = SingletonManager.instance.Servo_Model;

        #region// Update Thread
        private DispatcherTimer UpdateTimer { get; set; } = new DispatcherTimer();
        #endregion

        #region // Servo Control Datas
        public List<string> UnitList { get; set; } = new List<string>();
        private int _Selected_UnitIndex = 0;
        public int Selected_UnitIndex
        {
            get { return _Selected_UnitIndex; }
            set
            {
                SetValue(ref _Selected_UnitIndex, value);
                Calculate_DefaultSlaveIndex();
            }
        }
        private ObservableCollection<bool> _Selected_UnitExist = new ObservableCollection<bool>();
        public ObservableCollection<bool> Selected_UnitExist
        {
            get { return _Selected_UnitExist; }
            set { SetValue(ref _Selected_UnitExist, value); }
        }
        private ObservableCollection<int> _Selected_ServoIndex = new ObservableCollection<int>();
        public ObservableCollection<int> Selected_ServoIndex
        {
            get { return _Selected_ServoIndex; }
            set { SetValue(ref _Selected_ServoIndex, value); }
        }
        private ObservableCollection<string> _Selected_LimitCheck = new ObservableCollection<string>();
        public ObservableCollection<string> Selected_LimitCheck
        {
            get { return _Selected_LimitCheck; }
            set { SetValue(ref _Selected_LimitCheck, value); }
        }
        private ObservableCollection<double> _Current_Position = new ObservableCollection<double>();
        public ObservableCollection<double> Current_Position
        {
            get { return _Current_Position; }
            set { SetValue(ref _Current_Position, value); }
        }
        private ObservableCollection<double> _Target_Position = new ObservableCollection<double>();
        public ObservableCollection<double> Target_Position
        {
            get { return _Target_Position; }
            set { SetValue(ref _Target_Position, value); }
        }
        #endregion

        #region // Teaching Datas
        private int _TeachingIndex = -1;
        public int TeachingIndex
        {
            get { return _TeachingIndex; }
            set
            {
                SetValue(ref _TeachingIndex, value);
                Update_TeachingPosition();
            }
        }
       
        private int _Selected_LiftIndex = 0;
        public int Selected_LiftIndex
        {
            get { return _Selected_LiftIndex; }
            set { SetValue(ref _Selected_LiftIndex, value); }
        }
        private ObservableCollection<double> _TeachPosition = new ObservableCollection<double>();
        public ObservableCollection<double> TeachPosition
        {
            get { return _TeachPosition; }
            set { SetValue(ref _TeachPosition, value); }
        }
        #endregion

        private EziDio_Model _Dio = SingletonManager.instance.Dio;
        public EziDio_Model Dio
        {
            get { return _Dio; }
            set { SetValue(ref _Dio, value); }
        }
        private string _JogSpeed = "Low";
        public string JogSpeed
        {
            get { return _JogSpeed; }
            set { SetValue(ref _JogSpeed, value); }
        }
        private int _JogSpeedIndex = 0;
        public int JogSpeedIndex
        {
            get { return _JogSpeedIndex; }
            set { SetValue(ref _JogSpeedIndex, value); }
        }
        private bool _TopCVRunStop = false;
        public bool TopCVRunStop
        {
            get { return _TopCVRunStop; }
            set { SetValue(ref _TopCVRunStop, value); }
        }
        public Teach_ViewModel()
        {
            for (int i = 0; i < (int)ServoTarget.Max; i++)
            {
                Selected_UnitExist.Add(false);
                Selected_ServoIndex.Add(-1);
                Current_Position.Add(0.00);
                Target_Position.Add(0.00);
            }
            for (int i = 0; i < (int)MotionUnit_List.Max; i++)
            {
                UnitList.Add(((MotionUnit_List)i).ToString());
            }
            for (int i = 0; i < (int)ServoLimitType.Max; i++)
            {
                Selected_LimitCheck.Add("White");
            }
            for (int i = 0; i < (int)TeachingSection.Max; i++)
            {
                TeachPosition.Add(0.00);
            }

            Calculate_DefaultSlaveIndex();

            // Update Timer
            UpdateTimer.Interval = TimeSpan.FromMilliseconds(10);
            UpdateTimer.Tick += new EventHandler(UpdateTimer_DoWork);

        }
       
        private void Calculate_DefaultSlaveIndex()
        {
            for (int i = 0; i < (int)ServoTarget.Max; i++)
            {
                Selected_UnitExist[i] = false;
                Selected_ServoIndex[i] = -1;
            }
            for (int i = 0; i < (int)ServoLimitType.Max; i++)
            {
                Selected_LimitCheck[i] = "White";
            }

            var unitModel = SingletonManager.instance.Unit_Model[Selected_UnitIndex];
            foreach (var servoEnum in unitModel.ServoNames)
            {
                char target = servoEnum.ToString()[servoEnum.ToString().Length - 1]; // 마지막 문자 추출
                if (Enum.TryParse(target.ToString(), out ServoTarget servoTarget))
                {
                    Selected_UnitExist[(int)servoTarget] = true;
                    Selected_ServoIndex[(int)servoTarget] = (int)servoEnum;
                    Current_Position[(int)servoTarget] = EzModel.GetActualPos(Selected_ServoIndex[(int)servoTarget]);
                    LimitCheck(servoTarget);
                }
            }
        }
        // 각각의 Teach 버튼을 눌렀을때 Teaching Position을 UI에 업데이트하는 함수
        private void Update_TeachingPosition()
        {
            string key = ((Teaching_List)TeachingIndex).ToString();
            string[] split = key.Split('_');
            // Lift 일때 KeyIndex 재정의
            if (split[0] == "Lift")
            {
                key = ((Teaching_List)(TeachingIndex + (Selected_LiftIndex * 3))).ToString();
            }
            else if (split[0] == "Out")
            {
                key = ((Teaching_List)TeachingIndex).ToString();
            }
            if (SingletonManager.instance.Teaching_Data.ContainsKey(key) == false)
                return;

            double position = SingletonManager.instance.Teaching_Data[key];
            switch (split[0])
            {
                case "Top":
                    Selected_UnitIndex = (int)MotionUnit_List.Top_X;
                    TeachPosition[(int)TeachingSection.Top_Handler_X] = position;
                    break;
                case "Out":
                    if (key.ToString().IndexOf("Out_Y_Handler", StringComparison.Ordinal) != 0)
                    {
                        Selected_UnitIndex = (int)MotionUnit_List.Out_Z;
                        TeachPosition[(int)TeachingSection.Out_Handler_Z] = position;
                    }
                    else
                    {
                        Selected_UnitIndex = (int)MotionUnit_List.Out_Y;
                        TeachPosition[(int)TeachingSection.Out_Handler_Y] = position;
                    }
                    break;
                case "Lift":
                    Selected_UnitIndex = (int)MotionUnit_List.Lift_1 + Selected_LiftIndex;

                    TeachPosition[(int)TeachingSection.Lift] = position;
                    break;
            }
        }
        private async void MoveAllHome()
        {
            // BusyIndicator 시작
            Global.instance.BusyStatus = true;
            Global.instance.BusyContent = "Moving all motors to home position...";

            try
            {
                Dio.Set_HandlerUpDown(true); await Task.Delay(1000);
                // 실패한 모터를 추적하기 위한 리스트
                var failedMotors = new List<string>();

                // 모든 서보 슬레이브에 대해 홈 포지션 설정 작업을 병렬로 실행
                var tasks = new List<Task<(ServoSlave_List, bool)>>(); // (모터 이름, 성공 여부)
                for (int i = (int)ServoSlave_List.Max - 1; i >= 0 ; i--)
                {
                    
                    var motor = (ServoSlave_List)i; // 모터 이름 가져오기
                    if (i != (int)ServoSlave_List.Top_CV_X)
                        tasks.Add(Task.Run(async () => (motor, await EzModel.SetHomePositionWithTimeout(i))));
                    
                }
                // 모든 작업 완료 대기
                var results = await Task.WhenAll(tasks);

                //실패한 모터 이름 수집
                foreach (var result in results)
                {
                    if (!result.Item2) // 실패한 경우
                    {
                        failedMotors.Add(result.Item1.ToString());
                    }
                }

                // 결과에 따라 메시지 표시
                if (failedMotors.Count > 0)
                {
                    string failedMotorsMessage = string.Join(", ", failedMotors);
                    MessageBox.Show($"The following motors failed to move to the home position: {failedMotorsMessage}",
                                    "Home Position Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("All motors successfully moved to the home position.",
                                    "Home Position Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            finally
            {
                // BusyIndicator 종료
                Global.instance.BusyStatus = false;
                Global.instance.BusyContent = string.Empty;
            }
        }
        private void MoveAllStop()
        {
            for (int i = 1; i < (int)ServoSlave_List.Max; i++)
            {
                if (ServoModel[i].IsServoOn == true)
                    EzModel.ServoStop(i);
            }
        }
        private void AlarmAllReset()
        {
            for (int i = 1; i < (int)ServoSlave_List.Max; i++)
            {
                EzModel.ServoAlarmReset(i);
            }
        }
        private bool IsMoveDone(ServoTarget type)
        {
            if (Selected_UnitExist[(int)type] == false)
                return true;

            if (EzModel.IsMoveDone(Selected_ServoIndex[(int)type]) == true)
                return true;
            else
                return false;

        }
        private void LimitCheck(ServoTarget target)
        {
            if (Selected_UnitExist[(int)target] == true)
            {
                Current_Position[(int)target] = EzModel.GetActualPos(Selected_ServoIndex[(int)target]);
                // 리미트 상태 확인
                
                bool isPlusLimit =false;
                bool isMinusLimit = false;
                bool isHomeOK = EzModel.IsOriginOK(Selected_ServoIndex[(int)target]);
                EzModel.IsOverSWLimit(Selected_ServoIndex[(int)target], ref isPlusLimit, ref isMinusLimit);

                if (isPlusLimit || isMinusLimit)
                    EzModel.ServoStop(Selected_ServoIndex[(int)target]);

                // 색상 값 설정
                int limitIndex = (int)target * 2;
                Selected_LimitCheck[limitIndex] = isPlusLimit ? "OrangeRed" : "White";
                Selected_LimitCheck[limitIndex + 1] = isMinusLimit ? "OrangeRed" : (isHomeOK ? "LawnGreen" : "White"); 
            }
        }
        private void UpdateTimer_DoWork(object sender, EventArgs e)
        {
            try
            {
                // 현재 위치값 갱신
                LimitCheck(ServoTarget.X);
                LimitCheck(ServoTarget.Y);
                LimitCheck(ServoTarget.Z);

                // 움직임이 멈추면 타이머 종료
                if (IsMoveDone(ServoTarget.X) == true && IsMoveDone(ServoTarget.Y) == true && IsMoveDone(ServoTarget.Z) == true)
                    UpdateTimer.Stop();
            }
            catch (Exception ex)
            {
                Global.ExceptionLog.Error($"UpdateTimer_DoWork Exception : {ex}");
            }
        }
        private void OnServoMove_Command(object obj)
        {
            switch (obj.ToString())
            {
                case "Home":
                    MoveAllHome();
                    break;
                case "Stop":
                    MoveAllStop();
                    break;
                case "AlarmReset":
                    AlarmAllReset();
                    return;
                case "Move_X":
                    if (MessageBox.Show($"Do you want to move servo to target position?", "Servo Move", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    {
                        break;
                    }
                    EzModel.MoveABS(Selected_ServoIndex[(int)ServoTarget.X], Target_Position[(int)ServoTarget.X]);
                    break;
                case "Move_Y":
                    if (MessageBox.Show($"Do you want to move servo to target position?", "Servo Move", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    {
                        break;
                    }
                    EzModel.MoveABS(Selected_ServoIndex[(int)ServoTarget.Y], Target_Position[(int)ServoTarget.Y]);
                    break;
                case "Move_Z":
                    if (MessageBox.Show($"Do you want to move servo to target position?", "Servo Move", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    {
                        break;
                    }
                    EzModel.MoveABS(Selected_ServoIndex[(int)ServoTarget.Z], Target_Position[(int)ServoTarget.Z]);
                    break;
                case "Jog_X_CW":
                    MoveJog(JogType.X_CCW);
                    break;
                case "Jog_X_CCW":
                    MoveJog(JogType.X_CW);
                    break;
                case "Jog_Y_CW":
                    MoveJog(JogType.Y_CCW);
                    break;
                case "Jog_Y_CCW":
                    MoveJog(JogType.Y_CW);
                    break;
                case "Jog_Z_CW":
                    MoveJog(JogType.Z_CCW);
                    break;
                case "Jog_Z_CCW":
                    MoveJog(JogType.Z_CW);
                    break;
                case "Jog_Stop":
                    MoveJog(JogType.Stop);
                    break;
                case "Jog_Speed":
                    if (JogSpeed == "Low")
                    {
                        JogSpeed = "Mid";
                        JogSpeedIndex = 1;
                    }
                    else if (JogSpeed == "Mid")
                    {
                        JogSpeedIndex =2;
                        JogSpeed = "High";
                    }
                    else
                    {
                        JogSpeedIndex = 0;
                        JogSpeed = "Low";
                    }
                    break;
            }
            //if (SingletonManager.instance.Servo_Model[iSlaveNo].IsEzConnected == true && UpdateTimer.IsEnabled == false)
            if (UpdateTimer.IsEnabled == false)
                UpdateTimer.Start();
        }
        private void MoveJog(JogType jog)
        {
            switch (jog)
            {
                case JogType.X_CW:
                    EzModel.MoveJog(Selected_ServoIndex[(int)ServoTarget.X], (int)Direction.CCW, JogSpeedIndex);
                    break;
                case JogType.X_CCW:
                    EzModel.MoveJog(Selected_ServoIndex[(int)ServoTarget.X], (int)Direction.CW, JogSpeedIndex);
                    break;
                case JogType.Y_CW:
                    EzModel.MoveJog(Selected_ServoIndex[(int)ServoTarget.Y], (int)Direction.CW, JogSpeedIndex);
                    break;
                case JogType.Y_CCW:
                    EzModel.MoveJog(Selected_ServoIndex[(int)ServoTarget.Y], (int)Direction.CCW, JogSpeedIndex);
                    break;
                case JogType.Z_CW:
                    EzModel.MoveJog(Selected_ServoIndex[(int)ServoTarget.Z], (int)Direction.CW, JogSpeedIndex);
                    break;
                case JogType.Z_CCW:
                    EzModel.MoveJog(Selected_ServoIndex[(int)ServoTarget.Z], (int)Direction.CCW, JogSpeedIndex);
                    break;
                case JogType.Stop:
                    for (int i = 0; i < Selected_UnitExist.Count; i++)
                    {
                        if (Selected_UnitExist[i] == true)
                            EzModel.ServoStop(Selected_ServoIndex[i]);
                    }
                    break;
            }
        }
        private void OnTeachingSave_Command(object obj)
        {
            if (MessageBox.Show($"Do ypu want to save teaching data.", "Save Teaching", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
            {
                return;
            }
            string teachFilePath = Path.Combine(Global.instance.IniTeachPath);
            var iniFile = new IniFile(teachFilePath);
            string section = obj.ToString();
            if (section == "Lift")
            {
                iniFile.Write($"{((Teaching_List)(TeachingIndex + (Selected_LiftIndex * 3))).ToString()}", Current_Position[(int)ServoTarget.Z], section);
                TeachPosition[(int)TeachingSection.Lift] = Current_Position[(int)ServoTarget.Z];
                SingletonManager.instance.Teaching_Data[((Teaching_List)TeachingIndex + (Selected_LiftIndex * 3)).ToString()] = Current_Position[(int)ServoTarget.Z];
            }
            else if (section == "Top_X_Handler")
            {
                iniFile.Write($"{((Teaching_List)TeachingIndex).ToString()}", Current_Position[(int)ServoTarget.X], section);
                TeachPosition[(int)TeachingSection.Top_Handler_X] = Current_Position[(int)ServoTarget.X];
                SingletonManager.instance.Teaching_Data[((Teaching_List)TeachingIndex).ToString()] = Current_Position[(int)ServoTarget.X];
            }
            else if (section == "Out_Y_Handler")
            {
                iniFile.Write($"{((Teaching_List)TeachingIndex).ToString()}", Current_Position[(int)ServoTarget.Y], section);
                TeachPosition[(int)TeachingSection.Out_Handler_Y] = Current_Position[(int)ServoTarget.Y];
                SingletonManager.instance.Teaching_Data[((Teaching_List)TeachingIndex).ToString()] = Current_Position[(int)ServoTarget.Y];
            }
            else if (section == "Out_Z_Handler")
            {
                iniFile.Write($"{((Teaching_List)TeachingIndex).ToString()}", Current_Position[(int)ServoTarget.Z], section);
                TeachPosition[(int)TeachingSection.Out_Handler_Z] = Current_Position[(int)ServoTarget.Z];
                SingletonManager.instance.Teaching_Data[((Teaching_List)TeachingIndex).ToString()] = Current_Position[(int)ServoTarget.Z];
            }
        }
        private void OnTeachingMove_Command(object obj)
        {
            string cmd = obj.ToString();
            switch (cmd)
            {
                case "Top_X_Handler":
                    if (MessageBox.Show($"Do you want to move servo to target position?", "Servo Move", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    if ( EzModel.IsOutHandlerPickupPosY() == true)
                    {
                        Global.instance.ShowMessagebox("Pleass Check Servo Y Position");
                        return;
                    }
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                    if (EzModel.MoveABS((int)ServoSlave_List.Top_X_Handler_X, TeachPosition[(int)TeachingSection.Top_Handler_X])==false)
                    {
                        Global.instance.ShowMessagebox("Top X Servo Move Fail");
                    }
                    break;
                case "Out_Y_Handler":
                    if (MessageBox.Show($"Do you want to move servo to target position?", "Servo Move", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    if (EzModel.IsOutHandlerReadyDoneZ() == false)
                    {
                        Global.instance.ShowMessagebox("Z is not ready position.");
                        return;
                    }
                    if (EzModel.IsTopHandlerPutDownPos() == true)
                    {
                        Global.instance.ShowMessagebox("Top Handler is not Ready position.");
                        return;
                    }
                    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_X_LEFT_CYL_SS] != true)
                    {
                        Global.instance.ShowMessagebox("Bottom Handler is not Left Position.");
                        return;
                    }
                    if (EzModel.MoveABS((int)ServoSlave_List.Out_Y_Handler_Y, TeachPosition[(int)TeachingSection.Out_Handler_Y])== false)
                    {
                        Global.instance.ShowMessagebox("Loading Y Servo Move Fail");
                    }
                    break;
                case "Out_Z_Handler":
                    if (MessageBox.Show($"Do you want to move servo to target position?", "Servo Move", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    if (EzModel.MoveABS((int)ServoSlave_List.Out_Z_Handler_Z, TeachPosition[(int)TeachingSection.Out_Handler_Z])== false)
                    {
                        Global.instance.ShowMessagebox("Loading Z Servo Move Fail");
                    }
                    break;
                case "Lift":
                    if (MessageBox.Show($"Do you want to move servo to target position?", "Servo Move", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    if (EzModel.MoveABS((int)ServoSlave_List.Lift_1_Z + Selected_LiftIndex, TeachPosition[(int)TeachingSection.Lift])== false)
                    {
                        Global.instance.ShowMessagebox("Lift Serfo Move Fail");
                    }
                    break;
            }
            if (UpdateTimer.IsEnabled == false)
                UpdateTimer.Start();
        }
        
        #region // override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            ServoMove_Command = new RelayCommand(OnServoMove_Command);
            TeachingMove_Command = new RelayCommand(OnTeachingMove_Command);
            TeachingSave_Command = new RelayCommand(OnTeachingSave_Command);
        }
        protected override void DisposeManaged()
        {
            ServoMove_Command = null;
            TeachingSave_Command = null;
            TeachingMove_Command = null;

            UnitList.Clear();
            UnitList = null;

            // Update 타이머 정지 및 해제
            if (UpdateTimer != null)
            {
                UpdateTimer.Stop();
                UpdateTimer.Tick -= UpdateTimer_DoWork;
                UpdateTimer = null;
            }
            base.DisposeManaged();
        }
        #endregion
    }
}