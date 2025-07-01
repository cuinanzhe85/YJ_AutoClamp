using System;
using System.Collections.Generic;

namespace YJ_AutoClamp.Utils
{
    public class utilProtocol
    {
        List<itemProtocol> list;
        public utilProtocol()
        {
            list = new List<itemProtocol>();
        }

        public utilProtocol(string strProtocol)
        {
            list = new List<itemProtocol>();
            AddItem(strProtocol);
        }

        public int AddItem(string strProtocol)
        {
            string[] subStrs = strProtocol.Split(',');
            foreach (string subs in subStrs)
            {
                if (subs.Length > 0)
                {
                    if ((subs[0] == '{') && (subs[subs.Length - 1] == '}'))
                    {
                        string strData = subs.Substring(1, subs.Length - 2);
                        char[] spAry = { '=' };
                        string[] datas = strData.Split(spAry, 2);
                        if (datas.Length == 2)
                        {
                            AddItem(datas[0], datas[1].Replace('`', ','));
                        }
                    }
                }
            }

            return list.Count;
        }

        public int AddItem(string sKey, string sValue)
        {
            itemProtocol item = new itemProtocol(sKey, sValue);
            list.Add(item);
            return list.Count;
        }

        public int AddItem(itemProtocol item)
        {
            if (item != null)
            {
                list.Add(item);
            }

            return list.Count;
        }

        public itemProtocol GetItem(int idx)
        {
            itemProtocol retVal = null;

            if (idx < list.Count)
            {
                retVal = list[idx];
            }

            return retVal;
        }

        public int GetCount()
        {
            return list.Count;
        }

        public int RemoveItem(string sKey)
        {
            int retVal = 0;

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].mKey == sKey)
                    {
                        list.RemoveAt(i);
                        retVal = 1;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Global.ExceptionLog.Error($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {e}");
            }

            return retVal;
        }

        public string GetValue(string sKey)
        {
            string retVal = "";

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].mKey == sKey)
                {
                    retVal = list[i].mValue;
                    break;
                }
            }

            return retVal;
        }

        public string GetSendString()
        {
            string retVal = "";

            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    retVal += ("{" + list[i].mKey + "=" + list[i].mValue + "},");
                }
                retVal += "\n";
            }

            return retVal;
        }
    }

    public class itemProtocol
    {
        public string mKey;
        public string mValue;
        public itemProtocol(string sKey, string sValue)
        {
            mKey = sKey;
            mValue = sValue;
        }
    }
}
