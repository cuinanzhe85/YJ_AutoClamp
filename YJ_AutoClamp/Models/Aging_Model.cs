using Common.Managers;
using Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Threading;

namespace YJ_AutoClamp.Models
{
    public enum AgingCvList
    {
        Upper1,
        Upper2,
        Upper3,
        Low1,
        Low2,
        Low3,
        Max
    }
    
    public class Aging_Model : BindableAndDisposable
    {
        public ObservableCollection<AgingSetpList> AgingList { get; set; }
        private int _AgingIndex = 0;
        public int AgingIndex
        {
            get { return _AgingIndex; }
            set { SetValue(ref _AgingIndex, value); }
        }
        private int _TotalCount = 0;
        public int TotalCount
        {
            get { return _TotalCount; }
            set { SetValue(ref _TotalCount, value); }
        }
        public Aging_Model(int agingIndex)
        {
            AgingList = new ObservableCollection<AgingSetpList>();
            AgingIndex = agingIndex;

            for (int i = 0; i < 28; i++)
            {
                AgingList.Add(new AgingSetpList());
            }
            // 초기화 시 Aging 기록 호출
            AgingCountRead();
        }
        public void SetFirstLoadingFloor(int Index)
        {
            if (Index < 0)
            {
                Index = 0;
            }
            else if (Index > (int)Floor_Index.Max)
            {
                Index = (int)Floor_Index.Max;
            }
            for (int i = 0; i < Index; i++)
            {
                AgingList[0].Floor[i] = true;
            }
            AgingList[0].Count = Index;
            AgingList[0].AgingStartTimet = DateTime.Now.ToString("yyyyMMddHHmmss");
            // Test
            //AgingList[0].AgingStartTimet = DateTime.Now.AddHours(-AgingIndex).ToString("yyyyMMddHHmmss");
        }
        public void AgingStepShift()
        {
            // AgingList의 크기
            //int count = AgingList.Count;
            //TotalCount = 0;
            //// 뒤에서부터 한 칸씩 앞으로 복사
            //for (int i = count - 1; i >= 1; i--)
            //{
            //    // 깊은 복사하여 이전 항목을 새 항목으로 할당
            //    AgingList[i] = AgingList[i - 1].Clone<AgingSetpList>();
            //    TotalCount = TotalCount + AgingList[i].Count;
            //}
            //AgingList[0] = new AgingSetpList();
            //AgingCountWrite();
            int count = AgingList.Count;
            TotalCount = 0;
            // 뒤에서부터 한 칸씩 앞으로 복사 (깊은 복사)
            for (int i = count - 1; i >= 1; i--)
            {
                // 기존 객체를 교체하지 않고 값만 복사
                var src = AgingList[i - 1];
                var dest = AgingList[i];

                // 값 복사
                dest.Count = src.Count;
                dest.AgingTime = src.AgingTime;
                dest.AgingStartTimet = src.AgingStartTimet;
                for (int j = 0; j < dest.Floor.Count; j++)
                    dest.Floor[j] = src.Floor[j];

                TotalCount += dest.Count;
                src = null;
                dest = null;
            }
            // 첫 번째 값 초기화
            AgingList[0].Clear();
            AgingCountWrite();
        }
        public void AgingLastDelete()
        {
            // AgingList의 크기
            int count = AgingList.Count;

            // 마지막 영역 삭제
            for (int i = count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(AgingList[i].AgingStartTimet))
                {
                    //AgingList[i] = new AgingSetpList();
                    AgingList[i].Clear();
                    break;
                }
            }
            AgingCountWrite();
        }
        public void AgingCountReset()
        {
            // AgingList의 크기
            int count = AgingList.Count;

            // 마지막 영역 삭제
            for (int i = 0; i < count; i++)
            {
                AgingList[i].Clear();
            }
            TotalCount = 0;
            AgingCountWrite();
        }
        private void AgingCountWrite()
        {
            try
            {
                string setction = "";
                if (AgingIndex == 0)
                    setction = "UPPER_1";
                else if (AgingIndex == 1)
                    setction = "UPPER_2";
                else if (AgingIndex == 2)
                    setction = "UPPER_3";
                else if (AgingIndex == 3)
                    setction = "LOW_1";
                else if (AgingIndex == 4)
                    setction = "LOW_2";
                else if (AgingIndex == 5)
                    setction = "LOW_3";

                var myIni = new IniFile(Global.instance.IniAgingPath + "\\AgingRecord.ini");
                string values;
                for (int i = 0; i < 28; i++)
                {
                    if (!string.IsNullOrEmpty(AgingList[i].AgingStartTimet))
                    {
                        values = AgingList[i].Count.ToString() + "," + AgingList[i].AgingStartTimet;
                        myIni.Write(i.ToString(), values, setction);
                    }
                    else
                        myIni.Write(i.ToString(), "", setction);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AgingCountRead Error: " + ex.Message);
            }
        }
        private void AgingCountRead()
        {
            string setction = "";
            
            if (AgingIndex == 0)
                setction = "UPPER_1";
            else if (AgingIndex == 1)
                setction = "UPPER_2";
            else if (AgingIndex == 2)
                setction = "UPPER_3";
            else if (AgingIndex == 3)
                setction = "LOW_1";
            else if (AgingIndex == 4)
                setction = "LOW_2";
            else if (AgingIndex == 5)
                setction = "LOW_3";

            var myIni = new IniFile(Global.instance.IniAgingPath + "\\AgingRecord.ini");

            string values;
            for (int i = 0; i < 28; i++)
            {
                values = myIni.Read(i.ToString(), setction);
                if (!string.IsNullOrEmpty(values))
                {
                    string[] split = values.Split(',');
                    AgingList[i].Count = (int)Convert.ToInt32(split[0]);
                    AgingList[i].AgingStartTimet = split[1];
                    for (int j = 0; j < AgingList[i].Count; j++)
                    {
                        AgingList[i].Floor[j] = true;
                    }
                }
            }
        }
        // Dispose에서 Timer 해제
        protected override void DisposeManaged()
        {

            base.DisposeManaged();
        }
    }
    public class AgingSetpList : BindableAndDisposable
    {
        private ObservableCollection<bool> _Floor;
        public ObservableCollection<bool> Floor
        {
            get { return _Floor; }
            set { SetValue(ref _Floor, value); }
        }
        private int _Count = 0;
        public int Count
        {
            get { return _Count; }
            set { SetValue(ref _Count, value); }
        }
        private string _AgingTimet = "00:00:00";
        public string AgingTime
        {
            get { return _AgingTimet; }
            set { SetValue(ref _AgingTimet, value); }
        }
        private string _AgingStartTimet = string.Empty;
        public string AgingStartTimet
        {
            get { return _AgingStartTimet; }
            set { SetValue(ref _AgingStartTimet, value); }
        }
        public AgingSetpList()
        {
            Floor = new ObservableCollection<bool>();
            for (int i = 0; i < (int)Floor_Index.Max; i++)
            {
                Floor.Add(false);
            }
            Count = 0;
            AgingTime = "00:00:00";
            AgingStartTimet = string.Empty ;
        }
        public void Clear()
        {
            for (int i = 0; i < (int)Floor_Index.Max; i++)
            {
                Floor[i] = false;
            }
            Count = 0;
            AgingTime = "00:00:00";
            AgingStartTimet = string.Empty;
        }
    }
}
