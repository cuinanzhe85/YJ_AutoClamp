using Common.Commands;
using Common.Mvvm;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using YJ_AutoClamp.Models;
using YJ_AutoClamp.Utils;

namespace YJ_AutoClamp.ViewModels
{
    public class Safety_ViewModel : BindableAndDisposable
    {
        public ICommand Exit_ButtonCommand { get; private set; }
        public ICommand PreventCloseCommand { get; private set; }
        public ICommand DioSetCommand { get; private set; }

        private EziDio_Model _Dio { get; set; } = SingletonManager.instance.Dio;
        public EziDio_Model Dio
        {
            get { return _Dio; }
        }
        private EzMotion_Model_E Motion { get; set; } = SingletonManager.instance.Motion;
        private bool _IsTopmost = true; 
        public bool IsTopmost
        {
            get { return _IsTopmost; }
            set { SetValue(ref _IsTopmost, value); }
        }
        public Safety_ViewModel()
        {
            Global.Mlog.InfoFormat($"Safety Popup Open.");
            Global.instance.Set_TowerLamp(Global.TowerLampType.Error);
            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.BUZZER, true);

            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_START, false);
            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_STOP, false);
            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_RESET, true);

            Motion.ServoMovePause((int)ServoSlave_List.Out_Y_Handler_Y, 1);
            Motion.ServoMovePause((int)ServoSlave_List.Out_Z_Handler_Z, 1);
            Motion.ServoMovePause((int)ServoSlave_List.Top_X_Handler_X, 1);
        }
        private void OnDioSetCommand(object obj)
        {
            string cmd = obj.ToString();
            // Buzzer Y07
            if(cmd == "Buzzer")
            {
                if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.BUZZER] == false)
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.BUZZER, true);
                else
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.BUZZER, false);
            }
        }
        private void OnPreventClose(object obj) { }
        private void OnExit_Command(object obj)
        {
            Global.Mlog.Info("[USER] Safety 'Unlock' Button Click");

            if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.DOOR_FEEDBACK] == false
                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.FRONT_OP_EMERGENCY_FEEDBACK] == false
                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.REAR_OP_EMERGENCY_FEEDBACK] == false)
            {
                Global.instance.ShowMessagebox("SAFETY_PLC_POWER_OFF. Please Reset Swich Push.", false);
                return;
            }
            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.BUZZER, false);
            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_RESET, false);
            Global.instance.InspectionStop();

            Motion.ServoMovePause((int)ServoSlave_List.Out_Y_Handler_Y, 0);
            Motion.ServoMovePause((int)ServoSlave_List.Out_Z_Handler_Z, 0);
            Motion.ServoMovePause((int)ServoSlave_List.Top_X_Handler_X, 0);
            // Exit
            Global.instance.SafetyErrorMessage = string.Empty;
            WindowManager.Instance.CloseCommand.Execute("Safety");
        }
        #region override
        /// <summary>
        /// Initialize Commands
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            Exit_ButtonCommand = new RelayCommand(OnExit_Command);
            PreventCloseCommand = new RelayCommand(OnPreventClose);
            DioSetCommand = new RelayCommand(OnDioSetCommand);
        }
        protected override void DisposeManaged()
        {
            Exit_ButtonCommand = null;
            DioSetCommand = null;
            PreventCloseCommand = null;

            base.DisposeManaged();
        }
        #endregion //override
    }
}
