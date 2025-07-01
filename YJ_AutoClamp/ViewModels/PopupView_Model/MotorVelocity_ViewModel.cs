using Common.Commands;
using Common.Managers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels
{
    public class MotorVelocity_ViewModel : Child_ViewModel
    {
        #region// ICommands
        public ICommand Save_Command { get; private set; }
        #endregion
        public ObservableCollection<Servo_Model> ServoModels { get; set; }

        public MotorVelocity_ViewModel()
        {
            // SingletonManager의 Servo_Model 데이터를 복사하여 초기화 (0번째 인덱스 제외)
            ServoModels = new ObservableCollection<Servo_Model>(
                SingletonManager.instance.Servo_Model
                .Select(servo => new Servo_Model(servo.ServoName)
                {
                    Velocity = servo.Velocity,
                    Accelerate = servo.Accelerate,
                    Measurement_Vel = servo.Measurement_Vel,
                    Barcode_Vel = servo.Barcode_Vel,
                }));
        }
        private void OnSave_Command(object obj)
        {
            // DataGrid의 편집 상태 종료
            if (obj is DataGrid dataGrid)
            {
                // 현재 셀의 편집 상태 종료
                dataGrid.CommitEdit(DataGridEditingUnit.Cell, true);
                // 현재 행의 편집 상태 종료
                dataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            }

            var iniFile = new IniFile(Global.instance.IniVelocityPath);
            string section = "Motor_Velocity";

            // SingletonManager의 Servo_Model 데이터를 업데이트하고 INI 파일에 기록
            for (int i = 0; i < SingletonManager.instance.Servo_Model.Count; i++) // 0번째 제외
            {
                var originalServo = SingletonManager.instance.Servo_Model[i];
                var updatedServo = ServoModels[i];
                if (updatedServo.Accelerate > 1000)
                {
                    updatedServo.Accelerate = 1000;
                }
                // 원래 데이터에 변경된 값 반영
                originalServo.Velocity = updatedServo.Velocity;
                originalServo.Accelerate = updatedServo.Accelerate;
                originalServo.Decelerate = updatedServo.Accelerate; // Dec값은 Acc값과 동일하게 적용
                originalServo.Measurement_Vel = updatedServo.Measurement_Vel;
                originalServo.Barcode_Vel = updatedServo.Barcode_Vel;

                // INI 파일에 기록
                string servoName = originalServo.ServoName.ToString();
                iniFile.Write($"{servoName}_Velocity", originalServo.Velocity.ToString("F2"), section);
                iniFile.Write($"{servoName}_Accelerate", originalServo.Accelerate.ToString("D"), section);
                iniFile.Write($"{servoName}_Decelerate", originalServo.Accelerate.ToString("D"), section);
                iniFile.Write($"{servoName}_Measurement_Vel", originalServo.Measurement_Vel.ToString("F2"), section);
                iniFile.Write($"{servoName}_Barcode_Vel", originalServo.Barcode_Vel.ToString("F2"), section);
            }
            string massage = string.Empty;
            for (int i = 0; i < SingletonManager.instance.Servo_Model.Count; i++)
            {
                int acc = SingletonManager.instance.Servo_Model[i].Accelerate;
                int ret = SingletonManager.instance.Motion.SetServoAccel(i, acc);
                if (ret != 0)
                {
                    massage += $"{SingletonManager.instance.Servo_Model[i].ServoName} Accel set fail. Error code: {ret}";
                    Global.Mlog.Info(massage);
                    massage += "\r\n";
                }
                ret = SingletonManager.instance.Motion.SetServoDccel(i, acc);
                if (ret != 0)
                {
                    massage += $"{SingletonManager.instance.Servo_Model[i].ServoName} Dccel set fail. Error code: {ret}";
                    Global.Mlog.Info(massage);
                    massage += "\r\n";
                }
            }
            if (!string.IsNullOrEmpty(massage))
            {
                Global.instance.ShowMessagebox(massage, true);
                
                return;
            }
            Global.instance.ShowMessagebox("Motor velocity settings have been successfully saved.",false);
        }
        #region // override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            Save_Command = new RelayCommand(OnSave_Command);
        }
        protected override void DisposeManaged()
        {
            Save_Command = null;

            // ServoModels 정리
            if (ServoModels != null)
            {
                ServoModels.Clear();
                ServoModels = null;
            }
            base.DisposeManaged();
        }
        #endregion
    }
}
