using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Linq;
using VisionProcess.Core.Extensions;
using VisionProcess.Core.Mvvm;

namespace VisionProcess.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            Editors = [];
            Init();
            Editors.Add(new EditorViewModel
            {
                Name = $"Editor {Editors.Count + 1}"
            });
        }

        [JsonConstructor]
        public MainViewModel(NodifyObservableCollection<EditorViewModel> editors,
            bool autoSelectNewEditor, EditorViewModel? selectedEditor)
        {
            Editors = editors;
            this.autoSelectNewEditor = autoSelectNewEditor;
            if (selectedEditor is not null)
            {
                this.selectedEditor = editors.First(x => x.Id == selectedEditor.Id);
            }
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
            })
            .WhenRemoved((editor) =>
            {
                var childEditors = Editors.Where(ed => ed.Parent == editor).ToArray();
                childEditors.ForEach(ed => Editors.Remove(ed));
            });
        }

        #region Properties

        [ObservableProperty]
        private bool autoSelectNewEditor = true;

        [ObservableProperty]
        private EditorViewModel? selectedEditor;

        public NodifyObservableCollection<EditorViewModel> Editors { get; }

        #endregion Properties

        #region Commands

        [property: JsonIgnore]
        [RelayCommand]
        private void AddEditor()
        {
            Editors.Add(new EditorViewModel
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