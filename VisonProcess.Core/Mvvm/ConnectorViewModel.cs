using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using VisonProcess.Core.Extentions;

namespace VisonProcess.Core.Mvvm
{
    public class ConnectorViewModel : ObservableObject
    {
        public ConnectorViewModel(string title, object value, string path)
        {
            _title = title;
            _value = value;
            _valueType = value.GetType();
            _valuePath = path;
        }

        public ConnectorViewModel(string title, object value, Type type, string path)
        {
            _title = title;
            _value = value;
            _valueType = type;
            _valuePath = path;
        }


        private Point _anchor = default;
        private bool _isConnected = false;
        private bool _isInput = true;
        private OperationViewModel _operation = default!;
        private string _title;
        private object? _value;
        private string _valuePath;
        private Type _valueType;

        public string _ValuePath
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
            set => SetProperty(ref _isInput, value);
        }

        public OperationViewModel Operation
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
                    if (type.Name != ValueType.Name && type.GetInterface(ValueType.Name) == null && ValueType.GetInterface(type.Name) == null)
                    {
                        throw new ArgumentException($"The value's type must be {ValueType.Name} !!!");
                    }
                }
                SetProperty(ref _value, value).Then(() => ValueObservers.ForEach(o => o.Value = value));

            }
        }
        public List<ConnectorViewModel> ValueObservers { get; } = new List<ConnectorViewModel>();

        public Type ValueType
        {
            get => _valueType;
            protected set => SetProperty(ref _valueType, value);
        }
    }
}
