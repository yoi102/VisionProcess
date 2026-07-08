using System.Windows;
using System.Windows.Controls;
using VisionProcess.ViewModel.ViewModels;

namespace VisionProcess.wpf.Views
{
    /// <summary>
    /// Selects the appropriate LayoutItem container style based on whether
    /// the content is a document (<see cref="EditorViewModel"/>) or a toolbox anchorable.
    /// </summary>
    public class PanesStyleSelector : StyleSelector
    {
        public Style? ToolboxStyle { get; set; }

        public Style? DocumentStyle { get; set; }

        public override Style? SelectStyle(object item, DependencyObject container)
        {
            if (item is EditorViewModel)
            {
                return DocumentStyle;
            }

            return ToolboxStyle;
        }
    }
}
