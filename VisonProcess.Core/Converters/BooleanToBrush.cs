using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Diagnostics;

namespace VisonProcess.Core.Converters
{
    public class BooleanToBrush : BaseValueConverter
    {
        public Brush TrueValue { get; set; } = Brushes.Lime;
        public Brush FalseValue { get; set; }= Brushes.Red;

        public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
               
                    return b ? TrueValue : FalseValue;
            }
            else
                return Binding.DoNothing;



        }


        public override object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }




    }
}
