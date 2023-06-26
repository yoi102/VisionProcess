using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VisionProcess.Core.Converters
{
    public class EnumToVisibilityConverter : BaseValueConverter
    {
        public bool UseHidden { get; set; }
        public int VisibleValue { get; set; }
        //public params int[] VisibleValues { get; set; } 


        public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = value.GetType();
            if (type.IsEnum)
            {
                var v = (int)value;

                return v == VisibleValue ? Visibility.Visible : (object)(UseHidden ? Visibility.Hidden : Visibility.Collapsed);
            }
            return Binding.DoNothing;
        }

        public override object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}