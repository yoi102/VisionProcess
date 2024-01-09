using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Linq;
using VisionProcess.Core.Extentions;
using VisionProcess.Core.Mvvm;
using VisionProcess.Models;

namespace VisionProcess.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            Editors = new NodifyObservableCollection<EditorViewModel>();

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
            foreach (var editor in editors)
            {
                editor.OnOpenInnerProcess += OnOpenInnerProcess;
            }
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
                editor.OnOpenInnerProcess += OnOpenInnerProcess;
            })
            .WhenRemoved((editor) =>
            {
                editor.OnOpenInnerProcess -= OnOpenInnerProcess;
                var childEditors = Editors.Where(ed => ed.Parent == editor).ToArray();
                childEditors.ForEach(ed => Editors.Remove(ed));
            });
        }

        private void OnOpenInnerProcess(EditorViewModel parentEditor, ProcessModel process)
        {
            var editor = Editors.FirstOrDefault(e => e.Process == process);
            if (editor != null)
            {
                SelectedEditor = editor;
            }
            else
            {
                var childEditor = new EditorViewModel
                {
                    Parent = parentEditor,
                    Process = process,
                    Name = $"[Inner] Editor {Editors.Count + 1}"
                };
                Editors.Add(childEditor);
            }
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

        [property: JsonIgnore]
        [RelayCommand(CanExecute = nameof(CanCloseEditor))]
        private void CloseEditor(Guid id)
        {
            Editors.RemoveOne(editor => editor.Id == id);
        }

        private bool CanCloseEditor()
        {
            return Editors.Count > 0 && SelectedEditor != null;
        }

        #endregion Commands
    }
}