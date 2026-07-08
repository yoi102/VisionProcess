using Nodify;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisionProcess.Runtime.Models;

namespace VisionProcess.wpf.Views
{
    /// <summary>
    /// EditorView.xaml 的交互逻辑
    /// </summary>
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
            // NodifyEditor handles mouse events internally (e.g. panning on right-click),
            // so we must listen to already-handled events to reliably open/close the menu.
            EventManager.RegisterClassHandler(typeof(NodifyEditor), MouseLeftButtonDownEvent, new MouseButtonEventHandler(CloseOperationsMenu), handledEventsToo: true);
            EventManager.RegisterClassHandler(typeof(NodifyEditor), MouseRightButtonUpEvent, new MouseButtonEventHandler(OpenOperationsMenu), handledEventsToo: true);
        }

        private void OpenOperationsMenu(object sender, MouseButtonEventArgs e)
        {
            if (sender is not NodifyEditor editor || editor.IsPanning || editor.DataContext is not ProcessModel process)
            {
                return;
            }

            // Only open the operations menu on the editor background, not on nodes/connections/connectors.
            if (!IsBlankArea(editor, e.OriginalSource as DependencyObject))
            {
                return;
            }

            e.Handled = true;
            process.OperationsMenu?.OpenAt(editor.MouseLocation);
        }

        private void CloseOperationsMenu(object sender, RoutedEventArgs e)
        {
            if (sender is not NodifyEditor editor || editor.DataContext is not ProcessModel process)
            {
                return;
            }

            // Don't close the menu when the user is clicking inside it (e.g. on a menu item).
            if (e.OriginalSource is DependencyObject source && IsInsideOperationsMenu(source, editor))
            {
                return;
            }

            process.OperationsMenu?.Close();
        }

        private static bool IsBlankArea(NodifyEditor editor, DependencyObject? source)
        {
            if (source is null || ReferenceEquals(source, editor))
            {
                return true;
            }

            var current = source;
            while (current is not null && !ReferenceEquals(current, editor))
            {
                if (current is Node || current is ItemContainer || current is Connection || current is NodeInput || current is NodeOutput)
                {
                    return false;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return true;
        }

        private static bool IsInsideOperationsMenu(DependencyObject source, NodifyEditor editor)
        {
            var current = source;
            while (current is not null && !ReferenceEquals(current, editor))
            {
                if (current is OperationsMenuView)
                {
                    return true;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}