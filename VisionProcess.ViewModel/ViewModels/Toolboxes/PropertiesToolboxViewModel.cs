using System.ComponentModel;
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using VisionProcess.Application.ViewModels;
using VisionProcess.Runtime.Models;
using VisionProcess.ViewModel.ViewServices;

namespace VisionProcess.ViewModel.ViewModels.Toolboxes;

public partial class PropertiesToolboxViewModel : ObservableToolboxBase
{
    private readonly IToolboxIconsService _toolboxIconsService;
    private EditorViewModel? _editor;

    public PropertiesToolboxViewModel(IToolboxIconsService toolboxIconsService)
    {
        _toolboxIconsService = toolboxIconsService;
        Title = "Property";
        Zone = DockZone.RightTop;
        Icon = toolboxIconsService.Git;
        Shortcut = "Ctrl+Shift+G";
        IsOpenByDefault = true;
        ContentId = Id = Guid.NewGuid().ToString();
    }

    [ObservableProperty]
    public partial string ContentId { get; private set; }

    [ObservableProperty]
    public partial OperationModel? SelectedOperation { get; private set; }

    public void Attach(EditorViewModel? editor)
    {
        if (ReferenceEquals(_editor, editor))
        {
            Refresh();
            return;
        }

        if (_editor is not null)
        {
            _editor.Process.PropertyChanged -= OnProcessPropertyChanged;
        }

        _editor = editor;

        if (_editor is not null)
        {
            _editor.Process.PropertyChanged += OnProcessPropertyChanged;
        }

        Refresh();
    }

    private void OnProcessPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProcessModel.SelectedOperation))
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        SelectedOperation = _editor?.Process.SelectedOperation;
    }
}
