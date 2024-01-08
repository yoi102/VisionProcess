using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Windows;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Extentions;
using VisionProcess.Core.Mvvm;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Models
{
    public partial class OperationModel : ObservableObject
    {
        private readonly Guid id;
        private bool isSelected = false;
        private Point location;
        private IOperation? operation;
        private Size size;

        public OperationModel()
        {
            id = Guid.NewGuid();
        }

        [JsonConstructor]
        public OperationModel(IOperation operation, NodifyObservableCollection<ConnectorModel> inputs,
            NodifyObservableCollection<ConnectorModel> outputs, Point location, bool isSelected, Size size, Guid id)
        {
            this.operation = operation;
            this.operation.Executed += Operation_Executed;
            this.isSelected = isSelected;
            this.location = location;
            this.size = size;
            this.id = id;

            foreach (var input in inputs)
            {
                var type = PropertyMisc.GetType(operation, input.ValuePath);
                //var v = PropertyMisc.GetValue(operation, input.ValuePath);
                Inputs.Add(
                    new ConnectorModel(input.Title, type,
                    input.ValuePath, input.IsInput, input.OwnerGuid)
                    { Anchor = input.Anchor, IsConnected = input.IsConnected });
            }
            foreach (var output in outputs)
            {
                var type = PropertyMisc.GetType(operation, output.ValuePath);
                //var v = PropertyMisc.GetValue(operation, output.ValuePath);
                Outputs.Add(
                    new ConnectorModel(output.Title, type,
                    output.ValuePath, output.IsInput, output.OwnerGuid)
                    { Anchor = output.Anchor, IsConnected = output.IsConnected });
            }
        }

        public event EventHandler? OperationExecuted;

        public Guid Id => id;
        public NodifyObservableCollection<ConnectorModel> Inputs { get; } = new();

        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public Point Location
        {
            get => location;
            set => SetProperty(ref location, value);
        }

        public IOperation? Operation
        {
            get => operation;
            init
            {
                if (value is not null)
                {
                    value.Executed += Operation_Executed;
                    value.Name = value.GetType().Name.Replace("ViewModel", "");
                    var attributes = (DefaultToolConnectorAttribute[])value.GetType().GetCustomAttributes(typeof(DefaultToolConnectorAttribute), false);
                    foreach (var item in attributes)
                    {
                        var type = PropertyMisc.GetType(value, item.Path);
                        //var v = PropertyMisc.GetValue(value, item.Path);
                        if (type == null)
                        {
                            throw new ArgumentException("Error, DefaultToolConnectorAttribute setting  error");
                        }
                        if (item.IsInput)
                        {
                            Inputs.Add(new ConnectorModel(item.Title, type, item.Path, true, Id));
                        }
                        else
                        {
                            Outputs.Add(new ConnectorModel(item.Title, type, item.Path, false, Id));
                        }
                    }
                }
                else
                {
                    Inputs.Clear();
                    Outputs.Clear();
                }
                SetProperty(ref operation, value);
            }
        }

        public NodifyObservableCollection<ConnectorModel> Outputs { get; } = new();

        public Size Size
        {
            get => size;
            set => SetProperty(ref size, value);
        }

        [property: JsonIgnore]
        [RelayCommand]
        private void AddIO()
        {
        }

        [property: JsonIgnore]
        [RelayCommand]
        private void RemoveIO()
        {
        }

        private void Operation_Executed(object? sender, EventArgs e)
        {
            OperationExecuted?.Invoke(this, e);
        }
    }
}