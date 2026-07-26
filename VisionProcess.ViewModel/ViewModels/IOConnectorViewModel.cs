using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using VisionProcess.Abstractions.Attributes;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Common;
using VisionProcess.Lang.Strings;
using VisionProcess.Runtime.Models;

namespace VisionProcess.ViewModel.ViewModels
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [Flags]
    public enum ValueStatus
    {
        [LocalizedDescription("None", typeof(Strings))]
        None = 0,
        [LocalizedDescription("ReadOnly", typeof(Strings))]
        ReadOnly = 1,
        [LocalizedDescription("WriteOnly", typeof(Strings))]
        WriteOnly = 2,
        [LocalizedDescription("Read&Write", typeof(Strings))]
        Read_Write = ReadOnly | WriteOnly
    }

    public class TreeNode
    {
        public TreeNode(string path, object? value, Type type, ValueStatus state, TreeNode? parent)
        {
            Path = path;
            State = state;
            Type = type;
            Value = value;
            if (parent == null)
            {
                FullPath = path;
            }
            else
            {
                FullPath = path.StartsWith('[') ? parent.FullPath + path : parent.FullPath + "." + path;
            }
        }

        public ObservableCollection<TreeNode> ChildNodes { get; } = [];
        public string FullPath { get; }
        public string Path { get; }
        public ValueStatus State { get; }
        public Type Type { get; }
        public object? Value { get; }
    }

    public partial class IOConnectorViewModel : ObservableObject
    {
        private readonly OperationModel operationModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(
            nameof(AddInputCommand),
            nameof(AddOutputCommand),
            nameof(AddAllInputsCommand),
            nameof(AddAllOutputsCommand))]
        private TreeNode? selectedNode;

        [ObservableProperty]
        private ObservableCollection<TreeNode> treeNodes = [];

        public IOConnectorViewModel(OperationModel operationModel)
        {
            this.operationModel = operationModel;

            if (operationModel.Operator is IConnectorProvider provider)
            {
                BuildTree(provider);
            }
        }

        public int InputPortItemCount => GetPortNodes(isInput: true).Count();

        public int OutputPortItemCount => GetPortNodes(isInput: false).Count();

        private void BuildTree(IConnectorProvider provider)
        {
            var inputsNode = new TreeNode("Inputs", null, typeof(IInputs), ValueStatus.None, null);
            var outputsNode = new TreeNode("Outputs", null, typeof(IOutputs), ValueStatus.None, null);

            foreach (var descriptor in provider.GetConnectors())
            {
                var state = GetValueStatus(descriptor);
                var parent = descriptor.Path.StartsWith("Inputs.", StringComparison.OrdinalIgnoreCase) ? inputsNode : outputsNode;
                var shortPath = GetShortPath(descriptor.Path);
                var node = new TreeNode(shortPath, descriptor.GetValue(), descriptor.ValueType, state, parent);
                parent.ChildNodes.Add(node);
            }

            if (inputsNode.ChildNodes.Count > 0)
                TreeNodes.Add(inputsNode);
            if (outputsNode.ChildNodes.Count > 0)
                TreeNodes.Add(outputsNode);
        }

        private static string GetShortPath(string path)
        {
            const string inputsPrefix = "Inputs.";
            const string outputsPrefix = "Outputs.";

            if (path.StartsWith(inputsPrefix, StringComparison.OrdinalIgnoreCase))
                return path[inputsPrefix.Length..];
            if (path.StartsWith(outputsPrefix, StringComparison.OrdinalIgnoreCase))
                return path[outputsPrefix.Length..];
            return path;
        }

        private static ValueStatus GetValueStatus(IConnectorDescriptor descriptor)
        {
            if (descriptor.CanRead && descriptor.CanWrite)
                return ValueStatus.Read_Write;
            if (descriptor.CanWrite)
                return ValueStatus.WriteOnly;
            return ValueStatus.ReadOnly;
        }

        [RelayCommand(CanExecute = nameof(CanAddInput))]
        private void AddInput()
        {
            if (SelectedNode is null)
                return;

            AddConnector(SelectedNode, isInput: true);
            NotifyConnectorCommands();
        }

        [RelayCommand(CanExecute = nameof(CanAddOutput))]
        private void AddOutput()
        {
            if (SelectedNode is null)
                return;

            AddConnector(SelectedNode, isInput: false);
            NotifyConnectorCommands();
        }

        [RelayCommand(CanExecute = nameof(CanAddAllInputs))]
        private void AddAllInputs()
        {
            foreach (var node in GetPortNodes(isInput: true).Where(CanAddInputNode))
            {
                AddConnector(node, isInput: true);
            }

            NotifyConnectorCommands();
        }

        [RelayCommand(CanExecute = nameof(CanAddAllOutputs))]
        private void AddAllOutputs()
        {
            foreach (var node in GetPortNodes(isInput: false).Where(CanAddOutputNode))
            {
                AddConnector(node, isInput: false);
            }

            NotifyConnectorCommands();
        }

        private bool CanAddInput() => CanAddInputNode(SelectedNode);

        private bool CanAddOutput() => CanAddOutputNode(SelectedNode);

        private bool CanAddAllInputs()
            => GetPortNodes(isInput: true).Any(CanAddInputNode);

        private bool CanAddAllOutputs()
            => GetPortNodes(isInput: false).Any(CanAddOutputNode);

        private bool CanAddInputNode(TreeNode? node)
        {
            if (node is null)
                return false;
            return operationModel.Inputs.All(x => x.ValuePath != node.FullPath) &&
                   node.FullPath.StartsWith("Inputs.", StringComparison.OrdinalIgnoreCase) &&
                   (node.State & ValueStatus.WriteOnly) == ValueStatus.WriteOnly;
        }

        private bool CanAddOutputNode(TreeNode? node)
        {
            if (node is null)
                return false;
            return operationModel.Outputs.All(x => x.ValuePath != node.FullPath) &&
                   node.FullPath.StartsWith("Outputs.", StringComparison.OrdinalIgnoreCase) &&
                   (node.State & ValueStatus.ReadOnly) == ValueStatus.ReadOnly;
        }

        private IEnumerable<TreeNode> GetPortNodes(bool isInput)
        {
            var prefix = isInput ? "Inputs." : "Outputs.";
            return TreeNodes
                .SelectMany(root => root.ChildNodes)
                .Where(node => node.FullPath.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase));
        }

        private void AddConnector(TreeNode node, bool isInput)
        {
            var connectors = isInput ? operationModel.Inputs : operationModel.Outputs;
            connectors.Add(new ConnectorModel(
                node.Path,
                node.Type,
                node.FullPath,
                isInput,
                operationModel.Id,
                operationModel));
        }

        private void NotifyConnectorCommands()
        {
            AddInputCommand.NotifyCanExecuteChanged();
            AddOutputCommand.NotifyCanExecuteChanged();
            AddAllInputsCommand.NotifyCanExecuteChanged();
            AddAllOutputsCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void TreeNodeSelected(TreeNode treeNode)
        {
            SelectedNode = treeNode;
        }
    }
}
