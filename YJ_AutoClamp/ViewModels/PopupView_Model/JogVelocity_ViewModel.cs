using Common.Commands;
using Common.Managers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels
{
    public class JogVelocity_ViewModel : Child_ViewModel
    {
        #region// ICommands
        public ICommand Save_Command { get; private set; }
        public ObservableCollection<Servo_Model> ServoModels { get; set; }
        #endregion
        public JogVelocity_ViewModel()
        {
            // SingletonManager의 Servo_Model 데이터를 복사하여 초기화 (0번째 인덱스 제외)
            ServoModels = new ObservableCollection<Servo_Model>(
                SingletonManager.instance.Servo_Model
                .Select(servo => new Servo_Model(servo.ServoName)
                {
                    JogVelocity = servo.JogVelocity,
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
            string section = "Jog_Velocity";

            // 인덱스를 문자열로 매핑
            string[] jogLabels = { "LOW", "MIDDLE", "HIGH" };

            // SingletonManager의 Servo_Model 데이터를 업데이트하고 INI 파일에 기록
            for (int i = 0; i < SingletonManager.instance.Servo_Model.Count; i++)
            {
                var originalServo = SingletonManager.instance.Servo_Model[i];
                var updatedServo = ServoModels[i];

                // 원래 데이터에 변경된 JogVelocity 값 반영
                originalServo.JogVelocity = new List<double>(updatedServo.JogVelocity);

                // INI 파일에 기록
                string servoName = originalServo.ServoName.ToString();
                for (int j = 0; j < originalServo.JogVelocity.Count; j++)
                {
                    if (j < jogLabels.Length) // 인덱스가 jogLabels 범위 내에 있는지 확인
                    {
                        iniFile.Write($"{servoName}_JogVelocity_{jogLabels[j]}", originalServo.JogVelocity[j].ToString("F2"), section);
                    }
                }
            }

            MessageBox.Show("Jog velocity settings have been successfully saved.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
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
