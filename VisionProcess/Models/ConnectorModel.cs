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

        private readonly OperationModel owner;
        private readonly string valueName;
        private Point anchor;

        private bool isConnected = false;
        private Guid ownerGuid;

        private string title;

        private string valuePath;

        private Type valueType;

        public ConnectorModel(string title, Type valueType, string valuePath, bool isInput, Guid ownerGuid, OperationModel operationModel)
        {
            this.title = title;
            this.valueType = valueType;
            this.valuePath = valuePath;
            this.isInput = isInput;
            this.ownerGuid = ownerGuid;
            var p = valuePath.Split(".");
            valueName = p[^1];
            owner = operationModel;
            if (owner.Operator != null)
            {
                if (isInput)
                {
                    owner.Operator.InputsPropertyChanged += Inputs_PropertyChanged;
                }
                else
                {
                    owner.Operator.OutputsPropertyChanged += Outputs_PropertyChanged;
                }
            }
        }
        [JsonConstructor]
        public ConnectorModel(string title, Type valueType, string valuePath, bool isInput, Guid ownerGuid)
        {
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

        public bool IsConnected
        {
            get => isConnected;
            set => SetProperty(ref isConnected, value);
        }

        public bool IsInput
        {
            get => isInput;
            //set => SetProperty(ref isInput, value);
        }

        public Guid OwnerGuid
        {
            get => ownerGuid;
            set => SetProperty(ref ownerGuid, value);
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