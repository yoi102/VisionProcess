using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VisonProcess.Core.Extentions;
using VisonProcess.Core.ToolBase;

namespace VisonProcess.Core.Mvvm
{
    public class OperationViewModel : ObservableValidator
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



        private Point _location = default;
        public Point Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private Size _size = default;
        public Size Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        private string? _title;
        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private bool _isSelected = default;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }


        private IOperation? _operation;
        public IOperation? Operation
        {
            get => _operation;
            set => SetProperty(ref _operation, value)
                .Then(OnInputValueChanged);
        }





        public NodifyObservableCollection<ConnectorViewModel> Input { get; } = new NodifyObservableCollection<ConnectorViewModel>();

        public NodifyObservableCollection<ConnectorViewModel> Output { get; } = new NodifyObservableCollection<ConnectorViewModel>();


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


        protected virtual void OnOutputValueChanged()
        {








        }








    }
}
