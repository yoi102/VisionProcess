using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using VisionProcess.Models;

namespace VisionProcess.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        private string? name;

        private ProcessModel process;

        public EditorViewModel()
        {
            process = new ProcessModel();
        }

        [JsonConstructor]
        public EditorViewModel(ProcessModel process)
        {
            this.process = process;
        }

        public event Action<EditorViewModel, ProcessModel>? OnOpenInnerProcess;

        public Guid Id { get; } = Guid.NewGuid();

        public string? Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        [JsonIgnore]//暂时
        public EditorViewModel? Parent { get; set; }

        public ProcessModel Process
        {
            get => process;
            set => SetProperty(ref process, value);
        }

        [property: JsonIgnore]
        [RelayCommand]
        private void Disconnect(ConnectionModel connection)
        {
            connection.Input!.IsConnected = false;
            connection.Output!.IsConnected = false;
            Process.Connections.Remove(connection);
        }

        [property: JsonIgnore]
        [RelayCommand]
        private void OpenProcess(ProcessModel process)
        {
            OnOpenInnerProcess?.Invoke(this, process);
        }
    }
}