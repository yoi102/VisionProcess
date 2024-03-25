using System.ComponentModel;
using System.Reflection;

namespace VisionProcess.Core.Converters
{
    public class EnumDescriptionTypeConverter(Type type) : EnumConverter(type)
    {
        public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo? fi = value.GetType().GetField(value.ToString()!);
                    if (fi != null)
                    {
                        //var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        //return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();

                        var attributes = (DescriptionAttribute?)fi.GetCustomAttribute(typeof(DescriptionAttribute), false);
                        return ((attributes is not null) && (!String.IsNullOrEmpty(attributes.Description))) ? attributes.Description : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}