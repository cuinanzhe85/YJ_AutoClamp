using Common.Commands;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Input;
using YJ_AutoClamp.Utils;

namespace YJ_AutoClamp.ViewModels
{
    public class Data_ViewModel : Child_ViewModel
    {
        #region // ICommand Property
        public ICommand TileButton_Command { get; private set; }
        #endregion

        #region // PopupManager
        enum DataMenu_PopupList
        {
            System,
            //Model,
            Serial,
            Jog,
            Motor
        }
        private readonly Dictionary<DataMenu_PopupList, Func<(Window, Child_ViewModel)>> PopupFactories;
        #endregion

        #region // Properties

        private bool _IsTileSelected;
        public bool IsTileSelected
        {
            get { return _IsTileSelected; }
            set { SetValue(ref _IsTileSelected, false); }
        }
        #endregion

        public Data_ViewModel()
        {
            PopupFactories = new Dictionary<DataMenu_PopupList, Func<(Window, Child_ViewModel)>>
            {
                { DataMenu_PopupList.System, () => (new SystemData_View(), new SystemData_ViewModel()) },
                { DataMenu_PopupList.Serial, () => (new TcpSerial_View(), new TcpSerial_ViewModel()) },
                { DataMenu_PopupList.Jog, () => (new JogVelocity_View(), new JogVelocity_ViewModel()) },
                { DataMenu_PopupList.Motor, () => (new MotorVelocity_View(), new MotorVelocity_ViewModel()) },
            };
        }
        private void OnTileButton_Command(object obj)
        {
            if (Enum.TryParse(obj.ToString(), out DataMenu_PopupList popup))
            {
                PopupManager.ShowPopupView(PopupFactories, popup);
            }
        }
        #region // override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            // RelayCommand
            TileButton_Command = new RelayCommand(OnTileButton_Command);
        }
        protected override void DisposeManaged()
        {
            TileButton_Command = null;

            base.DisposeManaged();
        }
        #endregion
    }
}
