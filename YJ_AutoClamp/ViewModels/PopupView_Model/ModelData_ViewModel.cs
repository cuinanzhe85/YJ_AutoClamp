using Common.Commands;
using Common.Managers;
using Common.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace YJ_AutoClamp.ViewModels
{
    public class ModelData_ViewModel :Child_ViewModel
    {
        #region // Properties
        public ICommand VirtualKey_Command { get; private set; }
        public ICommand FileLoad_Command { get; private set; }
        public ICommand ModelSave_Command { get; private set; }
        public ICommand ModelChange_Command { get; private set; }
        public ICommand ModelDelete_Command { get; private set; }
        #endregion
        public ModelData_Model ModelData { get; set; }
        public ObservableCollection<SpecData_Model> SpecData { get; set; }
        public ObservableCollection<string> ModelList { get; set; }

        private int _SelectedModelIndex = 99;
        public int SelectedModelIndex
        {
            get { return _SelectedModelIndex; }
            set
            {
                SetValue(ref _SelectedModelIndex, value);

                if (value != -1 && value != 99)
                {
                    LoadModeFile(value);
                }
            }
        }
        public string LastFilePath { get; set; }
        
        public ModelData_ViewModel()
        {
            // Collection Init
            ModelData = new ModelData_Model();
            SpecData = new ObservableCollection<SpecData_Model>(); 
            ModelList = new ObservableCollection<string>();

            // Read Model Files
            string modelFolder = Path.Combine(Global.instance.IniConfigPath, "Model");
            // Model 폴더의 모든 파일 이름 읽기
            var files = Directory.GetFiles(modelFolder);
            // ModelList 초기화 및 파일 이름 추가
            foreach (var file in files)
            {
                ModelList.Add(Path.GetFileNameWithoutExtension(file)); // 경로를 제외한 파일 이름만 추가
            }

            // 현재 모델 Default로 초기화
            string current_model = Path.GetFileNameWithoutExtension(SingletonManager.instance.Current_Model.ModelFileName);
            // ModelList에서 current_model 검색
            int index = ModelList.IndexOf(current_model);
            if (index != -1)
            {
                SelectedModelIndex = index; // current_model이 존재하면 SelectedIndex 설정
            }
        }
        public void LoadModeFile(int index)
        {
            // Teach 데이터는 마지막에 Change눌렀을때 적용해도됨.
            try
            {
                // 선택된 모델 이름 가져오기
                string modelName = ModelList[index];
                string modelFolder = Path.Combine(Global.instance.IniConfigPath, "Model");
                string modelFilePath = Path.Combine(modelFolder, modelName + ".mod");

                // 파일이 없으면 리턴
                if (!File.Exists(modelFilePath))
                    return;

                // INI 파일 읽기
                ModelData.ModelFileName = modelName + ".mod";
                var lines = File.ReadAllLines(modelFilePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("SpecFileName="))
                    {
                        ModelData.SpecFileName = line.Substring("SpecFileName=".Length);
                    }
                    else if (line.StartsWith("JobFileName="))
                    {
                        ModelData.JobFileName = line.Substring("JobFileName=".Length);
                    }
                    else if (line.StartsWith("TeachFileName="))
                    {
                        ModelData.TeachFileName = line.Substring("TeachFileName=".Length);
                    }
                }

                // Spec File 읽기
                LoadSpecData();
            }
            catch (Exception ex)
            {
                // 예외 처리
                MessageBox.Show($"An error occurred while loading the model file: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnModelDelete_Command(object obj)
        {
            try
            {
                // 선택된게 없다면 리턴
                if (SelectedModelIndex < 0 || SelectedModelIndex >= ModelList.Count)
                    return;

                string selectedModelName = ModelList[SelectedModelIndex];
                string modelFolder = Path.Combine(Global.instance.IniConfigPath, "Model");
                string modelFilePath = Path.Combine(modelFolder, selectedModelName);
                string currentModel = Path.GetFileNameWithoutExtension(SingletonManager.instance.Current_Model.ModelFileName);
                // 현재 모델인지 확인
                if (selectedModelName == currentModel)
                {
                    MessageBox.Show("Cannot delete the current model. Please switch to another model before deleting.",
                                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 삭제 확인 메시지
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the model '{selectedModelName}'?",
                                                          "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // 파일 삭제
                    if (File.Exists(modelFilePath))
                    {
                        File.Delete(modelFilePath);
                    }

                    // ModelList에서 제거
                    ModelList.Remove(selectedModelName);
                    ModelData.Clear();
                    SpecData.Clear();

                    // 삭제 완료 메시지
                    MessageBox.Show($"Model '{selectedModelName}' has been deleted successfully.",
                                    "Delete Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // 예외 처리
                MessageBox.Show($"An error occurred while deleting the model: {ex.Message}",
                                "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnModelSave_Command(object obj)
        {
            // DataGrid의 편집 상태 종료
            if (obj is RadGridView dataGrid)
                dataGrid.CommitEdit();
            try
            {
                // 사용자에게 저장 여부를 묻는 메시지 박스 표시
                MessageBoxResult result = MessageBox.Show("Do you want to save your changes?", "Save Conform", MessageBoxButton.YesNo, MessageBoxImage.Question);

                // 사용자가 Yes를 선택한 경우 저장 작업 수행
                if (result == MessageBoxResult.Yes)
                {
                    // SpecFileName을 기준으로 .mod 확장자를 가진 파일 경로 생성
                    string modelFolder = Path.Combine(Global.instance.IniConfigPath, "Model");
                    string modelFileName = Path.GetFileNameWithoutExtension(ModelData.SpecFileName) + ".mod"; // 확장자 제거 후 mod 추가
                    string modelFilePath = Path.Combine(modelFolder, modelFileName);

                    // Model 파일이 존재하는지 확인
                    if (File.Exists(modelFilePath))
                    {
                        // 파일이 존재하면 내용을 업데이트
                        UpdateModelFile(modelFilePath);
                    }
                    else
                    {
                        // 파일이 없으면 새로 생성
                        CreateModelFile(modelFilePath);
                    }

                    // ModelList에 저장된 모델 이름 추가 (중복 방지)
                    string modelname = Path.GetFileNameWithoutExtension(ModelData.SpecFileName);
                    if (!ModelList.Contains(modelname))
                    {
                        ModelList.Add(modelname);
                        SelectedModelIndex = ModelList.IndexOf(modelname);
                    }

                    // SpecFile 저장. SpecFile은 불러와서 저장하기 때문에 파일이 존재함
                    string specFolder = Path.Combine(Global.instance.IniConfigPath, "Spec");
                    string specFilePath = Path.Combine(specFolder, ModelData.SpecFileName);
                    SaveSpecData(specFilePath);

                    // 저장 완료 메시지
                    MessageBox.Show("Model data was saved successfully.", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // 예외 처리
                MessageBox.Show($"An error occurred while saving model data: {ex.Message}", "Save Fail", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnModelChange_Command(object obj)
        {
            // DataGrid의 편집 상태 종료
            if (obj is RadGridView dataGrid)
                dataGrid.CommitEdit();

            try
            {
                // 사용자에게 저장 여부를 묻는 메시지 박스 표시
                MessageBoxResult result = MessageBox.Show("Do you want to save your changes?", "Save Conform", MessageBoxButton.YesNo, MessageBoxImage.Question);

                // 사용자가 Yes를 선택한 경우 저장 작업 수행
                if (result == MessageBoxResult.Yes)
                {
                    // Write System File. Currunt Model
                    var myIni = new IniFile(Global.instance.IniSystemPath);
                    string section = "SYSTEM";
                    myIni.Write("CURRENT_MODEL", ModelData.ModelFileName, section);

                    // Change SingletonManager Current Model Data
                    SingletonManager.instance.Current_Model.ModelFileName = ModelData.ModelFileName;
                    SingletonManager.instance.Current_Model.SpecFileName = ModelData.SpecFileName;
                    SingletonManager.instance.Current_Model.TeachFileName = ModelData.TeachFileName;

                    // Update Spec Data
                    SingletonManager.instance.Spec_Data.Clear();
                    SingletonManager.instance.Spec_Data = new ObservableCollection<SpecData_Model>
                    (
                        SpecData.Select(item => new SpecData_Model
                        {
                            Index = item.Index,
                            ScrewHeight = item.ScrewHeight,
                            LowLimit = item.LowLimit,
                            HighLimit = item.HighLimit,
                            X = item.X,
                            Y = item.Y,
                            Normal5GBoth = item.Normal5GBoth
                        })
                    );
                    // Teaching Data 섹션 데이터 로드
                    SingletonManager.instance.LoadTeachFile();

                }
            }
            catch (Exception ex)
            {
                // 예외 처리
                MessageBox.Show($"An error occurred while change the model file: {ex.Message}", "Model Change Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadSpecData()
        {
            string specFilePath = Path.Combine(Global.instance.IniSpecPath, ModelData.SpecFileName);

            var specLines = File.ReadAllLines(specFilePath);
            SpecData.Clear();
            foreach (var specLine in specLines.Skip(1))
            {
                // Spec 파일의 각 줄을 ','로 분리
                var parts = specLine.Split(',');

                if (parts.Length >= 6) // 최소한 필요한 데이터가 있는지 확인
                {
                    SpecData.Add(new SpecData_Model
                    {
                        Index = int.Parse(parts[0]),
                        X = parts[1],
                        Y = parts[2],
                        LowLimit = parts[3],
                        HighLimit = parts[4],
                        ScrewHeight = parts[5] == "0" ? "None" : parts[5] == "1" ? "Screw" : "Height",
                        Normal5GBoth = parts[6] == "0" ? "Normal" : parts[6] == "1" ? "5G" : "Both"
                    });
                }
            }
        }
        private void SaveSpecData(string specFilePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(specFilePath, false))
                {
                    writer.WriteLine("no,x,y,low,high,type,,"); // 헤더 작성

                    for (int i = 0; i < SpecData.Count; i++)
                    {
                        var spec = SpecData[i];
                        spec.Index = i + 1; // 인덱스 재정의

                        string screwHeightValue = spec.ScrewHeight == "None" ? "0" :
                                                  spec.ScrewHeight == "Screw" ? "1" : "2";

                        string normal5GBothValue = spec.Normal5GBoth == "Normal" ? "0" :
                                                   spec.Normal5GBoth == "5G" ? "1" : "2";

                        writer.WriteLine($"{spec.Index},{spec.X},{spec.Y},{spec.LowLimit},{spec.HighLimit},{screwHeightValue},{normal5GBothValue}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save SpecData: {ex.Message}");
            }
        }
        private void UpdateModelFile(string filePath)
        {
            try
            {
                // 기존 파일 내용을 업데이트
                var lines = File.ReadAllLines(filePath);
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("SpecFileName="))
                        {
                            writer.WriteLine($"SpecFileName={ModelData.SpecFileName}");
                        }
                        else if (line.StartsWith("JobFileName="))
                        {
                            writer.WriteLine($"JobFileName={ModelData.JobFileName}");
                        }
                        else if (line.StartsWith("TeachFileName="))
                        {
                            writer.WriteLine($"TeachFileName={ModelData.TeachFileName}");
                        }
                        else
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update the model file: {ex.Message}");
            }
        }
        private void CreateModelFile(string filePath)
        {
            try
            {
                // INI 형식으로 데이터 저장
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("[ModelData]");
                    writer.WriteLine($"SpecFileName={ModelData.SpecFileName}");
                    writer.WriteLine($"JobFileName={ModelData.JobFileName}");
                    writer.WriteLine($"TeachFileName={ModelData.TeachFileName}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create the model file: {ex.Message}");
            }
        }
        private void OnFileLoad_Command(object obj)
        {
            string cmd = obj.ToString();
            string operationFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            string targetFolder = Path.Combine(operationFolder, cmd);

            RadOpenFileDialog openFileDialog = new RadOpenFileDialog();
            StyleManager.SetTheme(openFileDialog, new MaterialTheme());
            openFileDialog.Multiselect = false;
            if (LastFilePath != string.Empty)
                openFileDialog.InitialDirectory = LastFilePath;

            if (cmd == "Spec")
                openFileDialog.Filter = "Model Files|*.csv";
            else if (cmd == "Job")
                openFileDialog.Filter = "Job Files|*.job";
            else
                openFileDialog.Filter = "Teach Files|*.ini";

            openFileDialog.ShowDialog();
            if (openFileDialog.DialogResult == true)
            {
                string fileName = openFileDialog.FileName;
                LastFilePath = Path.GetDirectoryName(fileName);

                // 명령어에 따라 대상 폴더 설정
                if (cmd == "Spec")
                {
                    targetFolder = Path.Combine(operationFolder, "Spec");
                    ModelData.SpecFileName = Path.GetFileName(fileName);
                }
                else if (cmd == "Job")
                {
                    targetFolder = Path.Combine(operationFolder, "Job");
                    ModelData.JobFileName = Path.GetFileName(fileName);
                }
                else if (cmd == "Teach")
                {
                    targetFolder = Path.Combine(operationFolder, "Teach");
                    ModelData.TeachFileName = Path.GetFileName(fileName);
                }

                // 대상 파일 경로 설정
                string targetFilePath = Path.Combine(targetFolder, Path.GetFileName(fileName));
                // 파일이 없을 경우에만 복사
                File.Copy(fileName, targetFilePath, true);

                if(cmd == "Spec")
                    LoadSpecData();
            }
        }
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private void OnVirtualKey_Command(object obj)
        {
            if (obj.ToString() == "Off")
            {
                var oskProcesses = Process.GetProcessesByName("osk");
                if (oskProcesses.Length == 0)
                {
                    return;
                }

                foreach (var proc in oskProcesses)
                {
                    try
                    {
                        proc.Kill();
                        proc.Dispose(); // 리소스 해제
                    }
                    catch (Exception ex)
                    {
                        Global.ExceptionLog.Error($"Failed to kill osk process: {ex.Message}");
                    }
                }
                return;
            }

            string oskPath = Path.Combine(Environment.SystemDirectory, "osk.exe");
            var running = Process.GetProcessesByName("osk");
            if (running.Length > 0)
            {
                IntPtr hWnd = running[0].MainWindowHandle;
                if (hWnd != IntPtr.Zero)
                    SetForegroundWindow(hWnd);

                foreach (var proc in running)
                {
                    proc.Dispose(); // 리소스 해제
                }
            }
            else
            {
                if (File.Exists(oskPath))
                    Process.Start(oskPath);
                else
                    MessageBox.Show("가상 키보드를 찾을 수 없습니다.");
            }
        }
        #region // override
        protected override void InitializeCommands()
        {
            // Initialization logic here
            base.InitializeCommands();
            VirtualKey_Command = new RelayCommand(OnVirtualKey_Command);
            FileLoad_Command = new RelayCommand(OnFileLoad_Command);
            ModelSave_Command = new RelayCommand(OnModelSave_Command);
            ModelChange_Command = new RelayCommand(OnModelChange_Command);
            ModelDelete_Command = new RelayCommand(OnModelDelete_Command);
        }
        protected override void DisposeManaged()
        {
            // Cleanup logic here
            VirtualKey_Command = null;
            FileLoad_Command = null;
            ModelSave_Command = null;
            ModelChange_Command = null;
            ModelDelete_Command = null;

            ModelData = null;
            SpecData.Clear();
            SpecData = null;
            ModelList.Clear();
            ModelList = null;

            base.DisposeManaged();
        }
        #endregion
    }

    public class ModelData_Model : BindableAndDisposable
    {
        private string _ModelFileName;
        public string ModelFileName
        {
            get { return _ModelFileName; }
            set { SetValue(ref _ModelFileName, value); }
        }
        private string _SpecFileName;
        public string SpecFileName
        {
            get { return _SpecFileName; }
            set { SetValue(ref _SpecFileName, value); }
        }
        private string _JobFileName;
        public string JobFileName
        {
            get { return _JobFileName; }
            set { SetValue(ref _JobFileName, value); }
        }
        private string _TeachFileName;
        public string TeachFileName
        {
            get { return _TeachFileName; }
            set { SetValue(ref _TeachFileName, value); }
        }
        public void Clear()
        {
            ModelFileName = string.Empty;
            SpecFileName = string.Empty;
            JobFileName = string.Empty;
            TeachFileName = string.Empty;
        }
    }
    public class SpecData_Model: BindableAndDisposable
    {
        private int _Index;
        public int Index 
        {
            get { return _Index; }
            set { SetValue(ref _Index, value); }
        }
        public string ScrewHeight { get; set; }
        public string LowLimit { get; set; }
        public string HighLimit { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Normal5GBoth { get; set; }
    }
}
