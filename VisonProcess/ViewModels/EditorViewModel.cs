using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using VisonProcess.Core.Mvvm;
using VisonProcess.Models;

namespace VisonProcess.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        public event Action<EditorViewModel, ProcessModel>? OnOpenInnerProcess;

        //用于Group
        public EditorViewModel? Parent { get; set; }

        public EditorViewModel()
        {
            Process = new ProcessModel();
        }

        [RelayCommand]
        private void OpenProcess(ProcessModel process)
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

        [RelayCommand]
        private void Disconnect(ConnectionModel connection)
        {
            connection.Input.IsConnected = false;
            connection.Output.IsConnected = false;
            Process.Connections.Remove(connection);
        }
    }
}