using Common.Commands;
using Common.Mvvm;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        private EziDio_Model _Dio { get; set; } = SingletonManager.instance.Ez_Dio;
        public EziDio_Model Dio
        {
            get { return _Dio; }
        }
        private bool _IsTopmost = true;
        public bool IsTopmost
        {
            get { return _IsTopmost; }
            set { SetValue(ref _IsTopmost, value); }
        }
        public Safety_ViewModel()
        {
            Global.Mlog.InfoFormat($"Safety Popup Open.");

        }
        private async void OnDioSet_Command(object obj)
        {
            string cmd = obj.ToString();
            // Buzzer Y0C
            if(cmd == "Buzzer")
            {
                if (SingletonManager.instance.Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.BUZZER] == false)
                    SingletonManager.instance.Dio.Set_OutputData((int)EziDio_Model.DO_MAP.BUZZER, true);
                else
                    SingletonManager.instance.Dio.Set_OutputData((int)EziDio_Model.DO_MAP.BUZZER, false);
            }
            // Reset Y20
            else
            {
                SingletonManager.instance.Dio.Set_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_RESET, true);
                await Task.Delay(1000); // 비동기 대기
                SingletonManager.instance.Dio.Set_OutputData((int)EziDio_Model.DO_MAP.OP_BOX_RESET, false);
            }
        }
        private void OnPreventClose(object obj) { }
        private void OnExit_Command(object obj)
        {
            Global.Mlog.Info("[USER] Safety 'Unlock' Button Click");

            if (Dio.DI_RAW_DATA[(int)NmcDio_Model.DI_MAP.DOOR_OPEN_SAFETY_CHECK_FEEDBACK] == true)
            {
                MessageBox.Show("SAFETY_PLC_POWER_OFF. Please Reset Swich Push.", "Reset Push !", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            DioSetCommand = new RelayCommand(OnDioSet_Command);
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
