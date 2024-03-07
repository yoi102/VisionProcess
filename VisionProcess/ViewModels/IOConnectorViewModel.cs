using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using OpenCvSharp;
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
    public enum ValueStatus
    {
        None = 0,
        CanRead = 1,
        CanWrite = 2,
        All = CanRead | CanWrite
    }

    public class TreeNode
    {
        public ObservableCollection<TreeNode> ChildNodes { get; } = [];
        public string FullPath { get; }

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
        public string Path { get; }
        public ValueStatus State { get; }
        public Type Type { get; }
        public object? Value { get; }


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
                FetchPropertyAndMethodInfo(operationModel.Operator, TreeNodes, null);
            visited = null;
        }

        [RelayCommand(CanExecute = nameof(CanAddInputCommand))]
        private void AddInput()
        {
            if (SelectedNode == null) return;
            operationModel.Inputs.Add(
                new ConnectorModel(SelectedNode.FullPath.Split('.')[^1], SelectedNode.Type,
                SelectedNode.FullPath, true, operationModel.Id, operationModel));
            AddInputCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanAddOutputCommand))]
        private void AddOutput()
        {
            if (SelectedNode == null) return;
            operationModel.Outputs.Add(
                new ConnectorModel(SelectedNode.FullPath.Split('.')[^1], SelectedNode.Type,
                SelectedNode.FullPath, false, operationModel.Id, operationModel));
            AddOutputCommand.NotifyCanExecuteChanged();
        }

        private bool CanAddInputCommand()
        {
            if (SelectedNode is null)
                return false;
            return operationModel.Inputs.FirstOrDefault(x => x.ValuePath == SelectedNode.FullPath) == null &&
                         (SelectedNode.State & ValueStatus.CanWrite) == ValueStatus.CanWrite &&
                         !SelectedNode.FullPath.Contains('(');//如果是方法获得的就不能当作输入

        }

        private bool CanAddOutputCommand()
        {
            if (SelectedNode is null)
                return false;
            return operationModel.Outputs.FirstOrDefault(x => x.ValuePath == SelectedNode.FullPath) == null &&
                         (SelectedNode.State & ValueStatus.CanRead) == ValueStatus.CanRead;


        }
        /// <summary>
        /// //只允许一些简单类型或当前项目类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool AllowFetchType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) ||
                   type == typeof(DateTime) || type.IsEnum ||
                   typeof(IList).IsAssignableFrom(type) ||
                   typeof(Mat).IsAssignableFrom(type) ||
                   (type.Namespace != null && (type.Namespace.Contains("VisionProcess") ||//当前项目的命名空间
                   type.Namespace.Contains("System.Collections.Generic")));

        }

        bool IsVisitedOrNoAllow(object instance, Type type)
        {
            bool isContains = visited!.Contains(instance);
            if (!isContains && type.IsClass)
                visited!.Add(instance);

            return isContains ||
                   instance is DisposableObject disposable && disposable.IsDisposed ||
                   type.IsPointer ||
                   type.IsNotPublic ||//若是不公开的类型都舍去
                   type == typeof(IntPtr) ||
                   type == typeof(UIntPtr) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type.IsAssignableTo(typeof(IEnumerator));
        }
        void FetchPropertyAndMethodInfo(object? instance, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            // 复杂度40+！！！！！！！返回方法的还没完成、 需要修改优化！！！！！
            if (instance is null)
                return;
            #region 获取所有属性
            Type instanceType = instance.GetType();
            if (IsVisitedOrNoAllow(instance, instanceType))
                return;

            PropertyInfo[] propertyInfos = instanceType.GetProperties();
            if (propertyInfos.Length < 1)
                return;
            //GetMethod 必须为 Public；且不能为静态
            var targetPropertyInfos = propertyInfos.Where(x => x.GetMethod is not null &&
                                                                                x.GetMethod.IsPublic &&
                                                                                !x.GetMethod.IsStatic);
            foreach (var propertyInfo in targetPropertyInfos)
            {
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(ThresholdIgnoreAttribute)) is not null ||
                    Attribute.GetCustomAttribute(propertyInfo, typeof(JsonPropertyAttribute)) is not null)
                    continue;
                //只允许和继承了 IList 接口的；即可用 int 参数的引锁器
                if (propertyInfo.PropertyType.IsArray)//如果是数组,先数组
                {
                    var propertyInstance = propertyInfo.GetValue(instance);
                    if (propertyInstance is not Array array)   //上面判断了必然是Array的
                        continue;
                    //这里先将整个 Array 加入TreeNode中先、
                    var newTreeNode = AssignTreeNodeByPropertyInfo(propertyInstance, propertyInfo, treeNodes, parent);
                    if (newTreeNode is null)
                        continue;
                    //再获取 Array 的所有属性、Length 等、
                    var arrayType = propertyInfo.PropertyType;
                    var targetArrayPropertyInfos = arrayType.GetProperties()
                        .Where(x => x.GetMethod != null && x.GetMethod.IsPublic && !x.GetMethod.IsStatic);
                    foreach (var arrayPropertyInfo in targetArrayPropertyInfos)
                    {
                        var arrayPropertyInstance = arrayPropertyInfo.GetValue(propertyInstance);
                        AssignPropertyTreeNode(arrayPropertyInstance, arrayPropertyInfo, treeNodes[^1].ChildNodes, newTreeNode);
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
                            AssignItemToTreeNode(item, string.Empty, elementType, ValueStatus.All, treeNodes[^1].ChildNodes, i, newTreeNode);
                    }
                }
                //如果是IList
                else if (propertyInfo.PropertyType.IsAssignableTo(typeof(IList)))
                {
                    //先将当前实例加入
                    var propertyInstance = propertyInfo.GetValue(instance);
                    var newTreeNode = AssignTreeNodeByPropertyInfo(propertyInstance, propertyInfo, treeNodes, parent);
                    if (newTreeNode is null)
                        continue;
                    //再获取 IList 的所有属性、Count 等、
                    var listType = propertyInfo.PropertyType;
                    var targetListPropertyInfos = listType.GetProperties()
                                 .Where(x => x.GetMethod != null && x.Name != "Item" && x.GetMethod.IsPublic && !x.GetMethod.IsStatic);
                    foreach (var listPropertyInfo in targetListPropertyInfos)
                    {
                        var listPropertyInstance = listPropertyInfo.GetValue(propertyInstance);
                        AssignPropertyTreeNode(listPropertyInfo.GetValue(propertyInstance), listPropertyInfo, treeNodes[^1].ChildNodes, newTreeNode);
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
                        AssignItemToTreeNode(item, string.Empty, itemPropertyInfo.PropertyType, ValueStatus.All, treeNodes[^1].ChildNodes, i, newTreeNode);
                    }
                }
                else if (propertyInfo.GetIndexParameters().Length > 0)//如果自定义类带引锁器,过滤。。。
                    continue;
                else//否则为普通object?
                {
                    var propertyInstance = propertyInfo.GetValue(instance);
                    AssignPropertyTreeNode(propertyInstance, propertyInfo, treeNodes, parent);
                }
            }

            #endregion 获取所有属性

            #region 获取所有无参带返回值方法
            //////导致  System.StackOverflowException！！！！！需要修改！！！
            //MethodInfo[] methods = instanceType.GetMethods();
            //var targetMethods = methods.Where(x => x.IsPublic &&
            //                                                             !x.IsStatic &&
            //                                                              x.GetParameters().Length == 0 &&
            //                                                              x.ReturnType != typeof(void) &&
            //                                                              x.ReturnType.IsPublic &&
            //                                                             !x.ReturnType.IsPointer &&
            //                                                             !x.ReturnType.IsGenericParameter &&
            //                                                              x.ReturnType != typeof(MatExpr) &&
            //                                                             !x.IsSpecialName &&
            //                                                             !x.Name.Contains("Clone", StringComparison.OrdinalIgnoreCase)&&
            //                                                             !x.Name.Contains("Copy", StringComparison.OrdinalIgnoreCase));
            //foreach (var method in targetMethods)
            //{
            //    //System.InvalidOperationException:
            //    //“Late bound operations cannot be
            //    //performed on types or methods for
            //    //which ContainsGenericParameters is true.”

            //    object? returnValue = method.Invoke(instance, null);
            //    //if (returnValue is null || IsVisitedOrNoAllow(returnValue))
            //    //    continue;
            //    string path = method.Name + "()";
            //    treeNodes.Add(new TreeNode(path, returnValue, method.ReturnType, ValueStatus.CanRead, parent));
            //    FetchPropertyAndMethodInfo(returnValue, treeNodes[^1].ChildNodes, parent);
            //}

            #endregion 获取所有无参带返回值方法



        }
        void AssignItemToTreeNode(object? instance, string propertyName, Type type, ValueStatus state, ObservableCollection<TreeNode> treeNodes, int index, TreeNode? parent)
        {
            string fullPath = propertyName + $"[{index}]";
            var newTreeNode = new TreeNode(fullPath, instance, type, state, parent);
            treeNodes.Add(newTreeNode);
            //获取当前的
            FetchPropertyAndMethodInfo(instance, treeNodes[^1].ChildNodes, newTreeNode);
        }
        void AssignPropertyTreeNode(object? instance, PropertyInfo propertyInfo, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            var newTreeNode = AssignTreeNodeByPropertyInfo(instance, propertyInfo, treeNodes, parent);
            if (newTreeNode is null)
                return;
            //获取当前的
            FetchPropertyAndMethodInfo(instance, treeNodes[^1].ChildNodes, newTreeNode);
        }

        private TreeNode? AssignTreeNodeByPropertyInfo(object? instance, PropertyInfo propertyInfo, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            ValueStatus state = ValueStatus.None;
            if (propertyInfo.GetMethod is null || propertyInfo.GetMethod.IsPublic && !propertyInfo.GetMethod.IsStatic)
                state |= ValueStatus.CanRead;
            if (propertyInfo.SetMethod is not null && propertyInfo.SetMethod.IsPublic)
                state |= ValueStatus.CanWrite;
            if (state == ValueStatus.None)
                return null;

            var newTreeNode = new TreeNode(propertyInfo.Name, instance, propertyInfo.PropertyType, state, parent);
            treeNodes.Add(newTreeNode);
            return newTreeNode;
        }

        [RelayCommand]
        private void TreeNodeSelected(TreeNode treeNode)
        {
            SelectedNode = treeNode;
        }
    }
}