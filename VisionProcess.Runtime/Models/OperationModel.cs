using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Common.Helpers;
using VisionProcess.Common.Mvvm;
using VisionProcess.Common.Primitives;
using VisionProcess.Runtime.Services;

namespace VisionProcess.Runtime.Models
{
    public partial class OperationModel : ObservableObject
    {
        private readonly Guid id;
        private IOperator? @operator;
        private bool isSelected = false;
        private VpPoint location;
        private VpSize size;

        public OperationModel() : this(Guid.NewGuid())
        {
        }

        public OperationModel(Guid id)
        {
            this.id = id;
            Init();
        }

        public event EventHandler? OperatorExecuted;

        public Guid Id => id;

        public NodifyObservableCollection<ConnectorModel> Inputs { get; } = [];

        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public VpPoint Location
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
                foreach (var item in value.GetDefaultConnectors())
                {
                    var type = PropertyReflectionHelper.GetType(value, item.Path) ?? throw new ArgumentException("Error, DefaultToolConnector setting error");
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

        /// <summary>
        /// 算子对应的 ViewModel，用于 View 绑定。不随文档持久化。
        /// </summary>
        public object? OperatorViewModel { get; set; }

        public VpSize Size
        {
            get => size;
            set => SetProperty(ref size, value);
        }

        public IWindowDialogService? WindowDialogService { get; set; }

        [RelayCommand]
        private void AddIO()
        {
            WindowDialogService?.OpenIOConnectorDialog(this);
        }

        private void Init()
        {
            Inputs.WhenAdded(x =>
            {
                if (Inputs.Where(y => y.ValuePath == x.ValuePath).Count() > 1)
                    Inputs.Remove(x);
            });
            Outputs.WhenAdded(x =>
            {

                if (Outputs.Where(y => y.ValuePath == x.ValuePath).Count() > 1)
                    Outputs.Remove(x);
            });
        }

        private void Operation_Executed(object? sender, EventArgs e)
        {
            OperatorExecuted?.Invoke(this, e);
        }
    }
}
