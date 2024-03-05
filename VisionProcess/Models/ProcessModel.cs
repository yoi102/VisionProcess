using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VisionProcess.Core.Extensions;
using VisionProcess.Core.Mvvm;
using VisionProcess.Core.ToolBase;
using VisionProcess.Extensions;
using VisionProcess.ViewModels;

namespace VisionProcess.Models
{
    public partial class ProcessModel : ObservableObject
    {
        public ProcessModel()
        {
            OperationsMenu = new(this);
            operations = new();
            selectedOperations = new();
            Init();
        }

        [JsonConstructor]
        public ProcessModel(NodifyObservableCollection<OperationModel> operations,
            NodifyObservableCollection<ConnectionModel> connections,
            NodifyObservableCollection<OperationModel> selectedOperations,
            OperationModel? selectedOperation)
        {
            OperationsMenu = new(this);
            this.operations = operations;
            foreach (var item in operations)
            {
                item.OperatorExecuted += OperatorExecuted;
            }

            foreach (var connection in connections)
            {
                //不出错的话必然是存在的
                var inputOperation = operations.First(x => x.Id == connection.Input!.OwnerGuid);
                var outputOperation = operations.First(x => x.Id == connection.Output!.OwnerGuid);
                var inputConnector = inputOperation.Inputs.First(x => x.ValuePath == connection.Input!.ValuePath);
                var outputConnector = outputOperation.Outputs.First(x => x.ValuePath == connection.Output!.ValuePath);
                outputConnector.ValueObservers.Add(inputConnector);
                Connections.Add(new ConnectionModel() { Input = inputConnector, Output = outputConnector });
            }
            Init();
            var selectedIds = selectedOperations.Select(x => x.Id);
            this.selectedOperations = new(operations.Where(x => selectedIds.Contains(x.Id)));
            if (selectedOperation is not null)
            {
                SelectedOperation = operations.FirstOrDefault(x => x.Id == selectedOperation.Id);
            }
        }

        private void Init()
        {
            Connections.WhenAdded(c =>
            {
                c.Input!.IsConnected = true;
                c.Output!.IsConnected = true;
                //当连接时反射设值。。。
                var outputOperationMode = operations.First(x => x.Id == c.Output.OwnerGuid);
                var inputOperationMode = operations.First(x => x.Id == c.Input.OwnerGuid);
                var outputValue = PropertyMisc.GetValue(outputOperationMode.Operator!, c.Output.ValuePath);
                PropertyMisc.TrySetValue(inputOperationMode.Operator!, c.Input.ValuePath, outputValue);
                inputOperationMode.Operator!.Execute();
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
                var outputValue = PropertyMisc.GetValue(operationModel.Operator!, output.ValuePath);
                output.ValueObservers.ForEach(x =>
                {
                    x.SetInputValue(outputValue);
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

        [JsonIgnore]
        public OperationsMenuViewModel OperationsMenu { get; }

        [JsonIgnore]
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
                    source.OwnerGuid != target.OwnerGuid &&
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

        [property: JsonIgnore]
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
                //OperationsMenu.OpenAt(PendingConnection.TargetLocation);
                //OperationsMenu.Closed += OnOperationsMenuClosed;
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

        //private void OnOperationsMenuClosed()
        //{
        //    PendingConnection.IsVisible = false;
        //    OperationsMenu.Closed -= OnOperationsMenuClosed;
        //}
        [property: JsonIgnore]
        [RelayCommand]
        private void DeleteSelection()
        {
            List<OperationModel>? list = SelectedOperations.ToList();
            list.ForEach(o => Operations.Remove(o));
        }

        [property: JsonIgnore]
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
        [property: JsonIgnore]
        [RelayCommand(CanExecute = nameof(CanGroupSelection))]
        private void GroupSelection()
        {
            var bounding = SelectedOperations.GetBoundingBox(50);

            Operations.Add(new OperationGroupModel
            {
                Location = bounding.Location,
                GroupSize = new Size(bounding.Width, bounding.Height)
            });
        }

        [property: JsonIgnore]
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
        [property: JsonIgnore]
        [RelayCommand]
        private void StartConnection()
        {
            PendingConnection.IsVisible = true;
        }

        #endregion Commands
    }
}