using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YJ_AutoClamp.Models
{
    public class HttpJson_Model
    {
        private bool _DataSendFlag = false;
        public bool DataSendFlag
        {
            get { return _DataSendFlag; }
            set { _DataSendFlag = value; }
        }
        private string _ResultCode = "";
        public string ResultCode
        {
            get { return _ResultCode; }
            set { _ResultCode = value; }
        }
        public HttpJson_Model()
        {
        }
        private string GetLocalIP()
        {
            string myIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    byte[] add = ip.GetAddressBytes();
                    if (add[0] == 10)
                        myIP = ip.ToString();
                }
            }
            return myIP;
        }
        public async void SendRequest(string methodName, string _prodcMagtNo, string _rsltCode = "")
        {
            DataSendFlag = false;
            ResultCode = "";
            var url = "http://168.219.108.30:81/gmes2/gmes2If.do";
            string Ticket = "credential:TICKET-839fd18b-596e-4f08-b257-9b30dd9d2275:d76f5914-0caa-430d-837c-c9c7257e41c4:1950074a-219a-44a4-898c-47ce4d36d5ea_6da98e02-005c-4a35-89e1-b038cc885665:-1:CZXIvSBHI86/NCZAd4h9jLJ9t8TXjmtFvEahao68ImG3zSIXPn5eobN2gmEWUljlFpR|C8IJYn1v2JYWFMnxMQ==:signature=mqCuwbiVsW1NCMAkb2+Iq68DsK/SDgegRnY/aTtBwzPfkN3kRaP0qGx+GXL0YytDRHy5VRV8frT/1mPFybvhcw==";

            var requestData = new RequestRoot
            {
                com_samsung_gmes2_qm_json_vo_QmSubInspForJsonSVO = new QmSubInspRequest
                {
                    qmSubInspForJson01DVO = new QmSubInspForJson01DVO
                    {
                        fctCode = "C100E",
                        plantCode = "P104",
                        inspTopCode = "TOPJ31",
                        prodcMagtNo = _prodcMagtNo,
                        rsltCode = _rsltCode,
                        bcrIp =  GetLocalIP() + ":1212",
                        jigNo = "1",
                        inspDt = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}"
                        //rsltCode = methodName == "saveInspInfo" ? _rsltCode : null,
                        //bcrIp = methodName == "saveInspInfo" ? GetLocalIP() : null,
                        //jigNo = null,
                        //inspDt = methodName == "saveInspInfo" ? $"{DateTime.Now.ToString("yyyyMMddHHmmss")}" : null
                    },
                    anyframeDVO = new AnyframeDVO
                    {
                        appName = "com.samsung.gmes2.qm.json.app.QmSubInspForJsonApp",
                        methodName = methodName,
                        inputSVOName = "com.samsung.gmes2.qm.json.vo.QmSubInspForJsonSVO",
                        pageNo = "0",
                        pageRowCount = "0"
                    }
                }
            };
            _ = Task.Run(async () =>
            {
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                Global.Mlog.Info(json);
                try
                {
                    // HttpRequestMessage로 헤더 추가
                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        request.Content = content;
                        if (!string.IsNullOrEmpty(Ticket))
                        {
                            request.Headers.Add("x-dep-ticket", Ticket);

                            Global.Mlog.Info($"HTTP Ticket : {Ticket}");
                        }
                            
                        var response = await SingletonManager.instance.HttpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();

                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonSerializer.Deserialize<ResponseRoot>(responseString);
                        Global.Mlog.Info($"Http R : {responseString}");
                        var result = responseObject.com_samsung_gmes2_qm_json_vo_QmSubInspForJsonSVO.qmSubInspForJson02DVO;
                        Global.Mlog.Info($"{methodName} rsltCode: {result?.rsltCode}, errCode: {result?.errCode}");

                        DataSendFlag = true;
                        ResultCode = result?.rsltCode;
                    }
                }
                catch (TaskCanceledException)
                {
                    DataSendFlag = true;
                    Global.Mlog.Info($"{methodName}요청 시간이 초과되었습니다 (Timeout).");
                }
                catch (Exception ex)
                {
                    DataSendFlag = true;
                    Global.Mlog.Info($"{methodName}요청 실패: {ex.Message}");
                }
            });
        }
    }

    // 데이터 모델

    public class RequestRoot
    {
        public QmSubInspRequest com_samsung_gmes2_qm_json_vo_QmSubInspForJsonSVO { get; set; }
    }

    public class QmSubInspRequest
    {
        public QmSubInspForJson01DVO qmSubInspForJson01DVO { get; set; }
        public AnyframeDVO anyframeDVO { get; set; }
    }

    public class QmSubInspForJson01DVO
    {
        public string userId { get; set; }
        public string UserNm { get; set; }
        public string fctCode { get; set; }
        public string plantCode { get; set; }
        public string inspTopCode { get; set; }
        public string prodcMagtNo { get; set; }
        public string rsltCode { get; set; }
        public string bcrIp { get; set; }
        public string jigNo { get; set; }
        public string inspDt { get; set; }
    }

    public class AnyframeDVO
    {
        public string appName { get; set; }
        public string methodName { get; set; }
        public string inputSVOName { get; set; }
        public string clientIPAddr { get; set; }
        public string userID { get; set; }
        public string pageNo { get; set; }
        public string pageRowCount { get; set; }
    }

    public class ResponseRoot
    {
        public QmSubInspResponse com_samsung_gmes2_qm_json_vo_QmSubInspForJsonSVO { get; set; }
    }

    public class QmSubInspResponse
    {
        public QmSubInspForJson01DVO qmSubInspForJson01DVO { get; set; }
        public QmSubInspForJson02DVO qmSubInspForJson02DVO { get; set; }
        public AnyframeDVO anyframeDVO { get; set; }
    }

    public class QmSubInspForJson02DVO
    {
        public string inspTopCode { get; set; }
        public string prodcMagtNo { get; set; }
        public string rsltCode { get; set; }
        public string errCode { get; set; }
        public string exinspTime { get; set; }
    }
}
