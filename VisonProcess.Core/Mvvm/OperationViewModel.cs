﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Windows;
using VisonProcess.Core.Extentions;
using VisonProcess.Core.ToolBase;

namespace VisonProcess.Core.Mvvm
{
    public class OperationViewModel : ObservableObject
    {
        public OperationViewModel()
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
                x.PropertyChanged += OnInputValueChanged;
            })
          .WhenRemoved(x =>
          {
              x.PropertyChanged -= OnInputValueChanged;
          });
        }

        private bool _isSelected = false;

        private Point _location = default;

        private IOperation? _operation;

        private Size _size = default;

        private string? _title;

        public NodifyObservableCollection<ConnectorViewModel> Input { get; } = new NodifyObservableCollection<ConnectorViewModel>();

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
            set => SetProperty(ref _operation, value)
                .Then(OnInputValueChanged);
        }

        public NodifyObservableCollection<ConnectorViewModel> Output { get; } = new NodifyObservableCollection<ConnectorViewModel>();

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
            if (Output != null && Operation != null)
            {
                try
                {
                    var input = Input.Select(i => i.Value).ToArray();
                    Operation?.Execute();
                }
                catch
                {
                }
            }
        }

        //private NodifyObservableCollection<ConnectorViewModel> _input = new NodifyObservableCollection<ConnectorViewModel>();
        //public NodifyObservableCollection<ConnectorViewModel> Input
        //{
        //    get { return _input; }
        //    private set { SetProperty(ref _input, value); }
        //}
        //private NodifyObservableCollection<ConnectorViewModel> _output = new NodifyObservableCollection<ConnectorViewModel>();
        //public NodifyObservableCollection<ConnectorViewModel> Output
        //{
        //    get { return _output; }
        //    private set { SetProperty(ref _output, value); }
        //}
        protected virtual void OnOutputValueChanged()
        {
        }

        private void OnInputValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectorViewModel.Value))
            {
                OnInputValueChanged();
            }
        }

        private void OnOutputValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConnectorViewModel.Value))
            {
                OnOutputValueChanged();
            }
        }
    }
}