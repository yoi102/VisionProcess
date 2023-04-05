using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Windows;
using VisonProcess.Core.Attributes;
using VisonProcess.Core.Extentions;
using VisonProcess.Core.ToolBase;

namespace VisonProcess.Core.Mvvm
{
    public class OperationModel : ObservableObject
    {
        public OperationModel()
        {
            Input.WhenAdded(x =>
            {
                x.Operation = this;
                x.IsInput = true;
                x.PropertyChanged += OnInputValueChanged;
            })
            .WhenRemoved(x =>
            {
                x.PropertyChanged -= OnInputValueChanged;
            });

            Output.WhenAdded(x =>
            {
                x.Operation = this;
                x.IsInput = false;
                x.PropertyChanged += OnOutputValueChanged;
            })
          .WhenRemoved(x =>
          {
              x.PropertyChanged -= OnOutputValueChanged;
          });
        }

        private bool _isSelected = false;

        private Point _location = default;

        private IOperation? _operation;

        private Size _size = default;

        private string? _title;

        public NodifyObservableCollection<ConnectorModel> Input { get; } = new NodifyObservableCollection<ConnectorModel>();

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public Point Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public IOperation? Operation
        {
            get => _operation;
            set
            {
                if (value is not null)
                {
                    value.Executed += Value_Executed; ;
                    Title = value.GetType().Name.Replace("ViewModel", "");
                    var attributes = (DefaultToolConnectorAttribute[])value.GetType().GetCustomAttributes(typeof(DefaultToolConnectorAttribute), false);
                    foreach (var item in attributes)
                    {
                        var t = PropertyMisc.GetType(value, item.Path);
                        var v = PropertyMisc.GetValue(value, item.Path);
                        //if (t == null || v == null)
                        if (t == null)
                        {
                            throw new ArgumentException("Error, DefaultToolConnectorAttribute setting  error");
                        }
                        if (item.IsInput)
                        {
                            Input.Add(new ConnectorModel(item.Title, v, t, item.Path));
                        }
                        else
                        {
                            Output.Add(new ConnectorModel(item.Title, v, t, item.Path));
                        }
                    }
                }
                else
                {
                    if (_operation is not null)
                    {
                        _operation.Executed -= Value_Executed; ;
                    }
                    Input.Clear();
                    Output.Clear();
                }

                SetProperty(ref _operation, value);
                //.Then(OnInputValueChanged);
            }
        }

        private void Value_Executed(object? sender, EventArgs e)
        {
            if (_operation is not null)
            {
                foreach (var item in Output)
                {
                    //获取不到值
                    item.Value = PropertyMisc.GetValue(_operation, item.ValuePath);
                }
            }



        }

        public NodifyObservableCollection<ConnectorModel> Output { get; } = new NodifyObservableCollection<ConnectorModel>();

        public Size Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        protected virtual void OnInputValueChanged()
        {
            if (Operation != null)
            {
                try
                {
                    foreach (var item in Input)
                    {
                        PropertyMisc.SetValue(Operation, item.ValuePath, item.Value);
                    }
                    Operation.Execute();
                }
                catch
                {
                }
            }
        }


        protected virtual void OnOutputValueChanged()
        {
        }

        private void OnInputValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectorModel.Value))
            {
                OnInputValueChanged();
            }
        }

        private void OnOutputValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectorModel.Value))
            {
                OnOutputValueChanged();
            }
        }
    }
}