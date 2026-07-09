using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VisionProcess.Common.Primitives;

namespace VisionProcess.wpf.Converters
{
    /// <summary>
    /// 在 <see cref="VpSize"/> 与 <see cref="System.Windows.Size"/> 之间转换，
    /// 供 Nodify 等 WPF 控件绑定使用。
    /// </summary>
    public class VpSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VpSize size)
                return new Size(size.Width, size.Height);

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Size size)
                return new VpSize(size.Width, size.Height);

            return DependencyProperty.UnsetValue;
        }
    }
}
