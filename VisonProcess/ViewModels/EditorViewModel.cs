using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisonProcess.Core.Mvvm;

namespace VisonProcess.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        public event Action<EditorViewModel, ProcessModel>? OnOpenInnerProcess;

        public EditorViewModel? Parent { get; set; }

        public EditorViewModel()
        {
            Process = new ProcessModel();
        }



        [RelayCommand]
        void OpenProcess(ProcessModel process)
        {
            OnOpenInnerProcess?.Invoke(this, process);
        }


        public Guid Id { get; } = Guid.NewGuid();

        private ProcessModel _process = default!;
        public ProcessModel Process
        {
            get => _process;
            set => SetProperty(ref _process, value);
        }

        private string? _name;
        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }








    }
}
