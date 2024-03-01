using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using VisionProcess.Core.Extentions;

namespace VisionProcess.Models
{
    public class ConnectorModel : ObservableObject
    {
        private readonly bool isInput;

        private readonly OperationModel? owner;
        private readonly string valueName;
        private Point anchor;

        private bool isConnected = false;
        private Guid ownerId;

        private string title;

        private string valuePath;

        private Type valueType;

        public ConnectorModel(string title, Type valueType, string valuePath, bool isInput, Guid ownerId, OperationModel operationModel)
        {
            this.title = title;
            this.valueType = valueType;
            this.valuePath = valuePath;
            this.isInput = isInput;
            this.ownerId = ownerId;
            var p = valuePath.Split(".");
            valueName = p[^1];
            owner = operationModel;
            if (owner.Operator == null)
                throw new ArgumentNullException(nameof(owner.Operator));
            if (isInput)
            {
                owner.Operator.Inputs.PropertyChanged += Inputs_PropertyChanged;
            }
            else
            {
                owner.Operator.Outputs.PropertyChanged += Outputs_PropertyChanged;
            }
            //当前节点被移除时取消订阅，以免内存泄露
            owner.Inputs.WhenRemoved(x =>
            {
                if (x == this)
                {
                    owner.Operator.Inputs.PropertyChanged -= Inputs_PropertyChanged;

                }
            });
            owner.Outputs.WhenRemoved(x =>
            {
                if (x == this)
                {
                    owner.Operator.Outputs.PropertyChanged -= Outputs_PropertyChanged;

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
            this.ownerId = ownerGuid;
            var p = valuePath.Split(".");
            valueName = p[^1];
        }

        public Point Anchor
        {
            get => anchor;
            set => SetProperty(ref anchor, value);
        }

        public bool IsConnected
        {
            get => isConnected;
            set => SetProperty(ref isConnected, value);
        }

        public bool IsInput
        {
            get => isInput;
        }

        public Guid OwnerId
        {
            get => ownerId;
            set => SetProperty(ref ownerId, value);
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

        private void Inputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == valueName)
            {
                OnPropertyChanged(nameof(Value));
            }
        }

        private void Outputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == valueName)
            {
                OnPropertyChanged(nameof(Value));
            }
        }
    }
}