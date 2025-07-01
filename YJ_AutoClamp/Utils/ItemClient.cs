using Common.Managers;
using System;
using System.Net.Sockets;
using System.Text;

namespace YJ_AutoClamp.Utils
{
    public class ItemClient
    {
        /// <summary>
        /// 통신 프로토콜에서 사용하는 명령 종류
        /// </summary>
        public enum CMD
        {
            DATA,
            Max
        }

        public string mIdString;            // 식별문자열 (IP주소를 주로 사용)

        /// <summary>
        /// TCP 연결의 통신 스트림을 관리
        /// </summary>
        private NetworkStream mStream;
        public NetworkStream stream
        {
            set
            {
                mStream = value;
            }
        }
        /// <summary>
        /// 현재 Item의 통신을 닫음
        /// </summary>
        public void Close()
        {
            try
            {
                mIdString = "";
                if (mStream != null)
                {
                    mStream.Close();
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Error($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {e}");
            }
        }

        /// <summary>
        /// Client Item 생성자
        /// </summary>
        /// <param name="strId">IP주소를 주로 사용</param>
        public ItemClient(string strId = "")
        {
            mIdString = strId;
        }

        /// <summary>
        /// 통신 스트림을 통하여 메시지를 전송하는 함수
        /// </summary>
        /// <param name="sendString"></param>
        public void SendStream(string messageBody)
        {
            if (mStream != null)
            {
                // STX(0x02), ETX(0x03) 자동 추가
                string sendString = $"{(char)0x02}{messageBody}{(char)0x03}";
                byte[] sendBuff = Encoding.UTF8.GetBytes(sendString);
                mStream.Write(sendBuff, 0, sendBuff.Length);

                // 전송 데이터를 디버깅 로그에 저장
                Global.Mlog.Info($"[{mIdString}-S]  {sendString.Replace('\n', ' ').Trim()}");
            }
        }

        //------------------------------------------------
        /// <summary>
        /// Client로부터 받은 메시지를 처리하는 함수 ** 젤로 중요 **
        /// </summary>
        /// <param name="strProtocol">받은 메시지 문자열</param>
        /// <returns>응답 데이터</returns>
        public string receiveMsgProc(string strProtocol)
        {
            utilProtocol revData = new utilProtocol(strProtocol);

            if (string.IsNullOrEmpty(strProtocol))
                return "FAIL"; // FAIL 반환
            string[] Splits = strProtocol.Split(':');
            string values = string.Empty;
            if (Splits[0] == "AGING")
            {
                var myIni = new IniFile((Global.instance.IniAgingPath + @"\AgingRecord.ini"));
                values = myIni.Read(strProtocol, "AGING");
            }
            else if (Splits[0] == "UNCLAMP_COUNT")
            {
                if (!string.IsNullOrEmpty(Splits[1]))
                {
                    var myIni = new IniFile(Global.instance.IniSystemPath);
                    values = myIni.Read("AGING_COUNT", "SYSTEM");
                    if (string.IsNullOrEmpty(values))
                    {
                        values = "0";
                    }
                    int loadingCoung= (int)Convert.ToInt32(SingletonManager.instance.Channel_Model[0].LoadCount);
                    int AgingCoung = (int)Convert.ToInt32(values);
                    int UnclampCoung = (int)Convert.ToInt32(Splits[1]);
                    if (loadingCoung > UnclampCoung)
                    {
                        SingletonManager.instance.Channel_Model[0].AgingCvTotalCount = ((loadingCoung + AgingCoung) - UnclampCoung).ToString();
                        
                        myIni.Write("AGING_COUNT", SingletonManager.instance.Channel_Model[0].AgingCvTotalCount, "SYSTEM");
                    }
                    else
                    {
                        SingletonManager.instance.Channel_Model[0].AgingCvTotalCount = SingletonManager.instance.Channel_Model[0].LoadCount;
                    }
                    values = $"AGING_TOTAL_COUNT:{SingletonManager.instance.Channel_Model[0].AgingCvTotalCount}";
                }
            }

            if (!string.IsNullOrEmpty(values))
                return values;
            else
                return "FAIL"; // FAIL 반환
        }
    }
}
