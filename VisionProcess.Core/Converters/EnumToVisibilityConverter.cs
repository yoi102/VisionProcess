using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace VisionProcess.Core.Converters
{
    public class EnumToVisibilityConverter : BaseValueConverter
    {

        public bool UseHidden { get; set; }
        public int VisibleValue { get; set; }
        public List<int> VisibleValues{ get; set; } = new List<int>();  

        public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            var type = value.GetType();
            if (type.IsEnum)
            {
                var v = (int)value;

                if (v == VisibleValue)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
                }
            }
            return Binding.DoNothing;

        }




        public override object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
