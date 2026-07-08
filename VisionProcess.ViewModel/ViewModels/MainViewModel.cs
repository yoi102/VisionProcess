using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Linq;
using VisionProcess.Core.Extensions;
using VisionProcess.Core.Mvvm;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;
using VisionProcess.ViewModel.ViewModels.Toolboxes;

namespace VisionProcess.ViewModel.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDialogService? dialogService;

        public MainViewModel(
            IDialogService? dialogService = null,
            FolderExplorerToolboxViewModel? folderExplorer = null,
            PropertiesToolboxViewModel? properties = null)
        {
            this.dialogService = dialogService;
            FolderExplorer = folderExplorer;
            Properties = properties;
            Editors = [];
            Init();
            Editors.Add(new EditorViewModel(dialogService)
            {
                Name = $"Editor {Editors.Count + 1}"
            });
        }

        [JsonConstructor]
        public MainViewModel(
            NodifyObservableCollection<EditorViewModel> editors,
            bool autoSelectNewEditor,
            EditorViewModel? selectedEditor,
            FolderExplorerToolboxViewModel? folderExplorer = null,
            PropertiesToolboxViewModel? properties = null)
        {
            Editors = editors;
            this.autoSelectNewEditor = autoSelectNewEditor;
            if (selectedEditor is not null)
            {
                this.selectedEditor = editors.First(x => x.Id == selectedEditor.Id);
            }
            FolderExplorer = folderExplorer;
            Properties = properties;
            Init();
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

        [property: JsonIgnore]
        [RelayCommand]
        private void AddEditor()
        {
            Editors.Add(new EditorViewModel(dialogService)
            {
                Name = $"Editor {Editors.Count + 1}"
            });
            SelectedEditor = Editors[^1];
        }

        private bool CanCloseEditor()
        {
            return Editors.Count > 0 && SelectedEditor != null;
        }

        [property: JsonIgnore]
        [RelayCommand(CanExecute = nameof(CanCloseEditor))]
        private void CloseEditor(Guid id)
        {
            Editors.RemoveOne(editor => editor.Id == id);
        }
        #endregion Commands
    }
}
