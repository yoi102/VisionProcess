using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VisionProcess.Common.Primitives;

namespace VisionProcess.wpf.Converters
{
    /// <summary>
    /// 在 <see cref="VpPoint"/> 与 <see cref="System.Windows.Point"/> 之间转换，
    /// 供 Nodify 等 WPF 控件绑定使用。
    /// </summary>
    public class VpPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VpPoint point)
                return new Point(point.X, point.Y);

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point point)
                return new VpPoint(point.X, point.Y);

            return DependencyProperty.UnsetValue;
        }
    }
}
