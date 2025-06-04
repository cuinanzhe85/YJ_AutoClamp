using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;

namespace YJ_AutoClamp.Converters
{
    public class ReversBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool m = (bool)value;
            return !m;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
    public class RadioBoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            else
                return (int)value == System.Convert.ToInt32(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;
            else if ((bool)value)
                return System.Convert.ToInt32(parameter);
            else
                return DependencyProperty.UnsetValue;
        }
    }
    public class ConnectionStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "Closed";
                }
                else
                {
                    return "Connected";
                }
            }
            catch
            {
                return "Closed";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class OnOffStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "OFF";
                }
                else
                {
                    return "ON";
                }
            }
            catch
            {
                return "OFF";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolenVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == false)
                {
                    return "Hidden";
                }
                else
                {
                    return "Visible";
                }
            }
            catch
            {
                return "Hidden";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IndexToCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedIndex && parameter is string indexString && int.TryParse(indexString, out int index))
            {
                return selectedIndex == index;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string indexString && int.TryParse(indexString, out int index))
            {
                return index;
            }
            return Binding.DoNothing;
        }
    }
    public class BoolenVisibleReversConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool m = (bool)value;
                if (m == true)
                {
                    return "Hidden";
                }
                else
                {
                    return "Visible";
                }
            }
            catch
            {
                return "Hidden";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
