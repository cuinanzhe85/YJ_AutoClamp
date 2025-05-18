using System;
using System.Windows.Data;

namespace YJ_AutoClamp.Converters
{
    public class ConnectionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "LightGray";
                }
                else
                {
                    return "LawnGreen";
                }
            }
            catch
            {
                return "Purple";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class FloorColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "LightGray";
                }
                else
                {
                    return "LimeGreen";
                }
            }
            catch
            {
                return "LightGray";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ChannelStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                Models.ChannelStatus m = (Models.ChannelStatus)value;
                if (m == Models.ChannelStatus.EMPTY)
                {
                    return "LightSlateGray";
                }
                if (m == Models.ChannelStatus.RUNNING)
                {
                    return "RoyalBlue";
                }
                else if (m == Models.ChannelStatus.OK)
                {
                    return "#009825";
                }
                else
                {
                    return "OrangeRed"; // NG
                }
            }
            catch
            {
                return "#262626";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LimitColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "White";
                }
                else
                {
                    return "OrangeRed";
                }
            }
            catch
            {
                return "Purple";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SafetyButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "#393637";
                }
                else
                {
                    return "Green";
                }
            }
            catch
            {
                return "#393637";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AlarmForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == true)
                {
                    return "White";
                }
                else
                {
                    return "Black";
                }
            }
            catch
            {
                return "Black";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AlarmBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == true)
                {
                    return "#F14B4D";
                }
                else
                {
                    return "#E7E6E6";
                }
            }
            catch
            {
                return "#E7E6E6";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AlarmForegroundColorReversConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "White";
                }
                else
                {
                    return "Black";
                }
            }
            catch
            {
                return "Black";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class AlarmBackgroundColorReversConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "#F14B4D";
                }
                else
                {
                    return "#E7E6E6";
                }
            }
            catch
            {
                return "#E7E6E6";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
