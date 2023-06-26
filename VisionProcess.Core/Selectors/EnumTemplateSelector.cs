using System.Windows;
using System.Windows.Controls;

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
