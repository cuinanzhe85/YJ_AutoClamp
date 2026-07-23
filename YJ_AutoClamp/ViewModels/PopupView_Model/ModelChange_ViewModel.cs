using Common.Commands;
using Common.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YJ_AutoClamp.ViewModels
{
    public class ModelChange_ViewModel : Child_ViewModel
    {
        #region // Properties
        public ICommand VirtualKey_Command { get; private set; }
        public ICommand FileLoad_Command { get; private set; }
        public ICommand ModelSave_Command { get; private set; }
        public ICommand ModelChange_Command { get; private set; }
        public ICommand ModelNewDelete_Command { get; private set; }
        public ICommand ScrewHeight_Command { get; private set; }
        #endregion
        public ObservableCollection<string> ModelList { get; set; }
        private string _TeachFileName = string.Empty;
        public string TeachFileName
        {
            get { return _TeachFileName; }
            set { SetValue(ref _TeachFileName, value); }
        }
        private string _ModelBcrData = string.Empty;
        public string ModelBcrData
        {
            get { return _ModelBcrData; }
            set { SetValue(ref _ModelBcrData, value); }
        }
        private int _SelectedFilelIndex = 99;
        public int SelectedFilelIndex
        {
            get { return _SelectedFilelIndex; }
            set
            {
                SetValue(ref _SelectedFilelIndex, value);

                if (value != -1 && value != 99)
                {
                    LoadTeachFile(value);
                }
            }
        }
        private string _SelectedFileNmae = string.Empty;
        public string SelectedFileNmae
        {
            get { return _SelectedFileNmae; }
            set { SetValue(ref _SelectedFileNmae, value); }
        }
        public string LastFilePath { get; set; }
        public ModelChange_ViewModel()
        {
            // Collection Init
            ModelList = new ObservableCollection<string>();

            // Model 폴더의 모든 파일 이름 읽기
            var files = Directory.GetFiles(Global.instance.IniTeachPath);
            // ModelList 초기화 및 파일 이름 추가
            foreach (var file in files)
            {
                ModelList.Add(Path.GetFileNameWithoutExtension(file)); // 경로를 제외한 파일 이름만 추가
            }

            // 현재 모델 Default로 초기화
            string current_Teach = SingletonManager.instance.TeachFileName;
            // ModelList에서 current_model 검색
            int index = ModelList.IndexOf(current_Teach);
            if (index != -1)
            {
                SelectedFilelIndex = index; // current_model이 존재하면 SelectedIndex 설정
            }
        }
        public void LoadTeachFile(int index)
        {
            // Teach 데이터는 마지막에 Change눌렀을때 적용해도됨.
            try
            {
                // 선택된 모델 이름 가져오기
                string modelName = ModelList[index];
                string modelFolder = Path.Combine(Global.instance.IniConfigPath, "Teach");
                string modelFilePath = Path.Combine(modelFolder, modelName + ".ini");

                // 파일이 없으면 리턴
                if (!File.Exists(modelFilePath))
                    return;
                var myIni = new IniFile(modelFilePath);
                string section = "MODEL_DATA";
                ModelBcrData = myIni.Read("BCR", section);
                TeachFileName = modelName;
            }
            catch (Exception ex)
            {
                // 예외 처리
                Global.instance.ShowMessagebox($"An error occurred while loading the model file: {ex.Message}");
            }
        }
        private void OnModelNewDelete_Command(object obj)
        {
            string cmd = obj.ToString();

            try
            {
                // 새로만들기를 선택했다면
                if (cmd == "New")
                {
                    SelectedFilelIndex = -1;
                    // 파일이 없으면 새로 생성
                    string sorcePath = Path.Combine(Global.instance.IniTeachPath, SingletonManager.instance.TeachFileName) + ".ini";
                    string destPath = Path.Combine(Global.instance.IniTeachPath, SelectedFileNmae) + ".ini";
                    File.Copy(sorcePath, destPath, false);
                    ModelList.Clear();
                    // Model 폴더의 모든 파일 이름 읽기
                    var files = Directory.GetFiles(Global.instance.IniTeachPath);
                    // ModelList 초기화 및 파일 이름 추가
                    foreach (var file in files)
                    {
                        ModelList.Add(Path.GetFileNameWithoutExtension(file)); // 경로를 제외한 파일 이름만 추가
                    }
                }
                // BCR 데이터 저장
                else if (cmd == "SAVE")
                {
                    string modelFilePath = Path.Combine(Global.instance.IniTeachPath, SelectedFileNmae) + ".ini";
                    var myIni = new IniFile(modelFilePath);
                    string section = "MODEL_DATA";
                    myIni.Write("BCR", ModelBcrData, section);
                }
                // 삭제를 선택했다면
                else
                {
                    // 선택된게 없다면 리턴
                    if (SelectedFilelIndex < 0 || SelectedFilelIndex >= ModelList.Count)
                        return;

                    string selectedModelName = ModelList[SelectedFilelIndex];
                    string modelFilePath = Path.Combine(Global.instance.IniTeachPath, selectedModelName) + ".ini";
                    string currentModel = SingletonManager.instance.TeachFileName;
                    // 현재 모델인지 확인
                    if (selectedModelName == currentModel)
                    {
                        Global.instance.ShowMessagebox("Cannot delete the current model. Please switch to another model before deleting");
                        return;
                    }

                    // 삭제 확인 메시지
                    if (Global.instance.ShowMessagebox($"Are you sure you want to delete the model '{selectedModelName}'?", false, false, false,true) == true)
                    {
                        // 파일 삭제
                        if (File.Exists(modelFilePath))
                        {
                            File.Delete(modelFilePath);
                        }
                        // ModelList에서 제거
                        ModelList.Remove(selectedModelName);
                        // 삭제 완료 메시지
                        Global.instance.ShowMessagebox($"Model '{selectedModelName}' has been deleted successfully", false);
                    }
                }
            }
            catch (Exception ex)
            {
                // 예외 처리
                Global.instance.ShowMessagebox($"An error occurred while deleting the model : {ex.Message}");
            }
        }
        private void OnModelChange_Command(object obj)
        {
            try
            {
                // 사용자가 Yes를 선택한 경우 저장 작업 수행
                if (Global.instance.ShowMessagebox($"Do you want to Model changes?", false, false,false,true) == true)
                {
                    // Write System File. Currunt Model
                    var myIni = new IniFile(Global.instance.IniSystemPath);
                    string section = "SYSTEM";
                    myIni.Write("CURRENT_TEACH", TeachFileName, section);

                    // Change SingletonManager Current Model Data
                    SingletonManager.instance.TeachFileName = TeachFileName;

                    // Teaching Data 섹션 데이터 로드
                    SingletonManager.instance.LoadTeachFile();
                    //if (SingletonManager.instance.SendToTcpClient((int)CMD.MDL_C, TeachFileName, true) == true)
                    //{
                    //    Global.instance.ShowMessagebox($"'{TeachFileName}' Model change success.", false);
                    //}
                    //else
                    //{
                    //    Global.instance.ShowMessagebox($"'{TeachFileName}' Model change failed.");
                    //}
                    Global.instance.ShowMessagebox($"'{TeachFileName}' Model change success.", false);
                }
            }
            catch (Exception ex)
            {
                // 예외 처리
                Global.instance.ShowMessagebox($"An error occurred while change the model file : {ex.Message}");
            }
        }
        #region // override
        protected override void InitializeCommands()
        {
            // Initialization logic here
            base.InitializeCommands();
            ModelChange_Command = new RelayCommand(OnModelChange_Command);
            ModelNewDelete_Command = new RelayCommand(OnModelNewDelete_Command);
        }
        protected override void DisposeManaged()
        {
            // Cleanup logic here
            VirtualKey_Command = null;
            FileLoad_Command = null;
            ModelSave_Command = null;
            ModelChange_Command = null;
            ModelNewDelete_Command = null;
            ScrewHeight_Command = null;

            ModelList.Clear();
            ModelList = null;

            base.DisposeManaged();
        }
        #endregion
    }
}
