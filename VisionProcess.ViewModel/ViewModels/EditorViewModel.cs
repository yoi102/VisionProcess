using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;

namespace VisionProcess.Application.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        private string? name;

        private ProcessModel process;

        public EditorViewModel(IDialogService? dialogService = null)
        {
            process = new ProcessModel();
            OperationsMenu = new OperationsMenuViewModel(process, dialogService);
            process.OperationsMenu = OperationsMenu;
            Id = Guid.NewGuid();
        }

        [JsonConstructor]
        public EditorViewModel(ProcessModel process, Guid id, string? name)
        {
            this.process = process;
            Id = id;
            this.name = name;
            OperationsMenu = new OperationsMenuViewModel(process);
            process.OperationsMenu = OperationsMenu;
        }

        public Guid Id { get; }

        public string? Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public EditorViewModel? Parent { get; set; }

        [JsonIgnore]
        [ObservableProperty]
        public partial string? FilePath { get; set; }

        [JsonIgnore]
        public OperationsMenuViewModel OperationsMenu { get; }

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
    }
}