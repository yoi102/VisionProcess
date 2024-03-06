using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VisionProcess.Core.Attributes;
using VisionProcess.Models;

namespace VisionProcess.ViewModels
{
    [Flags]
    public enum ValueStatus
    {
        None = 0,
        CanRead = 1,
        CanWrite = 2,
        All = CanRead | CanWrite
    }

    public class TreeNode(string fullPath, object? value, Type type, ValueStatus state)
    {
        public ObservableCollection<TreeNode> ChildNodes { get; } = [];
        public string FullPath { get; } = fullPath;
        public string NodeTitle { get; } = fullPath.Split('.')[^1];
        public ValueStatus State { get; } = state;
        public Type Type { get; } = type;
        public object? Value { get; } = value;
    }

    internal partial class IOConnectorViewModel : ObservableObject
    {
        private readonly OperationModel operationModel;

        private readonly HashSet<object?>? visited = [];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddInputCommand), nameof(AddOutputCommand))]
        private TreeNode? selectedNode;

        [ObservableProperty]
        private ObservableCollection<TreeNode> treeNodes = [];
        public IOConnectorViewModel(OperationModel operationModel)
        {
            this.operationModel = operationModel;

            if (operationModel.Operator is not null)
                FetchPropertyAndMethodInfo(operationModel.Operator, TreeNodes);
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
                       && (SelectedNode.State & ValueStatus.CanWrite) == ValueStatus.CanWrite;
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
                       && (SelectedNode.State & ValueStatus.CanRead) == ValueStatus.CanRead;
            }
            return false;
        }
        bool AllowFetchType(Type type)//只允许一些简单类型或当前项目类型
        {
            return type.IsPrimitive || type == typeof(string) ||
                   type == typeof(DateTime) || type.IsEnum ||
                   typeof(IList).IsAssignableFrom(type) ||
                   typeof(Mat).IsAssignableFrom(type) ||
                   (type.Namespace != null && (type.Namespace.Contains("VisionProcess") ||//当前项目的命名空间
                   type.Namespace.Contains("System.Collections.Generic")))
                   ;

        }
        //!!!!!!!!!!!!!!System.StackOverflowException:“Exception_WasThrown”!!!!!!!!!!!!!!!!!!!
        private void FetchPropertyAndMethodInfo(object? instance, ObservableCollection<TreeNode> treeNodes)
        {
            //结束条件？
            //instance为null，
            //propertyInfos个数、
            //targetMethods个数
            if (instance is null)
                if (instance is null || visited!.Contains(instance))
                    return;
            visited!.Add(instance);

            #region 获取所有属性
            Type instanceType = instance.GetType();
            if (!AllowFetchType(instanceType))
                return;

            PropertyInfo[] propertyInfos = instanceType.GetProperties();
            if (propertyInfos.Length < 1)
                return;
            foreach (var propertyInfo in propertyInfos)
            {
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(ThresholdIgnoreAttribute)) is not null)
                    continue;
                if (!AllowFetchType(propertyInfo.PropertyType))
                    continue;

                //只允许和继承了 IList 接口的；即可用 int 参数的引锁器
                if (propertyInfo.PropertyType.IsArray)//如果是数组,先数组
                {
                    var propertyInstance = propertyInfo.GetValue(instance);
                    if (propertyInstance is not Array array)   //上面判断了必然是Array的
                        continue;

                    //这里先将整个 Array 加入TreeNode中先、
                    AssignPropertyTreeNode(propertyInstance, propertyInfo, treeNodes);
                    //再获取 Array 的所有属性、Length 等、
                    var arrayType = propertyInfo.PropertyType;
                    foreach (var arrayPropertyInfo in arrayType.GetProperties())
                    {
                        AssignPropertyTreeNode(arrayPropertyInfo.GetValue(propertyInstance), arrayPropertyInfo, treeNodes[^1].ChildNodes);
                    }
                    //再将所有 item 加入
                    var lengthInfo = propertyInfo.PropertyType.GetProperty("Length");
                    if (lengthInfo == null || lengthInfo.PropertyType != typeof(int))   //这也必然会不成立
                        continue;
                    var lengthInstance = lengthInfo.GetValue(propertyInstance);
                    if (lengthInstance is not int length)  //必然会有值
                        continue;
                    for (int i = 0; i < length; i++)
                    {
                        var item = array.GetValue(i);
                        var elementType = arrayType.GetElementType();
                        if (elementType != null)
                            AssignItemToTreeNode(item, string.Empty, elementType, ValueStatus.All, treeNodes[^1].ChildNodes, i);

                    }
                }

                //如果是IList
                else if (propertyInfo.PropertyType.IsAssignableTo(typeof(IList)))
                {
                    //先将当前实例加入
                    var propertyInstance = propertyInfo.GetValue(instance);
                    AssignPropertyTreeNode(propertyInstance, propertyInfo, treeNodes);

                    //再获取 IList 的所有属性、Count 等、
                    var listType = propertyInfo.PropertyType;
                    foreach (var listPropertyInfo in listType.GetProperties().Where(x => x.Name != "Item"))
                    {
                        AssignPropertyTreeNode(listPropertyInfo.GetValue(propertyInstance), listPropertyInfo, treeNodes[^1].ChildNodes);
                    }
                    //再将所有 item 加入
                    if (propertyInstance == null)
                        continue;
                    PropertyInfo? itemPropertyInfo = propertyInstance.GetType().GetProperty("Item");
                    if (itemPropertyInfo == null)
                        continue;
                    var countInfo = propertyInfo.PropertyType.GetProperty("Count");
                    if (countInfo == null || countInfo.PropertyType != typeof(int))
                        continue;
                    var countObj = countInfo.GetValue(propertyInstance);
                    if (countObj is null)
                        continue;
                    var count = (int)countObj;
                    for (int i = 0; i < count; i++)
                    {
                        var item = itemPropertyInfo.GetValue(propertyInstance, [i]);
                        if (item is null) continue;
                        AssignItemToTreeNode(item, string.Empty, itemPropertyInfo.PropertyType, ValueStatus.All, treeNodes[^1].ChildNodes, i);
                    }
                }
                else if (propertyInfo.GetIndexParameters().Length > 0)//如果自定义类带引锁器,过滤。。。
                    continue;
                else//否则为普通属性
                {
                    var propertyInstance = propertyInfo.GetValue(instance);
                    AssignPropertyTreeNode(propertyInstance, propertyInfo, treeNodes);
                }
            }

            #endregion 获取所有属性

            #region 获取所有无参带返回值方法

            MethodInfo[] methods = instanceType.GetMethods(BindingFlags.Public);
            var targetMethods = methods.Where(x => x.GetParameters().Length == 0 && x.ReturnType != typeof(void));
            if (targetMethods.Count() < 1)
                return;
            foreach (var method in targetMethods)
            {
                if (Attribute.GetCustomAttribute(method, typeof(ThresholdIgnoreAttribute)) is not null)
                    continue;
                if (!AllowFetchType(method.ReturnType))
                    continue;
                object? value = method.Invoke(instance, null);
                string path = method.Name + "()";
                treeNodes.Add(new TreeNode(path, value, method.ReturnType, ValueStatus.CanRead));
                if (value is null) continue;
                FetchPropertyAndMethodInfo(value, treeNodes[^1].ChildNodes);
            }

            #endregion 获取所有无参带返回值方法



        }
        void AssignItemToTreeNode(object? instance, string propertyName, Type type, ValueStatus state, ObservableCollection<TreeNode> treeNodes, int index)
        {
            string fullPath = propertyName + $"[{index}]";
            treeNodes.Add(new TreeNode(fullPath, instance, type, state));
            //获取当前的
            FetchPropertyAndMethodInfo(instance, treeNodes[^1].ChildNodes);
        }
        private void AssignPropertyTreeNode(object? instance, PropertyInfo propertyInfo, ObservableCollection<TreeNode> treeNodes)
        {

            if (propertyInfo.GetMethod is null)
                return;
            if (!propertyInfo.GetMethod.IsPublic && !propertyInfo.GetMethod.IsPublic)
                return;
            ValueStatus state = ValueStatus.None;
            if (propertyInfo.CanRead)
                state |= ValueStatus.CanRead;
            if (propertyInfo.CanWrite)
                state |= ValueStatus.CanWrite;

            treeNodes.Add(new TreeNode(propertyInfo.Name, instance, propertyInfo.PropertyType, state));
            //获取当前的
            FetchPropertyAndMethodInfo(instance, treeNodes[^1].ChildNodes);
        }

        [RelayCommand]
        private void TreeNodeSelected(TreeNode treeNode)
        {
            SelectedNode = treeNode;
        }
    }
}