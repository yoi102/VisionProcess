using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VisionProcess.Core.Selectors
{
    public class EnumTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element)
            {
                if (item.GetType().IsEnum)
                {
                    return element.FindResource(item.ToString()) as DataTemplate;
                }
            }
            return null;
        }


    }
}
