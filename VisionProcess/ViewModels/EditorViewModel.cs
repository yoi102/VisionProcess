using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Xml.Linq;
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
            Id = Guid.NewGuid();
        }

        [JsonConstructor]
        public EditorViewModel(ProcessModel process, Guid id, string? name)
        {
            this.process = process;
            Id = id;
            this.name = name;
        }

        public Guid Id { get; }

        public string? Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

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
    }
}