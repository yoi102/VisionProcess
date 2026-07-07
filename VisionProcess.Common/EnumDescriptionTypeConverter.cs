using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace VisionProcess.Common;

public class EnumDescriptionTypeConverter(Type type) : EnumConverter(type)
{
    public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
            return string.Empty;

        if (value is null)
            return string.Empty;

        var valueString = value.ToString();
        if (valueString is null)
            return string.Empty;

        FieldInfo? fi = value.GetType().GetField(valueString);
        if (fi != null)
        {
            //var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            //return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();

            var attributes = (DescriptionAttribute?)fi.GetCustomAttribute(typeof(DescriptionAttribute), false);
            return ((attributes is not null) && (!String.IsNullOrEmpty(attributes.Description))) ? attributes.Description : value.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}