using Common.Commands;
using Common.Managers;
using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YJ_AutoClamp.Models;
using YJ_AutoClamp.Utils;

namespace YJ_AutoClamp.ViewModels
{
    public class MainWindow_ViewModel : BaseMainControlViewModel
    {
        #region // ICommand Property
        public ICommand BottomMenu_ButtonCommands { get; private set; }
        #endregion

        #region // ModulesManager Property
        public ModuleDescriptionCollection Modules { get; private set; }
        private Dictionary<string, Child_ViewModel> ViewModelCache = new Dictionary<string, Child_ViewModel>();

        private Child_ViewModel _MainContents_ViewModel;
        public Child_ViewModel MainContents_ViewModel
        {
            get { return _MainContents_ViewModel; }
            set { SetValue(ref _MainContents_ViewModel, value); }
        }
        #endregion

        #region // Popup Manager
        enum MainWindow_PopupList
        {
            Gocator
        }
        private readonly Dictionary<MainWindow_PopupList, Func<(Window, Child_ViewModel)>> PopupFactories;
        #endregion

        private string _DepartmentName = "Manufacturing Automation Group(Mobile)";
        public string DepartmentName
        {
            get { return _DepartmentName; }
            set { SetValue(ref _DepartmentName, value); }
        }
        private string _SoftwareName = "Auto Clmap Inspection";
        public string SoftwareName
        {
            get { return _SoftwareName; }
            set { SetValue(ref _SoftwareName, value); }
        }
        private string _SoftwareVersion = "[ Ver 1.0.0 ]";
        public string SoftwareVersion
        {
            get { return _SoftwareVersion; }
            set { SetValue(ref _SoftwareVersion, value); }
        }

        #region // Loading bacground
        private BackgroundWorker bgWorkerLoading;
        #endregion

        public MainWindow_ViewModel()
        {
            // View Module Manager 생성 및 초기화
            Modules = new ModuleDescriptionCollection();
            Modules.Add(new ModuleDescription() { ModuleType = typeof(Auto_ViewModel), ViewType = typeof(Auto_View) });
            Modules.Add(new ModuleDescription() { ModuleType = typeof(Data_ViewModel), ViewType = typeof(Data_View) });
            Modules.Add(new ModuleDescription() { ModuleType = typeof(Teach_ViewModel), ViewType = typeof(Teach_View) });
            Modules.Add(new ModuleDescription() { ModuleType = typeof(Log_ViewModel), ViewType = typeof(Log_View) });
            ModulesManager = new ModulesManager(new ViewsManager(Modules), Modules);

            // Loading Backgorund Thread
            bgWorkerLoading = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            bgWorkerLoading.DoWork += bgWorkerLoading_DoWork;
            bgWorkerLoading.RunWorkerCompleted += bgWorkerLoading_RunWorkerCompleted;
            bgWorkerLoading.RunWorkerAsync();
        }
        private void bgWorkerLoading_DoWork(object sender, DoWorkEventArgs e)
        {
            //Run
            SingletonManager.instance.Run();
            Application.Current.Dispatcher.BeginInvoke(
                (ThreadStart)(() =>
                {
                    MainContents_ViewModel = CreateAndCacheViewModel<Auto_ViewModel>("Auto");
                }), DispatcherPriority.Send);
        }
        private void bgWorkerLoading_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Loading End
            Global.instance.BusyStatus = false;
            Global.instance.BusyContent = string.Empty;
        }
        // ModulesManager 생성 및 캐시화 함수
        private TViewModel CreateAndCacheViewModel<TViewModel>(string key) where TViewModel : Child_ViewModel, new()
        {
            var _ViewModel = ModulesManager.CreateModule<TViewModel>(null, this);
            ViewModelCache[key] = _ViewModel;
            return _ViewModel;
        }
        // 캐시된 ViewModel 반환
        private TViewModel GetCachedViewModel<TViewModel>(string key) where TViewModel : Child_ViewModel
        {
            return ViewModelCache[key] as TViewModel;
        }
        // Center Contents 변경 함수
        public void ChangeMainContentsView<TViewModel>(string key) where TViewModel : Child_ViewModel, new()
        {
            // 현재 ViewModel이 동일한지 확인합니다.
            if (MainContents_ViewModel != null && ViewModelCache.ContainsKey(key) && ViewModelCache[key] == MainContents_ViewModel)
                return;

            // ViewModel이 캐시에 있는지 확인합니다.
            if (ViewModelCache.ContainsKey(key))
            {
                MainContents_ViewModel = GetCachedViewModel<TViewModel>(key);
            }
            else
            {
                // 새로운 ViewModel을 생성하고 캐시에 저장합니다.
                var newMainLeftViewModel = CreateAndCacheViewModel<TViewModel>(key);
                MainContents_ViewModel = newMainLeftViewModel;
            }
        }
        // 메인 하단 버튼동작 Commands
        private void OnBottomMenu_Commands(object obj)
        {
            if (obj == null) return;
            switch (obj.ToString())
            {
                case "Auto":
                    ChangeMainContentsView<Auto_ViewModel>("Auto");
                    break;
                case "Data":
                    ChangeMainContentsView<Data_ViewModel>("Data");
                    break;
                case "Teach":
                    ChangeMainContentsView<Teach_ViewModel>("Teach");
                    break;
                case "Log":
                    ChangeMainContentsView<Log_ViewModel>("Log");
                    break;
                case "Hide":
                    WindowManager.Instance.MinimizeCommand.Execute("Main");
                    break;
                case "Gocator":
                    PopupManager.ShowPopupView(PopupFactories, MainWindow_PopupList.Gocator);
                    break;
                case "Exit":
                    SoftwareExit();
                    break;
            }
        }
        private async void SoftwareExit()
        {
            Global.Mlog.Info($"---------- Software Exit ----------");
            Global.instance.BusyContent = "Please Wait.  Now Software Exit...";
            Global.instance.BusyStatus = true;

            // Soket & Thread Close
            await Task.Run(() =>
            {
                SingletonManager.instance.BackgroundThread_Stop();
                // Servo Stop
                for (int i = 0; i < (int)ServoSlave_List.Max; i++)
                {
                    SingletonManager.instance.Ez_Model.ServoStop(i);
                }
                // NMC Dio Thread Stop
                SingletonManager.instance.Ez_Dio.DioThreadStop();
                
                // Bcr Close
                SingletonManager.instance.Barcode_Model.Close();
                // Label Print Close
                SingletonManager.instance.LabelPrint_Model.Close();
            });

            // UI 스레드에서 Dispose 및 Shutdown 호출
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.Dispose();
                Application.Current.Shutdown();
            });
        }
        #region // override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            // RelayCommand
            BottomMenu_ButtonCommands = new RelayCommand(OnBottomMenu_Commands);
        }
        protected override void DisposeManaged()
        {
            BottomMenu_ButtonCommands = null;

            // Loading BackgroundWorker 해제
            bgWorkerLoading.DoWork -= bgWorkerLoading_DoWork;
            bgWorkerLoading.RunWorkerCompleted -= bgWorkerLoading_RunWorkerCompleted;
            bgWorkerLoading = null;

            // MainContents_ViewModel 해제
            if (MainContents_ViewModel != null)
            {
                MainContents_ViewModel.Dispose();
                MainContents_ViewModel = null;
            }
            // ViewModelCache에 저장된 모든 ViewModel 해제
            foreach (var _ViewModel in ViewModelCache.Values)
            {
                _ViewModel.Dispose();
            }
            ViewModelCache.Clear();

            // ModulesManager 해제
            ModulesManager = null;

            base.DisposeManaged();
        }
        #endregion
    }
}
