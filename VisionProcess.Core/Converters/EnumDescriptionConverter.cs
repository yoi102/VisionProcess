using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace VisionProcess.Core.Converters
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

        private string GetEnumDescription(object enumObject)
        {
            var fieldInfo = enumObject.GetType().GetField(enumObject.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo!.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return enumObject.ToString();
        }
    }
}