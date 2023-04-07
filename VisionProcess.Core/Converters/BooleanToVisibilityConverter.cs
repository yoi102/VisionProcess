using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VisionProcess.Core.Converters
{
    public class BooleanToVisibilityConverter : BaseValueConverter
    {
        public bool UseHidden { get; set; }
        public bool Reversed { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                if (Reversed) b = !b;
                if (b)
                    return Visibility.Visible;
                else
                    return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
            }
            else
                return Binding.DoNothing;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}