using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;
using System.Reflection;

namespace VisionProcess.wpf.Converters
{

    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            return GetEnumDescription(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        private static string GetEnumDescription(object enumObject)
        {
            string name = enumObject.ToString() ?? string.Empty;
            return enumObject.GetType()
                       .GetField(name)?
                       .GetCustomAttribute<DescriptionAttribute>()?
                       .Description
                   ?? name;
        }
    }
}
