using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Runtime;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;
using VisionProcess.Tools.Tooling;

namespace VisionProcess.ViewModel.ViewModels
{
    public partial class OperationsMenuViewModel : ObservableObject, IOperationsMenu
    {
        private readonly IDialogService? dialogService;

        public OperationsMenuViewModel(ProcessModel processModel, IDialogService? dialogService = null)
        {
            AvailableOperations = ToolRegistry.ToolTypes.Select(ToolRegistry.GetToolName).ToList();
            this.processModel = processModel;
            this.dialogService = dialogService;
        }

        [ObservableProperty]
        private Point _location;

        [ObservableProperty]
        private bool _isVisible;

        private readonly ProcessModel processModel;

        public IEnumerable<string> AvailableOperations { get; }

        public void OpenAt(Point targetLocation)
        {
            Close();
            Location = targetLocation;
            IsVisible = true;
        }

        public void Close()
        {
            IsVisible = false;
        }

        [property: JsonIgnore]
        [RelayCommand]
        private void CreateOperation(string operationName)
        {
            var instance = OperatorFactory.Create(operationName);
            instance.Name = operationName;
            processModel.Operations.Add(new OperationModel() { Operator = instance, Location = Location, DialogService = dialogService });
            IsVisible = false;
        }
    }
}
