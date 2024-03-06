using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using VisionProcess.Core.Attributes;
using VisionProcess.Models;

namespace VisionProcess.ViewModels
{
    [Flags]
    public enum TreeNodeStatus
    {
        None = 0,
        CanRead = 1,
        CanWrite = 2,
        All = CanRead | CanWrite
    }

    public class TreeNode
    {
        public TreeNode(string fullPath, object? value, Type type, TreeNodeStatus state)
        {
            NodeTitle = fullPath.Split('.')[^1];
            FullPath = fullPath;
            Value = value;
            Type = type;
            State = state;
        }

        public ObservableCollection<TreeNode> ChildNodes { get; } = new ObservableCollection<TreeNode>();
        public string FullPath { get; }
        public string NodeTitle { get; }
        public TreeNodeStatus State { get; }
        public Type Type { get; }
        public object? Value { get; }
    }

    internal partial class IOConnectorViewModel : ObservableObject
    {
        private readonly OperationModel operationModel;

        private readonly HashSet<object>? visited = new HashSet<object>();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddInputCommand), nameof(AddOutputCommand))]
        private TreeNode? selectedNode;

        [ObservableProperty]
        private ObservableCollection<TreeNode> treeNodes = new ObservableCollection<TreeNode>();
        public IOConnectorViewModel(OperationModel operationModel)
        {
            this.operationModel = operationModel;
            if (operationModel.Operator is not null)
                GetNodePropertyAndMethodInfo(operationModel.Operator, TreeNodes);

            visited = null;
        }

        [RelayCommand(CanExecute = nameof(CanAddInputCommand))]
        private void AddInput()
        {
        }

        [RelayCommand(CanExecute = nameof(CanAddOutputCommand))]
        private void AddOutput()
        {
        }

        private bool CanAddInputCommand()
        {
            if (SelectedNode is not null)
            {
                return operationModel.Inputs.FirstOrDefault(x => x.ValuePath == SelectedNode.FullPath) != null ?
                       false :
                       SelectedNode.FullPath.Contains('.')
                       && (SelectedNode.State & TreeNodeStatus.CanWrite) == TreeNodeStatus.CanWrite;
            }
            return false;
        }

        private bool CanAddOutputCommand()
        {
            if (SelectedNode is not null)
            {
                return operationModel.Outputs.FirstOrDefault(x => x.ValuePath == SelectedNode.FullPath) != null ?
                       false :
                       SelectedNode.FullPath.Contains('.')
                       && (SelectedNode.State & TreeNodeStatus.CanRead) == TreeNodeStatus.CanRead;
            }
            return false;
        }

        private void GetNodePropertyAndMethodInfo(object? instance, ObservableCollection<TreeNode> treeNodes)
        {
            if (instance is null)
                return;


            #region 获取所有属性

            var instanceType = instance.GetType();
            PropertyInfo[] propertyInfos = instanceType.GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                //只允许和继承了 IList 接口的；即可用 int 参数的引锁器
                if (propertyInfo.PropertyType.IsArray)//如果是数组
                {
                    var value = propertyInfo.GetValue(instance);
                    if (value is not Array array)
                        continue;
                    var lengthInfo = propertyInfo.PropertyType.GetProperty("Length");
                    if (lengthInfo == null || lengthInfo.PropertyType != typeof(int))
                        continue;
                    var lengthObj = lengthInfo.GetValue(value);
                    if (lengthObj is null)
                        continue;
                    var length = (int)lengthObj;
                    for (int i = 0; i < length; i++)
                    {
                        var item = array.GetValue(i);
                        if (item is null) continue;
                        GetNodeSubValue(item, propertyInfo, treeNodes, i);
                    }
                }
                else if (propertyInfo.PropertyType.IsAssignableTo(typeof(IList)))
                {
                    var value = propertyInfo.GetValue(instance);
                    if (value == null)
                        continue;
                    PropertyInfo? itemProperty = value.GetType().GetProperty("Item");
                    if (itemProperty == null)
                        continue;
                    var countInfo = propertyInfo.PropertyType.GetProperty("Count");
                    if (countInfo == null || countInfo.PropertyType != typeof(int))
                        continue;
                    var countObj = countInfo.GetValue(value);
                    if (countObj is null)
                        continue;
                    var count = (int)countObj;
                    for (int i = 0; i < count; i++)
                    {
                        var item = itemProperty.GetValue(value, [i]);
                        if (item is null) continue;
                        GetNodeSubValue(item, propertyInfo, treeNodes, i);
                    }
                }
                else if (propertyInfo.GetIndexParameters().Length > 0)//如果自定义类带引锁器
                    continue;
                else//否则为普通属性
                {
                    //try
                    //{
                    var value = propertyInfo.GetValue(instance);
                    GetNodeSubValue(value, propertyInfo, treeNodes, -1);
                    //}
                    //catch { }
                }
            }

            #endregion 获取所有属性

            #region 获取所有无参带返回值方法

            MethodInfo[] methods = instanceType.GetMethods(BindingFlags.Public);
            var targetMethods = methods.Where(x => x.GetParameters().Length == 0 && x.ReturnType != typeof(void));
            foreach (var method in targetMethods)
            {
                if (Attribute.GetCustomAttribute(method, typeof(ThresholdIgnoreAttribute)) is not null)
                    continue;
                object? value = method.Invoke(instance, null);
                string path = method.Name + "()";
                treeNodes.Add(new TreeNode(path, value, method.ReturnType, TreeNodeStatus.CanRead));
                if (value is null) continue;
                GetNodePropertyAndMethodInfo(value, treeNodes[^1].ChildNodes);
            }

            #endregion 获取所有无参带返回值方法
        }

        private void GetNodeSubValue(object? instance, PropertyInfo propertyInfo, ObservableCollection<TreeNode> treeNodes, int index = -1)
        {
            if (instance is null)
                return;
            if (visited!.Contains(instance))
                return;
            if (Attribute.GetCustomAttribute(propertyInfo, typeof(ThresholdIgnoreAttribute)) is not null)
                return;
            if (propertyInfo.GetMethod is null)
                return;
            if (!propertyInfo.GetMethod.IsPublic && !propertyInfo.GetMethod.IsPublic)
                return;
            TreeNodeStatus state = TreeNodeStatus.None;
            if (propertyInfo.CanRead)
                state |= TreeNodeStatus.CanRead;
            if (propertyInfo.CanWrite)
                state |= TreeNodeStatus.CanWrite;

            //StringBuilder sb = new StringBuilder();//不太需要。。。重复 new StringBuilder可能更...

            string fullPath = propertyInfo.Name +
                            (index == -1 ?
                            string.Empty :
                            ('[' + index + ']'));
            if (propertyInfo.PropertyType.IsClass)
                visited.Add(instance);
            treeNodes.Add(new TreeNode(fullPath, instance, propertyInfo.PropertyType, state));
            //获取当前的
            GetNodePropertyAndMethodInfo(instance, treeNodes[^1].ChildNodes);
        }

        [RelayCommand]
        private void TreeNodeSelected(TreeNode treeNode)
        {
            SelectedNode = treeNode;
        }
    }
}