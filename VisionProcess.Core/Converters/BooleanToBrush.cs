using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VisionProcess.Core.Converters
{
    public class BooleanToBrush : BaseValueConverter
    {
        public Brush TrueValue { get; set; } = Brushes.Lime;
        public Brush FalseValue { get; set; } = Brushes.Red;

        public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b ? b ? TrueValue : FalseValue : Binding.DoNothing;
        }

        public override object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}