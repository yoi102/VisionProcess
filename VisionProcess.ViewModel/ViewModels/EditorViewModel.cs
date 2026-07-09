using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.Tooling;

namespace VisionProcess.ViewModel.ViewModels
{
    public partial class EditorViewModel : ObservableObject
    {
        private string? name;

        private ProcessModel process;

        public EditorViewModel(IWindowDialogService _windowDialogService, IOperatorFactory operatorFactory, IOperatorViewModelFactory viewModelFactory)
        {
            process = new ProcessModel();
            OperationsMenu = new OperationsMenuViewModel(process, operatorFactory, viewModelFactory, _windowDialogService);
            process.OperationsMenu = OperationsMenu;
            Id = Guid.NewGuid();
        }

        public EditorViewModel(ProcessModel process, Guid id, string? name)
        {
            this.process = process;
            Id = id;
            this.name = name;
            var operatorFactory = Ioc.Default.GetService<IOperatorFactory>() ?? new OperatorFactory();
            var viewModelFactory = Ioc.Default.GetService<IOperatorViewModelFactory>() ?? new OperatorViewModelFactory();
            OperationsMenu = new OperationsMenuViewModel(process, operatorFactory, viewModelFactory);
            process.OperationsMenu = OperationsMenu;
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

        public ProcessModel Process
        {
            get => process;
            set => SetProperty(ref process, value);
        }

        [RelayCommand]
        private void Disconnect(ConnectionModel connection)
        {
            connection.Input!.IsConnected = false;
            connection.Output!.IsConnected = false;
            Process.Connections.Remove(connection);
        }
    }
}