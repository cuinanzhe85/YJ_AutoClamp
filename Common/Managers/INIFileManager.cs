using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.Managers
{
    public class IniFile
    {
        string Path;
        string EXE = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        /// <summary>
        /// INI 파일 매니저 생성자
        /// </summary>
        /// <param name="IniPath">INI 파일 경로</param>
        public IniFile(string IniPath = null)
        {
            Path = new FileInfo(IniPath ?? EXE + ".ini").FullName.ToString();
        }

        /// <summary>
        /// INI 파일에서 Key에 해당하는 Value 읽어들이기
        /// </summary>
        /// <param name="Key">INI에서 키값</param>
        /// <param name="Section">INI에서 섹션값</param>
        /// <returns>읽은 Value 값</returns>
        public string Read(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);

            string readStr = RetVal.ToString();
            readStr.Trim();
            if (readStr.IndexOf(';') > 0)
            {
                readStr = readStr.Substring(0, readStr.IndexOf(';')).Trim();
            }

            return readStr;
        }

        /// <summary>
        /// 모델파일에서 INI 파일에 Comment 처리 시, Comment 값 가져오기
        /// </summary>
        /// <param name="Key">INI의 키값</param>
        /// <param name="Section">INI의 섹션값</param>
        /// <returns>읽은 Comment 값</returns>
        public string GetComment(string Key, string Section = null)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);

            string readStr = RetVal.ToString();
            readStr.Trim();
            if (readStr.IndexOf(';') > 0)
            {
                readStr = readStr.Substring(readStr.IndexOf(';'));
            }
            else
            {
                return null;
            }

            return readStr;
        }

        /// <summary>
        /// 주어진 섹션, 키에 Value 값 저장 (format에 구조맞춤)
        /// </summary>
        /// <param name="Key">INI 키</param>
        /// <param name="Value">INI Value</param>
        /// <param name="Section">INI 섹션</param>
        /// <param name="format">저장 포맷</param>
        public void Write(string Key, double Value, string Section = null, string format = null)
        {
            string strVal = Value.ToString();
            if (format != null)
            {
                strVal = string.Format(format, Value);
            }
            Write(Key, strVal, Section);
        }

        /// <summary>
        /// 주어진 키와 섹션에 Value 저장
        /// </summary>
        /// <param name="Key">INI 키</param>
        /// <param name="Value">정수형 값</param>
        /// <param name="Section">INI 섹션</param>
        public void Write(string Key, int Value, string Section = null)
        {
            string strVal = Value.ToString();
            Write(Key, strVal, Section);
        }

        /// <summary>
        /// 주어진 키와 섹션에 Value 저장
        /// </summary>
        /// <param name="Key">INI 키</param>
        /// <param name="Value">문자열 값</param>
        /// <param name="Section">INI 섹션</param>
        public void Write(string Key, string Value, string Section = null)
        {
            string comment = GetComment(Key, Section);
            if (comment != null)
            {
                Value += comment;
            }

            WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
        }

        /// <summary>
        /// 키에 따른 값 삭제
        /// </summary>
        /// <param name="Key">INI 키</param>
        /// <param name="Section">INI 섹션</param>
        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section ?? EXE);
        }

        /// <summary>
        /// 섹션 삭제
        /// </summary>
        /// <param name="Section"></param>
        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section ?? EXE);
        }

        /// <summary>
        /// 해당 키의 값이 있는지 확인
        /// </summary>
        /// <param name="Key">INI 키값</param>
        /// <param name="Section">INI 섹션</param>
        /// <returns>값의 여부</returns>
        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
