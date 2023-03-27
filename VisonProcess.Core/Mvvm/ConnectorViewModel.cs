using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using VisonProcess.Core.Extentions;

namespace VisonProcess.Core.Mvvm
{
    public partial class ConnectorViewModel : ObservableObject
    {
        public ConnectorViewModel(string title, object value , string path)
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


        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                if (value is not null)
                {
                    Type type = value.GetType();
                    if (type.Name != ValueType.Name && type.GetInterface(ValueType.Name) == null && ValueType.GetInterface(type.Name) == null)
                    {
                        throw new ArgumentException($"The value type must be {ValueType.Name} !!!");
                    }
                    SetProperty(ref _value, value).Then(() => ValueObservers.ForEach(o => o.Value = value));
                }
            }
        }

        private Type _valueType;
        public Type ValueType
        {
            get => _valueType;
            set => SetProperty(ref _valueType, value);
        }

        private string _valuePath;
        public string _ValuePath
        {
            get { return _valuePath; }
            set { SetProperty(ref _valuePath, value); }
        }




        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        private bool _isInput = true;
        public bool IsInput
        {
            get => _isInput;
            set => SetProperty(ref _isInput, value);
        }

        private Point _anchor = default;
        public Point Anchor
        {
            get => _anchor;
            set => SetProperty(ref _anchor, value);
        }

        private OperationViewModel _operation = default!;

        public OperationViewModel Operation
        {
            get => _operation;
            set => SetProperty(ref _operation, value);
        }

        public List<ConnectorViewModel> ValueObservers { get; } = new List<ConnectorViewModel>();
    }
}
