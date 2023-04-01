using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace VisonProcess.Core.Converters
{
    public class ItemToListConverter : BaseValueConverter
    {
        public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var argType = value.GetType();
                var listType = typeof(List<>).MakeGenericType(argType);
                var list = Activator.CreateInstance(listType) as IList;
                list?.Add(value);

                return list;
            }

            return value;
        }



        public override object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }


    }
}
