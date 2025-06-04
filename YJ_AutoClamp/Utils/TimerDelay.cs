using System;

namespace YJ_AutoClamp.Utils
{
    public class TimerDelay
    {
        DateTime TimeStop;
        TimeSpan dwTime;
        public bool _TimStop = false;
        protected DateTime TimeSart;
        public TimerDelay()
        {
            TimeSart = DateTime.Now;
        }

        public void SetTimer()
        {
            dwTime = new TimeSpan(0);
            TimeStop = new DateTime(0);
            TimeSart = DateTime.Now;
            _TimStop = false;
        }
        public void PauseTimer()
        {
            if (_TimStop) return;
            dwTime += (DateTime.Now - TimeSart);
            _TimStop = true;

        }
        public void RunTimer()
        {
            if (_TimStop)
                _TimStop = false;
            else return;
            TimeSart = DateTime.Now;
        }
        public void StartTimer()
        {
            dwTime = new TimeSpan(0);
            TimeStop = new DateTime(0);
            TimeSart = DateTime.Now;
            _TimStop = false;
        }
        public void ResetTimer()
        {
            //if (_TimStop) {
            dwTime = new TimeSpan(0);
            TimeSart = DateTime.Now;
            //}
        }
        public double GetTime
        {
            get
            {
                if (!_TimStop)
                {
                    TimeSpan result = DateTime.Now - TimeSart + dwTime;
                    return (result.TotalMilliseconds / 1000);
                }
                else
                {
                    return (dwTime.TotalMilliseconds / 1000);
                }
            }
        }
        public bool MoreThan(double _time)
        {
            return (GetTime > _time) ? true : false;
        }
        public bool LessThan(double _time)
        {
            return (GetTime < _time) ? true : false;
        }
    }
}
