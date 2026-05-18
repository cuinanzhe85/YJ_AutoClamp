using Common.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Timers;
using YJ_AutoClamp.Models;

namespace YJ_AutoClamp.ViewModels.PopupView_Model
{
    public class Step_Model : BindableAndDisposable
    {
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { SetValue(ref _Title, value); }
        }
        private int _BottomHandStep;
        public int BottomHandStep
        {
            get { return _BottomHandStep; }
            set { SetValue(ref _BottomHandStep, value); }
        }
        private int _TopHandStep;
        public int TopHandStep
        {
            get { return _TopHandStep; }
            set { SetValue(ref _TopHandStep, value); }
        }
        private int _OutHandStep;
        public int OutHandStep
        {
            get { return _OutHandStep; }
            set { SetValue(ref _OutHandStep, value); }
        }
        private int _AsingStep;
        public int AgingStep
        {
            get { return _AsingStep; }
            set { SetValue(ref _AsingStep, value); }
        }
        private int _InputCVStep;
        public int InputCVStep
        {
            get { return _InputCVStep; }
            set { SetValue(ref _InputCVStep, value); }
        }
        private int _ClampCVStep;
        public int ClampCVStep
        {
            get { return _ClampCVStep; }
            set { SetValue(ref _ClampCVStep, value); }
        }

        public Step_Model(string title)
        {
            this.Title = title;
        }
    }
    public class Step_ViewModel : Child_ViewModel
    {
        private ObservableCollection<Step_Model> _Step_Model;
        public ObservableCollection<Step_Model> Step_Model
        {
            get { return _Step_Model; }
            set { SetValue(ref _Step_Model, value); }
        }
        private Timer StepTimer;
        public Step_ViewModel()
        {
            // Product Count Model 생성 및 데이터 할당
            Step_Model = new ObservableCollection<Step_Model>();
            Step_Model.Add(new Step_Model("Auto Clamp"));

            StepTimer = new Timer(50); // 1초마다 업데이트
            StepTimer.Elapsed += UpdateStepDisplay;
            StepTimer.Start();
        }
        private void UpdateStepDisplay(object sender, ElapsedEventArgs e)
        {
            var unitModel = SingletonManager.instance.Unit_Model;
            if (unitModel.Count > 0)
            {
                Step_Model[0].BottomHandStep = (int)unitModel[(int)MotionUnit_List.Top_X].Bottom_Step;
                Step_Model[0].TopHandStep = (int)unitModel[(int)MotionUnit_List.Top_X].Top_Handle_Step;
                Step_Model[0].OutHandStep = (int)unitModel[(int)MotionUnit_List.Top_X].Out_Handle_Step;
                Step_Model[0].AgingStep = (int)unitModel[(int)MotionUnit_List.Top_X].AgingCVStep;
                Step_Model[0].InputCVStep = (int)unitModel[(int)MotionUnit_List.Top_X].In_Cv_Step;
                Step_Model[0].ClampCVStep = (int)unitModel[(int)MotionUnit_List.Top_X].Out_Cv_Step;
            }
        }
        #region // Override
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
        }
        protected override void DisposeManaged()
        {
            // ObservableCollection 해제
            
            if (Step_Model != null)
            {
                foreach (var item in Step_Model)
                    (item as IDisposable)?.Dispose();
                Step_Model.Clear();
                Step_Model = null;
            }
            if (StepTimer != null)
            {
                StepTimer.Stop();
                StepTimer.Elapsed -= UpdateStepDisplay;
                StepTimer.Dispose();
                StepTimer = null;
            }
            base.DisposeManaged();
        }
        #endregion
    }
}
