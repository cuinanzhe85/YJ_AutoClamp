using Common.Commands;
using System.Windows.Input;
using YJ_AutoClamp.Utils;

namespace YJ_AutoClamp.ViewModels
{
    public class EquipmentMode_ViewModel : Child_ViewModel
    {
        #region ICommands
        public ICommand ChangeMode_Command { get; private set; }
        #endregion

        private int _SelectedMode = (int)SingletonManager.instance.EquipmentMode;
        public int SelectedMode
        {
            get { return _SelectedMode; }
            set { SetValue(ref _SelectedMode, value); }
        }
        public EquipmentMode_ViewModel() { }

        public void OnChangeMode_Command(object obj)
        {
            SingletonManager.instance.EquipmentMode = (EquipmentMode)SelectedMode;
            WindowManager.Instance.CloseCommand.Execute("Select_Mode");
        }
        #region // override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            ChangeMode_Command = new RelayCommand(OnChangeMode_Command);
        }
        protected override void DisposeManaged()
        {
            ChangeMode_Command = null;
            base.DisposeManaged();
        }
        #endregion
    }
}
