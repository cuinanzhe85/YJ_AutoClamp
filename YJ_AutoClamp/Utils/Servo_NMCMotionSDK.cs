//=============================================================================
//
// [ MMCE Motion SDK ]
//  : RSAutomation.co.,LTD. (2014-03-14)
//
// [ Motion DLL Version ]
//  : V12.2.0.19_D22.06.24
//
//=============================================================================

using System.Runtime.InteropServices;
using System.Text;

namespace YJ_AutoClamp.Utils
{
    static class NMCDEF
    {
        public const uint MAX_EEPROM_SIZE = 0xFFFF;  //16bit max (64k)
        public const uint ECAT_MAX_NAME = 0xFF;    //255
        public const uint MAX_BOARD_CNT = 4;
        public const uint MAX_AXIS_CNT = 64;
        public const uint MAX_AXIS_ID = 65535;   //Ver_0c010012_6
        public const uint MAX_NODE_CNT = 256;
        public const uint MAX_PHYSICAL_ADDR = 65535;   //Ver_0c010012_3
        public const uint MAX_MASTER_ID = 10;
        public const uint MAX_ERR_LEN = 128;
        public const uint MAX_PROFILE_ITEM_COUNT = 6;
        public const uint MAX_AXES_IN_GROUP = 8;       //v12.1.0.45
        public const uint MAX_AXES_GROUP_CNT = 32;
        public const uint MAX_AXES_STATUS_SIZE = 8;
        public const uint MAX_ALL_STATUS_SIZE = 1536;   //Ver_0c010019_1 : 계산식 = (MAX_AXIS_CNT + MAX_AXES_GROUP_CNT)

        public const uint MAX_LOGICAL_AXIS_COUNT = 64;  //v12.1.0.41
        public const uint MAX_LOGICAL_DEVICE_COUNT = 1024;  //v12.1.0.41
        public const uint MAX_DEVICE_NAME_SIZE = 256;   //v12.1.0.64

        public const ushort INVALID_BOARD = 0xFFFF;
        public const ushort INVALID_AXIS = 0xFFFF;
        public const ushort INVALID_MASTERID = 0xFFFF;
        public const uint INVALID_FLASH = 0xFFFFFFFF;

        //Axis Parameter Definition
        public const uint COMMANDED_POSITION = 1;
        public const uint SW_LIMIT_POS = 2;
        public const uint SW_LIMIT_NEG = 3;
        public const uint ENABLE_LIMIT_POS = 4;
        public const uint ENABLE_LIMIT_NEG = 5;
        public const uint ENABLE_POS_LAG_MONITORING = 6;
        public const uint MAX_POSITION_LAG = 7;
        public const uint MAX_VELOCITY_SYSTEM = 8;
        public const uint MAX_VELOCITY_APPL = 9;
        public const uint ACTUAL_VELOCITY = 10;
        public const uint COMMANDED_VELOCITY = 11;
        public const uint MAX_ACCELERATION_SYSTEM = 12;
        public const uint MAX_ACCELERATION_APPL = 13;
        public const uint MAX_DECELERATION_SYSTEM = 14;
        public const uint MAX_DECELERATION_APPL = 15;
        public const uint MAX_JERK = 16;
        public const uint ACTUAL_POSITION = 1001;
        public const uint PROFILE_DACCEL = 1100;

        public const uint IOMAP_HEADER_SIZE = 28; //DSP 0.13 이상에서 변경됨 2014.08.26 (32 - 4)
        public const uint IOPAGE_SIZE = 4096;
    }

    /// <summary>
    /// Library Class
    /// </summary>
    /// 
    public partial class NMCSDKLib
    {
        //DLL name should be specified what you have.
        public const string NMC_DLL_NAME = "NMC_Motion.dll";

        //axis_source status
        public enum MC_STATUS : uint
        {
            MC_OK = 0x00000000,

            //Device Driver Error Defines 
            MC_ERROR_HW_NOT_INSTALLED = 0x000000DC,
            MC_ERROR_DD_SEND_ERROR = 0x000000DD,
            MC_ERROR_DD_READ_ERROR = 0x000000DE,
            MC_DD_ERROR_SEND = 0x000000DF,
            MC_DD_ERROR_RECV = 0x000000E0,
            MC_DD_OPEN_FAIL = 0x000000E6,
            MC_DD_NOT_OPENED = 0x000000E7,
            MC_DD_CONN_FAIL = 0x000000E8,
            MC_DD_CLIENT_START_FAIL = 0x000000E9,
            MC_DD_OK = 0x00000000,
            MC_CN_NOT_CONNECTED = 0x000000F0,
            MC_CN_CONNECTED = 0x000000F1,
            MC_CN_CONNERROR = 0x000000F2,

            //PLCOpen Motion Command Response Error Defines (F/W)
            MC_INVALID_SYSTEM_STATE = 0x00010000,
            MC_UNSUPPORT_CMD = 0x00020000,
            MC_INVALID_AXIS_STATE_NOT_HOMING_MODE = 0x00020047, //v12.1.0.42 
            MC_UNSUPPORTED_CMD_AXIS_HOMING = 0x00024148, //v12.2.0.1
            MC_UNSUPPORTED_CMD_INVALID_ETHERCAT_CYCLE_TIME = 0x00024354, //v12.2.0.10
            MC_UNSUPPORTED_CMD_INVALID_ETHERCAT_OPERATION_MODE = 0x0002454D, //v12.2.0.1
            MC_UNSUPPORTED_CMD_BOARD_CATALOG = 0x00024743, //v12.2.0.1
            MC_UNSUPPORTED_CMD_GANTRY_ENABLING = 0x00024745, //v12.2.0.1
            MC_UNSUPPORTED_CMD_GANTRY_HOMING = 0x00024748, //v12.2.0.1
            MC_UNSUPPORTED_CMD_GANTRY_YAW_UNSTABLE = 0x00024759, //v12.2.0.1
            MC_UNSUPPORTED_CMD_BOARD_CATALOG_IO = 0x00024943, //v12.2.0.3
            MC_UNSUPPORTED_CMD_INVALID_REF_AXIS_STATUS = 0x00025241, //v12.2.0.10
            MC_UNSUPPORTED_CMD_SLAVE_HOMING = 0x00025348, //v12.2.0.1
            MC_UNSUPPORTED_CMD_SLAVE_HOME_MODE = 0x0002534D, //v12.2.0.1
            MC_UNSUPPORTED_CMD_TORQUE_LIMIT = 0x0002544C, //v12.2.0.10
            MC_INVALID_PARAM = 0x00030000,
            MC_INVALID_PARAM_1 = 0x00030001,
            MC_INVALID_PARAM_2 = 0x00030002,
            MC_INVALID_PARAM_3 = 0x00030003,
            MC_INVALID_PARAM_4 = 0x00030004,
            MC_INVALID_PARAM_5 = 0x00030005,
            MC_INVALID_PARAM_6 = 0x00030006,
            MC_INVALID_PARAM_7 = 0x00030007,
            MC_INVALID_PARAM_8 = 0x00030008,
            MC_INVALID_PARAM_9 = 0x00030009,
            MC_INVALID_PARAM_10 = 0x0003000A,
            MC_INVALID_GANTRY_CONDITION = 0x00030018, //v12.2.0.0
            MC_INVALID_SIZE = 0x00040000,
            MC_INVALID_AXIS_NUM = 0x00050000,
            MC_GANTRY_SLAVE_AXIS_CONFIG = 0x00050017, //v12.2.0.0
            MC_NOT_ENOUGH_RESOURCE = 0x00060000,
            MC_LIMIT_ERROR_PARAM = 0x00070000,
            MC_LIMIT_ERROR_PARAM_1 = 0x00070001,
            MC_LIMIT_ERROR_PARAM_2 = 0x00070002,
            MC_LIMIT_ERROR_PARAM_3 = 0x00070003,
            MC_LIMIT_ERROR_PARAM_4 = 0x00070004,
            MC_LIMIT_ERROR_PARAM_5 = 0x00070005,
            MC_LIMIT_ERROR_PARAM_6 = 0x00070006,
            MC_LIMIT_ERROR_PARAM_7 = 0x00070007,
            MC_LIMIT_ERROR_PARAM_8 = 0x00070008,
            MC_LIMIT_ERROR_PARAM_9 = 0x00070009,
            MC_LIMIT_ERROR_PARAM_10 = 0x0007000A,
            MC_INVALID_DEVICE_STATE = 0x00080000,
            MC_INVALID_DEVICE_STATE_ERROR = 0x00080001,
            MC_INVALID_DEVICE_STATE_GANTRY_OPERATION_MODE = 0x00080017, //v12.2.0.0
            MC_GANTRY_UNMATCHED_ETHERCAT_CYCLE_TIME = 0x00080025, //v12.2.0.1
            MC_INVALID_AXIS_STATE_DISABLED = 0x00090000,
            MC_INVALID_AXIS_STATE_STANDSTILL = 0x00090001, //v12.1.0.42
            MC_INVALID_AXIS_STATE_DISCRETE_MOTION = 0x00090002, //v12.1.0.42
            MC_INVALID_AXIS_STATE_CONTINUOUS_MOTION = 0x00090003, //v12.1.0.42
            MC_INVALID_AXIS_STATE_SYNC_MOTION = 0x00090004,
            MC_INVALID_AXIS_STATE_HOMING = 0x00090005, //v12.1.0.42
            MC_INVALID_AXIS_STATE_STOPPING = 0x00090006,
            MC_INVALID_AXIS_STATE_ERRORSTOP = 0x00090007,
            MC_INVALID_AXIS_STATE_MODE_CHANGE = 0x00090008, //v12.1.0.42
            MC_MASKED_IO_MASTER_AXIS = 0x00090016, //v12.2.0.0         
            MC_INVALID_AXIS_STATE_EC_ENABLED = 0x00090020,  //v12.2.0.0
            MC_INVALID_AXIS_CONFIG = 0x000A0000,
            MC_INVALID_CONFIG_AXIS_TYPE = 0x000A0002, //v12.2.0.0            
            MC_INVALID_AXIS_CONFIG_POS_LIMIT_SWITCH = 0x000A0006, //v12.1.0.42
            MC_INVALID_AXIS_CONFIG_NEG_LIMIT_SWITCH = 0x000A0009, //v12.1.0.42
            MC_INVALID_AXIS_CONFIG_HOME_SWITCH = 0x000A000B, //v12.1.0.42
            MC_INVALID_AXIS_CONFIG_Z_PHASE_INPUT = 0x000A000D, //v12.1.0.42
            MC_INVALID_AXIS_CONFIG_HOME_SENSOR = 0x000A0010,
            MC_INVALID_AXIS_CONFIG_MARK_PULSE = 0x000A0012,
            MC_AXIS_CONFIG_JOYSTICK = 0x000A001F,	//v12.2.0.0
            MC_INVALID_INPUT_SHAPING_FREQUENCY = 0x000A0020, //v12.2.0.0
            MC_INVALID_INPUT_SHAPING_DAMPIN_RATIO = 0x000A0021, //v12.2.0.0
            MC_UNSUPPORTED_INPUT_SHAPING_MODE_IN_THIS_API = 0x000A0022, //v12.2.0.0
            MC_UNSUPPORTED_AXIS_TYPE_IN_THIS_API = 0x000A0023, //v12.2.0.0
            MC_INVALID_AXIS_CONFIG_HOME_TYPE = 0x000A0032, //v12.1.0.42
            MC_INVALID_AXIS_CONFIG_HOME_FLAG_HANDLE = 0x000A003A, //v12.1.0.42
            MC_INVALID_AXIS_CONFIG_HOMING_MODE = 0x000A0064,
            MC_GEARING_RULE_VIOLATION = 0x000B0000,
            MC_LIMIT_POSITION_OVER = 0x000C0000,
            MC_POS_HW_LIMIT_POSITION_OVER = 0x000C0001, //v12.1.0.42
            MC_NEG_HW_LIMIT_POSITION_OVER = 0x000C0002, //v12.1.0.42
            MC_POS_SW_LIMIT_POSITION_OVER = 0x000C0004, //v12.1.0.42
            MC_NEG_SW_LIMIT_POSITION_OVER = 0x000C0008, //v12.1.0.42
            MC_INVALID_AXES_GROUP_NUM = 0x000D0000,
            MC_AXIS_ALREADY_ASSIGNED = 0x000E0002, //v12.1.0.42
            MC_IDENT_ALREADY_ASSIGNED = 0x000E0004, //v12.1.0.42
            MC_AXES_GROUP_INVALID_STATE = 0x000F0000,
            MC_GROUP_INVALID_STATE_MOVING = 0x000F0002, //v12.1.0.42
            MC_GROUP_INVALID_STATE_HOMING = 0x000F0003, //v12.1.0.42
            MC_GROUP_INVALID_STATE_STOPPING = 0x000F0004,   //v12.1.0.42
            MC_GROUP_INVALID_STATE_ERRORSTOP = 0x000F0005,  //v12.1.0.42
            MC_AXIS_IN_SINGLE_MOTION_STATE = 0x00100000,
            MC_1ST_AXIS_IN_MOTION_STATE = 0x00100001,   //v12.1.0.42
            MC_2ND_AXIS_IN_MOTION_STATE = 0x00100002,   //v12.1.0.42
            MC_3RD_AXIS_IN_MOTION_STATE = 0x00100003,   //v12.1.0.42
            MC_4TH_AXIS_IN_MOTION_STATE = 0x00100004,   //v12.1.0.42
            MC_5TH_AXIS_IN_MOTION_STATE = 0x00100005,   //v12.1.0.42
            MC_6TH_AXIS_IN_MOTION_STATE = 0x00100006,   //v12.1.0.42
            MC_7TH_AXIS_IN_MOTION_STATE = 0x00100007,   //v12.1.0.42
            MC_8TH_AXIS_IN_MOTION_STATE = 0x00100008,   //v12.1.0.42
            MC_GROUP_MEMBER_EMPTY = 0x00110000,
            MC_GROUP_MEMBER_EMPTY_1 = 0x00110001, //v12.2.0.1
            MC_GROUP_MEMBER_EMPTY_2 = 0x00110002, //v12.2.0.1
            MC_GROUP_MEMBER_EMPTY_3 = 0x00110003, //v12.2.0.1
            MC_GROUP_MEMBER_EMPTY_4 = 0x00110004, //v12.2.0.1
            MC_GROUP_MEMBER_EMPTY_5 = 0x00110005, //v12.2.0.1
            MC_GROUP_MEMBER_EMPTY_6 = 0x00110006, //v12.2.0.1
            MC_GROUP_MEMBER_EMPTY_7 = 0x00110007, //v12.2.0.1
            MC_1ST_AXIS_IN_GROUP_LIMIT_OVER = 0x00120000, //v12.1.0.42
            MC_2ND_AXIS_IN_GROUP_LIMIT_OVER = 0x00120001,   //v12.1.0.42
            MC_3RD_AXIS_IN_GROUP_LIMIT_OVER = 0x00120002,   //v12.1.0.42
            MC_4TH_AXIS_IN_GROUP_LIMIT_OVER = 0x00120003,   //v12.1.0.42
            MC_5TH_AXIS_IN_GROUP_LIMIT_OVER = 0x00120004,   //v12.1.0.42
            MC_6TH_AXIS_IN_GROUP_LIMIT_OVER = 0x00120005,   //v12.1.0.42
            MC_7TH_AXIS_IN_GROUP_LIMIT_OVER = 0x00120006,   //v12.1.0.42
            MC_8TH_AXIS_IN_GROUP_LIMIT_OVER = 0x00120007,   //v12.1.0.42
            MC_GROUP_CMD_SIZE_ERROR = 0x00130000,
            MC_GROUP_CMD_PARAMETER_SIZE_ERROR = 0x00130003, //v12.1.0.42
            MC_GROUP_MEMBER_NOT_ALLOCATED_X = 0x00140000,   //v12.1.0.42
            MC_GROUP_MEMBER_NOT_ALLOCATED_Y = 0x00140001,	//v12.1.0.42
            MC_GROUP_NOT_ALLOCATED_ROTATION_AXIS = 0x00140003, //v12.2.0.0
            MC_GROUP_NOT_ALLOCATED_TILT_AXIS = 0x00140004, //v12.2.0.0            
            MC_AXIS_IN_GROUP_MOTION = 0x00150000,
            MC_GANTRY_ASSIGNED_NOT = 0x001A0000, //v12.2.0.0
            MC_GANTRY_SLAVE_AXIS = 0x00170000, //v12.2.0.0
            MC_MASKED_IO_SLAVE_AXIS = 0x00170016, //v12.2.0.0            
            MC_GANTRY_ASSIGNED_ALREADY_AXIS = 0x00180000, //v12.2.0.0
            MC_ALREADY_ENABLED_GANTRY_ID = 0x00180047, //v12.2.0.0
            MC_CHECK_LASTINDEX_ON_EC_DATA_FILE = 0x001B0000, //v12.2.0.0	
            MC_CHECK_EC_DATA_FILE_FOCUS_ON_1ST_COMMAND_POSITION = 0x001C0001, //v12.2.0.0
            MC_CHECK_EC_DATA_FILE_FOCUS_ON_2ND_COMMAND_POSITION = 0x001C0002, //v12.2.0.0
            MC_CHECK_EC_DATA_FILE_FOCUS_ON_3RD_COMMAND_POSITION = 0x001C0003, //v12.2.0.0
            MC_CHECK_EC_DATA_FILE_FOCUS_ON_1ST_ACTUAL_POSITION = 0x001D0001, //v12.2.0.0
            MC_CHECK_EC_DATA_FILE_FOCUS_ON_2ND_ACTUAL_POSITION = 0x001D0002, //v12.2.0.0
            MC_CHECK_EC_DATA_FILE_FOCUS_ON_3RD_ACTUAL_POSITION = 0x001D0003, //v12.2.0.0
            MC_CAN_NOT_CHANGE_EC_MODE_BY_INVALID_AMP_STATE = 0x001E0019, //v12.2.0.0
            MC_ALREADY_EC_MODE_ENABLE = 0x001E0020, //v12.2.0.0
            MC_ALREADY_EC_MODE_DISABLE = 0x001E0021, //v12.2.0.0
            MC_EC_DIMENSION_MISMATCH = 0x001E0022, //v12.2.0.0
            MC_EC_TABLE_EMPTIED = 0x001E0023, //v12.2.0.0
            MC_EC_TABLE_INDEX_MISMATCH = 0x001E0024, //v12.2.0.0
            MC_IO_INVALID_STATE_EMPTY_MASKED_IO = 0x00200000, //v12.1.0.66
            MC_IO_INVALID_STATE_ALREADY_MASKED_IO = 0x0020000E, //v12.1.0.66
            MC_IO_INVALID_STATE_REQUEST_UNMASK_IO = 0x00200016, //v12.1.0.66
            MC_NOT_ENOUGH_EC_MEMORY = 0x00240000, //v12.2.0.0
            MC_ALREADY_DISABLED_GANTRY_ID = 0x00250047, //v12.2.0.0
            MC_INVALID_GANTRY_STATE_NOT_COMPLETE_MOTION = 0x00260000, //v12.2.0.0
            MC_GANTRY_IN_HOMING_SEQUENCE = 0x00270000, //v12.2.0.0
            MC_INVALID_GANTRY_STATE_DO_NOT_CHANGE_RATIO = 0x00280001, //v12.2.0.1
            MC_INVALID_GANTRY_STATE_EC_ENABLED = 0x00280020, //v12.2.0.0
            MC_INVALID_GANTRY_STATE_ENABLED_TORQUE_LIMIT = 0x00280026, //v12.2.0.10
            MC_INVALID_GANTRY_STATE_ALIGN_SW_LIMIT = 0x00280027, //v12.2.0.10
            MC_INVALID_AXIS_ID_ENTER_GANTRY_SALVE_AXIS_ID = 0x00290000, //v12.2.0.10
            MC_INVALID_PARAMETER_ATTRIBUTE = 0x002A0000, //v12.2.0.10            
            MC_SENSORSTOP_EMG_SIGNAL_IS_NOT_CLEARED = 0x002B0000, //v12.2.0.15
            MC_GROUP_EC_MODE_ENABLED = 0x11010020, //v12.2.0.0
            MC_GANTRY_EC_MODE_ENABLED = 0x12000020, //v12.2.0.0

            //Motion Library Error Defines
            MC_FAIL = 0xE00C0001,
            MC_ERROR = 0xE00C0002,
            MC_IOMAPING_ERR = 0xE00C0003,
            MC_COMMINIT_ERR = 0xE00C0004,
            MC_COMM_EVENT_INIT_ERR = 0xE00C0005,
            MC_READ_ENI_NODE_ERR = 0xE00C0006,
            MC_INVALID_AXIS_ERR = 0xE00C0007,
            MC_INVALID_BOARD_ERR = 0xE00C0008,
            MC_XML_PARSING_ERR = 0xE00C0009,
            MC_XML_ITEM_COUNT_MISMATCH = 0xE00C000A,
            MC_NO_BOARD_INSTALLED = 0xE00C000B,
            MC_INVALID_DOWNLOAD_FILE_TYPE = 0xE00C000C,
            MC_OPEN_ENI_ERR = 0xE00C000D,
            MC_FILE_OPEN_FAIL = 0xE00C000E,
            MC_NO_MATCHING_DOWNLOADINFORMATION = 0xE00C000F,
            MC_NONE_OP = 0xE00C0010,
            MC_FAIL_GEN_DOWNLOAD_FILE = 0xE00C0011,
            MC_REG_KEY_READ_FAIL = 0xE00C0012,
            MC_NOT_ALLOWED_IN_THIS_MASTER_MODE = 0xE00C0014,
            MC_MASTERID_OUT_OF_RANGE = 0xE00C0015,
            MC_BOARDNO_OUT_OF_RANGE = 0xE00C0016,
            MC_AXISNO_OUT_OF_RANGE = 0xE00C0017,
            MC_BOARDCNT_OUT_OF_RANGE = 0xE00C0018,
            MC_RETURN_SIZE_NOT_EQUAL = 0xE00C001A,
            MC_MASTERID_DUPLICATION_ERR = 0xE00C001B,
            MC_PARAM_ERROR_FILE_IS_NULL = 0xE00C001C,
            MC_NO_MATCHING_BOARDID_FOUND = 0xE00C001D,
            MC_NOT_READY_NETWORK_CONFIGURATION = 0xE00C001E,
            MC_INVALID_MASTERID_ERR = 0xE00C001F,
            MC_MASTER_MODE_CHANGE_NOT_ALLOWED = 0xE00C0020,
            MC_MASTER_REQUEST_PARAM_ERROR = 0xE00C0021,
            MC_MASTER_INVALID_STATE = 0xE00C0022,
            MC_NOT_MOTION_LIBRAY_INITIALIZED = 0xE00C0023, //2014.08.22
            MC_IOMANAGER_NOT_RUNNING = 0xE00C0024, //2014.08.22
            MC_ANOTHER_PROGRAM_IS_USING_NMC_LIBRARY = 0xE00C0025, //2014.10.02
            MC_SLAVE_ITEM_MISMATCH = 0xE00C0026,
            MC_SLAVE_ITEM_COUNT_MISMATCH = 0xE00C0027,
            MC_AXIS_COUNT_OUT_OF_RANGE = 0xE00C0028, //v12.1.0.46 
            MC_SLAVE_DEVICEID_OUT_OF_RANGE = 0xE00C0029, //v12.1.0.63
            MC_FM_TRIG_COUNT_OUT_OF_RANGE = 0xE00C002A, //v12.1.0.64
            MC_FM_IDXNO_OUT_OF_RANGE = 0xE00C002B, //v12.1.0.64
            MC_MODULE_INFO_FILE_OPEN_FAIL = 0xE00C002C, //v12.1.0.64
            MC_ID_IDX_TYPE_OUT_OF_RANGE = 0xE00C002D, //v12.1.0.66
            MC_WAIT_TIME_OUT_OF_RANGE = 0xE00C0030, //v12.1.0.67	
            MC_SELF_HOLD_IO_NOT_FOUND = 0xE00C0031, //v12.1.0.67
            MC_INVALID_SLAVE_SAFEOP_STATE = 0xE00C0032, //v12.1.0.67
            MC_AXIS_TYPE_OUT_OF_RANGE = 0xE00C0040, //v12.2.0.0
            MC_EC_TYPE_OUT_OF_RANGE = 0xE00C0041, //v12.2.0.0
            MC_EC_MODE_OUT_OF_RANGE = 0xE00C0042, //v12.2.0.0
            MC_EC_MAPID_OUT_OF_RANGE = 0xE00C0043, //v12.2.0.0
            MC_EC_LAST_INDEX_OUT_OF_RANGE = 0xE00C0044, //v12.2.0.0
            MC_BOARD_CATALOG_MISMATCH_ERROR = 0xE00C0045, //v12.2.0.10
            MC_COMMON_PARAMETER_OUT_OF_RANGE = 0xE00C0046, //v12.2.0.19

            //Comm Library Error Defines
            MC_PCICIP_GEN_10 = 0xCC100000,
            COMM_CONNECTION_ESTABLISHED = 0xED000001,
            COMM_CONN_CONFIG_FAILED_INVALID_NETWORK_PATH = 0xED000002,
            COMM_CONN_CONFIG_FAILED_NO_RESPONSE = 0xED000003,
            COMM_CONN_CONFIG_FAILED_ERROR_RESPONSE = 0xED000004,
            COMM_CONNECTION_TIMED_OUT = 0xED000005,
            COMM_CONNECTION_CLOSED = 0xED000006,
            COMM_INCOMING_CONNECTION_RUN_IDLE_FLAG_CHANGED = 0xED000007,
            COMM_ASSEMBLY_NEW_INSTANCE_DATA = 0xED000008,
            COMM_ASSEMBLY_NEW_MEMBER_DATA = 0xED000009,
            COMM_CONNECTION_NEW_INPUT_SCANNER_DATA = 0xED00000A,
            COMM_CONNECTION_VERIFICATION = 0xED00000B,
            COMM_CONNECTION_RECONFIGURED = 0xED00000C,
            COMM_REQUEST_RESPONSE_RECEIVED = 0xED000064,
            COMM_REQUEST_FAILED_INVALID_NETWORK_PATH = 0xED000065,
            COMM_REQUEST_TIMED_OUT = 0xED000066,
            COMM_CLIENT_OBJECT_REQUEST_RECEIVED = 0xED000067,
            COMM_NEW_CLASS3_RESPONSE = 0xED000068,
            COMM_CLIENT_PCCC_REQUEST_RECEIVED = 0xED000069,
            COMM_NEW_LIST_IDENTITY_RESPONSE = 0xED00006A,
            COMM_ID_RESET = 0xED00006B,
            COMM_BACKPLANE_REQUEST_RECEIVED = 0xED00006C,
            COMM_OUT_OF_MEMORY = 0xED0000C8,
            COMM_UNABLE_INTIALIZE_WINSOCK = 0xED0000C9,
            COMM_UNABLE_START_THREAD = 0xED0000CA,
            COMM_ERROR_USING_WINSOCK = 0xED0000CB,
            COMM_ERROR_SETTING_SOCKET_TO_NONBLOCKING = 0xED0000CC,
            COMM_ERROR_SETTING_TIMER = 0xED0000CD,
            COMM_SESSION_COUNT_LIMIT_REACHED = 0xED0000CE,
            COMM_CONNECTION_COUNT_LIMIT_REACHED = 0xED0000CF,
            COMM_PENDING_REQUESTS_LIMIT_REACHED = 0xED0000D0,
            COMM_PENDING_REQUEST_GROUPS_LIMIT_REACHED = 0xED0000D1,
            COMM_ERROR_UNABLE_START_MODBUS = 0xED0000D2,
            COMM_ERROR_HW_NOT_INSTALLED = 0xED0000DC,
            COMM_ERROR_DD_SEND_ERROR = 0xED0000DD,
            COMM_ERROR_DD_READ_ERROR = 0xED0000DE,
            COMM_DD_ERROR_SEND = 0xED0000DF,
            COMM_DD_ERROR_RECV = 0xED0000E0,
            COMM_DD_OPEN_FAIL = 0xED0000E6,
            COMM_DD_NOT_OPENED = 0xED0000E7,
            COMM_DD_CONN_FAIL = 0xED0000E8,
            COMM_DD_CLIENT_START_FAIL = 0xED0000E9,
            COMM_DD_OK = 0xED000000,
            COMM_CN_NOT_CONNECTED = 0xED0000F0,
            COMM_CN_CONNECTED = 0xED0000F1,
            COMM_CN_CONNERROR = 0xED0000F2,
            //
            COMM_ERROR_SUCCESS = 0xEE000000,
            COMM_ERROR_FAILURE = 0xEE010000,
            COMM_EXT_ERR_DUPLICATE_FWD_OPEN = 0xEE010100,
            COMM_EXT_ERR_CLASS_TRIGGER_INVALID = 0xEE010103,
            COMM_EXT_ERR_OWNERSHIP_CONFLICT = 0xEE010106,
            COMM_EXT_ERR_CONNECTION_NOT_FOUND = 0xEE010107,
            COMM_EXT_ERR_INVALID_CONN_TYPE = 0xEE010108,
            COMM_EXT_ERR_INVALID_CONN_SIZE = 0xEE010109,
            COMM_EXT_ERR_DEVICE_NOT_CONFIGURED = 0xEE010110,
            COMM_EXT_ERR_RPI_NOT_SUPPORTED = 0xEE010111,
            COMM_EXT_ERR_CONNECTION_LIMIT_REACHED = 0xEE010113,
            COMM_EXT_ERR_VENDOR_PRODUCT_CODE_MISMATCH = 0xEE010114,
            COMM_EXT_ERR_PRODUCT_TYPE_MISMATCH = 0xEE010115,
            COMM_EXT_ERR_REVISION_MISMATCH = 0xEE010116,
            COMM_EXT_ERR_INVALID_CONN_POINT = 0xEE010117,
            COMM_EXT_ERR_INVALID_CONFIG_FORMAT = 0xEE010118,
            COMM_EXT_ERR_NO_CONTROLLING_CONNECTION = 0xEE010119,
            COMM_EXT_ERR_TARGET_CONN_LIMIT_REACHED = 0xEE01011A,
            COMM_EXT_ERR_RPI_SMALLER_THAN_INHIBIT = 0xEE01011B,
            COMM_EXT_ERR_CONNECTION_TIMED_OUT = 0xEE010203,
            COMM_EXT_ERR_UNCONNECTED_SEND_TIMED_OUT = 0xEE010204,
            COMM_EXT_ERR_PARAMETER_ERROR = 0xEE010205,
            COMM_EXT_ERR_MESSAGE_TOO_LARGE = 0xEE010206,
            COMM_EXT_ERR_UNCONN_ACK_WITHOUT_REPLY = 0xEE010207,
            COMM_EXT_ERR_NO_BUFFER_MEMORY_AVAILABLE = 0xEE010301,
            COMM_EXT_ERR_BANDWIDTH_NOT_AVAILABLE = 0xEE010302,
            COMM_EXT_ERR_TAG_FILTERS_NOT_AVAILABLE = 0xEE010303,
            COMM_EXT_ERR_REAL_TIME_DATA_NOT_CONFIG = 0xEE010304,
            COMM_EXT_ERR_PORT_NOT_AVAILABLE = 0xEE010311,
            COMM_EXT_ERR_LINK_ADDR_NOT_AVAILABLE = 0xEE010312,
            COMM_EXT_ERR_INVALID_SEGMENT_TYPE_VALUE = 0xEE010315,
            COMM_EXT_ERR_PATH_CONNECTION_MISMATCH = 0xEE010316,
            COMM_EXT_ERR_INVALID_NETWORK_SEGMENT = 0xEE010317,
            COMM_EXT_ERR_INVALID_LINK_ADDRESS = 0xEE010318,
            COMM_EXT_ERR_SECOND_RESOURCES_NOT_AVAILABLE = 0xEE010319,
            COMM_EXT_ERR_CONNECTION_ALREADY_ESTABLISHED = 0xEE01031A,
            COMM_EXT_ERR_DIRECT_CONN_ALREADY_ESTABLISHED = 0xEE01031B,
            COMM_EXT_ERR_MISC = 0xEE01031C,
            COMM_EXT_ERR_REDUNDANT_CONNECTION_MISMATCH = 0xEE01031D,
            COMM_EXT_ERR_NO_MORE_CONSUMER_RESOURCES = 0xEE01031E,
            COMM_EXT_ERR_NO_TARGET_PATH_RESOURCES = 0xEE01031F,
            COMM_EXT_ERR_VENDOR_SPECIFIC = 0xEE010320,
            COMM_ERROR_NO_RESOURCE = 0xEE020000,
            COMM_ERROR_INVALID_PARAMETER_VALUE = 0xEE030000,
            COMM_ERROR_INVALID_SEG_TYPE = 0xEE040000,
            COMM_ERROR_INVALID_DESTINATION = 0xEE050000,
            COMM_ERROR_PARTIAL_DATA = 0xEE060000,
            COMM_ERROR_CONN_LOST = 0xEE070000,
            COMM_ERROR_BAD_SERVICE = 0xEE080000,
            COMM_ERROR_BAD_ATTR_DATA = 0xEE090000,
            COMM_ERROR_ATTR_LIST_ERROR = 0xEE0A0000,
            COMM_ERROR_ALREADY_IN_REQUESTED_MODE = 0xEE0B0000,
            COMM_ERROR_OBJECT_STATE_CONFLICT = 0xEE0C0000,
            COMM_ERROR_OBJ_ALREADY_EXISTS = 0xEE0D0000,
            COMM_ERROR_ATTR_NOT_SETTABLE = 0xEE0E0000,
            COMM_ERROR_PERMISSION_DENIED = 0xEE0F0000,
            COMM_ERROR_DEV_IN_WRONG_STATE = 0xEE100000,
            COMM_ERROR_REPLY_DATA_TOO_LARGE = 0xEE110000,
            COMM_ERROR_FRAGMENT_PRIMITIVE = 0xEE120000,
            COMM_ERROR_NOT_ENOUGH_DATA = 0xEE130000,
            COMM_ERROR_ATTR_NOT_SUPPORTED = 0xEE140000,
            COMM_ERROR_TOO_MUCH_DATA = 0xEE150000,
            COMM_ERROR_OBJ_DOES_NOT_EXIST = 0xEE160000,
            COMM_ERROR_NO_FRAGMENTATION = 0xEE170000,
            COMM_ERROR_DATA_NOT_SAVED = 0xEE180000,
            COMM_ERROR_DATA_WRITE_FAILURE = 0xEE190000,
            COMM_ERROR_REQUEST_TOO_LARGE = 0xEE1A0000,
            COMM_ERROR_RESPONSE_TOO_LARGE = 0xEE1B0000,
            COMM_ERROR_MISSING_LIST_DATA = 0xEE1C0000,
            COMM_ERROR_INVALID_LIST_STATUS = 0xEE1D0000,
            COMM_ERROR_SERVICE_ERROR = 0xEE1E0000,
            COMM_ERROR_VENDOR_SPECIFIC = 0xEE1F0000,
            COMM_ERROR_INVALID_PARAMETER = 0xEE200000,
            COMM_ERROR_WRITE_ONCE_FAILURE = 0xEE210000,
            COMM_ERROR_INVALID_REPLY = 0xEE220000,
            COMM_ERROR_BAD_KEY_IN_PATH = 0xEE250000,
            COMM_ERROR_BAD_PATH_SIZE = 0xEE260000,
            COMM_ERROR_UNEXPECTED_ATTR = 0xEE270000,
            COMM_ERROR_INVALID_MEMBER = 0xEE280000,
            COMM_ERROR_MEMBER_NOT_SETTABLE = 0xEE290000,
            COMM_ERROR_UNKNOWN_MODBUS_ERROR = 0xEE2B0000,
            COMM_ERROR_HW_NOT_INSTALLED1 = 0xEE2C0000,
            COMM_ERROR_ENCAP_PROTOCOL = 0xEE6A0000,
            COMM_ERROR_STILL_PROCESSING = 0xEEFF0000,
            MC_DOWNLOAD_FAIL_DUE_TO_ANOTHER_PROGRAM_IS_RUNNING = 0xEE800000,
        }

        //=============================================================================
        //
        //
        //
        //
        // Enum Types
        //
        //
        //
        //
        //=============================================================================
        public enum EcState : uint
        {
            eST_UNKNOWN = 0,
            eST_INIT = 0x01,	//Ver_0c010016_2
            eST_PREOP = 0x02,	//Ver_0c010016_2
            eST_BOOTSTRAP = 0x03,   //v12.1.0.48
            eST_SAFEOP = 0x04,	//Ver_0c010016_2
            eST_OP = 0x08,	//Ver_0c010016_2
            eST_ACKERR = 0x10	//Ver_0c010016_2
        }
        public enum EcMstMode : uint
        {
            eMM_IDLE = 0,
            eMM_SCAN,
            eMM_RUN,
            eMM_INTRANSITION,
            eMM_ERR,
            eMM_LINKBROKEN,
            eMM_FREE_RUN, //v12.1.0.54
        }
        public enum EcScanMode : uint
        {
            SCAN_ALL = 0,
            SCAN_SINGLE,
        }
        public enum EcScanSts : uint
        {
            SCAN_NONE = 0,
            SCAN_BUSY,
            SCAN_DONE
        }
        public enum IOBufMode : ushort
        {
            BUF_OUT = 0,
            BUF_IN = 1,
        }
        public enum MC_AXISSTATUS : uint
        {
            mcErrorStop = 0x00000001, //Bit0 Axis State 중 하나로 축의 Error상태로 모션 동작이 불가능한 상태
            mcDisabled = 0x00000002, //Bit1 Axis State 중 하나로 축의 Amp Off 상태로 모션 동작이 불가능한 상태
            mcStopping = 0x00000004, //Bit2 Axis State 중 하나로 MC_Stop에 의해 감속정지 중 또는 감속정지는 완료 되었지만 MC_Stop의 Execute가 유지되어 있는 상태로 모션 동작이 불가능한 상태
            mcStandStill = 0x00000008, //Bit3 Axis State 중 하나로 축의 Amp On 상태로 모션 동작이 가능한 준비 상태
            mcDiscreteMotion = 0x00000010, //Bit4 Axis State 중 하나로 MC_MoveAbsoulte, MC_MoveRelative, MC_Halt 등에 의해 단축 모션 중인 상태
            mcContinuousMotion = 0x00000020, //Bit5 Axis State 중 하나로 MC_MoveVelocity에 의해 지속적인 모션 중인 상태
            mcSynchroMotion = 0x00000040, //Bit6 Axis State 중 하나로 MC_GearIn 또는 GroupMotion에 의한 모션 상태
            mcHoming = 0x00000080, //Bit7 Axis State 중 하나로 축이 MC_Home에 의해 원점복귀 중인 상태
            mcSWLimitSwitchPosEvent = 0x00000100, //Bit8 Software Position Positive Limit Value를 벗어나면 On으로 변경
            mcSWLimitSwitchNegEvent = 0x00000200, //Bit9 Software Position Negative Limit Value를 벗어나면 On으로 변경
            mcConstantVelocity = 0x00000400, //Bit10 모션이 등속동작 중인 상태
            mcAccelerating = 0x00000800, //Bit11 모션이 가속동작 중인 상태
            mcDecelerating = 0x00001000, //Bit12 모션이 감속동작 중인 상태
            mcDirectionPositive = 0x00002000, //Bit13 모션이 정방향으로 동작 중인 상태(모션이 정지 중일 때 Default로 동작)
            mcDirectionNegative = 0x00004000, //Bit14 모션이 역방향으로 동작 중인 상태
            mcLimitSwitchNeg = 0x00008000, //Bit15 Negative Hardware Limit Switch의 상태
            mcLimitSwitchPos = 0x00010000, //Bit16 Positive Hardware Limit Switch의 상태
            mcHomeAbsSwitch = 0x00020000, //Bit17 Home Switch의 상태
            mcLimitSwitchPosEvent = 0x00040000, //Bit18 Positive Hardware Limit Switch가 Active 되었을 때 On으로 변경
            mcLimitSwitchNegEvent = 0x00080000, //Bit19 Negative Hardware Limit Switch가 Active 되었을 때 On으로 변경
            mcDriveFault = 0x00100000, //Bit20 해당 축에 링크된 Slave Drive가 Fault상태(Slave Statusword bit 3)
            mcSensorStop = 0x00200000, //Bit21 해당 축에 설정된 SensorStop에 의해 모션이 정지된 상태
            mcReadyForPowerOn = 0x00400000, //Bit22 AmpOn이 가능한 상태
            mcPowerOn = 0x00800000, //Bit23 AmpOn이 정상적으로 완료된 상태
            mcIsHomed = 0x01000000, //Bit24 MC_Home에 의해 원점복귀가 완료된 상태
            mcAxisWarning = 0x02000000, //Bit25 모션 동작에는 문제가 없지만 축의 Warning상태
            mcMotionComplete = 0x04000000, //Bit26 모션 동작이 완료된 Inposition 상태
            mcGearing = 0x08000000, //Bit27 MC_GearIn에 의해 Gearing 동작 중인 상태
            mcGroupMotion = 0x10000000, //Bit28 해당 축이 그룹에 속한 상태이며 해당 그룹이 Enable인 상태
            mcBufferFull = 0x20000000, //Bit29 해당 축의 Buffer 1000개가 모두 사용된 상태
            mcReserved_as_30 = 0x40000000, //Bit30 Reserved for Future Use
            mcDebugLimitEvent = 0x80000000, //Bit31 해당 축에 링크된 Slave Drive의 Sensor data의 버그 상태
        }

        //MC_ReadStatus
        public enum MC_Status : uint
        {
            mcASErrorStop = 0x00000001,
            mcASDisabled = 0x00000002,
            mcASStopping = 0x00000004,
            mcASStandStill = 0x00000008,
            mcASDiscreteMotion = 0x00000010,
            mcASContinuousMotion = 0x00000020,
            mcASSynchroMotion = 0x00000040,
            mcASHoming = 0x00000080,
        }

        //MC_ReadMotionState
        public enum MC_MOTIONSTATE : uint
        {
            mcMSConstantVelocity = 0x00000001,
            mcMSAccelerating = 0x00000002,
            mcMSDecelerating = 0x00000004,
            mcMSDirectionPositive = 0x00000008,
            mcMSDirectionNegative = 0x00000010,
        }
        //MC_ReadAxisInfo
        public enum MC_AXISINFO : uint
        {
            mcAIHomeAbsSwitch = 0x00000001,
            mcAILimitSwitchPos = 0x00000002,
            mcAILimitSwitchNeg = 0x00000004,
            mcAIReserved3 = 0x00000008,
            mcAIReserved4 = 0x00000010,
            mcAIReadyForPowerOn = 0x00000020,
            mcAIPowerOn = 0x00000040,
            mcAIIsHomed = 0x00000080,
            mcAIAxisWarining = 0x00000100,
            mcAIMotionComplete = 0x00000200,
            mcAIGearing = 0x00000400,
            mcAIGroupMotion = 0x00000800,
            mcAIBufferFull = 0x00001000,
            mcAIReseved13 = 0x00002000,
        }
        public enum MC_AXISERROR : uint
        {
            mcAxis_NO_ERROR = 0x0000,
            mcAxis_DEVICE_ERROR = 0x0001,
            mcAxis_INVALID_AXIS_STATE = 0x0002,
            mcAxis_PARAMETER_INVALID = 0x0003,
            mcAxis_UNSUPPORT_CMD_REQUEST = 0x0004,
            mcAxis_CMD_REQUEST_FORMAT_WRONG = 0x0005,
            mcAxis_RESOURCE_ERROR = 0x0006,
            mcAxis_CONFIG_INVALID = 0x0007,
            mcAxis_POSITION_FOLLOWING_ERROR = 0x0008,
            mcAxis_VELOCITY_FOLLOWING_ERROR = 0x0009,
            mcAxis_SYSTEM_MAX_VELOCITY_OVER_ERROR = 0x000A,
            mcAxis_SYSTEM_MAX_ACCEL_OVER_ERROR = 0x000B,
            mcAxis_SYSTEM_MAX_DECEL_OVER_ERROR = 0x000C,
            mcAxis_SYSYEM_MAX_JERK_OVER_ERROR = 0x000D,
            mcAxis_MALFUNCTION_ERROR = 0x000E,
            mcAxis_GEARING_RULE_VIOLATION = 0x000F,
            mcAxis_HW_LIMIT_REACHED_WARNING = 0x8001,
            mcAxis_SW_LIMIT_REACHED_WARNING = 0x8002,
        }
        public struct MC_AxisErrorInfo
        {
            ushort ErrorId;
            ushort ErrorInfo0;
            ushort ErrorInfo1;
        }
        public struct MC_AxesGroupRawDataStatus
        {
            byte Mode;				    //Raw Data Mode
            byte Enabled;				//Enabled Flag
            uint EmptyBufferCount;	    //Empty Buffer Count
            ushort InBufferIndex;		//In Buffer Index
            ushort OutBufferIndex;		//Out Buffer Index
            uint CurrentRawDataID;	    //Current Out RawDataSet ID
            byte CoordSystem;			//Reserved
        }
        public enum MC_SOURCE : uint
        {
            mcSetValue = 0,             //Synchronization on master set value
            mcActualValue,              //Synchronization on master actual value
            mcSetValueFixedGear = 0x10,
            mcActualValueFixedGear = 0x11,
        }
        public enum MC_EXECUTION_MODE : uint
        {
            mcImmediately = 0,
            mcQueued,
        }
        public enum MC_BUFFER_MODE : uint
        {
            //0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh
            mcAborting = 0,
            mcBuffered,
            mcBlendingLow,
            mcBlendingPrevious,
            mcBlendingNext,
            mcBlendingHigh,
            mcBufferedBlendingLow,
            mcBufferedBlendingPrevious,
            mcBufferedBlendingNext,
            mcBufferedBlendingHigh,
        }
        public enum MC_DIRECTION : uint
        {
            // 0:Positive_Direction, 1:Shortest_way, 2:Negative_Direction, 3:Current_Direction
            mcPositiveDirection = 0,
            mcShortestWay,
            mcNegativeDirection,
            mcCurrentDirection,
        }
        public enum MC_GearStatus : ushort
        {
            mcGearActive = 0x0001,
            mcGearIn = 0x0002,
            mcGearReserved2 = 0x0004,
            mcGearReserved3 = 0x0008,
            mcGearReserved4 = 0x0010,
            mcGearReserved5 = 0x0020,
            mcGearReserved6 = 0x0040,
            mcGearReserved7 = 0x0080,
            mcGearReserved8 = 0x0100,
            mcGearReserved9 = 0x0200,
            mcGearReserved10 = 0x0400,
            mcGearReserved11 = 0x0800,
            mcGearReserved12 = 0x1000,
            mcGearReserved13 = 0x2000,
            mcGearReserved14 = 0x4000,
            mcGearReserved15 = 0x8000,
        }
        public enum AxisStopType : uint
        {
            IMMEDIATE = 0,
            DECEL
        }
        //Do not use!!! (Old Version)
        public enum MC_AXIS_DIRECTION : uint
        {
            CW = 0,
            CCW
        }
        //New Version
        public enum MC_HOME_AXIS_DIRECTION : uint
        {
            HOMING_DIR_CCW = 0,
            HOMING_DIR_CW
        }
        public enum MC_AXIS_CONTROL : uint
        {
            OL_PULSE_DIRECTION = 0,
            OL_TWO_PULSE,
            OL_QUDARATURE_PULSE,
            CL_VELOCITY_LEVEL_ANALOG,
            CL_TORQUE_LEVEL_ANALOG,
        }
        public enum MC_AXIS_I_MODE : uint
        {
            IN_STANDING = 0,
            ALWAYS,
        }
        public enum BinFileType : uint
        {
            BIN_BOOT = 1,
            BIN_A8OS = 2,
            BIN_TM = 3,
            BIN_DSPOS = 5,
            BIN_ENI = 6
        }
        public enum SWVerType : ushort
        {
            SW_VER_MOTION = 0,
            SW_VER_PCICIP,
            SW_VER_DDSDK,
            SW_VER_DD,
        }
        public enum MC_CoordSystem : byte
        {
            mcACS = 1,
            mcMCS,
            mcPCS,
        }
        public enum MC_TRANSITION_MODE : byte
        {
            mcTMNone = 0,			//Insert no transition curve (default mode)
            mcTMStartVelocity,		//Transition with given start velocity
            mcTMConstantVelocity,	//Transition with given constant velocity
            mcTMCornerDistance,		//Transition with given corner distance
            mcTMMaxCornerDeviation, //Transition with given maximum corner deviation
            //5 - 9 Reserved by PLCopen
            //10 - Supplier specific modes
            mcTMProfileContinue = 11, //RSA Transiton Mode (Special) //v12.1.0.71
        }
        public enum MC_CIRC_MODE : byte
        {
            mcBORDER = 0,
            mcCENTER,
            mcRADIUS,
            mcCENTER_ANGLE = 10,
            mcBORDER_ANGLE = 11,
            mcSPIRAL = 20, //v12.1.0.71
            mcCYLINDER = 21, //v12.1.0.71  
            mcSPHERE = 22, //v12.1.0.71
        }
        //v12.2.0.19
        public enum MC_3D_MODE : byte
        {
            mc_NORMAL = 0,
            mc_NORMAL_SA1,
            mc_NORMAL_SA2,
            mc_NORMAL_SA1_2,
            mc_SA1,
            mc_SA2,
            mc_SA1_2,
            mc_ZAXIS
        }
        public enum MC_CIRC_PATHCHOICE : byte
        {
            mcClockWise = 0,
            mcCounterClockWise,
        }
        public enum MC_RAW_DATA_MODE : byte
        {
            mcPositionCmdMode = 0,
            mcVelocityCmdMode,
            mcTorqueCmdMode,
        }
        public enum MC_SAVE_MODE : byte
        {
            mcSMAuto = 0,
            mcSMIndex,
        }
        //v12.2.0.12
        public enum STATUS_COMBINATION : byte
        {
            STATUS_COMBINATION_1 = 1,   //Init.
            STATUS_COMBINATION_2 = 2,   //PreOP
            STATUS_COMBINATION_3 = 3,   //ProOP + Init or Bootstrap
            STATUS_COMBINATION_4 = 4,   //SafeOP
            STATUS_COMBINATION_5 = 5,   //SafeOP + Init
            STATUS_COMBINATION_6 = 6,   //SafeOP + PreOP
            STATUS_COMBINATION_7 = 7,   //SafeOP + PreOP + Init or SafeOP + Bootstrap
            STATUS_COMBINATION_8 = 8,   //OP
            STATUS_COMBINATION_9 = 9,   //OP + Init
            STATUS_COMBINATION_10 = 10, //OP + PreOP
            STATUS_COMBINATION_11 = 11, //OP + PreOP + Init or OP + Bootstrap
            STATUS_COMBINATION_12 = 12, //OP + SafeOP
            STATUS_COMBINATION_13 = 13, //OP + SafeOP + Init
            STATUS_COMBINATION_14 = 14, //OP + SafeOP + PreOP
            STATUS_COMBINATION_15 = 15,	//OP + SafeOP + PreOP + Init or OP + SafeOP + Bootstrap
        }
        public enum MC_GroupStatus : uint
        {
            GroupMoving = 0x00000001,
            GroupHoming = 0x00000002,
            GroupErrorStop = 0x00000004,
            GroupStandby = 0x00000008,
            GroupStopping = 0x00000010,
            GroupDisabled = 0x00000020,
            ConstantVelocity = 0x00000040,
            Accelerating = 0x00000080,
            Decelerating = 0x00000100,
            InPosition = 0x00000200,
        }
        //v12.2.0.1
        public enum MC_GantryStatus : uint
        {
            mcGantry_MotionCompleted = 0x00000001, //Bit 00
            mcGantry_Fault = 0x00000002, //Bit 01
            mcGantry_Warning = 0x00000004, //Bit 02
            mcGantry_FollowingError = 0x00000008, //Bit 03
            mcGantry_YawError = 0x00000010, //Bit 04
            mcGantry_YawStable = 0x00000020, //Bit 05
            mcGantry_AmpOff = 0x00000040, //Bit 06
            mcGantry_Moving = 0x00000080, //Bit 07
            mcGantry_Buffering = 0x00000100, //Bit 08
            mcGantry_IsHomed = 0x00000200, //Bit 09
            mcGantry_Homing = 0x00000400, //Bit 10
            mcGantry_PosHWLimitMaster = 0x00000800, //Bit 11
            mcGantry_NegHWLimitMaster = 0x00001000, //Bit 12
            mcGantry_PosSWLimitMaster = 0x00002000, //Bit 13
            mcGantry_NegSWLimitMaster = 0x00004000, //Bit 14
            mcGantry_PosHWLimitSlave = 0x00008000, //Bit 15
            mcGantry_NegHWLimitSlave = 0x00010000, //Bit 16
            mcGantry_PosSWLimitSlave = 0x00020000, //Bit 17
            mcGantry_NegSWLimitSlave = 0x00040000, //Bit 18
            mcGantry_HWLimitEventMaster = 0x00080000, //Bit 19
            mcGantry_HWLimitEventSlave = 0x00100000, //Bit 20
            mcGantry_InputShapingZV = 0x00200000, //Bit 21
            mcGantry_InputShapingZVD = 0x00400000, //Bit 22
            mcGantry_Reserved_23 = 0x00800000, //Bit 23
            mcGantry_Reserved_24 = 0x01000000, //Bit 24
            mcGantry_Reserved_25 = 0x02000000, //Bit 25
            mcGantry_Reserved_26 = 0x04000000, //Bit 26
            mcGantry_Reserved_27 = 0x08000000, //Bit 27
            mcGantry_Reserved_28 = 0x10000000, //Bit 28
            mcGantry_Reserved_29 = 0x20000000, //Bit 29
            mcGantry_Reserved_30 = 0x40000000, //Bit 30
            mcGantry_Reserved_31 = 0x80000000, //Bit 31
        }
        public enum MC_ParamID : uint
        {
            mcpCommandedPosition = 1,
            mcpSWLimitPos_ = 2,
            mcpSWLimitNeg_ = 3,
            mcpEnableLimitPos_ = 4,
            mcpEnableLimitNeg_ = 5,
            mcpEnablePosLagMonitoring = 6,
            mcpMaxPositionLag_ = 7,
            mcpMaxVelocitySystem_ = 8,
            mcpMaxVelocityAppl_ = 9,
            mcpActualVelocity = 10,
            mcpCommandedVelocity = 11,
            mcpMaxAccelerationSystem_ = 12,
            mcpMaxAccelerationAppl = 13,
            mcpMaxDecelerationSystem_ = 14,
            mcpMaxDecelerationAppl = 15,
            mcpMaxJerkSystem = 16,
            mcpMaxJerkAppl_ = 17,
            mcpActualPosition = 1000,
            mcpCommandedAccel = 1001,
            mcpActualAccel = 1002,
            mcpCommandedJerk = 1003,
            mcpActualJerk = 1004,
            mcpTotalBufferCount = 1010,
            mcpAvailableBufferCount = 1011,
            mcpECCommandPosition = 1501, //v12.2.0.0
            mcpECActualPosition = 1502, //v12.2.0.0
            mcpAxisType = 2002,
            mcpModuloAxis = 2003,
            mcpModuloValue = 2004,
            mcpEnableHWLimitPos = 2010,
            mcpHWLimitPosInputNum = 2011,
            mcpHWLimitPosActLevel = 2012,
            mcpEnableHWLimitNeg = 2013,
            mcpHWLimitNegInputNum = 2014,
            mcpHWLimitNegActLevel = 2015,
            mcpHomeInputNum = 2016,
            mcpHomeActLevel = 2017,
            mcpMarkerInputNum = 2018,
            mcpMarkerActLevel = 2019,
            mcpInputActLevel = 2020,
            mcpEnableLimitPos = 2030,
            mcpSWLimitPos = 2031,
            mcpEnableLimitNeg = 2032,
            mcpSWLimitNeg = 2033,
            mcpMaxVelocityAppl = 2034,
            mcpMaxAccelAppl = 2035,
            mcpMaxDecelAppl = 2036,
            mcpMaxJerkAppl = 2037,
            mcpMaxVelocitySystem = 2038,
            mcpMaxAccelerationSystem = 2039,
            mcpMaxDecelerationSystem = 2040,
            mcpmcpMaxJerkSystem = 2041,
            mcpmcpEStopType = 2060,
            mcpEStopDecel = 2061,
            mcpEStopJerk = 2062,
            mcpInvertCmdDir = 2070,
            mcpCmdScaleFactor = 2071,
            mcpFeedbackMode = 2072,
            mcpInvertFeedbackDir = 2073,
            mcpFeedbackScaleFactor = 2074,
            mcpPositionFeedbackFilter = 2075,
            mcpVelocityFeedbackFilter = 2076,
            mcpAccelerationFeedbackFilter = 2077,
            mcpStartVelocityOffset = 2078,
            mcpStopVelocityOffset = 2079,
            mcpInPositionCheckType = 2080,
            mcpInPositionWindowSize = 2081,
            mcpInVelocityWindowSize = 2082,
            mcpEnablePositionLagMonit = 2083,
            mcpMaxPositionLag = 2084,
            mcpPositionLagCalMethod = 2085,
            mcpEnableVelocityLagMonit = 2086,
            mcpMaxVelocityLag = 2087,
            mcpVelocityLagCalMethod = 2088,
            mcpHomingType = 2100,
            mcpHomingDir = 2101,
            mcpHomingVelocity = 2102,
            mcpHomingAcceleration = 2103,
            mcpHomingDeceleration = 2104,
            mcpHomingJerk = 2105,
            mcpHomingCreepVelocity = 2106,
            mcpHomePositionOffset = 2107,
            mcpHomeCompleteFlagHandle = 2108,
            //v12.1.0.33
            mcpSensor0StopEnable = 2109,
            mcpSensor0StopMode = 2110,
            mcpSensor0StopIOOffset = 2111,
            mcpSensor0StopIOSize = 2112,
            mcpSensor0StopIOBit = 2113,
            mcpSensor0StopPosOffset = 2114,
            mcpSensor1StopEnable = 2115,
            mcpSensor1StopMode = 2116,
            mcpSensor1StopIOOffset = 2117,
            mcpSensor1StopIOSize = 2118,
            mcpSensor1StopIOBit = 2119,
            mcpSensor1StopPosOffset = 2120,
            mcpSensor2StopEnable = 2121,
            mcpSensor2StopMode = 2122,
            mcpSensor2StopIOOffset = 2123,
            mcpSensor2StopIOSize = 2124,
            mcpSensor2StopIOBit = 2125,
            mcpSensor2StopPosOffset = 2126,
            mcpSensor3StopEnable = 2127,
            mcpSensor3StopMode = 2128,
            mcpSensor3StopIOOffset = 2129,
            mcpSensor3StopIOSize = 2130,
            mcpSensor3StopIOBit = 2131,
            mcpSensor3StopPosOffset = 2132,
            mcpSensor4StopEnable = 2133,
            mcpSensor4StopMode = 2134,
            mcpSensor4StopIOOffset = 2135,
            mcpSensor4StopIOSize = 2136,
            mcpSensor4StopIOBit = 2137,
            mcpSensor4StopPosOffset = 2138,
            //v12.1.0.40
            mcpPositiveLimitErrorStop = 2139,
            mcpNegativeLimitErrorStop = 2140,
            //
            mcpSensor0MovingVelocity = 2141,
            mcpSensor1MovingVelocity = 2142,
            mcpSensor2MovingVelocity = 2143,
            mcpSensor3MovingVelocity = 2144,
            mcpSensor4MovingVelocity = 2145,
            //v12.1.0.67
            mcpPositionOffset = 2146,
            mcpPositionOffsetValue = 2147,
            mcpMaintainGearStateEnable = 2148,
            mcpClampingVelocity = 2149,
            //v12.2.0.2
            mcpMaxTorqueMode = 2150,
            //v12.2.0.19
            mcpJoystickPauseMode = 2151,
            mcpKeepHaltMotion = 2152,
        }
    }

    //MMC Library APIs
    public partial class NMCSDKLib
    {
        //=============================================================================
        //
        //
        //
        //
        // System APIs - Initialization, General & Utility Functions
        //
        //
        //
        //
        //=============================================================================
        #region Public Function
        //-----------------------------------------------------------------------------
        // Summary : 설치된 모든 MMCE 보드를 초기화
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_Init();

        //-----------------------------------------------------------------------------
        // Summary : 지정된 MMCE 보드를 초기화
        // Paremeter(I) : MasterID -> MMCE 보드 Switch 번호 (0~9)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MasterInit(ushort MasterID);

        //-----------------------------------------------------------------------------
        // Summary : 지정된 MMCE Master의 EtherCAT 동작 상태를 RUN 상태로 변경
        // Parameter(I) : MasterID -> MMCE 보드 Switch 번호 (0~9)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MasterRUN(ushort MasterID);

        //-----------------------------------------------------------------------------
        // Summary : 지정된 MMCE Master의 EtherCAT 동작 상태를 STOP 상태로 변경
        // Parameter(I) : MasterID -> MMCE 보드 Switch 번호 (0~9)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MasterSTOP(ushort MasterID);

        //-----------------------------------------------------------------------------
        // Summary : MMCE S/W Version 정보를 가져 옴
        // Parameter(I) : Type -> Software Type 지정
        // Parameter(O) : Major -> Version 상위 정보
        // Parameter(O) : Minor -> Version 하위 정보
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GetSWVersion(ushort Type, ref ushort Major, ref ushort Minor);

        //-----------------------------------------------------------------------------
        // Summary : MMCE Motion Library 사용 하면서 발생한 에러 코드에 대한 문자열을 읽음
        // Parameter(I) : ErrorCode -> 문자열을 얻고자 하는 Error Code 지정
        // Parameter(I) : Size -> MAX_ERR_LEN(128) 고정값 지정
        // Parameter(O) : ErrorMessage -> Error Code 에 해당하는 문자열
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GetErrorMessage(uint ErrorCode, uint Size, StringBuilder ErrorMessage);

        //-----------------------------------------------------------------------------
        // Summary : ENI.xml 과 실제 Device Slave를 비교하여 VendorID or ProductCode or 
        //           RevisionNo or AliasNo 가 다를 경우, Mismatched 된 Slave 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : MisMatchSlaveCount -> Mismatched 된 Slave의 Count
        // Parameter(O) : MismatchSlaveID[MAX_AXIS_CNT] -> Mismatched 된 Slave의 ID
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GetMismatchErrorInfo(ushort BoardID, ref ushort MismatchSlaveCount, ushort[] MismatchSlaveID);

        //-----------------------------------------------------------------------------
        // Summary : 현재 설치된 모든 보드의 ID List 및 설치보드 개수 를 가져 옴
        // Parameter(O) : MasterMap[MAX_BOARD_CNT] -> 설치된 보드 ID List (0~9)
        // Parameter(O) : MasterCount -> 설치된 보드 갯수
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GetMasterMap(ushort[] MasterMap, ref ushort MasterCount);

        //-----------------------------------------------------------------------------
        // Summary : 현재 설치된 보드의 개수를 가져 옴
        // Parameter(O) : MasterCount -> 설치된 보드 개수
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GetMasterCount(ref ushort MasterCount);

        //-----------------------------------------------------------------------------
        // Summary : 지정된 BoardID 에 해당하는 Master 의 Scan Number 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : BoardScanNo -> BoardScanNo (0,1,2...)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GetBoardScanNo(ushort BoardID, ref ushort BoardScanNo);

        //-----------------------------------------------------------------------------
        // Summary : 속도 개선 API에 대한 Delay Level Count 조정
        // Parameter(I) : Level -> Deley Level Count 입력 (Default:25)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_SetSystemPerformance(ushort Level);

        //-----------------------------------------------------------------------------
        // Summary : 지정된 MMCE 보드의 Fail 된 Slave Node ID 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : NodeFailureID -> Slave Node Failure ID
        //-----------------------------------------------------------------------------
        //v12.1.0.55
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetNodeFailureID(
                                    ushort BoardID,
                                    ref ushort NodeFailureID
                                    );

        //-----------------------------------------------------------------------------
        // Summary : S/W Emergency 상황을 해제 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : RestoreStatus -> S/W EMG 상태 정보
        // * Status 1 : S/W EMG 발생 되었고, EMG 발생 조건 해제 되었음
        // * Status 2 : S/W EMG 가 발생 되지 않은 상태
        // * Status 3 : S/W EMG 가 발생 되었고, EMG 발생 조건이 해제 되지 않음
        //-----------------------------------------------------------------------------
        //v12.1.0.58
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_RestoreSWEmergency(
                                    ushort BoardID,
                                    ref byte RestoreStatus
                                    );

        //-----------------------------------------------------------------------------
        // Summary : I/O 자기 유지 기능을 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : WaitTime -> Wait Time Before Slave SafeOP Check (Range: 0 ~ 100000 ms)
        //-----------------------------------------------------------------------------
        //v12.1.0.67
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_SetIOSelfHoldFunc(
                                       ushort BoardID,
                                     ushort WaitTime
                                     );

        //-----------------------------------------------------------------------------
        // Summary : ENI.xml File을 MMCE B/D에 다운로드 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : sNvsFileName -> eMMC.nvs Full Path File Name ex) c:\\test\\eMMC.nvs
        // Parameter(I) : sEniFileName -> ENI.xml Full Path File Name ex) c:\\test\\ENI.xml
        //-----------------------------------------------------------------------------
        //v12.1.0.68
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_EniFileDownload(
                                    ushort BoardID,
                                    StringBuilder sNvsFileName,
                                    StringBuilder sEniFileName
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Master APIs
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : 지정한 MMCE 보드의 Master State (EtherCAT Network State)를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : MasterMode -> Master State 를 리턴
        //-----------------------------------------------------------------------------
        // * Master State
        // 0 : IDLE -> 정지 상태
        // 1 : SCAN -> 스캔 중
        // 2 : RUN -> 정상 동작 상태
        // 3 : INTRANSITION -> 상태 변경 중
        // 4 : ERROR -> 에러 상태
        // 5 : LINKBROKEN -> 통신이 끊긴 상태
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetCurMode(ushort BoardID, ref byte MasterMode);

        //-----------------------------------------------------------------------------
        // Summary : 시스템에 설치 되어 있는 MMCE BoardID를 OS에서 Scan한 번호를 통해 가져 옴
        // Parameter(I) : MasterScanNo -> OS 에서 Scan 순서에 따라 지정한 Scan 번호 (0~3)
        // Parameter(O) : MasterID -> Scan 번호에 해당하는 Board의 ID 값
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetBoardID(ushort MasterScanNo, ref ushort MasterID);

        //-----------------------------------------------------------------------------
        // Summary : 시스템에 설치 되어 있는 MMCE 보드의 ARM OS 버전 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : Major -> 상위 Majer Version 정보
        // Parameter(O) : Minor -> 하위 Minor Version 정보
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetOSRevision(ushort BoardID, ref byte Major, ref byte Minor);

        //-----------------------------------------------------------------------------
        // Summary : 시스템에 설치 되어 있는 MMCE 보드의 Motion DSP 버전 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : Major -> 상위 Majer Version 정보
        // Parameter(O) : Minor -> 하위 Minor Version 정보
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetMCRevision(ushort BoardID, ref byte Major, ref byte Minor);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에서 Scan한 Slave ScanNo.를 통해 해당 Slave의 SDO의 Data를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : SlaveNo -> Slave ScanNo. 지정, MMCE 보드에서 Scan시 지정한 번호 (0~)
        // Parameter(I) : SDOIndex -> Slave가 제공하는 유효한 SDO Index 번호
        // Parameter(I) : SubIndex -> Slave가 제공하는 유효한 SDO SubIndex 번호 (SubIndex가 없을 경우 0 을 지정)
        // Parameter(I) : DataSize -> 읽고자 하는 Data의 Size (Byte) (1~1000)
        // Parameter(O) : respDataSize -> 읽은 Data Size 리턴 값
        // Parameter(O) : bDataArray -> 읽은 Data (Data는 배열의 형태로 배열개수 = Byte 개수로 선언 해야함)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetSDOData(
                                    ushort BoardID,
                                    ushort SlaveNo,
                                    ushort SDOIndex,
                                    byte SubIndex,
                                    uint DataSize,
                                    ref uint respDataSize,
                                    byte[] bDataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 EtherCAT Address에 해당하는 Slave SDO의 Data를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device의 EtherCAT Address 지정 (1~)
        // Parameter(I) : SDOIndex -> Slave가 제공하는 유효한 SDO Index 번호
        // Parameter(I) : SubIndex -> Slave가 제공하는 유효한 SDO SubIndex 번호 
        // Parameter(I) : DataSize -> 읽고자 하는 Data의 Size (Byte) (1~1000)
        // Parameter(O) : respDataSize -> 읽은 Data Size 리턴 값
        // Parameter(O) : bDataArray -> 읽은 Data (Data는 배열의 형태로 배열개수 = Byte 개수로 선언 해야함)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetSDODataEcatAddr(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort SDOIndex,
                                    byte SubIndex,
                                    uint DataSize,
                                    ref uint respDataSize,
                                    byte[] bDataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에서 Scan한 Slave ScanNo.를 통해 해당 Slave의 SDO에 Data를 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : SlaveNo -> Slave ScanNo. 지정, MMCE 보드에서 Scan시 지정한 번호 (0~)
        // Parameter(I) : SDOIndex -> Slave가 제공하는 유효한 SDO Index 번호
        // Parameter(I) : SubIndex -> Slave가 제공하는 유효한 SDO SubIndex 번호 (SubIndex가 없을 경우 0 지정)
        // Parameter(I) : DataSize -> 쓰고자 하는 Data의 Size(Byte) (1~1000)
        // Parameter(O) : respDataSize -> 쓰고자 하는 Data의 Size 리턴 값
        // Parameter(I) : bDataArray -> 쓰고자 하는 Data     
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterSetSDOData(
                                    ushort BoardID,
                                    ushort SlaveNo,
                                    ushort SDOIndex,
                                    byte SubIndex,
                                    uint DataSize,
                                    ref uint respDataSize,
                                    byte[] bDataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 EtherCAT Address에 해당하는 Slave의 SDO에 Data를 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9) (0~9)
        // Parameter(I) : EcatAddr -> Slave Device의 EtherCAT Address 지정 (1~)
        // Parameter(I) : SDOIndex -> Slave가 제공하는 유효한 SDO Index 번호
        // Parameter(I) : SubIndex -> Slave가 제공하는 유효한 SDO SubIndex 번호 (SubIndex가 없을 경우 0 지정)
        // Parameter(I) : DataSize -> 쓰고자 하는 Data의 Size(Byte) (1~1000)
        // Parameter(O) : respDataSize -> 쓰고자 하는 Data의 Size 리턴 값
        // Parameter(I) : bDataArray -> 쓰고자 하는 Data
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterSetSDODataEcatAddr(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort SDOIndex,
                                    byte SubIndex,
                                    uint DataSize,
                                    ref uint respDataSize,
                                    byte[] bDataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE Board에서 발생한 마지막 Error 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : pSequenceNo -> Error Sequence Number
        // Parameter(O) : pErrorCode -> Error Code
        // Parameter(O) : ExtErrorInfo -> Extention Error Information (6Byte)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetLastError(
                                    ushort BoardID,
                                    ref uint pSequenceNo,
                                    ref uint pErrorCode,
                                    byte[] ExtErrorInfo
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE Board에서 발생한 마지막 Error 정보를 지움
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterClearError(
                                    ushort BoardID
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Master APIs - Device Info.
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 축의 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : TotalAxisCount -> Axis Count 합계
        //-----------------------------------------------------------------------------
        //Master Get Axes Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetAxesCount(ushort BoardID, ref ushort TotalAxisCount);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 축들의 ID List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : AxisIDArray -> Axis ID Array
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetAxesID(ushort BoardID, ushort[] AxisIDArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Deivce의 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : TotalDeviceCount -> Device Count 합계
        //-----------------------------------------------------------------------------
        //Master Get Device Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetDeviceCount(ushort BoardID, ref ushort TotalDeviceCount);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Deivce ID List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceIDArray -> Device ID Array (EcatAddr)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGetDeviceID(ushort BoardID, ushort[] DeviceIDArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Input 의 Device 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceCount -> Digital Input Device 개수
        //-----------------------------------------------------------------------------
        //Master Get Digital Input Info. (Module Info. Update v12.1.0.64)
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DI_DeviceCount(ushort BoardID, ref ushort DeviceCount);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Input 의 Device ID List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceIDArray -> Digital Input Device ID Array (EcatAddr)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DI_DeviceID(ushort BoardID, ushort[] DeviceIDArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Input 의 Total Channel Size 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Input DeviceID (EcatAddr) 
        // Parameter(O) : DeviceTotalChSize -> Digital Input Device Total Channel Size
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DI_DeviceTotalChSize(ushort BoardID, ushort DeviceID, ref ushort DeviceTotalChSize);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Input 의 Device Name 을 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Input DeviceID (EcatAddr) 
        // Parameter(O) : DeviceTotalChSize -> Digital Input Device Name Array [256] 
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DI_DeviceName(ushort BoardID, ushort DeviceID, byte[] DeviceNameArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Output 의 Device 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceCount -> Digital Output Device 개수
        //-----------------------------------------------------------------------------        
        //Master Get Digital Output Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DO_DeviceCount(ushort BoardID, ref ushort DeviceCount);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Output 의 Device ID List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceIDArray -> Digital Output Device ID Array (EcatAddr)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DO_DeviceID(ushort BoardID, ushort[] DeviceIDArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Output 의 Total Channel Size 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Output DeviceID (EcatAddr) 
        // Parameter(O) : DeviceTotalChSize -> Digital Output Device Total Channel Size
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DO_DeviceTotalChSize(ushort BoardID, ushort DeviceID, ref ushort DeviceTotalChSize);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Output 의 Device Name 을 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Output DeviceID (EcatAddr) 
        // Parameter(O) : DeviceTotalChSize -> Digital Output Device Name Array [256] 
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DO_DeviceName(ushort BoardID, ushort DeviceID, byte[] DeviceNameArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Input 의 Device 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceCount -> Analog Input Device 개수
        //-----------------------------------------------------------------------------
        //Master Get Analog Input Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AI_DeviceCount(ushort BoardID, ref ushort DeviceCount);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Input 의 Device ID List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceIDArray -> Analog Input Device ID Array (EcatAddr)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AI_DeviceID(ushort BoardID, ushort[] DeviceIDArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Input 의 Total Channel Size 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Input DeviceID (EcatAddr) 
        // Parameter(O) : DeviceTotalChSize -> Analog Input Device Total Channel Size
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AI_DeviceTotalChSize(ushort BoardID, ushort DeviceID, ref ushort DeviceTotalChSize);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Input 의 Device Name 을 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Input DeviceID (EcatAddr) 
        // Parameter(O) : DeviceTotalChSize -> Analog Input Device Name Array [256] 
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AI_DeviceName(ushort BoardID, ushort DeviceID, byte[] DeviceNameArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Output 의 Device 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceCount -> Analog Output Device 개수
        //-----------------------------------------------------------------------------
        //Master Get Analog Output Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AO_DeviceCount(ushort BoardID, ref ushort DeviceCount);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Output 의 Device ID List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceIDArray -> Analog Output Device ID Array (EcatAddr)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AO_DeviceID(ushort BoardID, ushort[] DeviceIDArray);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Output 의 Total Channel Size 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Output DeviceID (EcatAddr) 
        // Parameter(O) : DeviceTotalChSize -> Analog Output Device Total Channel Size
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AO_DeviceTotalChSize(ushort BoardID, ushort DeviceID, ref ushort DeviceTotalChSize);

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Output 의 Device Name 을 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Output DeviceID (EcatAddr) 
        // Parameter(O) : DeviceTotalChSize -> Analog Output Device Name Array [256] 
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AO_DeviceName(ushort BoardID, ushort DeviceID, byte[] DeviceNameArray);

        //=============================================================================
        //
        //
        //
        //
        // Master APIs - Device I/O 모듈 Info. (v12.2.0.10)
        // 
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Input I/O 모듈의 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Input DeviceID (EcatAddr)						
        // Parameter(O) : Count -> Digital Input I/O 모듈의 개수
        //-----------------------------------------------------------------------------
        //Digital Input - I/O Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DI_IO_Count(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    ref ushort Count
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Input I/O 모듈의 Name List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Input DeviceID (EcatAddr)						
        // Parameter(O) : NameArray -> Digital Input I/O 모듈의 Name Array 를 가져 옴 [][256]
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DI_IO_Name(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    byte[,] NameArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Input I/O 모듈의 BitSize, Raw IOOffset List 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Input DeviceID (EcatAddr)						
        // Parameter(O) : BitSizeArray -> Digital Input I/O 모듈의 Bit Size Array
        // Parameter(O) : RawIOOffsetArray -> Digital Input I/O 모듈의 Raw IOOffet Array
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DI_IO_Data(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    ushort[] BitSizeArray,
                                    ushort[] RawIOOffsetArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Output I/O 모듈의 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Output DeviceID (EcatAddr)
        // Parameter(O) : Count -> Digital Output I/O 모듈의 개수
        //-----------------------------------------------------------------------------
        //Digital Output - I/O Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DO_IO_Count(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    ref ushort Count
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Output I/O 모듈의 Name List 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Output DeviceID (EcatAddr)						
        // Parameter(O) : NameArray -> Digital Output I/O 모듈의 Name Array 를 가져 옴 [][256]
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DO_IO_Name(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    byte[,] NameArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Digital Output I/O 모듈의 BitSize, Raw IOOffset List 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Digital Output DeviceID (EcatAddr)						
        // Parameter(O) : BitSizeArray -> Digital Output I/O 모듈의 Bit Size Array
        // Parameter(O) : RawIOOffsetArray -> Digital Output I/O 모듈의 Raw IOOffet Array
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_DO_IO_Data(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    ushort[] BitSizeArray,
                                    ushort[] RawIOOffsetArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Input I/O 모듈의 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Input DeviceID (EcatAddr)
        // Parameter(O) : Count -> Analog Input I/O 모듈의 개수
        //-----------------------------------------------------------------------------
        //Analog Input - I/O Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AI_IO_Count(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    ref ushort Count
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Input I/O 모듈의 Name List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Input DeviceID (EcatAddr)						
        // Parameter(O) : NameArray -> Analog Input I/O 모듈의 Name Array 를 가져 옴 [][256]
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AI_IO_Name(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    byte[,] NameArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Input I/O 모듈의 BitSize, Raw IOOffset List 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Input DeviceID (EcatAddr)						
        // Parameter(O) : BitSizeArray -> Analog Input I/O 모듈의 Bit Size Array
        // Parameter(O) : RawIOOffsetArray -> Analog Input I/O 모듈의 Raw IOOffet Array
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AI_IO_Data(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    ushort[] BitSizeArray,
                                    ushort[] RawIOOffsetArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Output I/O 모듈의 개수를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Output DeviceID (EcatAddr)
        // Parameter(O) : Count -> Analog Output I/O 모듈의 개수
        //-----------------------------------------------------------------------------
        //Analog Output - I/O Info.
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AO_IO_Count(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    ref ushort Count
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Output I/O 모듈의 Name List 를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Output DeviceID (EcatAddr)						
        // Parameter(O) : NameArray -> Analog Output I/O 모듈의 Name Array 를 가져 옴 [][256]
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AO_IO_Name(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    byte[,] NameArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 구성 되어 있는 Analog Output I/O 모듈의 BitSize, Raw IOOffset List 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : DeviceID -> Analog Output DeviceID (EcatAddr)						
        // Parameter(O) : BitSizeArray -> Analog Output I/O 모듈의 Bit Size Array
        // Parameter(O) : RawIOOffsetArray -> Analog Output I/O 모듈의 Raw IOOffet Array
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterGet_AO_IO_Data(
                                    ushort BoardID,
                                    ushort DeviceID,
                                    ushort[] BitSizeArray,
                                    ushort[] RawIOOffsetArray
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Slave APIs
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : Slave Device 의 Alias Switch No.를 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device의 EtherCAT Address 지정
        // Parameter(O) : AliasID -> Slave Device 의 Alias Switch 번호
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS SlaveGetAliasNo(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ref ushort AliasID
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Slave Device 의 Alias Switch No.를 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device의 EtherCAT Address 지정
        // Parameter(I) : AliasID -> Slave Device 의 Alias ID 번호 지정
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS SlaveSetAliasNo(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort AliasID
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Slave Device 의 현재 EtherCAT State 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device의 EtherCAT Address 지정
        // Parameter(O) : data -> Slave Device 의 EtherCAT Slave State 정보
        //-----------------------------------------------------------------------------
        // * EtherCAT Slave State 정보
        // 0x00 : eST_UNKNOWN   -> 인식 불가 상태
        // 0x01 : eST_INIT	    -> 초기화 상태
        // 0x02 : eST_PREOP     -> 동작 전 상태
        // 0x03 : eST_BOOTSTRAP -> Boot 상태 
        // 0x04 : eST_SAFEOP	-> 안전 동작 상태
        // 0x08 : eST_OP		-> 정상 동작 상태
        // 0x10 : eST_ACKERR    -> 에러 상태  
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS SlaveGetCurState(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ref byte data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 연결 된 모든 Slave Device 의 현재 EtherCAT State 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : DeviceCount -> ENI.xml을 기반의 Slave Device Count 정보
        // Parameter(O) : WorkingCount -> 실제 물리적인 Slave Device Count 정보
        // Parameter(O) : StatusCombination -> 연결 된 전체 Slave 에 대한 State 조합 정보
        //                (*enum StatusCombination_Tag 상태 참고)
        //-----------------------------------------------------------------------------
        //v12.2.0.12
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS SlaveGetCurStateAll(
                                    ushort BoardID,
                                    ref ushort DeviceCount,
                                    ref ushort WorkingCount,
                                    ref byte StatusCombination
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE Master의 EtherCAT 명령으로 Slave ET1100 레지스터를 직접 접근 처리
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatCmd -> EtherCAT 명령
        // Parameter(I) : Adp -> Slave EhterCAT Addr.
        // Parameter(I) : Ado -> Slave Physical Memory Addr. (ET1100 레지스터 주소)
        // Parameter(I) : ReqDataSize -> 송신 데이터 사이즈
        // Parameter(I) : bReqDataArray -> 송신 데이터
        // Parameter(O) : RespDataSize -> 수신 데이터 사이즈
        // Parameter(O) : bRespDataArray -> 수신 데이터
        // Parameter(O) : WC -> 수신 Working Count 정보
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MasterECatDirectAccess(
                                    ushort BoardID,
                                    byte EcatCmd,
                                    ushort Adp,
                                    ushort Ado,
                                    ushort ReqDataSize,
                                    byte[] bReqDataArray,
                                    ref ushort RespDataSize,
                                    byte[] bRespDataArray,
                                    ref ushort WC
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Slave APIs - Slave Home
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : Home Flag 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : EcatAddr -> EtherCAT Addr.
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_SetHomeFlag(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ushort EcatAddr
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Home Flag 정보 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(O) : pHomeFlag -> Home Flag 정보
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GetHomeFlag(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref uint pHomeFlag
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Slave의 동작 모드를 변경
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Mode -> Mode 설정 (6:HM-원점복귀모드, 8:CSP-주기적동기위치모드)
        // Parameter(I) : ReservedZero -> MC_BUFFER_MODE BufferMode (Reserved)
        // * 주의 사항 : ModeOfOperation(0x6060)이 포함된 PDO Map으로 변경 해야 함 
        //-----------------------------------------------------------------------------
        //v12.1.0.35
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ModeChange(
                                    ushort BoardID,
                                    ushort AxisID,
                                    byte Mode,
                                    byte ReservedZero
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 원점 복귀 모드에서 동작하는 원점복귀 동작 조건을 설정 (SDO 통신으로 파라미터 설정)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave의 EtherCat 주소    
        // Parameter(I) : Offset -> 유효한 EtherCAT 주소 Offset		
        // Parameter(I) : Method -> CiA402를 기반으로 하는 원점 복귀 방법     
        // Parameter(I) : SpeedSwitch -> Sensor를 찾아가는 이동 속도
        // Parameter(I) : SpeedZero -> 원점을 찾아가는 이동 속도   
        // Parameter(I) : Acceleration -> 원점 복귀 과정에서 유효한 가속도
        // Parameter(I) : ReservedZero -> MC_BUFFER_MODE BufferMode (Reserved) 
        //-----------------------------------------------------------------------------
        //v12.1.0.35
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_SlaveHomeSet(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    int Offset,
                                    byte Method,
                                    uint SpeedSwitch,
                                    uint SpeedZero,
                                    uint Acceleration,
                                    byte ReservedZero
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Slave(CSD7)의 원점 복귀 기능을 동작 및 정지 시킴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number    
        // Parameter(I) : Start -> 지정한 축의 원점 복귀 동작의 시작과 정지 (0:정지(Stop), 1:시작(Start))	
        // Parameter(I) : ReservedZero -> Reserved 0 (MC_BUFFER_MODE BufferMode)
        //-----------------------------------------------------------------------------
        //v12.1.0.35
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_SlaveHome(
                                    ushort BoardID,
                                    ushort AxisID,
                                    byte Start,
                                    byte ReservedZero
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Slave Home 과정을 중지 (ECAT 동작모드를 Slave Home(6) 모드로 변경후 동작시키는 Home 방법)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number    
        // Parameter(I) : ReservedZero -> Reserved 0 (MC_BUFFER_MODE BufferMode)
        //-----------------------------------------------------------------------------
        //v12.1.0.35
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_SlaveHomeHalt(
                                    ushort BoardID,
                                    ushort AxisID,
                                    byte ReservedZero
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 원점 복귀 모드에서 Slave의 상태를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number    
        // Parameter(O) : HomeStatus -> 원점복귀 동작상태 (Bit0:Homing Complete, Bit1:Homing Error)
        //-----------------------------------------------------------------------------
        //v12.1.0.35
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadSlaveHomeStatus(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref uint HomeStatus
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Slave의 동작 모드를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number    
        // Parameter(O) : HomeStatus -> Slave의 동작모드 (6:HM-원점복귀모드, 8:CSP-주기적동기위치모드)
        //-----------------------------------------------------------------------------
        //v12.1.0.35
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadSlaveModeStatus(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref byte ModeStatus
                                    );

        //=============================================================================
        //
        //
        //
        //
        // I/O APIs
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드의 I/O 동작 상태를 확인 
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : type -> Buffer Type 설정 (0x00:BUF_OUT, 0x01:BUF_IN)
        // Parameter(O) : State -> I/O 동작 상태
        // * I/O 동작 상태
        //	 0x0000 : IO_INIT -> I/O 초기화 상태
        //	 0x0001 : IO_CONFIG_DONE -> I/O 구성 완료 상태
        //	 0x0002 : STATE_IO_RUN -> I/O 정상 동작 상태
        //   0x0003 : STATE_IO_STOP -> I/O 동작 정지 상태
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GetIOState(
                                    ushort BoardID,
                                    uint type,
                                    ref uint State
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 전체 Device 의 Output I/O 에 설정 하고자 하는 Data size 의 값을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : Offset -> I/O Memory 전체 영역에서 해당 위치
        // Parameter(I) : Size -> 쓰고자 하는 IO Memory 영역의 Size
        // Parameter(I) : DataArray -> 쓰고자 하는 I/O Data
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_RAW_WRITE(
                                    ushort BoardID,
                                    uint Offset,
                                    uint Size,
                                    byte[] DataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Output I/O 에 설정 하고자 하는 Data Size 의 값을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address 
        // Parameter(I) : Offset -> Slave I/O Memory 영역에서 해당 위치
        // Parameter(I) : Size -> 쓰고자 하는 IO Memory 영역의 Size
        // Parameter(I) : DataArray -> 쓰고자 하는 I/O Data
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_WRITE(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    uint Offset,
                                    uint Size,
                                    byte[] DataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Output I/O 에 BIT 값을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address 
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : bitOffset -> BYTE 단위에서 (0-7bit) Bit Offset
        // Parameter(I) : data -> bool 데이터 값 입력 (true:set, false:reset)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_WRITE_BIT(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    uint Offset,
                                    byte bitOffset,
                                    bool data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 전체 Device 의 Output I/O 에 대한 BIT 값을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : bitOffset -> BYTE 단위에서 (0~7bit) Bit Offset
        // Parameter(I) : data -> bool 데이터 값 입력 (true:set, false:reset)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_RAW_WRITE_BIT(
                                    ushort BoardID,
                                    uint Offset,
                                    byte bitOffset,
                                    bool data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Output I/O 에 대한 BYTE 값을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address 
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : data -> Byte Data 값을 입력
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_WRITE_BYTE(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    uint Offset,
                                    byte data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Output I/O 에 대한 WORD 값을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address 
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : data -> WORD Data 값을 입력
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_WRITE_WORD(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    uint Offset,
                                    ushort data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Output I/O 에 대한 DWORD 값을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address 
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : data -> DWORD Data 값을 입력
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_WRITE_DWORD(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    uint Offset,
                                    ushort data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Input/Output I/O 에 대한 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address
        // Parameter(I) : BufferInOut -> Buffer In/Out 설정 (0:BUF_OUT, 1:BUF_IN)
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : Size -> Data Size
        // Parameter(O) : DataArray -> 읽은 데이터 값 (Data Array)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_READ(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort BufferInOut,
                                    uint Offset,
                                    uint Size,
                                    byte[] DataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 전체 Device 의 Input/Output I/O 에 대한 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : BufferInOut -> Buffer In/Out 설정 (0:BUF_OUT, 1:BUF_IN)
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : Size -> Data Size
        // Parameter(O) : DataArray -> 읽은 데이터 값 (Data Array)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_RAW_READ(
                                    ushort BoardID,
                                    ushort BufferInOut,
                                    uint Offset,
                                    uint Size,
                                    byte[] DataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Input/Output I/O 에 대한 BIT 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address
        // Parameter(I) : BufferInOut -> Buffer In/Out 설정 (0:BUF_OUT, 1:BUF_IN)
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : BitOffset -> BYTE 내에서 Bit Offset : 0~7
        // Parameter(O) : data -> 읽은 데이터 값 (true or false)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_READ_BIT(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort BufferInOut,
                                    uint Offset,
                                    byte BitOffset,
                                    ref bool data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 전체 Device 의 Input/Output I/O 에 대한 BIT 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address
        // Parameter(I) : BufferInOut -> Buffer In/Out 설정 (0:BUF_OUT, 1:BUF_IN)
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(I) : BitOffset -> BYTE 내에서 Bit Offset : 0~7
        // Parameter(O) : data -> 읽은 데이터 값 (true or false)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_RAW_READ_BIT(
                                    ushort BoardID,
                                    ushort BufferInOut,
                                    uint Offset,
                                    byte BitOffset,
                                    ref bool data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Input/Output I/O 에 대한 BYTE 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address
        // Parameter(I) : BufferInOut -> Buffer In/Out 설정 (0:BUF_OUT, 1:BUF_IN)
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(O) : data -> 읽은 데이터 값 (Byte Data)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_READ_BYTE(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort BufferInOut,
                                    uint Offset,
                                    ref byte data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Input/Output I/O 에 대한 WORD 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address
        // Parameter(I) : BufferInOut -> Buffer In/Out 설정 (0:BUF_OUT, 1:BUF_IN)
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(O) : data -> 읽은 데이터 값 (WORD Data)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_READ_WORD(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort BufferInOut,
                                    uint Offset,
                                    ref ushort data
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Device 의 Input/Output I/O 에 대한 DWORD 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Slave Device 의 EtherCAT Address
        // Parameter(I) : BufferInOut -> Buffer In/Out 설정 (0:BUF_OUT, 1:BUF_IN)
        // Parameter(I) : Offset -> Device 의 Output Memory 에서 해당 위치
        // Parameter(O) : data -> 읽은 데이터 값 (DWORD Data)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_READ_DWORD(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort BufferInOut,
                                    uint Offset,
                                    ref uint data
                                    );

        //=============================================================================
        //
        //
        //
        //
        // I/O APIs - Buffering I/O Control
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : Buffering I/O 기능을 사용 할 Output 신호를 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> 축 or 그룹 or 겐트리 ID
        // Parameter(I) : IDIndexType -> ID 분류 코드 입력 (1-Axis or 2-Group or 3-Gantry) 
        // Parameter(I) : EcatAddr -> Buffering I/O 기능을 사용 할 Slave의 EtherCAT Addr.
        // Parameter(I) : SlaveOffset -> Buffering I/O 기능을 사용 할 Slave의 EtherCAT 주소 내 Offset
        // Parameter(I) : OutputBitNumber -> Buffering I/O 기능을 사용 할 Slave 의 Bit 번호(0~31)
        // Parameter(I) : OutputValue -> Buffering I/O 기능을 사용 할 Slave 의 Bit 출력 값 (1:Set, 0:Clear)
        //-----------------------------------------------------------------------------
        //v12.1.0.66
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_MaskDigitalOutputBit(
                                    ushort BoardID,
                                    ushort AxisID,
                                    byte IDIndexType,
                                    ushort EcatAddr,
                                    ushort SlaveOffset,
                                    byte OutputBitNumber,
                                    bool OutputValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Buffering I/O 기능의 Output 신호를 해제 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Buffering I/O 기능을 해제 할 Slave의 EtherCAT Addr.
        // Parameter(I) : SlaveOffset -> Buffering I/O 기능을 해제 할 Slave의 EtherCAT 주소 내 Offset
        // Parameter(I) : OutputBitNumber -> Buffering I/O 기능을 해제 할 Slave 의 Bit 번호(0~31)
        //-----------------------------------------------------------------------------
        //v12.1.0.66 
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_UnMaskDigitalOutputBit(
                                ushort BoardID,
                                ushort EcatAddr,
                                ushort SlaveOffset,
                                byte OutputBitNumber
                                );

        //-----------------------------------------------------------------------------
        // Summary : 특정 Slave의 Buffering I/O 기능이 설정 되었는지 Bit Number 로 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Buffering I/O 기능을 확인 할 Slave의 EtherCAT Addr.
        // Parameter(I) : SlaveOffset -> Buffering I/O 기능을 확인 할 Slave의 EtherCAT 주소 내 Offset
        // Parameter(I) : OutputBitNumber -> Buffering I/O 기능을 확인 할 Slave 의 Bit 번호(0~31)
        // Parameter(O) : AxisID -> Buffering I/O 기능이 설정 된 축 or 그룹 or 겐트리 ID (설정 되지 않은 경우에는 0xFFFF(65535)가 리턴)
        // Parameter(O) : IDIndexType -> Buffering I/O 기능이 설정 된 ID 타입 (1-Axis or 2-Group or 3-Gantry)
        //               (설정 되지 않은 경우에는 0xFF(255)가 리턴)
        //-----------------------------------------------------------------------------
        //v12.1.0.66 
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_GetMaskedDigitalOutputBit(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort SlaveOffset,
                                    byte OutputBitNumber,
                                    ref ushort AxisID,
                                    ref byte IDIndexType
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 설정된 Buffering I/O 기능이 설정 되어 있는지 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : MaskCount -> 보드에 설정 되어 있는 Buffering I/O 기능의 개수
        // Parameter(O) : EcatAddr -> Buffering I/O 기능이 설정 된 Slave의 EtherCAT 주소
        // Parameter(O) : SlaveOffset -> Buffering I/O 기능이 설정 된 Slave의 EtherCAT 주소내 Offset
        // Parameter(O) : OutputBitNumber -> Buffering I/O 기능이 설정 된 Slave의 Bit 넘버(0~31) (설정 되지 않은 경우에는 0xFF(255)가 리턴)
        // Parameter(O) : AxisID -> Buffering I/O 기능이 설정 된 축 or 그룹 or 겐트리 ID
        // Parameter(O) : IDIndexType -> Buffering I/O 기능이 설정 된 ID 타입 (AxisID:1, GroupID:2, GantryID:3)  
        //-----------------------------------------------------------------------------
        //v12.1.0.66 
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_GetMaskedDigitalOutput(
                                ushort BoardID,
                                ref byte MaskCount,
                                ushort[] EcatAddr,
                                ushort[] SlaveOffset,
                                byte[] OutputBitNumber,
                                ushort[] AxisID,
                                byte[] IDIndexType
                                );

        //-----------------------------------------------------------------------------
        // Summary : Buffering I/O 기능이 설정 된 Slave의 출력을 제어 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Buffering I/O 기능 으로 제어 할 Slave의 EtherCAT 주소
        // Parameter(I) : SlaveOffset -> Buffering I/O 기능 으로 제어 할 Slave의 EtherCAT 주소내 Offset
        // Parameter(I) : OutputBitNumber -> Buffering I/O 기능 으로 제어 할 Slave의 Bit 넘버(0~31)
        // Parameter(I) : Value -> Buffering I/O 기능 으로 제어 할 Slave의 Bit의 출력 값 (1:Set, 0:Clear)
        //-----------------------------------------------------------------------------
        //v12.1.0.66  
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_IO_WriteDigitalOutputBit(
                                ushort BoardID,
                                ushort EcatAddr,
                                ushort SlaveOffset,
                                byte OutputBitNumber,
                                bool Value
                                );

        //=============================================================================
        //
        //
        //
        //
        // Motion APIs - Single Axis Motion
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis에 대하여 Power Stage Control(Amp Enable/Disable)을 수행 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Enable -> Axis Power Enable Flag (0:Disable(AmpOff), 1:Enable(AmpOn))
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_Power(
                                    ushort BoardID,
                                    ushort AxisID,
                                    bool Enable
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis를 설정한 Motion Parameter 들을 사용하여 절대 위치로 이동 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Position -> Target Position
        // Parameter(I) : Velocity -> Max Velocity
        // Parameter(I) : Accel	-> Max Acceleration
        // Parameter(I) : Decel -> Max Deceleration 
        // Parameter(I) : Jerk -> Max Jerk  
        // Parameter(I) : Dir -> 방향 설정 (0:Positive_Direction, 1:Shortest_way, 2:Negative_Direction, 3:Current_Direction)
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveAbsolute(
                                    ushort BoardID,
                                    ushort AxisID,
                                    double Position,
                                    double Velocity,
                                    double Accel,
                                    double Decel,
                                    double Jerk,
                                    MC_DIRECTION Dir,
                                    MC_BUFFER_MODE BufferMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis를 Function Block이 Trig.된 위치에서 설정한 Motion Parameter들을 사용하여 지정한 Distance 만큼 상대 이동 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Distance -> Distance
        // Parameter(I) : Velocity -> Max Velocity
        // Parameter(I) : Accel	-> Max Acceleration
        // Parameter(I) : Decel -> Max Deceleration 
        // Parameter(I) : Jerk -> Max Jerk  
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveRelative(
                                    ushort BoardID,
                                    ushort AxisID,
                                    double Distance,
                                    double Velocity,
                                    double Accel,
                                    double Decel,
                                    double Jerk,
                                    MC_BUFFER_MODE BufferMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis에 대하여 지정한 Velocity로 Continuous Motion 이동 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Velocity -> Target Velocity
        // Parameter(I) : Accel	-> Max Acceleration
        // Parameter(I) : Decel -> Max Deceleration 
        // Parameter(I) : Jerk -> Max Jerk  
        // Parameter(I) : Dir -> 방향 설정 (0:Positive_Direction, 1:Shortest_way, 2:Negative_Direction, 3:Current_Direction)
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveVelocity(
                                    ushort BoardID,
                                    ushort AxisID,
                                    double Velocity,
                                    double Accel,
                                    double Decel,
                                    double Jerk,
                                    MC_DIRECTION Dir,
                                    MC_BUFFER_MODE BufferMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Axis Configuration 에서 지정 된 Homing Mode, Velocity, Acceleration, Deceleration, Jerk 값을 이용하여 원점 위치를 찾음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Position -> Absolute position when the reference signal is detected
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_Home(
                                    ushort BoardID,
                                    ushort AxisID,
                                    double Position,
                                    MC_BUFFER_MODE BufferMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정된 Axis의 Motion 을 멈추게 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Execute -> Execute Flag (true or false)
        // Parameter(I) : Decel -> Max Deceleration
        // Parameter(I) : Jerk -> Max Jerk
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_Stop(
                                    ushort BoardID,
                                    ushort AxisID,
                                    bool Execute,
                                    double Decel,
                                    double Jerk
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정된 Axis의 Status 를 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(O) : pStatus -> Axis 상태 정보
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadStatus(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref uint pStatus
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정된 Axis의 Error 를 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(O) : pErrorID -> Axis Error ID
        // Parameter(O) : pErrorInfo -> Axis Error Info.
        // Parameter(O) : pErrorInfoExt -> Axis Error Extention
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadAxisError(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref ushort pErrorID,
                                    ref ushort pErrorInfo,
                                    ref ushort pErrorInfoExt
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis의 Error State 를 해제 하고, Link 된 Device의 Error가 있을 경우 Error를 해제 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_Reset(
                                    ushort BoardID,
                                    ushort AxisID
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis의 ParameterNum 에 해당 하는 [ Double ] Parameter 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : ParameterNum -> Parameter Number
        // Parameter(O) : pValue -> Parameter Value
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadParameter(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint ParameterNum,
                                    ref double pValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis의 ParameterNum 에 해당 하는 [ Bool ] Parameter 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : ParameterNum -> Parameter Number
        // Parameter(O) : pValue -> Parameter Value
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadBoolParameter(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint ParameterNum,
                                    ref bool pValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis의 ParameterNum 에 해당 하는 [ Int ] Parameter 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : ParameterNum -> Parameter Number
        // Parameter(O) : pValue -> Parameter Value
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadIntParameter(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint ParameterNum,
                                    ref uint pValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis의 ParameterNum 에 해당 하는 [ Double ] Parameter 값을 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : ParameterNum -> Parameter Number
        // Parameter(I) : dValue -> Parameter 설정 값
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_WriteParameter(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint ParameterNum,
                                    double dValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis의 ParameterNum 에 해당 하는 [ Bool ] Parameter 값을 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : ParameterNum -> Parameter Number
        // Parameter(I) : dValue -> Parameter 설정 값
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_WriteBoolParameter(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint ParameterNum,
                                    bool Value
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis의 ParameterNum 에 해당 하는 [ Int ] Parameter 값을 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : ParameterNum -> Parameter Number
        // Parameter(I) : dValue -> Parameter 설정 값
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_WriteIntParameter(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint ParameterNum,
                                    uint dwValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Axis의 Actual Position Value 를 읽음 (속도 개선 API)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(O) : pPosition -> Actual Position Value
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadActualPosition(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref double pPosition
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Digital Gear를 위한 Master, Slave 설정 및 속도 비율을 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : MasterAxisID -> Master Axis Number
        // Parameter(I) : SlaveAxisID -> Slave Axis Number
        // Parameter(I) : RatioNumerator -> Ratio Numerator
        // Parameter(I) : RatioDenominator -> Ratio Denominator
        // Parameter(I) : MasterValueSource -> Defines the source for synchronization (0: mcSetValue, 1: mcActualValue, 2: mcSetValueFixedGear, 3: mcActualValueFixedGear)
        // Parameter(I) : Acceleration -> Max Acceleration
        // Parameter(I) : Deceleration -> Max Deceleration
        // Parameter(I) : Jerk -> Max Jerk
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GearIn(
                                    ushort BoardID,
                                    ushort MasterAxisID,
                                    ushort SlaveAxisID,
                                    uint RatioNumerator,
                                    uint RatioDenominator,
                                    MC_SOURCE MasterValueSource,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    MC_BUFFER_MODE BufferMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Slave Axis를 Master Axis로 부터 분리 시킴 (Gearing 해제)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : SlaveAxisID -> 분리 시킬 Slave Axis Number
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GearOut(
                                    ushort BoardID,
                                    ushort SlaveAxisID
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Input에서 Trigger가 발생 시 Axis의 Actual Position을 기록 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : TriggerInput -> Trigger Signal Source
        // Parameter(I) : WindowOnly -> 설정 시 가능한 범위 내에서 위치를 기록
        // Parameter(I) : FirstPosition -> WindowOnly 설정을 위한 시작 위치
        // Parameter(I) : LastPosition -> WindowOnly 설정을 위한 마지막 위치
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_TouchProbe(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint TriggerInput,
                                    bool WindowOnly,
                                    double FirstPosition,
                                    double LastPosition
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Touch Probe 기능을 중지 시키기 위한 용도로 사용 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : TriggerInput -> Trigger Signal Source
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_AbortTrigger(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint TriggerInput
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 축에 Mapping 된 Digital Input 에 대한 각 비트별 상태 정보를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : InputNumber -> Input Number (Bit Unit)
        // Parameter(O) : pValue -> Bit State (true or false)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadDigitalInput(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint InputNumber,
                                    ref bool pValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 축에 Mapping 된 Digital Output 에 대한 각 비트별 상태 정보를 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : OutputNumber -> Output Number (Bit Unit)
        // Parameter(O) : pValue -> Bit State (true or false)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadDigitalOutput(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint OutputNumber,
                                    ref bool pValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 축에 Mapping 된 Digital Output 에 대한 비트 상태를 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : OutputNumber -> Output Number (Bit Unit)
        // Parameter(I) : Value -> Bit 설정 값 (true or false)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_WriteDigitalOutput(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint OutputNumber,
                                    bool Value
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 해당 축의 Digital Input을 Byte 단위로 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Offset -> 확인 할 Data의 바이트 오프셋 (Range : 0 ~ 3)
        // Parameter(I) : Size -> 확인 할 Data의 바이트 사이즈 (Range : 1 ~ 4)
        // Parameter(O) : InputValue -> 현재 확인 된 Digital Input Data
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadDigitalInputByte(
                                    ushort BoardID,
                                    ushort AxisID,
                                    byte Offset,
                                    byte Size,
                                    byte[] InputValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 해당 축의 Digital Output을 Byte 단위로 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Offset -> 확인할 Data의 바이트 오프셋 (Range : 0 ~ 3)
        // Parameter(I) : Size -> 확인할 Data의 바이트 사이즈 (Range : 1 ~ 4)
        // Parameter(O) : OutputValue -> 현재 확인된 Digital Output Data
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadDigitalOutputByte(
                                    ushort BoardID,
                                    ushort AxisID,
                                    byte Offset,
                                    byte Size,
                                    byte[] OutputValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 해당 축의 Digital Output을 Byte 단위로 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Offset -> 제어 할 Data의 바이트 오프셋 (Range : 0 ~ 3)
        // Parameter(I) : Size -> 제어 할 Data의 바이트 사이즈 (Range : 1 ~ 4)
        // Parameter(I) : OutputValue -> 제어 할 Digital Output Data
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_WriteDigitalOutputByte(
                                    ushort BoardID,
                                    ushort AxisID,
                                    byte Offset,
                                    byte Size,
                                    byte[] OutputValue
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis를 Motion에 영향을 주지 않고 사용자가 지정한 값으로 위치 좌표 값을 변경 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Position -> 변경 하고자 하는 Actual Position 값
        // Parameter(I) : Relative -> Actual Position 변경 방법 선택 (0:Postion, 1: Position + Actual Position)
        // Parameter(I) : Mode -> 실행 시점 선택 (0: mcImmediately-즉시실행, 1: mcQueued-이전동작완료후실행)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_SetPosition(
                                    ushort BoardID,
                                    ushort AxisID,
                                    double Position,
                                    bool Relative,
                                    MC_EXECUTION_MODE Mode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Axis의 Actual Velocity Value 를 읽음 (속도 개선 API)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(O) : pVelocity -> Velocity Actual Value
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadActualVelocity(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref double pVelocity
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정된 Axis의 Motion Stop에 사용 됨 (지정한 Deceleration 및 Jerk 값을 사용하여 감속 정지 시킴)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : Deceleration -> Max Deceleration
        // Parameter(I) : Jerk -> Max Jerk
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_Halt(
                                    ushort BoardID,
                                    ushort AxisID,
                                    double Deceleration,
                                    double Jerk,
                                    MC_BUFFER_MODE BufferMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Touch Probe의 Active 상태, Probe 상태 및 Trigger 발생 시 저장된 Actual Position 값의 확인 용도로 사용 됨
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : TriggerInput -> Trigger 신호로 사용되는 Input 의 번호 지정
        // Parameter(O) : pDone -> Trigger Event Recorded (Set:1-Trigger 발생, Reset:0-Position 기록 완료)
        // Parameter(O) : pRecordedPosition -> 기록된 Actual Position 값
        // Parameter(O) : pProbeActive -> Probe Active (Set:1-해당 Input의 Trigger 설정시, Reset:0-해당 Input에 MC_AbortTrigger 실행시)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_TriggerMonitor(
                                    ushort BoardID,
                                    ushort AxisID,
                                    uint TriggerInput,
                                    ref bool pDone,
                                    ref double pRecordedPosition,
                                    ref bool pProbeActive
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정된 Axis의 Motion State 를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(O) : pMotionState -> Motion State
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadMotionState(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref uint pMotionState
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Motion 관련 Input, Motion Mode 등과 같은 Axis 정보를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(O) : pAxisInfo -> Axis Info.
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadAxisInfo(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref uint pAxisInfo
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정된 Axis의 Status 를 확인 (속도 개선 API)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(O) : pAxisInfo -> Axis Status 상태 정보
        //----------------------------------------------------------------------------- 
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadAxisStatus(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref uint pAxisStatus
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Gearing 상태 및 Gearing 완료 상태를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Slave Axis Number
        // Parameter(O) : pStatus -> Status (Bit0: Active, Bit1: InGear)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GearMonitor(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref ushort pStatus
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Axis의 Motion Profile 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Slave Axis Number
        // Parameter(O) : TickCount -> TickCount (ms)
        // Parameter(O) : Position -> Position
        // Parameter(O) : Velocity -> Velocity
        // Parameter(O) : Accel -> Accel
        // Parameter(O) : Jerk -> Jerk
        // Parameter(O) : ActPos -> Actual Position
        // Parameter(O) : ActVel -> Actual Velocity
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadProfileData(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref uint TickCount,
                                    ref double Position,
                                    ref double Velocity,
                                    ref double Accel,
                                    ref double Jerk,
                                    ref double ActPos,
                                    ref double ActVel
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Axis의 Commanded Position 정보를 가져 옴 (속도 개선 API)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Slave Axis Number
        // Parameter(O) : pPosition -> Commmanded Position Value
        //-----------------------------------------------------------------------------
        //v12.1.0.45
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadCommandedPosition(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref double pPosition
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정 된 Axis의 Commanded Velocity 정보를 가져 옴 (속도 개선 API)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Slave Axis Number
        // Parameter(O) : pPosition -> Commmanded Velocity Value
        //-----------------------------------------------------------------------------
        //v12.1.0.46
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadCommandedVelocity(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ref double pVelocity
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Motion APIs - Coordinated Motion
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis를 AxesGroup 에서 설정한 Group 에 추가
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis Number
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 지정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        // Parameter(I) : IDInGroup -> AxesGroup 내에서 축을 구별하기 위한 식별자 (0:X축, 1:Y축, 2:Z축)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_AddAxisToGroup(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ushort AxesGroupNo,
                                    ushort IDInGroup
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 축을 속한 AxesGroup 에서 제외 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 지정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        // Parameter(I) : IDInGroup ->AxesGroup에서 제외하고자 하는 축의 IdentInGroup (0:X축, 1:Y축, 2:Z축)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_RemoveAxisFromGroup(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ushort IDInGroup
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 AxesGroup 에 속한 모든 축을 제거 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 지정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_UngroupAllAxes(
                                    ushort BoardID,
                                    ushort AxesGroupNo
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 AxesGroup의 IdentInGroup에 Mapping된 Axis 정보를 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 지정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        // Parameter(I) : IDInGroup -> AxisID를 읽고자 하는 축의 IdentInGroup (0:X축, 1:Y축, 2:Z축)
        // Parameter(I) : CoordSystem -> 좌표계 지정 (0: Axes Coordinate System, 1: Reserved, 2:Reserved)
        // Parameter(O) : AxisNo -> IdentInGroup에 해당하는 축의 AxisID 리턴
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReadConfiguration(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ushort IDInGroup,
                                    MC_CoordSystem CoordSystem,
                                    ref ushort AxisNo
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 AxesGroup의 State를 GroupStandby로 변경 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 지정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupEnable(
                                    ushort BoardID,
                                    ushort AxesGroupNo
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 AxesGroup의 State를 GroupDisabled 로 변경 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 지정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupDisable(
                                    ushort BoardID,
                                    ushort AxesGroupNo
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 AxesGroup의 축들을 동작시켜 특정한 궤도를 따라 직선운동 하여 목표 위치에 도달 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 설정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        // Parameter(I) : PositionCount -> 동작할 축의 개수 설정 (1~3)
        // Parameter(I) : PositionData -> 각 축의 목표 위치 설정
        // Parameter(I) : Velocity -> 최대 속도 설정
        // Parameter(I) : Acceleration -> 최대 가속도 설정
        // Parameter(I) : Deceleration -> 최대 감속도 설정
        // Parameter(I) : Jerk -> 최대 저크 설정
        // Parameter(I) : CoordSystem -> 좌표계 설정 (0: Axes Coordinate System, 1: Reserved, 2:Reserved)
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        // Parameter(I) : TransitionMode -> TransitionMode 설정 (0: No Transition Curve, 1: Reserved, 2: Reserved, 3: Reserved, 4: Reserved)
        // Parameter(I) : TransitionParameterCount -> TransitionParameter 의 Count 를 설정 (Reserved)
        // Parameter(I) : TransitionParameter -> TransitionParameterCount 에 명시된 크기 만큼의 Data 를 설정 (Reserved)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveLinearAbsolute(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ushort PositionCount,
                                    double[] PositionData,
                                    double Velocity,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    MC_CoordSystem CoordSystem,
                                    MC_BUFFER_MODE BufferMode,
                                    MC_TRANSITION_MODE TransitionMode,
                                    ushort TransitionParameterCount,
                                    double[] TransitionParameter
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 AxesGroup을 설정한 Motion Parameter들을 사용하여 감속 정지 시킴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 설정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        // Parameter(I) : Deceleration -> 최대 감속도 설정
        // Parameter(I) : Jerk -> 최대 저크 설정
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupHalt(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    double Deceleration,
                                    double Jerk,
                                    MC_BUFFER_MODE BufferMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 AxesGroup을 설정한 Motion Parameter들을 사용하여 감속정지 시키고 다른 모든 명령을 중단 시킴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 설정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        // Parameter(I) : Execute -> Execute Flag 설정 (true: 모션 정지/Stopping유지, false: Stopping 해제)
        // Parameter(I) : Deceleration -> 최대 감속도 설정
        // Parameter(I) : Jerk -> 최대 저크 설정
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupStop(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    bool Execute,
                                    double Deceleration,
                                    double Jerk
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 AxesGroup의 2축을 동기시켜 합성벡터(Vector Sum) 위치가 평면상의 원 혹은 원호의 궤적에 따라 목표 위치에 도달하게 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> AxesGroup 번호 설정 (8축보드:0~1, 16축보드:0~3, 32축보드:0~7, 64축보드:0~15)
        // Parameter(I) : CircMode -> Circular Mode 설정
        // Parameter(I) : PathChoice -> 회전 방향 설정
        // Parameter(I) : AuxPoint -> 각 축의 AuxPoint 설정
        // Parameter(I) : EndPoint -> 각 축의 EndPoint 설정
        // Parameter(I) : Angle -> 회전 각도 지정 (단위:Degree)
        // Parameter(I) : Velocity -> 최대 속도 설정
        // Parameter(I) : Acceleration -> 최대 가속도 설정
        // Parameter(I) : Deceleration -> 최대 감속도 설정
        // Parameter(I) : Jerk -> 최대 저크 설정
        // Parameter(I) : CordSystem -> 좌표계 설정 (0: Axes Coordinate System, 1: Reserved, 2:Reserved)
        // Parameter(I) : BufferMode -> 버퍼모드 설정 (0:Aborting, 1:Buffered, 2:BlendingLow, 3:BlendingPrevious, 4:BlendingNext, 5:BlendingHigh)
        // Parameter(I) : TransitionMode -> TransitionMode 설정 (0: No Transition Curve, 1: Reserved, 2: Reserved, 3: Reserved, 4: Reserved)
        // Parameter(I) : TransitionParamCount -> Reserved
        // Parameter(I) : TransitionParameter -> Reserved
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveCircularAbsolute2D(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    MC_CIRC_MODE CircMode,
                                    MC_CIRC_PATHCHOICE PathChoice,
                                    double[] AuxPoint,
                                    double[] EndPoint,
                                    double Angle,
                                    double Velocity,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    MC_CoordSystem CordSystem,
                                    MC_BUFFER_MODE BufferMode,
                                    MC_TRANSITION_MODE TransitionMode,
                                    ushort TransitionParamCount,
                                    double[] TransitionParameter
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 그룹이 반지름 변화가 있는 나선 모션을 수행 함 [원의 크기가 변경 됨]
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> 모션이 수행 될 Group ID
        // Parameter(I) : SpiralMode -> 나선 모션의 동작 모드 (3D operation mode)
        //				  * mc_NORMAL, mc_NORMAL_SA1, mc_NORMAL_SA2, mc_NORMAL_SA1_2, mc_SA1, mc_SA2, mc_SA1_2, mc_ZAXIS
        // Parameter(I) : CircMode -> 원의 중심을 결정하는 모드(보드 or 중심)
        //				  * find origin mode (mcBORDER, mcCENTER)
        // Parameter(I) : AuxPoint[2] -> 보드 방식에서 사용되는 보조지점 / 중심모드에서 원의 중심좌표
        //                * Auxillary Point(in mcBOADER), Center(origin) Point(in mcCENTER)
        // Parameter(I) : EndPoint[2] -> 보드 방식에서 사용되는 종료지점 / 중심모드에서는 무시됨
        //				  * End point(in mcBOADER), Don't care(in mcCENTER)
        // Parameter(I) : AngleTurn -> 수행 할 각도(중심모드) 또는 회전수(보드모드)
        //                * Turn(in mcBOADER) - …, -2, -1, 0, 1, 2 ,... / Angle(in mcCENTER) - degree
        // Parameter(I) : ZAxisEnable -> Z 축의 이동 여부를 결정함
        //				  * Z axis operation mode (Enable:1, Disable:0)
        // Parameter(I) : Displacement[5] -> 나선 모션을 수행 하는데 필요한 파라미터
        //				  * Displacement[0] - Z 축 최종 위치 (Z axis end position)
        //				  * Displacement[1] - SA1 축 최종 위치 (SA1 axis end position)
        //				  * Displacement[2] - SA2 축 최종 위치 (SA2 axis end position)
        //				  * Displacement[3] - 반지름 변화율 (end radius value(mcCENTER / unit : radious value) / total radius various(mcBORDER : unit : ratio))
        //				  * Displacement[4] - 모션의 속도 유닛 선택 (Velocity Unit(0 : Line, 1 : Angle))
        // Parameter(I) : Velocity -> 나선 모션의 속도 (mc_SA1, mc_SA2, mc_SA1_2 에서는 장축 기준으로 적용됨)
        // Parameter(I) : Acceleration -> 나선 모션의 가속도 (mc_SA1, mc_SA2, mc_SA1_2 에서는 장축 기준으로 적용됨)
        // Parameter(I) : Deceleration -> 나선 모션의 감속도 (mc_SA1, mc_SA2, mc_SA1_2 에서는 장축 기준으로 적용됨)
        // Parameter(I) : Jerk -> 나선 모션의 저크 (mc_SA1, mc_SA2, mc_SA1_2 에서는 장축 기준으로 적용됨)
        // Parameter(I) : CordSystem -> Reserved
        // Parameter(I) : BufferMode -> 나선 모션의 버퍼모드
        // Parameter(I) : TransitionMode -> 나선 모션의 트랜지션 모드 (블렌딩 옵션에서 적용됨)
        // Parameter(I) : TransitionParamCount -> 나선 모션의 트랜지션 파라미터 개수 (블렌딩 옵션에서 적용됨)
        // Parameter(I) : TransitionParameter -> 나선 모션의 트랜지션 파라미터 (블렌딩 옵션에서 적용됨)	
        //-----------------------------------------------------------------------------
        //v12.2.0.19
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveSpiralAbsolute(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    MC_3D_MODE SpiralMode,
                                    MC_CIRC_MODE CircMode,
                                    double[] AuxPoint,
                                    double[] EndPoint,
                                    double AngleTurn,
                                    byte ZAxisEnable,
                                    double[] Displacement,
                                    double Velocity,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    MC_CoordSystem CordSystem,
                                    MC_BUFFER_MODE BufferMode,
                                    MC_TRANSITION_MODE TransitionMode,
                                    ushort TransitionParamCount,
                                    double[] TransitionParameter
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 그룹이 반지름 변화가 없는 나선 모션을 수행 함 [원의 크기가 유지 됨]
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> 모션이 수행 될 Group ID
        // Parameter(I) : HelicalMode -> 나선 모션의 동작 모드 (3D operation mode)
        //				  * mc_NORMAL, mc_NORMAL_SA1, mc_NORMAL_SA2, mc_NORMAL_SA1_2, mc_SA1, mc_SA2, mc_SA1_2, mc_ZAXIS
        // Parameter(I) : CircMode -> 원의 중심을 결정하는 모드(보드 or 중심)
        //				  * find origin mode (mcBORDER, mcCENTER)
        // Parameter(I) : AuxPoint[2] -> 보드 방식에서 사용되는 보조지점 / 중심모드에서 원의 중심좌표
        //                * Auxillary Point(in mcBOADER), Center(origin) Point(in mcCENTER)
        // Parameter(I) : EndPoint[2] -> 보드 방식에서 사용되는 종료지점 / 중심모드에서는 무시됨
        //				  * End point(in mcBOADER), Don't care(in mcCENTER)
        // Parameter(I) : AngleTurn -> 수행 할 각도(중심모드) 또는 회전수(보드모드)
        //                * Turn(in mcBOADER) - …, -2, -1, 0, 1, 2 ,... / Angle(in mcCENTER) - degree
        // Parameter(I) : ZAxisEnable -> Z 축의 이동 여부를 결정함
        //				  * Z axis operation mode (Enable:1, Disable:0)
        // Parameter(I) : Displacement[5] -> 나선 모션을 수행 하는데 필요한 파라미터
        //				  * Displacement[0] - Z 축 최종 위치 (Z axis end position)
        //				  * Displacement[1] - SA1 축 최종 위치 (SA1 axis end position)
        //				  * Displacement[2] - SA2 축 최종 위치 (SA2 axis end position)
        //				  * Displacement[3] - 반지름 변화율 (end radius value(mcCENTER / unit : radious value) / total radius various(mcBORDER : unit : ratio))
        //				  * Displacement[4] - 모션의 속도 유닛 선택 (Velocity Unit(0 : Line, 1 : Angle))
        // Parameter(I) : Velocity -> 나선 모션의 속도 (mc_SA1, mc_SA2, mc_SA1_2 에서는 장축 기준으로 적용됨)
        // Parameter(I) : Acceleration -> 나선 모션의 가속도 (mc_SA1, mc_SA2, mc_SA1_2 에서는 장축 기준으로 적용됨)
        // Parameter(I) : Deceleration -> 나선 모션의 감속도 (mc_SA1, mc_SA2, mc_SA1_2 에서는 장축 기준으로 적용됨)
        // Parameter(I) : Jerk -> 나선 모션의 저크 (mc_SA1, mc_SA2, mc_SA1_2 에서는 장축 기준으로 적용됨)
        // Parameter(I) : CordSystem -> Reserved
        // Parameter(I) : BufferMode -> 나선 모션의 버퍼모드
        // Parameter(I) : TransitionMode -> 나선 모션의 트랜지션 모드 (블렌딩 옵션에서 적용됨)
        // Parameter(I) : TransitionParamCount -> 나선 모션의 트랜지션 파라미터 개수 (블렌딩 옵션에서 적용됨)
        // Parameter(I) : TransitionParameter -> 나선 모션의 트랜지션 파라미터 (블렌딩 옵션에서 적용됨)		
        //-----------------------------------------------------------------------------
        //v12.2.0.19
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveHelicalAbsolute(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    MC_3D_MODE HelicalMode,
                                    MC_CIRC_MODE CircMode,
                                    double[] AuxPoint,
                                    double[] EndPoint,
                                    double AngleTurn,
                                    byte ZAxisEnable,
                                    double[] Displacement,
                                    double Velocity,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    MC_CoordSystem CordSystem,
                                    MC_BUFFER_MODE BufferMode,
                                    MC_TRANSITION_MODE TransitionMode,
                                    ushort TransitionParamCount,
                                    double[] TransitionParameter
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 해당 Board에서 Group의 상태를 확인 함 (from MMCE Board)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> Axes Group No.설정
        // Parameter(O) : pGroupStatus -> Group Status 확인 
        //-----------------------------------------------------------------------------
        //v12.1.0.71
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadGroupStatus(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ref uint pGroupStatus
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 해당 Board에서 Group의 상태를 확인 함 (속도 개선 API) (from Window Shared Memory)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> Axes Group No.설정
        // Parameter(O) : pGroupStatus -> Group Status 확인 
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReadStatus(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ref uint pGroupStatus
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axes Group의 현재 Error 및 확장 Error 정보를 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> Axes Group No.설정
        // Parameter(O) : pErrorID -> Error ID 정보
        // Parameter(O) : pErrorInfo0 -> Error Info 0 정보
        // Parameter(O) : pErrorInfo1 -> Error Info 1 정보
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReadError(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ref ushort pErrorID,
                                    ref ushort pErrorInfo0,
                                    ref ushort pErrorInfo1
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axes Group의 State를 GroupErrorStop에서 GroupStandby로 변경하고 Group관련 Error를 초기화 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> Axes Group No.설정
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReset(
                                    ushort BoardID,
                                    ushort AxesGroupNo
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axes Group의 Profile Data 정보를 가져 옴
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> Axes Group No.설정
        // Parameter(O) : AxisCount -> Axes Group에 속한 Axis Count
        // Parameter(O) : TimeTick -> Time Tick (msec)
        // Parameter(O) : ProfileDataArray -> Profile Data [8그룹][6아이템] 
        //	* [0]Pos, Vel, Acc, Jerk, ActPos, ActVel
        //  * [1]Pos, Vel, Acc, Jerk, ActPos, ActVel ...
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReadProfileData(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ref ushort AxisCount,
                                    ref uint TimeTick,
                                    double[,] ProfileDataArray
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axes Group에 속한 축들의 번호 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> Axes Group No. 설정
        // Parameter(O) : LastIdentNum -> 그룹에 속한 축 중 가장 마지막 번호
        // Parameter(O) : IdentAxisNumList -> 그룹에 속한 축 목록
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReadInfo(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ref ushort LastIdentNum,
                                    ushort[] IdentAxisNumList
                                    );

        //-----------------------------------------------------------------------------
        // *** Special API *** (사용 못함)
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadAllStatus(
                                  ushort BoardID,
                                  uint[] StatusData
                                  );

        //-----------------------------------------------------------------------------
        // *** Special API ***
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupSetRawDataMode(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    MC_RAW_DATA_MODE Mode,
                                    bool Enable,
                                    byte ReservedZero1,
                                    byte ReservedZero2
                                    );

        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReadRawDataStatus(
                                  ushort BoardID,
                                  ushort AxesGroupNo,
                                  ref MC_AxesGroupRawDataStatus AxesGroupRawDataStatus
                                  );

        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupClearRawData(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ushort ReservedZero1,
                                    ushort ReservedZero2
                                    );

        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupSetRawData(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    MC_SAVE_MODE SaveMode,
                                    ushort IndexNum,
                                    ushort RawDataSize,
                                    ushort RawDataCount,
                                    byte[] RawDataArray,
                                    ref ushort StoredNum
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axis에 남아있는 Buffer 개수를 확인 (속도 개선 API)(Buffer 개수는 각 축당 1000개)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesNo -> Axis Number
        // Parameter(O) : BufferSize -> 현재 남아있는 버퍼의 개수
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadRemainBuffer(
                                    ushort BoardID,
                                    ushort AxesNo,
                                    ref uint BufferSize
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Axes Group 에 남아있는 Buffer 개수를 확인 (속도 개선 API)(Buffer 개수는 각 축당 1000개)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxesGroupNo -> Axes Group Number
        // Parameter(O) : BufferSize -> 현재 남아있는 버퍼의 개수
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReadRemainBuffer(
                                    ushort BoardID,
                                    ushort AxesGroupNo,
                                    ref uint BufferSize
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 그룹의 Parameter 에 값을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : GroupNum -> AxesGroup 번호 지정 (0~1:8축, 3:16축, 7:32축, 15:64축)
        // Parameter(I) : ParameterNum -> 쓰고자 하는 Parameter 번호
        // Parameter(I) : Value -> 해당 Parameter 에 쓰고자 하는 값 
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupWriteParameter(
                                    ushort BoardID,
                                    ushort GroupNum,
                                    uint ParameterNum,
                                    double Value
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 그룹의 Parameter 에 값을 읽음
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : GroupNum -> AxesGroup 번호 지정 (0~1:8축, 3:16축, 7:32축, 15:64축)
        // Parameter(I) : ParameterNum -> 읽고자 하는 Parameter 번호
        // Parameter(O) : pValue -> 해당 Parameter의 값 리턴
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GroupReadParameter(
                                    ushort BoardID,
                                    ushort GroupNum,
                                    uint ParameterNum,
                                    ref double pValue
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Motion APIs - Multi Motion
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : 지정한 축(1~64축)들을 설정한 Motion Parameter들을 사용하여 절대 위치로 이동
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisCount -> Multi Motion을 수행할 축의 개수 지정
        // Parameter(I) : AxisArray -> AxisID 지정 (AxisCount에서 설정한 개수만큼 배열로 지정)
        // Parameter(I) : PositionArray -> 목표 위치 지정
        // Parameter(I) : Velocity -> 최대 속도 지정
        // Parameter(I) : Acceleration -> 최대 가속도 지정
        // Parameter(I) : Deceleration -> 최대 감속도 지정
        // Parameter(I) : Jerk -> 최대 저크 지정
        // Parameter(I) : DirectionArray -> 이동 방향 지정 (Modulo Type일 경우 유효)
        // Parameter(I) : ErrorStopMode -> ErrorStop 처리 방법 설정
        // 	             (0: Error가 발생한 축만 모션 정지, 1: 멀티 모션중인 축 모두 정지)
        //-----------------------------------------------------------------------------
        //v12.1.0.40
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveAbsoluteMultiAxis(
                                    ushort BoardID,
                                    ushort AxisCount,
                                    ushort[] AxisArray,
                                    double[] PositionArray,
                                    double Velocity,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    MC_DIRECTION[] DirectionArray,
                                    byte ErrorStopMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 축(1~64축)들을 설정한 Motion Parameter들을 사용하여 상대 위치로 이동
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisCount -> Multi Motion을 수행할 축의 개수 지정
        // Parameter(I) : AxisArray -> AxisID 지정 (AxisCount에서 설정한 개수만큼 배열로 지정)
        // Parameter(I) : PositionArray -> 목표 위치 지정
        // Parameter(I) : Velocity -> 최대 속도 지정
        // Parameter(I) : Acceleration -> 최대 가속도 지정
        // Parameter(I) : Deceleration -> 최대 감속도 지정
        // Parameter(I) : Jerk -> 최대 저크 지정
        // Parameter(I) : ErrorStopMode -> ErrorStop 처리 방법 설정
        // 	             (0: Error가 발생한 축만 모션 정지, 1: 멀티 모션중인 축 모두 정지)
        //-----------------------------------------------------------------------------
        //v12.1.0.40
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MoveRelativeMultiAxis(
                                    ushort BoardID,
                                    ushort AxisCount,
                                    ushort[] AxisArray,
                                    double[] PositionArray,
                                    double Velocity,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    byte ErrorStopMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 축(1~64축)들에 설정된 Multi Motion Parameter들을 사용하여 감속정지 
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisCount -> Multi Motion 정지를 수행 할 축의 개수 지정
        // Parameter(I) : AxisArray -> AxisID 지정 (AxisCount에서 설정한 개수만큼 배열로 지정)
        // Parameter(I) : ErrorStopMode -> ErrorStop 처리 방법 설정
        // 	             (0: Error가 발생한 축만 모션 정지, 1: 멀티 모션중인 축 모두 정지)
        //-----------------------------------------------------------------------------
        //v12.1.0.42 
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_HaltMultiAxis(
                                    ushort BoardID,
                                    ushort AxisCount,
                                    ushort[] AxisArray,
                                    byte ErrorStopMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 축(1~64축)들의 Axis State를 확인하여 멀티모션 수행가능 여부를 판단
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisCount -> 읽을 축의 개수 지정
        // Parameter(I) : AxisArray -> Axis ID 지정 (AxisCount에서 설정한 개수만큼 배열로 지정)
        // Parameter(O) : Status -> Multi Axis Status 정보 리턴
        //-----------------------------------------------------------------------------
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadMultiAxisStatus(
                                    ushort BoardID,
                                    ushort AxisCount,
                                    ushort[] AxisArray,
                                    ref uint Status
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Motion APIs - Gantry Motion
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : Gantry 를 설정 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : GantryID -> Gantry 구성 할 ID (0 or 1)
        // Parameter(I) : MasterAxisID -> Gantry 로 구성 할 Master Axis ID
        // Parameter(I) : SlaveAxisID -> Gantry 로 구성 할 Slave Axis ID
        // Parameter(I) : VirtualRatio -> 구성 할 Gantry의 가상비율 (하기 참조)
        //  * Master/Slave Mode - Master(0), Virtual Axis Mode : 1 ~ 50
        // Parameter(I) : ErrorRange ->	Yaw Error 판정 기준 (하기 참조)
        //	* 0 : Yaw Control Disable (모션중 Gantry로 구성한 두 축의 틀어짐이 발생해도 제어 하지 않음)
        //	* 1 ~ : Yaw Control Enable (Gantry로 구성한 두 축의 틀어짐이 설정 값 초과시 Error Stop)
        //-----------------------------------------------------------------------------
        //Gantry
        //v12.2.0.0
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GantryEnable(
                                    ushort BoardID,
                                    ushort GantryID,
                                    ushort MasterAxisID,
                                    ushort SlaveAxisID,
                                    double VirtualRatio,
                                    double ErrorRange
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Gantry 를 해제 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : GantryID -> Gantry 구성 할 ID (0 or 1)
        //-----------------------------------------------------------------------------
        //v12.2.0.0
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GantryDisable(
                                    ushort BoardID,
                                    ushort GantryID
                                    );

        //-----------------------------------------------------------------------------
        // Summary : MMCE 보드에 설정 된 Gantry 구성을 확인 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(O) : MasterAxisID -> 설정 된 Gantry의 Master Axis ID Array (array[2], array[3] Reserved)
        // Parameter(O) : SlaveAxisID -> 설정 된 Gantry의 Slave Axis ID Array (array[2], array[3] Reserved)
        // Parameter(O) : VirtualRatio -> 설정 된 Gantry의 가상축 비율 Array (array[2], array[3] Reserved)
        //  * (Master/Slave Mode - Master : 0, Virtual Axis Mode : 1 ~ 50)
        // Parameter(O) : ErrorRange -> 설정 된 Gantry의 Yaw Error 기준 Array (array[2], array[3] Reserved)
        //  * (Yaw Error Range - Yaw Control Disable : 0, Yaw Control Enable : 1 ~ )
        //-----------------------------------------------------------------------------
        //v12.2.0.0
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GantryReadConfiguration(
                                    ushort BoardID,
                                    ushort[] MasterAxisID,
                                    ushort[] SlaveAxisID,
                                    double[] VirtualRatio,
                                    double[] ErrorRange
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Gantry 의 상태를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : GantryID -> Gantry ID (0 or 1)
        // Parameter(O) : Status -> Gantry 의 상태 정보
        //-----------------------------------------------------------------------------
        //v12.2.0.0
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GantryReadStatus(
                                    ushort BoardID,
                                    ushort GantryID,
                                    ref uint Status
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 구성된 Gantry 의 원점을 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : MasterAxisID -> Gantry로 구성 할 Master Axis ID
        // Parameter(I) : HomeMethod -> 수행 할 원점 설정 방식	
        // Parameter(I) : HomingVel -> 원점 설정시 수행 속도	
        // Parameter(I) : HomingAcc -> 원점 설정시 수행 가속도	
        // Parameter(I) : HomingDec -> 원점 설정시 수행 감속도 		
        // Parameter(I) : HomingJerk -> 원점 설정시 수행 저크
        // Parameter(I) : HomingCreepVel -> 원점 설정시 정밀 수행 속도
        // Parameter(I) : HomingDirection -> 원점 설정시 수행 시작 방향 (1:CW, 0:CCW)
        // Parameter(I) : AlignOffset -> Align 조정 방식에서 Slave 축의 이동량 (Direct & Align 메소드에서만 유효)
        //-----------------------------------------------------------------------------
        //v12.2.0.0
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_GantryHome(
                                    ushort BoardID,
                                    ushort MasterAxisID,
                                    ushort HomeMethod,
                                    double HomingVel,
                                    double HomingAcc,
                                    double HomingDec,
                                    double HomingJerk,
                                    double HomingCreepVel,
                                    byte HomingDirection,
                                    double AlignOffset
                                    );

        //-----------------------------------------------------------------------------
        // Summary : Gantry 의 Align(Yaw)을 변경 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : GantrySlaveAxisID -> Align을 변경 할 Gantry 구성의 Slave Axis ID (Align 모션은 Slave Axis ID만 유효)
        // Parameter(I) : AlignPosition -> 이동 할 Align 거리
        // Parameter(I) : Velocity -> Align 변경 시 수행 속도	
        // Parameter(I) : Acceleration -> Align 변경 시 수행 가속도	
        // Parameter(I) : Deceleration -> Align 변경 시 수행 감속도
        // Parameter(I) : Jerk -> Align 변경 시 수행 저크
        // Parameter(I) : DisplayMode -> Slave Axis Position Data 표기 방식 (하기참고)
        //  * 0 : Yaw Error Display (Master Axis - Slave Axis)
        //  * 1 : Yaw Diff. Display (Virtual Axis - Slave Axis)
        //  * 2 : Yaw Error Display (Slave Axis - Master Axis)
        //  * 3 : Yaw Diff. Display (Slave Axis - Virtual Axis)
        //-----------------------------------------------------------------------------
        //v12.2.0.2
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ChangeGantryAlign(
                                    ushort BoardID,
                                    ushort GantrySlaveAxisID,
                                    double AlignPosition,
                                    double Velocity,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    byte DisplayMode
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Motion APIs - Direct Output
        //
        //
        //
        //
        //=============================================================================        
        //-----------------------------------------------------------------------------
        // Summary : PDO 출력 값을 직접 제어 함
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> PDO value 를 제어 할 AxisID
        // Parameter(I) : OutputValue -> 출력 할 PDO Output Value (하기참고)
        //	* CSP : Target Position (단위: 위치)
        //	* CSV : Target Velocity (단위: 속도)
        //	* CST : Target Torque   (단위: 0.1%)
        //          (10. 0 > 1.0% / 15.0 > 1.5%)
        //-----------------------------------------------------------------------------
        //Pdo Output
        //v12.2.0.0
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_DirectOutput(
                                    ushort BoardID,
                                    ushort AxisID,
                                    double OutputValue
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Motion APIs - Error Compensation
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : MMCE의 Error Compensation(Error Mapping) 기능을 설정 제어
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> EC 기능을 적용 한 AxisID (AxisID:1~65535, Gantry:0~1, Group:0~15, 2D/3D_Combination AxisID:1~65535) 
        // Parameter(I) : Type -> ID의 Type 을 지정 (Axis:0, Gantry:1, Group:2, 2D_Combination:3, 3D_Combination:4)
        // Parameter(I) : Mode -> EC 기능을 활성화/비활성화 시킴 (EC Data Mode Enable:1, EC Data Mode Disable:0)
        // Parameter(I) : ECMapID -> EC Data 가 저장된 MapID (Range:0~3) (Table1 ~ Table4)		
        //-----------------------------------------------------------------------------
        //Error Compensation
        //v12.2.0.0/v12.2.0.13/v12.2.0.15
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ChangeErrorCompensationMode(
                                    ushort BoardID,
                                    ushort[] AxisID,
                                    byte Type,
                                    ushort Mode,
                                    byte ECMapID
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 Error Compensation Map(Table)의 상태를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : ECMapID -> 상태를 확인 할 ECMapID (Range:0~3) (Table1 ~ Table4)		
        // Parameter(O) : AxisID -> EC Map 에 할당 된 AxisID (AxisID:1~65535, Gantry:0~1, Group:0~15, Combination AxisID:1~65535)
        // Parameter(O) : Type -> EC Map의 차원 정보 (Axis:0, Gantry:1, Group-2D:2, Group-3D:3, 2D_Combination:4, 3D_Combination:5)
        // Parameter(O) : Status -> EC Map의 상태 정보 (Disabled:0, Enabled:1, Disabling:2, Enabling:3)
        //-----------------------------------------------------------------------------
        //v12.2.0.0/v12.2.0.13
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ReadErrorCompensationStatus(
                                    ushort BoardID,
                                    byte ECMapID,
                                    ushort[] AxisID,
                                    ref byte Type,
                                    ref byte Status
                                    );


        //-----------------------------------------------------------------------------
        // Summary : Error Compensation Map 정보를 보드에 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : ECType -> EC Map의 차원 지정 (0: 1D Axis or Gantry, 1: 2D Group, 2: 3D Group)
        // Parameter(I) : ECMapID -> Data를 저장 할 EC Map ID (Range:0~3) (Table1 ~ Table4)		
        // Parameter(I) : sFullPathFileName -> Error Compensation 데이터 파일 경로 (확장자: .ecf)
        //-----------------------------------------------------------------------------
        //v12.2.0.0
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_WriteErrorCompensationDataFile(
                                    ushort BoardID,
                                    ushort ECType,
                                    byte ECMapID,
                                    StringBuilder sFullPathFileName
                                    );

        //=============================================================================
        //
        //
        //
        //
        // Motion APIs - etc. Application
        //
        //
        //
        //
        //=============================================================================
        //-----------------------------------------------------------------------------
        // Summary : 특정 위치를 기준으로 Slave(서보)의 최대 출력 토크 값을 변경 하는 이벤트 기능
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> 해당 기능을 적용 할 Axis ID
        // Parameter(I) : EventPosition -> 최대 출력 토크 값이 변경되는 기준위치
        // Parameter(I) : Width -> 방향에 따라서 변경되는 기준범위 
        // Parameter(I) : NormalTorque -> 노멀 상태시 최대 출력 값
        // Parameter(I) : EventTorque -> 기준 위치 이후에 적용되는 최대 출력값 
        //-----------------------------------------------------------------------------
        //v12.2.0.3
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_ChangeTorqueLimitPositionEvent(
                                    ushort BoardID,
                                    ushort AxisID,
                                    double EventPosition,
                                    double Width,
                                    ushort NormalTorque,
                                    ushort EventTorque
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 지정한 축이 특정 위치를 지날 때, 정의한 단축 모션이 수행 되는 이벤트 기능
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> 조건 충족시 모션을 수행 할 AxisID	
        // Parameter(I) : RefAxisID -> 조건 충족을 확인 할 AxisID
        // Parameter(I) : EventPosition -> 이벤트 감지 위치
        // Parameter(I) : EventEdge -> 이벤트 감지 방식 (0: Rising, 1: Falling)	
        // Parameter(I) : EventMotion -> 이벤트 모션 종류	(0: Absolute, 1: Relative)
        // Parameter(I) : Position -> 이벤트 모션 수행 위치
        // Parameter(I) : Velocity -> 이벤트 모션 수행 속도
        // Parameter(I) : Acceleration -> 이벤트 모션 수행 가속도
        // Parameter(I) : Deceleration -> 이벤트 모션 수행 감속도
        // Parameter(I) : Jerk -> 이벤트 모션 수행 저크
        // Parameter(I) : BufferMode -> 미지원 (Reserved Aborting)
        //-----------------------------------------------------------------------------
        //v12.2.0.4
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_MovePositionEvent(
                                    ushort BoardID,
                                    ushort AxisID,
                                    ushort RefAxisID,
                                    double EventPosition,
                                    byte EventEdge,
                                    byte EventMotion,
                                    double Position,
                                    double Velocity,
                                    double Acceleration,
                                    double Deceleration,
                                    double Jerk,
                                    MC_BUFFER_MODE BufferMode
                                    );

        //-----------------------------------------------------------------------------
        // Summary : 기준 축의 일정범위(시작위치 ~ 목표위치)에서 지정축을 모션에 동기화(현재위치 기준)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> 동기화 시킬 AxisID	
        // Parameter(I) : AxisEndPosition -> RefEndPosition 에서 동기화 축의 목표 위치
        // Parameter(I) : RefAxisID -> 기준 축 ID
        // Parameter(I) : RefStartPosition -> 기준 축의 동기화 시작 위치
        // Parameter(I) : RefEndPosition -> 기준 축의 동기화 종료 위치
        // Parameter(I) : BufferMode -> 모션 버퍼 모드 (0: Aborting, 1: Buffered 옵션만 지원)
        //-----------------------------------------------------------------------------
        //v12.2.0.13
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_SyncAxisToRefAxis(
                                   ushort BoardID,
                                   ushort AxisID,
                                   double AxisEndPosition,
                                   ushort RefAxisID,
                                   double RefStartPosition,
                                   double RefEndPosition,
                                   MC_BUFFER_MODE BufferMode
                                   );

        //-----------------------------------------------------------------------------
        // Summary : 기준 축의 일정범위(시작위치 ~ 목표위치)에서 지정 축을 모션에 동기화(시작위치 ~ 목표위치) 
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> 동기화 시킬 축 ID		
        // Parameter(I) : AxisStartPosition -> RefStartPosition 에서 동기화 축의 시작 위치
        // Parameter(I) : AxisEndPosition -> RefEndPosition 에서 동기화 축의 목표 위치
        // Parameter(I) : RefAxisID -> 기준 축 ID
        // Parameter(I) : RefStartPosition -> 기준 축의 동기화 시작 위치
        // Parameter(I) : RefEndPosition -> 기준 축의 동기화 종료 위치
        // Parameter(I) : BufferMode -> 모션 버퍼 모드 (0: Aborting, 1: Buffered 옵션만 지원)
        //-----------------------------------------------------------------------------
        //v12.2.0.13
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_RTAFPositionSync(
                                   ushort BoardID,
                                   ushort AxisID,
                                   double AxisStartPosition,
                                   double AxisEndPosition,
                                   ushort RefAxisID,
                                   double RefStartPosition,
                                   double RefEndPosition,
                                   MC_BUFFER_MODE BufferMode
                                   );

        //-----------------------------------------------------------------------------
        // Summary : MC_RTAFPositionSync() 의 수행 카운트를 확인
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : AxisID -> Axis ID
        // Parameter(I) : IndexInit -> Index 초기화 및 제어 (0: Read, 1: Clear, 2: Dec, 3~9: Reserved)
        // Parameter(O) : RTAFIndex -> MC_RTAFPositionSync 의 수행 횟수 (0~65535)
        //-----------------------------------------------------------------------------
        //v12.2.0.13
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_RTAFListIndex(
                                   ushort BoardID,
                                   ushort AxisID,
                                   byte IndexInit,
                                   ref ushort RTAFIndex
                                   );

        //=============================================================================
        //
        //
        //
        //
        // Function Module APIs - Special
        //
        //
        //
        //
        //=============================================================================      
        //-----------------------------------------------------------------------------
        // Summary : RSA Function Module 의 Interval Trigger 파라미터 들을 내부 SDO API로 설정
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Ethercat Address
        // Parameter(I) : AxisID ->	AxisID	  
        // Parameter(I) : StartPosition -> Start Position
        // Parameter(I) : EndPosition -> End Position		
        // Parameter(I) : IntervalPeriod -> Interval Period
        // Parameter(I) : IntervalPeriod -> Pulse Width
        //-----------------------------------------------------------------------------
        //v12.1.0.48
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_WriteIntervalTrigParameterFM(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort AxisID,
                                    double StartPosition,
                                    double EndPosition,
                                    ushort IntervalPeriod,
                                    ushort PulseWidth
                                    );

        //-----------------------------------------------------------------------------
        // Summary : RSA Function Module 의 Interval Trigger 를 Enable or Disable 함 (내부 SDO API로 설정)
        // Parameter(I) : BoardID -> MMCE 보드 Switch 번호 (0~9)
        // Parameter(I) : EcatAddr -> Ethercat Address
        // Parameter(I) : AxisID ->	AxisID	  
        // Parameter(I) : Enable -> Configuration Enable or Disable (true or false)
        //-----------------------------------------------------------------------------
        //v12.1.0.48
        [DllImport(NMC_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern MC_STATUS MC_WriteIntervalTrigEnableFM(
                                    ushort BoardID,
                                    ushort EcatAddr,
                                    ushort AxisID,
                                    bool Enable
                                    );
        #endregion
    }
}
