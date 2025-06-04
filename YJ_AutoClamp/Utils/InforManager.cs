using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Windows;
using static YJ_AutoClamp.Models.MESSocket;

namespace YJ_AutoClamp.Utils
{
    public class InforManager
    {
        private static InforManager instance;
        public static InforManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InforManager();
                }
                return instance;
            }
        }
        [JsonProperty]
        public string ModelName = "MODEL_DEFUALT";
        [JsonProperty]
        public string LineName = "LINE";
        [JsonProperty]
        public string GMESIP = "192.168.0.1";
        [JsonProperty]
        public string GMESPORT = "5000";
        #region ["COM"]
        [JsonProperty]
        public string GMES_SCAN_COMPORT = "COM10";

        [JsonProperty]
        public bool shouldBlockContinuousScanFail = false;
        [JsonProperty]
        public string TPPort = "COM5";
        [JsonProperty]
        public string JIG1Port = "COM13";
        [JsonProperty]
        public string JIG2Port = "COM14";
        [JsonProperty]
        public string JIG3Port = "COM15";
        [JsonProperty]
        public string JIG4Port = "COM16";
        [JsonProperty]
        public string JIG5Port = "COM17";
        [JsonProperty]
        public string JIG6Port = "COM18";
        [JsonProperty]
        public string Boot1Port = "COM21";
        [JsonProperty]
        public string Boot2Port = "COM23";
        [JsonProperty]
        public string Boot3Port = "COM25";
        [JsonProperty]
        public string Boot4Port = "COM27";
        [JsonProperty]
        public string Boot5Port = "COM29";
        [JsonProperty]
        public string Boot6Port = "COM31";
        [JsonProperty]
        public string SetDirect1Port = "COM22";
        [JsonProperty]
        public string SetDirect2Port = "COM24";
        [JsonProperty]
        public string SetDirect3Port = "COM26";
        [JsonProperty]
        public string SetDirect4Port = "COM28";
        [JsonProperty]
        public string SetDirect5Port = "COM30";
        [JsonProperty]
        public string SetDirect6Port = "COM32";
        [JsonProperty]
        public string PowerSupplyPort = "COM19";
        [JsonProperty]
        public double m_dPowerSupplyVoltage = 5f;
        [JsonProperty]
        public double modemBootTime = 7f;
        [JsonProperty]
        public bool magazineLabelReadingUse = false;
        [JsonProperty]
        public int magazineLabelLength = 17;
        [JsonProperty]
        public string[] SetCameraPort = new string[2] { "COM10", "COM11" };
        public string SKUPGMPort = "COM50";
        #endregion
        [JsonProperty]
        public int TRAY_USE_SIDE = 0;  // 0 : BOTH 1:LEFT 2: RIGHT
        [JsonProperty]
        public double m_dTrayHeight = 50f;

        [JsonProperty]
        public double m_iBTDeleayTime = 3f;
        [JsonProperty]
        public double m_dSetPowerOnTime = 5f;

        //[JsonProperty]
        //public bool m_bWorkSideReset = true;
        [JsonProperty]
        public bool m_bMagazinePickUpUse = false;
        [JsonProperty]
        public double m_dInBatteryTime = 20f;
        [JsonProperty]
        public double[] m_d1TrayPitch = new double[2] { 27, 27 };
        [JsonProperty]
        public bool m_bUnloaderReverse = false;
        [JsonProperty]
        public bool m_bOutputCVReverse = false;
        [JsonProperty]
        public bool m_bTrayVinyleCheck = false;
        [JsonProperty]
        public char m_cAgvLineNumber = '1';
        [JsonProperty]
        public char m_cAgvRfIdL = '1';
        [JsonProperty]
        public bool shouldPackRetry = false;
        [JsonProperty]
        public bool shouldScanRetry = false;
        [JsonProperty]
        public bool m_bNADMode = false;
        [JsonProperty]
        public bool m_bOneByOne = false;
        [JsonProperty]
        public bool m_bTrayHeightCheck = true; // TRAY 투입 간 높이체크 ( 리밋 ) 센서 확인
        //   [JsonProperty]
        //   public int ScanType = 0; //LOADER 의 경우 PACK JIG 만 사용한다.
        [JsonProperty]
        public GMES_MODE m_dGMESMode = GMES_MODE.USE;
        public bool IsGmesUse = false;
        public bool IsMESAPIUse = false;
        public bool IsSKUMappingUse = false;
        [JsonProperty]
        public bool vBusTriggerUse = false;
        [JsonProperty]
        public bool shouldRebootSet = false;
        [JsonProperty]
        public bool m_bSetBTMode = false;
        [JsonProperty]
        public bool ShouldAirplaneModeOff = false;
        public bool m_iDoor = true;
        public bool m_bSafetyMode = false;
        [JsonProperty]
        public bool m_iBuzzer = true;
        #region ["OFFSET"]
        [JsonProperty]
        public float m_dLStackingLiftOffset = 0;
        [JsonProperty]
        public float m_dRStackingLiftOffset = 0;
        [JsonProperty]
        public float m_dLTrayLiftOffset = 0;
        [JsonProperty]
        public float m_dRTrayLiftOffset = 0;
        [JsonProperty]
        public double m_dReverseOffsetX = 0;
        [JsonProperty]
        public double m_dReverseOffsetY = 0;
        [JsonProperty]
        public double m_dReverseOffsetT = 0;
        [JsonProperty]
        public double m_dMagazineOffsetX = 0;
        [JsonProperty]
        public double m_dMagazineOffsetY = 0;
        [JsonProperty]
        public double m_dMagazineOffsetZ = 0;

        [JsonProperty]
        public bool[] m_dStackingLiftOutput = new bool[2]; //Stacking Lift가 배출 중이였으면 true 아니면 false
        #endregion
        #region ["AGV"]
        [JsonProperty]
        public string m_sAGVCallIP = "127.0.0.1";
        [JsonProperty]
        public string m_sAGVCallPort = "1000";
        #endregion
        #region ["OPTION"]
        [JsonProperty]
        public int m_dStackingLiftStackCount = 10; // STACKING LIFT 적재 카운트
        [JsonProperty]
        public int RateBlock_MinCount = 0;  // SET JIG BLOCK N번 연속불량시 BLOCK
        [JsonProperty]
        public float RateBlock_NGRate = 0;  // SET JIG 불량률 N%이상시 BLOCK
        [JsonProperty]
        public bool LOADER_PICK_UP_MODE = false; // FALSE : VACCUM->GRIP(해외법인)  TRUE :  ONLY GRIP(국내)
        [JsonProperty]
        public bool AGV_USE = true; // DEFAULT : USE  
        [JsonProperty]
        public bool GEIM_USE = false; // GEIM
        [JsonProperty]
        public bool VISION_USE = false; //Tray Lift 세트 상태 판단 Vision 사용 여부
        [JsonProperty]
        public string VisionProgramPath = string.Empty;
        [JsonProperty]
        public double m_dTrayLiftTrayDetectSpeed = 40; // ( TRAY LIFT / STACKING LIFT ) TRAY DETECT(위치검색) SPEED
        [JsonProperty]
        public double m_dStackingLiftTrayDetectSpeed = 40; // ( TRAY LIFT / STACKING LIFT ) TRAY DETECT(위치검색) SPEED
        #endregion


        public void LoadSetting()
        {
            if (File.Exists(Config.SystemFilePath))
            {
                string _s = File.ReadAllText(Config.SystemFilePath, Encoding.UTF8);
                try
                {
                    instance = JsonConvert.DeserializeObject<InforManager>(_s);
                    instance.m_iDoor = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Info SystemFile need to check");
                }
            }
            else
            {
                Directory.CreateDirectory(Config.SystemSavePath);
                SaveSettings();
            }
        }
        public void SaveSettings()
        {
            try
            {
                string _sTestSpecString = JsonConvert.SerializeObject(Instance);
                string _sTestSpecStringIndeneted = JToken.Parse(_sTestSpecString).ToString(Formatting.Indented);
                File.WriteAllText(Config.SystemFilePath, _sTestSpecStringIndeneted);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }
    }
    public class Config
    {

        //Infor manager
        public static string FolderPath = @"C:\FA\AUTOSETLOADER-C-V3\";
        public static string SystemSavePath = FolderPath + @"System\Manager\";
        public static string SystemFilePath = SystemSavePath + "SystemManager.json";

        //servo parameter
        public static string ServoSavePath = FolderPath + @"Info\Axis\";
        public static string ServoFilePath = ServoSavePath + "axis.json";

        //Model position
        public static string ModelSavePath = FolderPath + @"Info\Model\";
        //public static string ModelFilePath = ModelSavePath + InforManager.Instance.ModelName + ".json";

        // GMES
        public static string DGSLogPath = @"C:\DGS\LOGS\";
        //Log Data
        public static string LogException = @"C:\FA\AUTOSETLOADER-C-V3\Log\Exception\";
        public static string LogDevice = @"C:\FA\AUTOSETLOADER-C-V3\Log\DevLog\";
        public static string LogError = @"C:\FA\AUTOSETLOADER-C-V3\Log\Error\";
        public static string LogDataSave = @"C:\FA\AUTOSETLOADER-C-V3\Log\DataSave\";
        public static string LogAGVCall = @"C:\FA\AUTOSETLOADER-C-V3\Log\AGVCall\";
        public static string VisionRecevieDataSave = @"C:\FA\AUTOSETLOADER-C-V3\Log\Vision\";
        public static string NGOUTLogDataSave = @"C:\FA\AUTOSETLOADER-C-V3\Log\NGOut\";
        public static string ThreadStepLogDataSave = @"C:\FA\AUTOSETLOADER-C-V3\Log\ThreadStep\";
        public static string MESLogDataSave = @"C:\FA\AUTOSETLOADER-C-V3\Log\MES\";
        public static string PhoneLogDataSave = @"C:\FA\AUTOSETLOADER-C-V3\Log\Phone\";
        //Data Running
        public static string SysTemRunning = @"C:\FA\AUTOSETLOADER-C-V3\System\DataRunning\";
    }
}
