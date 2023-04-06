using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using VisonProcess.Core.Extentions;
using VisonProcess.Core.Mvvm;
using VisonProcess.Tools.ViewModels;
using VisonProcess.Models;
using System.Collections.Generic;
using System.Reflection;

namespace VisonProcess.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
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

            Editors.Add(new EditorViewModel
            {
                Name = $"Editor {Editors.Count + 1}"
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
        private bool _autoSelectNewEditor = true;

        [ObservableProperty]
        private EditorViewModel? _selectedEditor;

        public NodifyObservableCollection<EditorViewModel> Editors { get; } = new NodifyObservableCollection<EditorViewModel>();

        #endregion Properties

        #region Commands

        [RelayCommand]
        private void AddEditor()
        {
            Editors.Add(new EditorViewModel
            {
                Name = $"Editor {Editors.Count + 1}"
            });
        }

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