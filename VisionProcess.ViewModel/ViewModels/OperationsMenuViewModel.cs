using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Common.Primitives;
using VisionProcess.Runtime;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.Tooling;

namespace VisionProcess.ViewModel.ViewModels
{
    public partial class OperationsMenuViewModel : ObservableObject, IOperationsMenu
    {
        private readonly IWindowDialogService? _windowDialogService;
        private readonly IOperatorFactory _operatorFactory;
        private readonly IOperatorViewModelFactory _viewModelFactory;

        public OperationsMenuViewModel(ProcessModel processModel, IOperatorFactory operatorFactory, IOperatorViewModelFactory viewModelFactory, IWindowDialogService? _windowDialogService = null)
        {
            AvailableOperations = ToolRegistry.ToolTypes.Select(ToolRegistry.GetToolName).ToList();
            this.processModel = processModel;
            _operatorFactory = operatorFactory;
            _viewModelFactory = viewModelFactory;
            this._windowDialogService = _windowDialogService;
        }

        [ObservableProperty]
        private VpPoint _location;

        [ObservableProperty]
        private bool _isVisible;

        private readonly ProcessModel processModel;

        public IEnumerable<string> AvailableOperations { get; }

        public void OpenAt(VpPoint targetLocation)
        {
            Close();
            Location = targetLocation;
            IsVisible = true;
        }

        public void Close()
        {
            IsVisible = false;
        }

        [RelayCommand]
        private void CreateOperation(string operationName)
        {
            var instance = _operatorFactory.Create(operationName);
            instance.Name = operationName;
            var viewModel = _viewModelFactory.Create(instance);
            processModel.Operations.Add(new OperationModel()
            {
                Operator = instance,
                OperatorViewModel = viewModel,
                Location = Location,
                WindowDialogService = _windowDialogService
            });
            IsVisible = false;
        }
    }
}
