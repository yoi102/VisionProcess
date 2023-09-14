using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows;
using VisionProcess.Core.Extentions;
using VisionProcess.Core.Strings;

namespace VisionProcess.Models
{
    public class ConnectorModel : ObservableObject
    {
        //public ConnectorModel(string title, object value, string path)
        //{
        //    _title = title;
        //    _value = value;
        //    _valueType = value.GetType();
        //    _valuePath = path;
        //}

        public ConnectorModel(string title, object value, Type valueType, string valuePath,bool isInput)
        {
            _title = title;
            _value = value;
            _valueType = valueType;
            _valuePath = valuePath;
            _isInput = isInput;
        }

        private Point _anchor = default;
        private bool _isConnected = false;
        private readonly bool _isInput = true;
        private OperationModel _operation = default!;
        private string _title;
        private object? _value;
        private string _valuePath;
        private Type _valueType;

        public string ValuePath
        {
            get { return _valuePath; }
            protected set { SetProperty(ref _valuePath, value); }
        }

        public Point Anchor
        {
            get => _anchor;
            set => SetProperty(ref _anchor, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public bool IsInput
        {
            get => _isInput;
            //set => SetProperty(ref _isInput, value);
        }

        public OperationModel Operation
        {
            get => _operation;
            set => SetProperty(ref _operation, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public object? Value
        {
            get => _value;
            set
            {
                if (value is not null)
                {
                    Type type = value.GetType();
                    if (type.Name != ValueType.Name && 
                        type.GetInterface(ValueType.Name) == null &&
                        ValueType.GetInterface(type.Name) == null)
                    {
                        throw new ArgumentException($"{Strings.ValueTypeMustBeX} ", ValueType.Name);
                    }
                }
                SetProperty(ref _value, value).Then(() => ValueObservers.ForEach(o => o.Value = value));
            }
        }

        public List<ConnectorModel> ValueObservers { get; } = new List<ConnectorModel>();

        public Type ValueType
        {
            get => _valueType;
            protected set => SetProperty(ref _valueType, value);
        }
    }
}