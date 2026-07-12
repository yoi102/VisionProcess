using Nodify;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VisionProcess.Common.Primitives;
using VisionProcess.Runtime.Models;
using VisionProcess.ViewModel.ViewModels;

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

            // Nodify consumes the mouse events used for panning. Listen to handled
            // routed events on this editor instance so a background right-click is
            // still visible to the operations menu without globally changing Nodify.
            Editor.AddHandler(
                Mouse.PreviewMouseUpEvent,
                new MouseButtonEventHandler(OnEditorPreviewMouseUp),
                handledEventsToo: true);
            Editor.AddHandler(
                Mouse.PreviewMouseDownEvent,
                new MouseButtonEventHandler(OnEditorPreviewMouseDown),
                handledEventsToo: true);
        }

        private void OnEditorPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Right ||
                sender is not NodifyEditor editor ||
                DataContext is not EditorViewModel editorViewModel)
            {
                return;
            }

            // Nodify consumes right-click for panning, so WPF's ContextMenuService
            // doesn't get a chance to open menus declared on graph elements.
            if (FindContextMenuOwner(e.OriginalSource as DependencyObject, editor) is FrameworkElement owner)
            {
                editorViewModel.OperationsMenu.Close();
                OpenContextMenu(owner);
                return;
            }

            editorViewModel.OperationsMenu.OpenAt(new VpPoint(editor.MouseLocation.X, editor.MouseLocation.Y));
        }

        private void OnEditorPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left ||
                sender is not NodifyEditor editor ||
                DataContext is not EditorViewModel editorViewModel)
            {
                return;
            }

            // Allow buttons in the operations menu to execute before it is hidden.
            if (!IsDescendantOf<OperationsMenuView>(e.OriginalSource as DependencyObject, editor))
            {
                editorViewModel.OperationsMenu.Close();
            }
        }

        private static FrameworkElement? FindContextMenuOwner(DependencyObject? source, NodifyEditor editor)
        {
            var current = source;
            while (current is not null && !ReferenceEquals(current, editor))
            {
                if (current is FrameworkElement { ContextMenu: not null } element &&
                    current is ItemContainer or Node or NodeInput or NodeOutput or Connection)
                {
                    return element;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static void OpenContextMenu(FrameworkElement owner)
        {
            var contextMenu = owner.ContextMenu;
            if (contextMenu is null)
            {
                return;
            }

            // ContextMenu is hosted in a separate visual tree and therefore cannot
            // reliably inherit the node/connector/connection DataContext by itself.
            contextMenu.DataContext = owner.DataContext;
            contextMenu.PlacementTarget = owner;

            // Open after Nodify has finished processing the same mouse-up event.
            owner.Dispatcher.BeginInvoke(() => contextMenu.IsOpen = true);
        }

        private static bool IsDescendantOf<T>(DependencyObject? source, DependencyObject root)
            where T : DependencyObject
        {
            var current = source;
            while (current is not null && !ReferenceEquals(current, root))
            {
                if (current is T)
                {
                    return true;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
