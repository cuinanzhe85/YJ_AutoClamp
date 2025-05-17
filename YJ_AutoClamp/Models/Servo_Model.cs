using System.Collections.Generic;

namespace YJ_AutoClamp.Models
{
    public class Servo_Model
    {
        // 10000 / (지름 * Pi) * 감속기 : 파슨택
        // 파라소닉 기어비 필요
        // Scale * (속도 / 기어비) - Move, Scale * (속도 * 기어비) - 정지 : 파라소닉
        // 기어비 : 리드 / 83886080
        public double[] ServoScales { get; set; } = new double[]
        {
            0,            // EtherCat Master
            20000/20,     // In_Handler_Y
            20000/20,     // Out_Handler_X
            20000/20,     // Out_Handler_Z
            20000/20,     // Lift_Z_1
            20000/20,     // Lift_Z_2
            20000/20,     // Lift_Z_3
        };
        
        public ServoSlave_List ServoName { get; set; }
        public int SlaveID { get; set; }
        public bool IsEzConnected { get; set; } = false;                        // EZ Motion E Model 연결여부.
        public bool IsServoOn { get; set; } = false;
        public double ServoScale { get; set; }
        public double Velocity { get; set; }
        public double Measurement_Vel { get; set; }
        public double Barcode_Vel { get; set; }
        public double Accelerate { get; set; }
        public double Decelerate { get; set; }
        public double MaxSpeed { get; set; }
        public List<double> JogVelocity { get; set; } // Jog 속도. Low, Middle, High
        public Servo_Model(ServoSlave_List iSlaveNo)
        {
            this.ServoName = iSlaveNo;
            this.SlaveID = (int)iSlaveNo;
            this.ServoScale = ServoScales[SlaveID];
            JogVelocity = new List<double> { 0.0, 0.0, 0.0 }; // 초기값 설정
        }
    }
}
