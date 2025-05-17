using System.Text;
using System;

namespace YJ_AutoClamp.Utils
{
    public class utilProtocolVision
    {
        public enum CMD
        {
            ACK,
            NAK,
            Unknown
        };
        public static byte[] makeVisionSendData(CMD cmd, Models.Channel_Model modeldata = null, byte[] binData = null)
        {
            byte[] retBuff = null;
            string strCmd = string.Empty;
            int totalSize = 0;

            switch (cmd)
            {
                default:
                    {
                        strCmd = string.Format($"{cmd.ToString()}");
                        break;
                    }
            }

            // 알수 없는 명령이 아니면
            if (cmd != CMD.Unknown)
            {
                // Buff Size Set
                if (binData != null)
                    totalSize = strCmd.Length + binData.Length + 8 + 1;
                else
                    totalSize = strCmd.Length + 8 + 1;

                byte[] byteCmd = Encoding.ASCII.GetBytes(strCmd);
                retBuff = new byte[totalSize];

                // Size Check Sum
                int checksum = 0;
                for (int i = 0; i < byteCmd.Length; i++)
                    checksum += byteCmd[i];

                if (binData != null)
                {
                    for (int i = 0; i < binData.Length; i++)
                        checksum += binData[i];
                }

                // Make Header
                Buffer.BlockCopy(BitConverter.GetBytes(totalSize), 0, retBuff, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, retBuff, 4, 4);
                // Make Cmd
                int pos = 8;
                Buffer.BlockCopy(byteCmd, 0, retBuff, pos, byteCmd.Length);
                // 추가 데이터가 있을경우
                if (binData != null)
                {
                    pos += byteCmd.Length;
                    Buffer.BlockCopy(binData, 0, retBuff, pos, binData.Length);
                }
                // End Null
                retBuff[retBuff.Length - 1] = 0x00;
            }

            return retBuff;
        }
        public static CMD parseVisionReceiveData(string cmd, int visionindex)
        {
            CMD retVal = CMD.Unknown;

            switch (cmd)
            {
                case "ACK":
                    retVal = CMD.ACK;
                    break;
                case "NAK":
                    retVal = CMD.NAK;
                    break;
            }

            return retVal;
        }
    }
}
