using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VisionProcess.Core.Extensions;

namespace VisionProcess.Models
{
    public class ConnectorModel : ObservableObject
    {
        private readonly bool isInput;

        private readonly OperationModel? owner;
        private readonly string valueName;
        private Point anchor;

        private bool isConnected = false;
        private Guid ownerGuid;

        private string title;

        private string valuePath;

        private Type valueType;

        public ConnectorModel(string title, Type valueType, string valuePath, bool isInput, Guid ownerId, OperationModel operationModel)
        {
            this.title = title;
            this.valueType = valueType;
            this.valuePath = valuePath;
            this.isInput = isInput;
            this.ownerGuid = ownerId;
            var p = valuePath.Split(".");
            valueName = p[^1];
            owner = operationModel;
            if (owner.Operator == null)
                throw new ArgumentNullException(nameof(owner.Operator));
            if (isInput)
            {
                owner.Operator.Inputs.PropertyChanged += Connector_PropertyChanged;
            }
            else
            {
                owner.Operator.Outputs.PropertyChanged += Connector_PropertyChanged;
            }
            //当前节点被移除时取消订阅，以免内存泄露
            owner.Inputs.WhenRemoved(x =>
            {
                if (x == this)
                {
                    owner.Operator.Inputs.PropertyChanged -= Connector_PropertyChanged;
                }
            });
            owner.Outputs.WhenRemoved(x =>
            {
                if (x == this)
                {
                    owner.Operator.Outputs.PropertyChanged -= Connector_PropertyChanged;
                }
            });
        }

        [JsonConstructor]
        public ConnectorModel(string title, Type valueType, string valuePath, bool isInput, Guid ownerGuid)
        {
            //由于反序列化时会重新 new，所以只需基础信息
            this.title = title;
            this.valueType = valueType;
            this.valuePath = valuePath;
            this.isInput = isInput;
            this.ownerGuid = ownerGuid;
            var p = valuePath.Split(".");
            valueName = p[^1];
        }

        public Point Anchor
        {
            get => anchor;
            set => SetProperty(ref anchor, value);
        }

        [JsonIgnore]
        public bool IsAssigned { get; set; } = false;

        public bool IsConnected
        {
            get => isConnected;
            set => SetProperty(ref isConnected, value);
        }

        public bool IsInput
        {
            get => isInput;
        }

        [JsonIgnore]
        public OperationModel Owner => owner!;

        public Guid OwnerGuid
        {
            get => ownerGuid;
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        [JsonIgnore]
        public object? Value
        {
            get
            {
                if (owner is null || owner.Operator is null)
                {
                    return null;
                }
                return PropertyMisc.GetValue(owner.Operator, valuePath);
            }
        }

        [JsonIgnore]
        public List<ConnectorModel> ValueObservers { get; } = new();

        public string ValuePath
        {
            get { return valuePath; }
            protected set { SetProperty(ref valuePath, value); }
        }

        [JsonIgnore]
        public Type ValueType
        {
            get => valueType;
            protected set => SetProperty(ref valueType, value);
        }

        public void SetInputValue(object? value)
        {
            if (!isInput)
                return;
            IsAssigned = true;
            PropertyMisc.TrySetValue(owner!.Operator!, ValuePath, value);
            //当全部已经链接的Inputs被赋值后才运行
            var connectedInputsCount = owner!.Inputs.Count(x => x.IsConnected);
            var assignedInputsCount = owner.Inputs.Count(x => x.IsAssigned);
            if (connectedInputsCount == assignedInputsCount)
            {
                owner.Operator?.Execute();
                owner.Inputs.ForEach(x => x.IsAssigned = false);
            }
        }

        private void Connector_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == valueName)
            {
                OnPropertyChanged(nameof(Value));
            }
        }
    }
}