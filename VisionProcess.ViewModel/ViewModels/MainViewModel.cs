using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using VisionProcess.Common.Extensions;
using VisionProcess.Common.Mvvm;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.Tooling;
using VisionProcess.ViewModel.ViewModels.Toolboxes;
using VisionProcess.ViewModel.ViewServices;

namespace VisionProcess.ViewModel.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ICultureSettingService cultureSettingService = null!;
        private readonly IThemeSettingService themeSettingService = null!;
        private readonly IDialogService dialogService = null!;
        private readonly ISnackbarService snackbarService = null!;
        private readonly IWindowDialogService _windowDialogService = null!;
        private readonly IOperatorFactory _operatorFactory = null!;
        private readonly IOperatorViewModelFactory _viewModelFactory = null!;

        public MainViewModel(
             ICultureSettingService cultureSettingService,
            IThemeSettingService themeSettingService,
            IDialogService dialogService,
            ISnackbarService snackbarService,
            IWindowDialogService _windowDialogService,
            IOperatorFactory operatorFactory,
            IOperatorViewModelFactory viewModelFactory,
            FolderExplorerToolboxViewModel folderExplorer,
            PropertiesToolboxViewModel properties)
        {
            this.cultureSettingService = cultureSettingService;
            this.themeSettingService = themeSettingService;
            this.dialogService = dialogService;
            this.snackbarService = snackbarService;
            this._windowDialogService = _windowDialogService;
            _operatorFactory = operatorFactory;
            _viewModelFactory = viewModelFactory;
            FolderExplorer = folderExplorer;
            Properties = properties;
            Editors = [];
            Init(); 

            var editor = Ioc.Default.GetRequiredService<EditorViewModel>();
            editor.Name = $"Editor {Editors.Count + 1}";
            Editors.Add(editor);
            IsDarkTheme = themeSettingService.IsDarkTheme;
            CurrentCultureLCID = cultureSettingService.GetCurrentCultureLCID();
        }

        private void Init()
        {
            Editors.WhenAdded((editor) =>
            {
                if (AutoSelectNewEditor || Editors.Count == 1)
                {
                    SelectedEditor = editor;
                }
                FolderExplorer?.RefreshDocuments();
            })
            .WhenRemoved((editor) =>
            {
                var childEditors = Editors.Where(ed => ed.Parent == editor).ToArray();
                childEditors.ForEach(ed => Editors.Remove(ed));

                if (SelectedEditor == editor)
                {
                    SelectedEditor = Editors.FirstOrDefault();
                }

                FolderExplorer?.RefreshDocuments();
            });

            FolderExplorer?.Attach(this);
            Properties?.Attach(SelectedEditor);
        }

        partial void OnSelectedEditorChanged(EditorViewModel? value)
        {
            FolderExplorer?.SetActiveDocument(value);
            Properties?.Attach(value);
        }

        #region Properties

        [ObservableProperty]
        private bool autoSelectNewEditor = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedOperation))]
        private EditorViewModel? selectedEditor;

        public OperationModel? SelectedOperation => SelectedEditor?.Process.SelectedOperation;

        public NodifyObservableCollection<EditorViewModel> Editors { get; }

        public FolderExplorerToolboxViewModel? FolderExplorer { get; }
        public PropertiesToolboxViewModel? Properties { get; }

        #endregion Properties

        #region Commands

        [RelayCommand]
        private void AddEditor()
        {
            Editors.Add(new EditorViewModel(_windowDialogService, _operatorFactory, _viewModelFactory)
            {
                Name = $"Editor {Editors.Count + 1}"
            });
            SelectedEditor = Editors[^1];
        }

        private bool CanCloseEditor()
        {
            return Editors.Count > 0 && SelectedEditor != null;
        }

        [RelayCommand(CanExecute = nameof(CanCloseEditor))]
        private void CloseEditor(Guid id)
        {
            Editors.RemoveOne(editor => editor.Id == id);
        }
        #endregion Commands


        #region TitleBar

        [ObservableProperty]
        public partial bool Topmost { get; set; }

        [ObservableProperty]
        public partial int CurrentCultureLCID { get; set; }

        [ObservableProperty]
        public partial bool IsDarkTheme { get; set; }

        partial void OnIsDarkThemeChanged(bool value)
        {
            themeSettingService.ApplyThemeLightDark(value);
        }

        [RelayCommand]
        private void ChangeCulture(string lcidString)
        {
            if (!int.TryParse(lcidString, out var lcid))
                return;
            CurrentCultureLCID = lcid;
            cultureSettingService.ChangeCulture(lcid);
        }

        [RelayCommand]
        private void ChangeTopmost()
        {
            Topmost = !Topmost;
        }

        #endregion TitleBar
    }
}
