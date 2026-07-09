using OpenCvSharp;
using System;
using System.Globalization;
using System.Windows.Data;
using VisionProcess.wpf.Extensions;

namespace VisionProcess.wpf.Converters
{
    /// <summary>
    /// 将 <see cref="Mat"/> 转换为 WPF <see cref="System.Windows.Media.Imaging.BitmapSource"/>，
    /// 供 ImageRecord 等控件绑定使用。转换逻辑位于 wpf 层，ViewModel 不依赖 WPF。
    /// </summary>
    public class MatToBitmapSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Mat mat)
                return mat.ToFrozenBitmapSource();

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 不支持反向转换
            return null;
        }
    }
}
