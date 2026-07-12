using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Runtime.Models;
using VisionProcess.ViewModel.Tooling;
using VisionProcess.ViewModel.ViewServices;

namespace VisionProcess.ViewModel.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        private string? name;

        private readonly ProcessModel process;

        private readonly IWindowDialogService windowDialogService;

        public EditorViewModel(
            ProcessModel process,
            Guid id,
            string? name,
            IWindowDialogService windowDialogService,
            IOperatorFactory operatorFactory,
            IOperatorViewModelFactory viewModelFactory)
        {
            this.process = process ?? throw new ArgumentNullException(nameof(process));
            this.windowDialogService = windowDialogService ?? throw new ArgumentNullException(nameof(windowDialogService));
            Id = id;
            this.name = name;
            OperationsMenu = new OperationsMenuViewModel(process, operatorFactory, viewModelFactory);
            AttachViewModels(viewModelFactory);
        }

        private void AttachViewModels(IOperatorViewModelFactory viewModelFactory)
        {
            foreach (var operation in process.Operations)
            {
                if (operation.Operator is not null && operation.OperatorViewModel is null)
                {
                    operation.OperatorViewModel = viewModelFactory.Create(operation.Operator);
                }
            }
        }

        public Guid Id { get; }

        public string? Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public EditorViewModel? Parent { get; set; }

        [ObservableProperty]
        public partial string? FilePath { get; set; }

        public OperationsMenuViewModel OperationsMenu { get; }

        public ProcessModel Process => process;

        [RelayCommand]
        private void Disconnect(ConnectionModel connection)
        {
            connection.Input!.IsConnected = false;
            connection.Output!.IsConnected = false;
            Process.Connections.Remove(connection);
        }

        [RelayCommand]
        private void OpenIOConnector(OperationModel operation)
        {
            windowDialogService.OpenIOConnectorDialog(operation);
        }
    }
}
