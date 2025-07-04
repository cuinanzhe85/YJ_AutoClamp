using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace YJ_AutoClamp.Models
{
    public class Initialize_Model
    {
        private EziDio_Model Dio = SingletonManager.instance.Dio;
        private EzMotion_Model_E Ez_Model = SingletonManager.instance.Motion;
        public Initialize_Model() { }

        public async Task<bool> ServoInitY()
        {
            if (Ez_Model.IsOutHandlerReadyDoneZ() == false)
            {
                Global.instance.ShowMessagebox("Y initialization failed.\r\nPleass Move the Z axis to Ready position.\r\n(로딩 Z축 대기위치 이동 후 다시 진행 해주세요.)");
                return false; // 실패 시 false 반환
            }
            Global.Mlog.Info("Initialize : Loading Y Init Start");
            Ez_Model.ServoMovePause((int)ServoSlave_List.Out_Z_Handler_Z, 0);
            Ez_Model.ServoStop((int)ServoSlave_List.Out_Z_Handler_Z);
            if (Ez_Model.MoveOutHandlerReadyY() == false)
                return false; // 실패 시 false 반환
            bool result = false;
            await Task.Run(async () =>
            {
                Stopwatch sw = new Stopwatch();
                while (true)
                {
                    if (Ez_Model.IsMoveOutHandlerReadyY() == true)
                    {
                        result = true;
                        break; // 성공 시 루프 종료
                    }
                    if (sw.ElapsedMilliseconds > 10000)
                    {
                        result = false; // 10초 후에 중단
                        break; // 10초 후에 중단
                    }
                    await Task.Delay(100);
                }
            });
            // Step 초기화 설정
            SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
            return result; // 성공 여부 반환
        }
        public async Task<bool> ServoInitZ()
        {
            Ez_Model.ServoMovePause((int)ServoSlave_List.Out_Z_Handler_Z, 0);
            Ez_Model.ServoStop((int)ServoSlave_List.Out_Z_Handler_Z);
            if (Ez_Model.MoveOutHandlerRadyZ() == false)
                return false; // 실패 시 false 반환
            Global.Mlog.Info("Initialize : Loading Z Init Start");
            bool result = false;
            await Task.Run(async() =>
            {
                Stopwatch sw = new Stopwatch();
                while (true)
                {
                    if (Ez_Model.IsOutHandlerReadyDoneZ() == true)
                    {
                        result = true;
                        break; // 성공 시 루프 종료
                    }
                    if (sw.ElapsedMilliseconds > 10000)
                    {
                        result = false; // 10초 후에 중단
                        break; // 10초 후에 중단
                    }
                     await Task.Delay(100);
                }
            });
            return result; // 성공 여부 반환
        }
        public async Task<bool> ServoInitX()
        {
            Ez_Model.ServoMovePause((int)ServoSlave_List.Out_Z_Handler_Z, 0);
            Ez_Model.ServoStop((int)ServoSlave_List.Out_Z_Handler_Z);
            if (Ez_Model.IsOutHandlerPickupPosY_2() == true || Ez_Model.IsOutHandlerPickupPosY_2() == true)
            {
                Global.instance.ShowMessagebox("X initialization failed. Move the Y axis to Ready position.\r\n(로딩 Y 대기위치로 이동후 다시 진행하세요.)");
                return false; // 실패 시 false 반환
            }
            Global.Mlog.Info("Initialize : Top X Init Start");
            bool result = false;
            await Task.Run(async() =>
            {

                Stopwatch sw = new Stopwatch();

                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_1, false);
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_DOWN_SOL_2, false);
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TOP_JIG_TR_Z_GRIP_SOL, false);
                sw.Restart();
                while (true)
                {
                    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_1] == true
                        && Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_TR_Z_UP_CYL_SS_2] == true
                        && Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TOP_JIG_RT_Z_UNGRIP_CYL_SS] == true)
                    {
                        result = true;
                        break; // 핸들러가 Down 상태로 이동했으면 루프 종료
                    }
                    if (sw.ElapsedMilliseconds > 3000)
                    {
                        Global.instance.ShowMessagebox("Top Handler Up failed.");
                        result = false;
                        break; // 3초 후에 중단
                    }
                }
                if (result == true)
                {
                    if (Ez_Model.MoveTopHandlerPickUpPos() == false)
                    {
                        result = false; // 실패 시 false 반환
                    }
                    sw.Restart();
                    while (true)
                    {
                        if (Ez_Model.IsTopHandlerPickUpPos() == true)
                        {
                            result = true;
                            break; // 성공 시 루프 종료
                        }
                        if (sw.ElapsedMilliseconds > 5000)
                        {
                            result = false; // 10초 후에 중단
                            break; // 10초 후에 중단
                        }
                        await Task.Delay(100);
                    }
                }
            });
            // Ready Position까지 이동했으면 
            SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Top_Handle_Step = Unit_Model.TopHandle.Idle;
            SingletonManager.instance.IsY_PickupColl = false;
            SingletonManager.instance.Unit_Model[(int)MotionUnit_List.In_CV].TopCvStep = Unit_Model.Top_CV.Idle;

            return result; // 성공 여부 반환
        }
        public async Task<bool> BottomHandlerInit()
        {
            bool result = false;
            Global.Mlog.Info("Initialize : Bottom Handler Init Start");
            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_VACUUM_SOL, false);
            await Task.Run(() =>
            {
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_DOWN_SOL, false);
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_LZ_DOWN_SOL, false);
                Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_RZ_GRIP_SOL, false);
                Stopwatch sw = new Stopwatch();
                sw.Restart();

                while (true)
                {
                    if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_LZ_UP_CYL_SS] == true
                        && Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_RZ_UP_CYL_SS] == true)
                    {
                        result = true;
                        break; // 핸들러가 Up 상태로 이동했으면 루프 종료
                    }
                    if (sw.ElapsedMilliseconds > 3000)
                    {
                        result = false;
                        Global.instance.ShowMessagebox("Bottom Handler Up failed.");
                    }
                    Task.Delay(100).Wait(); // 100ms 대기
                }
                if (result == true)
                {
                    // Bottom Handler Left Move
                    Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.TRANSFER_FORWARD_SOL, true);
                    sw.Restart();
                    while (true)
                    {
                        if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.TRANSFER_X_LEFT_CYL_SS] == true)
                        {
                            result = true;
                            break; // 핸들러가 Up 상태로 이동했으면 루프 종료
                        }
                        if (sw.ElapsedMilliseconds > 3000)
                        {
                            result = false;
                            Global.instance.ShowMessagebox("Bottom Handler Left move failed.");
                            break;
                        }
                        Task.Delay(100).Wait(); // 100ms 대기
                    }
                }
            });
            Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.CLAMPING_CV_RUN, false);
            SingletonManager.instance.BottomClampDone = false;
            SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Top_X].Bottom_Step = Unit_Model.BottomHandle.Idle;
            SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_CV].Out_Cv_Step = Unit_Model.OutCvSequence.Idle;
            return result; // 성공 여부 반환
        }
        public async Task<bool> LiftInit()
        {
            if (Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_1_1] == true
                || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_2_1] == true
                || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_UPPER_RUN_3_1] == true
                || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_1_1] == true
                || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_2_1] == true
                || Dio.DO_RAW_DATA[(int)EziDio_Model.DO_MAP.AGING_INVERT_CV_LOW_RUN_3_1] == true
                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_UPPER_INTERFACE_1] == true
                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_UPPER_INTERFACE_2] == true
                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_UPPER_INTERFACE_3] == true
                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_LOW_INTERFACE_1] == true
                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_LOW_INTERFACE_2] == true
                || Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.AGING_CV_LOW_INTERFACE_3] == true)
            {
                Global.instance.ShowMessagebox("Aging Conveyor Logic is Running\r\n(에이징 컨베어 로직 동작 중입니다. 동작 완료 후 다시 진행해주세요)");
                return false;
            }
            SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_CV_UPPER_INTERFACE_1, false);
            SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_CV_LOW_INTERFACE_1, false);
            SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_CV_UPPER_INTERFACE_2, false);
            SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_CV_LOW_INTERFACE_2, false);
            SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_CV_UPPER_INTERFACE_3, false);
            SingletonManager.instance.Dio.SetIO_OutputData((int)EziDio_Model.DO_MAP.AGING_CV_LOW_INTERFACE_3, false);
            Global.Mlog.Info("Initialize : Lift Init Start");
            bool result = true;
            if (Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.LIFT_1_CV_DETECT_IN_SS_1] == false
                && Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.LIFT_2_CV_DETECT_IN_SS_1] == false
                && Dio.DI_RAW_DATA[(int)EziDio_Model.DI_MAP.LIFT_3_CV_DETECT_IN_SS_1] == false)
            {
                SingletonManager.instance.LoadStageNo = 0;
                SingletonManager.instance.AgingCvIndex = 0;
                SingletonManager.instance.LoadAgingCvIndex = 0;
                for (int index =0; index < 3; index++)
                {
                    //SingletonManager.instance.LoadComplete[index] = false;
                    SingletonManager.instance.LoadFloor[index] = 0;
                    for (int i = 0; i < (int)Floor_Index.Max; i++)
                        SingletonManager.instance.Display_Lift[index].Floor[i] = false;

                }
                await Task.Run(async () =>
                {
                    Ez_Model.MoveLiftLoding(0);
                    Thread.Sleep(200);
                    Ez_Model.MoveLiftLoding(1);
                    Thread.Sleep(200);
                    Ez_Model.MoveLiftLoding(2);
                    Thread.Sleep(200);
                    Stopwatch sw = new Stopwatch();
                    while (true)
                    {
                        if (Ez_Model.IsMoveLiftLodingDone(0) == true && Ez_Model.IsMoveLiftLodingDone(1) == true && Ez_Model.IsMoveLiftLodingDone(2) == true)
                        {
                            result = true;
                            break; // 성공 시 루프 종료
                        }
                        if (sw.ElapsedMilliseconds > 20000)
                        {
                            result = false; // 10초 후에 중단
                            break; // 10초 후에 중단
                        }
                        await Task.Delay(100);
                    }
                });
                if (result == true)
                {
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Out_Y].Out_Handle_Step = Unit_Model.OutHandle.Idle;
                    SingletonManager.instance.Unit_Model[(int)MotionUnit_List.Lift_1].AgingCVStep = Unit_Model.Aging_CV_Step.Idle;
                }
            }
            return result;
        }
    }
}
