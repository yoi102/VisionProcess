using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Windows;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Extensions;
using VisionProcess.Core.Mvvm;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Models
{
    public partial class OperationModel : ObservableObject
    {
        private readonly Guid id;
        private IOperator? @operator;
        private bool isSelected = false;
        private Point location;
        private Size size;

        public OperationModel()
        {
            id = Guid.NewGuid();
        }

        [JsonConstructor]
        public OperationModel(IOperator @operator, NodifyObservableCollection<ConnectorModel> inputs,
            NodifyObservableCollection<ConnectorModel> outputs, Point location, bool isSelected, Size size, Guid id)
        {
            this.@operator = @operator;
            this.@operator.Executed += Operation_Executed;
            this.isSelected = isSelected;
            this.location = location;
            this.size = size;
            this.id = id;

            foreach (var input in inputs)
            {
                var type = PropertyMisc.GetType(@operator, input.ValuePath)!;
                Inputs.Add(new ConnectorModel(input.Title, type,
                    input.ValuePath, input.IsInput, input.OwnerId, this)
                { Anchor = input.Anchor, IsConnected = input.IsConnected });
            }
            foreach (var output in outputs)
            {
                var type = PropertyMisc.GetType(@operator, output.ValuePath)!;
                Outputs.Add(new ConnectorModel(output.Title, type,
                    output.ValuePath, output.IsInput, output.OwnerId, this)
                { Anchor = output.Anchor, IsConnected = output.IsConnected });
            }
        }

        public event EventHandler? OperatorExecuted;

        public Guid Id => id;
        public NodifyObservableCollection<ConnectorModel> Inputs { get; } = [];

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

        public IOperator? Operator
        {
            get => @operator;
            init
            {
                if (@operator != null)
                    @operator.Executed -= Operation_Executed;
                SetProperty(ref @operator, value);
                if (value is null)
                {
                    Inputs.Clear();
                    Outputs.Clear();
                    return;
                }
                value.Executed += Operation_Executed;
                value.Name = value.GetType().Name.Replace("ViewModel", "");
                var attributes = (DefaultToolConnectorAttribute[])value.GetType().GetCustomAttributes(typeof(DefaultToolConnectorAttribute), false);
                foreach (var item in attributes)
                {
                    var type = PropertyMisc.GetType(value, item.Path) ?? throw new ArgumentException("Error, DefaultToolConnectorAttribute setting  error");
                    if (item.IsInput)
                    {
                        Inputs.Add(new ConnectorModel(item.Title, type, item.Path, true, Id, this));
                    }
                    else
                    {
                        Outputs.Add(new ConnectorModel(item.Title, type, item.Path, false, Id, this));
                    }
                }
            }
        }

        public NodifyObservableCollection<ConnectorModel> Outputs { get; } = [];

        public Size Size
        {
            get => size;
            set => SetProperty(ref size, value);
        }

        public void RunOperatorByConnection()
        {
            //当全部已经链接的Inputs被赋值后才运行
            var connectedInputsCount = Inputs.Count(x => x.IsConnected);
            var assignedInputsCount = Inputs.Count(x => x.HadAssigned);
            if (connectedInputsCount == assignedInputsCount)
            {
                Operator?.Execute();
                Inputs.ForEach(x => x.HadAssigned = false);
            }
        }

        [property: JsonIgnore]
        [RelayCommand]
        private void AddIO()
        {
        }

        private void Operation_Executed(object? sender, EventArgs e)
        {
            OperatorExecuted?.Invoke(this, e);
        }

        [property: JsonIgnore]
        [RelayCommand]
        private void RemoveIO()
        {
        }
    }
}