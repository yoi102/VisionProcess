using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Common.Primitives;
using VisionProcess.Runtime.Models;
using VisionProcess.ViewModel.Tooling;

namespace VisionProcess.ViewModel.ViewModels
{
    public partial class OperationsMenuViewModel : ObservableObject
    {
        private readonly IOperatorFactory _operatorFactory;
        private readonly IOperatorViewModelFactory _viewModelFactory;

        public OperationsMenuViewModel(
            ProcessModel processModel,
            IOperatorCatalog operatorCatalog,
            IOperatorFactory operatorFactory,
            IOperatorViewModelFactory viewModelFactory)
        {
            AvailableOperations = operatorCatalog.Operators;
            this.processModel = processModel;
            _operatorFactory = operatorFactory;
            _viewModelFactory = viewModelFactory;
        }

        [ObservableProperty]
        private VpPoint _location;

        [ObservableProperty]
        private bool _isVisible;

        private readonly ProcessModel processModel;

        public IReadOnlyList<OperatorDescriptor> AvailableOperations { get; }

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
        private void CreateOperation(OperatorDescriptor descriptor)
        {
            var instance = _operatorFactory.Create(descriptor.Id);
            instance.Name = descriptor.DisplayName;
            var viewModel = _viewModelFactory.Create(instance);
            processModel.Operations.Add(new OperationModel()
            {
                Operator = instance,
                OperatorViewModel = viewModel,
                Location = Location
            });
            IsVisible = false;
        }
    }
}
