using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisionProcess.Common.Extensions;
using VisionProcess.Common.Helpers;
using VisionProcess.Common.Mvvm;
using VisionProcess.Common.Primitives;
using VisionProcess.Runtime.Extensions;

namespace VisionProcess.Runtime.Models
{
    public partial class ProcessModel : ObservableObject
    {
        public ProcessModel()
        {
            operations = new();
            selectedOperations = new();
            Init();
        }

        private void Init()
        {
            Connections.WhenAdded(async c =>
            {
                c.Input!.IsConnected = true;
                c.Output!.IsConnected = true;
                //当连接时反射设值。。。
                var outputOperationMode = operations.First(x => x.Id == c.Output.OwnerId);
                var inputOperationMode = operations.First(x => x.Id == c.Input.OwnerId);
                var outputValue = PropertyReflectionHelper.GetValue(outputOperationMode.Operator!, c.Output.ValuePath);
                PropertyReflectionHelper.TrySetValue(inputOperationMode.Operator!, c.Input.ValuePath, outputValue);
                await inputOperationMode.Operator!.ExecuteAsync();
                c.Output.ValueObservers.Add(c.Input);
            })
            .WhenRemoved(c =>
            {
                var ic = Connections.Count(con => con.Input == c.Input || con.Output == c.Input);
                var oc = Connections.Count(con => con.Input == c.Output || con.Output == c.Output);
                if (ic == 0)
                {
                    c.Input!.IsConnected = false;
                }
                if (oc == 0)
                {
                    c.Output!.IsConnected = false;
                }
                c.Output!.ValueObservers.Remove(c.Input!);
            });

            Operations.WhenAdded(x =>
            {
                x.OperatorExecuted += OperatorExecuted;

                x.Inputs.WhenRemoved(RemoveConnection);
                void RemoveConnection(ConnectorModel i)
                {
                    var c = Connections.Where(con => con.Input == i || con.Output == i).ToArray();
                    c.ForEach(con => Connections.Remove(con));
                }
            })
            .WhenRemoved(x =>
            {
                x.OperatorExecuted -= OperatorExecuted;//当移除时，取消订阅

                foreach (var input in x.Inputs)
                {
                    DisconnectConnector(input);
                }

                foreach (var output in x.Outputs)
                {
                    DisconnectConnector(output);
                }
            });
        }

        private void OperatorExecuted(object? sender, EventArgs e)
        {   //一个操作运行完时，将当前输出节点所连接的操作输入赋值，并运行。。可能导致运行多次。。
            if (sender is not OperationModel operationModel)
                return;

            foreach (var output in operationModel.Outputs)
            {
                if (!output.IsConnected)
                    continue;
                var outputValue = PropertyReflectionHelper.GetValue(operationModel.Operator!, output.ValuePath);
                output.ValueObservers.ForEach(async x =>
                {
                    await x.SetInputValue(outputValue);
                });
            }
        }

        #region Properties

        private NodifyObservableCollection<OperationModel> operations;
        private OperationModel? selectedOperation;
        private NodifyObservableCollection<OperationModel> selectedOperations;
        public NodifyObservableCollection<ConnectionModel> Connections { get; } = new();

        public NodifyObservableCollection<OperationModel> Operations
        {
            get => operations;
            set => SetProperty(ref operations, value);
        }

        public PendingConnectionModel PendingConnection { get; set; } = new();

        public OperationModel? SelectedOperation
        {
            get => selectedOperation;
            set
            {
                if (selectedOperation is not null && selectedOperation.Operator is not null)
                {
                    selectedOperation.Operator.IsRealTime = false;
                }
                if (value is not null && value.Operator is not null)
                {
                    value.Operator.IsRealTime = true;
                }
                SetProperty(ref selectedOperation, value);
            }
        }

        public NodifyObservableCollection<OperationModel> SelectedOperations
        {
            get => selectedOperations;
            set
            {
                SetProperty(ref selectedOperations, value);
                SelectedOperation = value?.FirstOrDefault();
            }
        }

        #endregion Properties

        #region Commands

        internal static bool IsCanCreateConnection(ConnectorModel source, ConnectorModel? target) => target == null ||
                    (source != target &&
                    source.OwnerId != target.OwnerId &&
                    source.IsInput != target.IsInput &&
                    source.ValueType.IsAssignableTo(target.ValueType));

        private bool CanCreateConnection()
        {
            return IsCanCreateConnection(PendingConnection.Source, PendingConnection.Target);
        }

        private bool CanGroupSelection()
        {
            return SelectedOperations.Count > 0;
        }

        [RelayCommand(CanExecute = nameof(CanCreateConnection))]
        private void CreateConnection()
        {
            CreateConnection(PendingConnection.Source, PendingConnection.Target);
        }

        private void CreateConnection(ConnectorModel source, ConnectorModel? target)
        {
            if (target == null)
            {
                PendingConnection.IsVisible = false;
                return;
            }

            var input = source.IsInput ? source : target;
            var output = target.IsInput ? source : target;
            PendingConnection.IsVisible = false;
            DisconnectConnector(input);

            Connections.Add(new ConnectionModel
            {
                Input = input,
                Output = output
            });
        }

        [RelayCommand]
        private void DeleteSelection()
        {
            List<OperationModel>? list = SelectedOperations.ToList();
            list.ForEach(o => Operations.Remove(o));
        }

        [RelayCommand]
        private void DisconnectConnector(ConnectorModel connector)
        {
            var connections = Connections.Where(c => c.Input == connector || c.Output == connector).ToArray();
            connections.ForEach(c => Connections.Remove(c));
        }

        //[RelayCommand]
        //private void DeletLineConnection(ConnectionModel connection)
        //{
        //    connection.Inputs.IsConnected = false;
        //    connection.Outputs.IsConnected = false;
        //    Connections.Remove(connection);
        //}
        [RelayCommand(CanExecute = nameof(CanGroupSelection))]
        private void GroupSelection()
        {
            var bounding = SelectedOperations.GetBoundingBox(50);

            Operations.Add(new OperationGroupModel
            {
                Location = bounding.Location,
                GroupSize = new VpSize(bounding.Width, bounding.Height)
            });
        }

        [RelayCommand]
        private void RemoveIO(ConnectorModel connector)
        {
            DisconnectConnector(connector);
            if (connector.IsInput)
            {
                connector.Owner.Inputs.Remove(connector);
            }
            else
            {
                connector.Owner.Outputs.Remove(connector);
            }
        }
        [RelayCommand]
        private void StartConnection()
        {
            PendingConnection.IsVisible = true;
        }

        #endregion Commands
    }
}
