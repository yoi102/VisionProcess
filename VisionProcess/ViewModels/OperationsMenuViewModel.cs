using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using VisionProcess.Core.ToolBase;
using VisionProcess.Models;
using VisionProcess.Services;

namespace VisionProcess.ViewModels
{
    public partial class OperationsMenuViewModel : ObservableObject
    {
        public OperationsMenuViewModel(ProcessModel processModel)
        {
            List<string> list = new();
            foreach (var itemType in InfoService.Instance.ToolViewModelTypes!)//遍历所有类型进行查找
            {
                list.Add(itemType.Name.Replace("ViewModel", string.Empty));
            }
            AvailableOperations = list;
            this.processModel = processModel;
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
            //前提，需要规范命名
            var type = InfoService.Instance.ToolViewModelTypes!.FirstOrDefault(t => t.Name == operationName + "ViewModel") ??
                   throw new ArgumentNullException($"{operationName} + ViewModel");

            //var type = App.ToolsAssembly.GetType("VisionProcess.Tools.ViewModels." + operationName + "ViewModel");

            var instance = InfoService.Instance.Services!.GetService(type);
            //var instance = Activator.CreateInstance(type!);

            processModel.Operations.Add(new OperationModel() { Operator = (IOperator)instance!, Location = Location });
            IsVisible = false;
        }
    }
}