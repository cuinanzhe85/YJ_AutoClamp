using Common.Commands;
using Common.Managers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Data;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels
{
    public class AgingStep_ViewModel : Child_ViewModel
    {
        #region // ICommands
        public ICommand Reset_Command { get; private set; }
        #endregion
        public RadObservableCollection<Channel_Model> Channels
        {
            get { return SingletonManager.instance.Channel_Model; }
        }
        private ObservableCollection<Aging_Model> _AgingCvList = SingletonManager.instance.Display_AgingCv;
        public ObservableCollection<Aging_Model> AgingCvList
        {
            get { return _AgingCvList; }
        }
        public AgingStep_ViewModel()
        {
            InitializeCommands();
        }
        private void OnReset_Command(object obj)
        {
            if (MessageBox.Show($"Do you want to reset the production quantity?", "Product", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
            {
                return;
            }
            Channels[0].InputCount = "0";
            Channels[0].LoadCount = "0";
            Channels[0].AgingCvTotalCount = "0";

            var myIni = new IniFile(Global.instance.IniSystemPath);
            string section = "SYSTEM";
            myIni.Write("INPUT_COUNT", Channels[0].InputCount, section);
            myIni.Write("LOAD_COUNT", Channels[0].LoadCount, section);
            myIni.Write("AGING_CV_COUNT", Channels[0].AgingCvTotalCount, section);
        }
        #region override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            Reset_Command = new RelayCommand(OnReset_Command);
        }
        protected override void DisposeManaged()
        {
            Reset_Command = null;
            base.DisposeManaged();
        }
        #endregion
    }
}
