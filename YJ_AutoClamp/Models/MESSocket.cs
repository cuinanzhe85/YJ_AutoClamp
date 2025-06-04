using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using YJ_AutoClamp.Utils;

namespace YJ_AutoClamp.Models
{
    public class MESSocket
    {
        public enum RESULT
        {
            NONE, PASS, FAIL, TSI
        }
        public enum GMES_MODE
        {
            NOT_USE,
            ONLY_SCANNER_MODE,
            USE
        }
        public Socket sock
        {
            get; set;
        }
        private const byte STX = 0x02;
        private const byte ETX = 0x03;
        private const int DATA_SIZE = 1024;
        //MINFO: PO_NO = 010234924465,PLAN_QTY = 150,RMN_QTY = 150,ITEM_SIZE = 50,MODEL_CODE = SM2G991BZADEUE

        private static readonly object _lockCommunication = new object();

        #region ["MES Flag"]
        public bool m_bScanRequestFlag = false; // MES -> ROBOT 세트 스캔 요청
        public bool m_bResultResponeFlag = false;
        public bool m_bLotEndResponeFlag = false;
        public bool m_bResultWait = false; // true : 사용중 , AT 통신 시 JIG 4개 순차적으로 처리하기 위해 사용 
        public RESULT m_Result = RESULT.NONE;
        public string m_sErrorMessage = string.Empty; // MEROR Error Message 저장
        public string m_sMAGCode = string.Empty;      // LotEnd시 매거진 코드 저장 
        //MINFO
        public string m_sPO_NO = string.Empty;
        public int m_dPLAN_QTY = 0;
        public int m_dPROD_QTY = 0;
        public int m_dITEM_SIZE = 0;
        public string m_sBASIC_MODEL_CODE = string.Empty;
        public string m_sMODEL_CODE = string.Empty;
        public int m_dCurrent_QTY = 0;

        private bool _hasReceivedBatt1Fail = false;
        #endregion
        //인코딩 타입
        private Encoding encoding_type
        {
            get; set;
        }
        public TimerDelay timer_mes = new TimerDelay();
        private byte[] m_byte_recevie = new byte[DATA_SIZE];
        private Thread thread_work;
        public MESSocket()
        {
            thread_work = new Thread(new ThreadStart(Connect));
            thread_work.Start();
        }
        public bool IsConnected()
        {
            return sock.Connected;
        }
        public void Close()
        {
            try
            {
                sock.Close();
            }
            catch (Exception) { }
        }
        public void Connect()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            encoding_type = Encoding.ASCII;

            try
            {
                if (InforManager.Instance.m_dGMESMode != GMES_MODE.USE)
                {
                    return;
                }
                var result = sock.BeginConnect(IPAddress.Parse(InforManager.Instance.GMESIP), int.Parse(InforManager.Instance.GMESPORT), null, null);

                bool success = result.AsyncWaitHandle.WaitOne(2000, true);
                if (success)
                {
                    sock.EndConnect(result);
                    sock.BeginReceive(m_byte_recevie, 0, m_byte_recevie.Length, SocketFlags.None, new AsyncCallback(RecevieCallback), sock);
                }
                else
                {
                    sock.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public bool ConnectRequest()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            encoding_type = Encoding.ASCII;

            try
            {
                if (InforManager.Instance.m_dGMESMode != GMES_MODE.USE)
                {
                    return false;
                }
                var result = sock.BeginConnect(IPAddress.Parse(InforManager.Instance.GMESIP), int.Parse(InforManager.Instance.GMESPORT), null, null);

                bool success = result.AsyncWaitHandle.WaitOne(2000, true);
                if (success)
                {
                    sock.EndConnect(result);
                    sock.BeginReceive(m_byte_recevie, 0, m_byte_recevie.Length, SocketFlags.None, new AsyncCallback(RecevieCallback), sock);
                    return true;
                }
                else
                {
                    sock.Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public void ConnectRequestVoid()
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            encoding_type = Encoding.ASCII;

            try
            {
                if (InforManager.Instance.m_dGMESMode != GMES_MODE.USE)
                {
                    return;
                }
                var result = sock.BeginConnect(IPAddress.Parse(InforManager.Instance.GMESIP), int.Parse(InforManager.Instance.GMESPORT), null, null);

                bool success = result.AsyncWaitHandle.WaitOne(2000, true);
                if (success)
                {
                    sock.EndConnect(result);
                    sock.BeginReceive(m_byte_recevie, 0, m_byte_recevie.Length, SocketFlags.None, new AsyncCallback(RecevieCallback), sock);
                    return;
                }
                else
                {
                    sock.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        private void RecevieCallback(IAsyncResult iar)
        {
            try
            {
                Socket sock = (Socket)iar.AsyncState;
                int size = sock.EndReceive(iar);
                m_byte_recevie = SplitArray<byte>(m_byte_recevie, 0, size);
                DataFuntion(m_byte_recevie);
                m_byte_recevie = new byte[DATA_SIZE];
                sock.BeginReceive(m_byte_recevie, 0, m_byte_recevie.Length, SocketFlags.None, new AsyncCallback(RecevieCallback), sock);
            }
            catch (Exception) { }
        }
        public void Send(string m_sdata)
        {
            lock (_lockCommunication)
            {
                try
                {
                    byte[] temp = encoding_type.GetBytes("{");
                    sock.Send(temp, 0, temp.Length, SocketFlags.None);
                    temp = new byte[1] { STX };
                    sock.Send(temp, 0, temp.Length, SocketFlags.None);
                    byte[] data = encoding_type.GetBytes(m_sdata);
                    sock.Send(data, 0, data.Length, SocketFlags.None);
                    temp = new byte[1] { ETX };
                    sock.Send(temp, 0, temp.Length, SocketFlags.None);
                    temp = encoding_type.GetBytes("}");
                    sock.Send(temp, 0, temp.Length, SocketFlags.None);
                    Global.Mlog.Info(m_sdata);
                }
                catch (Exception ex)
                {
                    Global.ExceptionLog.Error($"Fail To Send Data ({m_sdata}) : {ex}");
                }
            }
        }
        Queue<byte> Buffer = new Queue<byte>();

        private void DataFuntion(byte[] data)
        {
            try
            {
                foreach (byte temp in data)
                {
                    if (STX == temp)
                    {
                        Buffer.Clear();
                    }
                    else if (ETX == temp)
                    {
                        DataRecevie(encoding_type.GetString(Buffer.ToArray()));
                        Buffer.Clear();
                    }
                    else
                    {
                        Buffer.Enqueue(temp);
                    }
                }
            }
            catch (Exception) { }
        }
        private void DataRecevie(string data)
        {
            Global.Mlog.Info(data);
            Hashtable data_table = TokenParsing(data);
            switch (data_table["FT"].ToString())
            {
                case "MCOMM": //통신확인
                    Send("RCOMM:SYNCACK");
                    break;
                case "MSTRT": //MES START
                    Send("RRESP:ROBOTRUN");
                    break;
                case "MSTOP": //MES STOP
                    m_dCurrent_QTY = 0;
                    m_bLotEndResponeFlag = false;
                    m_sPO_NO = string.Empty;
                    m_dPLAN_QTY = 0;
                    m_dPROD_QTY = 0;
                    m_dITEM_SIZE = 0;
                    m_sBASIC_MODEL_CODE = string.Empty;
                    m_sMODEL_CODE = string.Empty;
                    Send("RRESP:ROBOTWAIT");
                    break;
                case "MINFO": // PO,MODEL,PLAN,잔량 연계 + ITEM 항목
                    m_dCurrent_QTY = 0;
                    m_bLotEndResponeFlag = false;
                    m_bScanRequestFlag = false;
                    m_sPO_NO = data_table["PO_NO"].ToString();
                    m_dPLAN_QTY = int.Parse(data_table["PLAN_QTY"].ToString());
                    m_dPROD_QTY = int.Parse(data_table["PROD_QTY"].ToString()); // PROD QTY  첫페이지 설명이랑 후반부 설명 PO INFO 다름 MES  프로토콜 확인 필요함
                                                                                //     m_dITEM_SIZE = int.Parse(data_table["ITEM_SIZE"].ToString());
                    m_sBASIC_MODEL_CODE = data_table["BASIC_MODEL"].ToString();
                    m_sMODEL_CODE = data_table["MODEL_CODE"].ToString();
                    Send("RRESP:POACK");
                    break;
                case "MSCIF": // 작업 유형별 SCAN 항목
                    Send("RRESP:SCANACK");
                    break;

                case "MNSCN": // NEXT SCAN 정보 요청
                    m_bScanRequestFlag = true;
                    if (data_table["RESULT"].ToString() == "BATT_1")
                        _hasReceivedBatt1Fail = true;
                    break;

                case "MDCHG": // 자동 모델 변경
                    m_dCurrent_QTY = 0;
                    //관련 FLAG 초기화
                    m_bLotEndResponeFlag = false;
                    m_sPO_NO = string.Empty;
                    m_dPLAN_QTY = 0;
                    m_dPROD_QTY = 0;
                    m_dITEM_SIZE = 0;
                    m_sMODEL_CODE = string.Empty;
                    Send("RRESP:POMODELCHGACK");
                    break;
                case "MCBOX": // BOX 구성
                    Send("RRESP:DAYEND");
                    break;
                case "MATTR":
                    Send("RRATT:ACK");
                    break;
                case "MBMFC": //BOX 강제 마감
                    break;
                case "MRSLT": //데이터 반영 결과
                    if (data_table["RESULT"].ToString() == "PASS")
                    {
                        m_dCurrent_QTY++;
                        m_bResultResponeFlag = true;
                        m_Result = RESULT.PASS;
                        // PutAck();
                        break;
                    }
                    else if (data_table["RESULT"].ToString() == "FAIL")
                    {
                        m_Result = RESULT.FAIL;
                        break;
                    }
                    else if (data_table["RESULT"].ToString() == "MITSI")
                    {
                        m_Result = RESULT.TSI;
                    }
                    timer_mes.PauseTimer();
                    break;
                case "MPFNL": //베이직 모델 생산 완료
                    m_bLotEndResponeFlag = true;
                    Send("RRESP:POENDACK");
                    break;
                case "MDEND": // Daily 생산 완료(일일 생산 완료)
                    Send("RRESP:DAYEND");
                    break;
                case "MRESP":
                    if (data_table["RESULT"].ToString().Contains("STOPACK"))
                    {
                        //당일계획 모델 완료시에 (MES->설비) 가동 중지 요청을 보낸다.

                    }
                    break;
                case "MEROR": // 데이터 처리 에러 발생 ( NG )
                    m_sErrorMessage = data_table["RESULT"].ToString();
                    if (m_sErrorMessage.Contains("POACK")) //POACK 요청 에러시
                    {
                        Send("RRESP:POACK");
                    }
                    else //SET 결과에 대한 에러처리
                    {
                        //MSystem.MyMessagerBottom(m_sErrorMessage);
                        //FAIL의 경우 에러메시지까지 받아야 RESPONE FLAG ON
                        m_sErrorMessage = data_table["RESULT"].ToString();
                        m_bResultResponeFlag = true;
                        //   ErrorAck();
                        //   PutAck(); // 원래는 NG BUFFER 이동 후 보내야한다. ( 설비에서는 바로 처리하고 넘긴다 )
                    }
                    break;
                case "MITSI":
                    m_bResultResponeFlag = true;
                    TsiAck();
                    break;
                case "MATTS":
                    Send("ROUTP:MOVE");
                    break;
                case "MCINF": //MC BOX 정보
                    m_sMAGCode = data_table["MG"].ToString();
                    Send("ROUTP:MOVE");
                    break;
                case "MRREQ":
                    //재호출 응답
                    Send("RRESP:RE-ACK");
                    DataRecevie(data.Split('(')[1].Split(')')[0].Trim());
                    break;
            }
        }

        public bool RNCAN(string cn)
        {
            timer_mes = new TimerDelay();
            timer_mes.StartTimer();
            m_bResultResponeFlag = false;
            m_bScanRequestFlag = false;
            _hasReceivedBatt1Fail = false;
            m_Result = RESULT.NONE;
            Send(string.Format("RSCAN:CN={0}", cn));
            return true;
        }
        public void PutAck()
        {
            Send("RRESP:PUTACK");
            m_bResultResponeFlag = false;
        }
        public void ErrorAck()
        {
            Send("RRESP:ERRORACK");
        }
        public void TsiAck()
        {
            Send("RRESP:TSIACK");
            m_bResultResponeFlag = false;
        }
        //Lable(매거진) 작업 완료 후 발송 해야함
        public void LableComplete()
        {
            Send("ROUTP:MOVE");
            m_dCurrent_QTY = 0;
        }
        public void MachineStopRequest()
        {
            //m_bStopResponeFlag = false;
            Send(string.Format("RSTOP:"));
        }
        public void MachineStartRequest()
        {
            //  m_bStartResponeFlag = false;
            Send(string.Format("RSTART:"));
        }
        private Hashtable TokenParsing(string data)
        {
            Hashtable tokens = new Hashtable();
            string temp = string.Empty;
            if (data.Contains(":"))
            {
                temp = data.Split(':')[1];
                tokens.Add("FT", data.Split(':')[0]);

                string value = string.Empty;
                if (temp.Contains(","))
                {
                    foreach (string p in temp.Split(','))
                    {
                        if (p.Contains("="))
                        {
                            tokens.Add(p.Split('=')[0], p.Split('=')[1]);
                        }
                    }
                }
                else if (temp.Contains("="))
                {
                    tokens.Add(temp.Split('=')[0], temp.Split('=')[1]);
                }
                else
                {
                    tokens.Add("RESULT", temp);
                }
            }
            return tokens;
        }

        public bool HasReceivedBatt1Fail()
        {
            return _hasReceivedBatt1Fail;
        }
        public static T[] SplitArray<T>(T[] array, int startIndex, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, startIndex, result, 0, length);
            return result;
        }
    }
}
