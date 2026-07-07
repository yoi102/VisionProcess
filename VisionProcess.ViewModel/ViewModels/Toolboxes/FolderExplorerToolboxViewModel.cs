using System.Collections.ObjectModel;
using AvalonDock.Core;
using AvalonDock.Mvvm.CommunityToolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisionProcess.Application.ViewModels;
using VisionProcess.ViewModel.ViewServices;

namespace VisionProcess.ViewModel.ViewModels.Toolboxes;

public partial class FolderExplorerToolboxViewModel : ObservableToolboxBase, IDisposable
{
    private MainViewModel? _mainViewModel;
    private readonly IFileDocumentService? _fileDocumentService;
    private bool _isSynchronizingSelection;

    public FolderExplorerToolboxViewModel(
        IToolboxIconsService toolboxIconsService,
        IFileDocumentService? fileDocumentService = null)
    {
        _fileDocumentService = fileDocumentService;
        ContentId = Id = Guid.NewGuid().ToString();
        Title = "FolderExplorer";
        Zone = DockZone.LeftTop;
        Icon = toolboxIconsService.Explorer;
        Shortcut = "Ctrl+Shift+E";
        IsOpenByDefault = true;
    }

    [ObservableProperty]
    public partial string ContentId { get; private set; }

    public ObservableCollection<FolderExplorerDocumentItemViewModel> Documents { get; } = [];

    public bool HasDocuments => Documents.Count > 0;

    [ObservableProperty]
    public partial FolderExplorerDocumentItemViewModel? SelectedDocument { get; set; }

    public void Attach(MainViewModel mainViewModel)
    {
        if (ReferenceEquals(_mainViewModel, mainViewModel))
        {
            RefreshDocuments();
            return;
        }

        Detach();
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

        _mainViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        _mainViewModel.Editors.WhenAdded(_ => RefreshDocuments())
                              .WhenRemoved(_ => RefreshDocuments());

        RefreshDocuments();
        SetActiveDocument(_mainViewModel.SelectedEditor);
    }

    public void Detach()
    {
        if (_mainViewModel is null)
            return;

        _mainViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
        _mainViewModel = null;
    }

    public void RefreshDocuments()
    {
        var activeDocument = _mainViewModel?.SelectedEditor;

        foreach (var document in Documents)
            document.Dispose();

        Documents.Clear();

        if (_mainViewModel is not null)
        {
            foreach (var editor in _mainViewModel.Editors)
                Documents.Add(new FolderExplorerDocumentItemViewModel(editor));
        }

        OnPropertyChanged(nameof(HasDocuments));
        SetActiveDocument(activeDocument);
    }

    public void SetActiveDocument(EditorViewModel? document)
    {
        _isSynchronizingSelection = true;
        try
        {
            SelectedDocument = document is null
                ? null
                : Documents.FirstOrDefault(x => ReferenceEquals(x.Document, document));
        }
        finally
        {
            _isSynchronizingSelection = false;
        }
    }

    partial void OnSelectedDocumentChanged(FolderExplorerDocumentItemViewModel? value)
    {
        if (_isSynchronizingSelection || value is null || _mainViewModel is null)
            return;

        _mainViewModel.SelectedEditor = value.Document;
    }

    [RelayCommand]
    private void Refresh()
    {
        RefreshDocuments();
    }

    [RelayCommand]
    private void AddEditor()
    {
        _mainViewModel?.AddEditorCommand.Execute(null);
    }

    [RelayCommand]
    private void LoadDocument()
    {
        if (_mainViewModel is null || _fileDocumentService is null)
            return;

        var editor = _fileDocumentService.LoadDocument();
        if (editor is null)
            return;

        _mainViewModel.Editors.Add(editor);
        _mainViewModel.SelectedEditor = editor;
    }

    [RelayCommand]
    private void SaveDocument(FolderExplorerDocumentItemViewModel? item)
    {
        if (item is null || _fileDocumentService is null)
            return;

        _fileDocumentService.SaveDocument(item.Document);
    }

    [RelayCommand]
    private void SaveDocumentAs(FolderExplorerDocumentItemViewModel? item)
    {
        if (item is null || _fileDocumentService is null)
            return;

        _fileDocumentService.SaveDocumentAs(item.Document);
    }

    [RelayCommand]
    private void CloseDocument(FolderExplorerDocumentItemViewModel? item)
    {
        if (item is null || _mainViewModel is null)
            return;

        _mainViewModel.CloseEditorCommand.Execute(item.Document.Id);
    }

    private void OnMainViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedEditor))
        {
            SetActiveDocument(_mainViewModel?.SelectedEditor);
        }
    }

    public void Dispose()
    {
        Detach();
    }
}

public sealed partial class FolderExplorerDocumentItemViewModel : ObservableObject, IDisposable
{
    private readonly EditorViewModel _document;
    private bool _isRefreshing;

    public FolderExplorerDocumentItemViewModel(EditorViewModel document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        _document.PropertyChanged += OnDocumentPropertyChanged;
        RefreshFromDocument();
    }

    public EditorViewModel Document => _document;

    [ObservableProperty]
    public partial string DocumentName { get; set; } = string.Empty;

    public string DisplayPath => DocumentName;

    public string ToolTipText => DocumentName;

    public void RefreshFromDocument()
    {
        _isRefreshing = true;
        try
        {
            DocumentName = _document.Name ?? string.Empty;
        }
        finally
        {
            _isRefreshing = false;
        }

        OnPropertyChanged(nameof(DisplayPath));
        OnPropertyChanged(nameof(ToolTipText));
    }

    partial void OnDocumentNameChanged(string value)
    {
        if (_isRefreshing)
            return;

        _document.Name = value;
        RefreshFromDocument();
    }

    private void OnDocumentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditorViewModel.Name))
        {
            RefreshFromDocument();
        }
    }

    public void Dispose()
    {
        _document.PropertyChanged -= OnDocumentPropertyChanged;
    }
}
