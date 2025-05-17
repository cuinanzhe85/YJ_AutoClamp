using System.Collections.Concurrent;
using System.Text;

namespace YJ_AutoClamp.Utils
{
    /// <summary>
    /// 전역적으로 메시지를 이용하여 데이터 송수신 메시지 큐 구성
    /// </summary>
    public class msgQueue
    {
        /// <summary>
        /// 저장 큐
        /// </summary>
        ConcurrentQueue<msgItem> queue;

        /// <summary>
        /// 생성자
        /// </summary>
        public msgQueue()
        {
            queue = new ConcurrentQueue<msgItem>();
        }

        /// <summary>
        /// 큐에 메시지 추가
        /// </summary>
        /// <param name="msg">메시지</param>
        /// <returns>큐의 항목 개수</returns>
        public int AddItem(msgItem msg)
        {
            queue.Enqueue(msg);
            return queue.Count;
        }

        /// <summary>
        /// 주어진 Index가 큐에 존재하면 항목 반환
        /// </summary>
        /// <param name="idx">원하는 Index</param>
        /// <returns>반환 항목</returns>
        public msgItem GetItem(int idx)
        {
            msgItem msg = null;
            if (queue.TryDequeue(out msg) == true)
            {
                return msg;
            }

            return null;
        }

        /// <summary>
        /// 큐에 있는 아이템의 개수
        /// </summary>
        /// <returns>개수</returns>
        public int GetCount()
        {
            return queue.Count;
        }

        public void Clear()
        {
            msgItem msg = null;

            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out msg);
            }
        }
    }

    /// <summary>
    /// 큐에 저장되는 데이터인 메시지 아이템
    /// </summary>
    public class msgItem
    {
        /// <summary>
        /// 이벤트 종류
        /// </summary>
        public enum Event
        {
            None,               // 아무것도 아님
            SendVisionMsg,
            ReceiveVisionMsg,
            WriteLog,           // 로그 쓰기를 위함
        };
        public enum Target
        {
            Pg,
            Qspi
        }

        /// <summary>
        /// 메시지에서 다루는 항목들
        /// </summary>
        public Event evt;
        public utilProtocolVision.CMD cmdVision;
        public byte[] pDataBuff;    // 버퍼 형식의 배열
        public int channel;         // 송수신 대상의 ch
        public string sender;       // 송신자
        public string msgString;    // 보낼 문자열
        public int visionIndex;

        /// <summary>
        /// 생성자 (이벤트는 기본적으로 None임)
        /// </summary>
        public msgItem()
        {
            evt = Event.None;
        }


        /// <summary>
        /// Vision 전송 메시지 만들기
        /// </summary>
        /// <param name="strVal">전송문자열</param>
        public void msgSendVisionMsg(string strVal)
        {
            evt = Event.SendVisionMsg;

            msgString = strVal;
        }

        /// <summary>
        /// Vision으로부터 받은 수신 메시지 만들기
        /// </summary>
        /// <param name="pBuff">받은데이터버퍼</param>
        /// <param name="rcvSize">받은크기</param>
        /// <param name="senderIp">보낸통신보드IP</param>
        public void msgReceiveVisionData(byte[] pBuff, int rcvSize, utilProtocolVision.CMD cmd, int index)
        {
            evt = Event.ReceiveVisionMsg;

            this.cmdVision = cmd;

            if (rcvSize > pBuff.Length)
            {
                rcvSize = pBuff.Length;
            }

            pDataBuff = new byte[rcvSize];
            for (int i = 0; i < rcvSize; i++)
            {
                pDataBuff[i] = pBuff[i];
            }

            visionIndex = index;
        }
    }
}
