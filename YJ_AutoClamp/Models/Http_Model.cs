using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace YJ_AutoClamp.Models
{
    public class Http_Model
    {
        class ResultList
        {
            public string fctCode { get; set; }
            public string prodcMagtNo { get; set; }
            public string inspTopCode { get; set; }
            public string rsltCode { get; set; }
            public string errCode { get; set; }
            public string exinspTime { get; set; }

        }
        class Root
        {
            public int resultCode { get; set; }
            public string resultStr { get; set; }
            public int resultListCnt { get; set; }
            public List<ResultList> resultList { get; set; }
        }

        public string GetprocCodeData(string _inputCN)
        {
            //매개변수 복사
            string inputCN = _inputCN;

            //JSON 세팅
            string factoryCode = string.Empty;
            string plantCode = string.Empty;
            string plantUrl = string.Empty;
            string APIName = string.Empty;

            //TOKEN KEY
            string ticket = string.Empty;

            //최종 송부 URL
            string APIURL = string.Empty;

            factoryCode = "C100E";
            plantCode = "P104";
            plantUrl = "http://168.219.108.30:81/gmes2/gmes2If.do";
            APIName = "com_samsung_gmes2_qm_json_vo_QmSubInspForJsonSVO/1/qmSubInspForJson01DVO";
            //ticket = "credential:TICKET-d419685c-ebc5-44c4-9c95-51254a872697:23ba025a-e5f7-48a8-9cca-9968765107f3:dc3ef8d0-69b5-45b4-8312-c129b3583610_6da98e02-005c-4a35-89e1-b038cc885665:-1:6nAo3jZN+JAcZWvBuAoesS/WdfxWEMTXeWQqBZuoGvYcKcSbz5WDwGdp5h0dVdpPfYFexmw5Cz9w53/v/y9pZQ==:signature=QaZOJfq1BZrw3/W7uJMXvL0il0VxvHt/1hpkHmpk41N1Pt+0gOmEC8sTIw4pfw8p73I2Mn4VWlogBZM/WJ1yAg==";
            ticket = "credential:TICKET-839fd18b-596e-4f08-b257-9b30dd9d2275:d76f5914-0caa-430d-837c-c9c7257e41c4:1950074a-219a-44a4-898c-47ce4d36d5ea_6da98e02-005c-4a35-89e1-b038cc885665:-1:CZXIvSBHI86/NCZAd4h9jLJ9t8TXjmtFvEahao68ImG3zSIXPn5eobN2gmEWUljlFpR|C8IJYn1v2JYWFMnxMQ==:signature=mqCuwbiVsW1NCMAkb2+Iq68DsK/SDgegRnY/aTtBwzPfkN3kRaP0qGx+GXL0YytDRHy5VRV8frT/1mPFybvhcw==";
            // inspTopCode TopCode 필스로 입력할 예정
            APIURL = plantUrl + "/" + APIName + "?fctCode=" + factoryCode + "&plantCode=" + plantCode + "&inspTopCode=TOP123" + "&prodcMagtNo=" + inputCN;
            Global.Mlog.Info($"http send : " + APIURL);
            string ret = SendHttp(ticket, APIURL);

            return ret;
        }

        public string SendHttp(string ticket, string APIURL)
        {
            Global.Mlog.Info($"http send : WebRequest.Create");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APIURL);

            string responseText = string.Empty;

            request.Method = "GET";
            request.Timeout = 30 * 1000;
            //request.Headers.Add("x-dep-ticket", ticket);
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    HttpStatusCode status = resp.StatusCode;

                    Stream stream = resp.GetResponseStream();

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        responseText = reader.ReadToEnd();
                        var objectString = JsonConvert.DeserializeObject<Root>(responseText);
                        //var objectString = JsonSerializer.Deserialize<Root>(responseText);//json 객체 제작 
                        Global.Mlog.Info($"http receive : " + objectString.resultList[0].rsltCode);
                        if (objectString.resultList[0].rsltCode != "")
                        {
                            return objectString.resultList[0].rsltCode;
                        }
                    }
                }
            }
            catch
            {
                Global.Mlog.Info($"http : GetResponse fail" );
            }
            

            return "";
        }
    }
}
