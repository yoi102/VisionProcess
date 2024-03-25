using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Converters;
using VisionProcess.Core.Strings;
using VisionProcess.Models;

namespace VisionProcess.ViewModels
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
                FetchMemberInfo(operationModel.Operator, TreeNodes, null);
            visited = null;
        }

        private static bool IsNoAllowType(Type type)
        {
            return type.IsPointer ||
                   type.IsNotPublic ||//若是不公开的类型都舍去
                   type == typeof(IntPtr) ||
                   type == typeof(UIntPtr) ||
                   type == typeof(DateTime) ||//DateTime、DateTimeOffset 中有个 Date 属性 导致无限递归
                   type == typeof(DateTimeOffset) ||
                   type == typeof(MatExpr) ||
                   type.IsAssignableTo(typeof(IEnumerator)) ||
                   //type.IsAssignableTo(typeof(IVec)) ||
                   type.IsAssignableTo(typeof(Scalar));
        }

        [RelayCommand(CanExecute = nameof(CanAddInput))]
        private void AddInput()
        {
            if (SelectedNode == null) return;
            operationModel.Inputs.Add(
                new ConnectorModel(SelectedNode.FullPath.Split('.')[^1], SelectedNode.Type,
                SelectedNode.FullPath, true, operationModel.Id, operationModel));
            AddInputCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanAddOutput))]
        private void AddOutput()
        {
            if (SelectedNode == null) return;
            operationModel.Outputs.Add(
                new ConnectorModel(SelectedNode.FullPath.Split('.')[^1], SelectedNode.Type,
                SelectedNode.FullPath, false, operationModel.Id, operationModel));
            AddOutputCommand.NotifyCanExecuteChanged();
        }

        private void AssignItemToTreeNode(object? instance, string propertyName, Type type, ValueStatus state, ObservableCollection<TreeNode> treeNodes, int index, TreeNode? parent)
        {
            string fullPath = propertyName + $"[{index}]";
            var newTreeNode = new TreeNode(fullPath, instance, type, state, parent);
            treeNodes.Add(newTreeNode);
            //获取当前的
            FetchMemberInfo(instance, treeNodes[^1].ChildNodes, newTreeNode);
        }

        private void AssignPropertyTreeNode(object? instance, PropertyInfo propertyInfo, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            var newTreeNode = AssignTreeNodeByPropertyInfo(instance, propertyInfo, treeNodes, parent);
            if (newTreeNode is null)
                return;
            //获取当前的
            FetchMemberInfo(instance, treeNodes[^1].ChildNodes, newTreeNode);
        }

        private TreeNode? AssignTreeNodeByPropertyInfo(object? instance, PropertyInfo propertyInfo, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            ValueStatus state = ValueStatus.None;
            if (propertyInfo.GetMethod is null || propertyInfo.GetMethod.IsPublic && !propertyInfo.GetMethod.IsStatic)
                state |= ValueStatus.ReadOnly;
            if (propertyInfo.SetMethod is not null && propertyInfo.SetMethod.IsPublic)
                state |= ValueStatus.WriteOnly;
            if (state == ValueStatus.None)
                return null;

            var newTreeNode = new TreeNode(propertyInfo.Name, instance, propertyInfo.PropertyType, state, parent);
            treeNodes.Add(newTreeNode);
            return newTreeNode;
        }

        private bool CanAddInput()
        {
            if (SelectedNode is null)
                return false;
            return operationModel.Inputs.FirstOrDefault(x => x.ValuePath == SelectedNode.FullPath) == null &&
                         (SelectedNode.State & ValueStatus.WriteOnly) == ValueStatus.WriteOnly &&
                         !SelectedNode.FullPath.Contains('(');//如果是方法获得的就不能当作输入
        }

        private bool CanAddOutput()
        {
            if (SelectedNode is null)
                return false;
            return operationModel.Outputs.FirstOrDefault(x => x.ValuePath == SelectedNode.FullPath) == null &&
                         (SelectedNode.State & ValueStatus.ReadOnly) == ValueStatus.ReadOnly;
        }

        /// <summary>
        /// 特殊处理类型为IList Array 的实例、,且将搜索到的实例再扔进<see cref="FetchMemberInfo"/>递归搜索更多
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="treeNodes"></param>
        /// <param name="parent"></param>
        private void FetchArrayOrIListPropertyInfo(object instance, PropertyInfo propertyInfo, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            //复杂度较高 16 但是算了、、、
            //1、这里先将整个  加入TreeNode中先、
            var propertyInstance = propertyInfo.GetValue(instance);
            var newTreeNode = AssignTreeNodeByPropertyInfo(propertyInstance, propertyInfo, treeNodes, parent);

            if (newTreeNode is null)
                return;
            if (propertyInstance == null)
                return;
            //2、再获取IList Array 的所有属性、Length Count等、
            //Ilist的Item需要去掉Array中没有Item无妨
            var iListInstancePropertyInfos = propertyInfo.PropertyType.GetProperties()
                                 .Where(x => x.GetMethod != null && x.Name != "Item" && x.GetMethod.IsPublic && !x.GetMethod.IsStatic);
            foreach (var iListPropertyInfo in iListInstancePropertyInfos)
            {
                var iListPropertyInstance = iListPropertyInfo.GetValue(propertyInstance);
                AssignPropertyTreeNode(iListPropertyInstance, iListPropertyInfo, treeNodes[^1].ChildNodes, newTreeNode);
            }
            //3、再将所有 item 加入
            PropertyInfo? countInfo;
            Array? array = null;
            if (propertyInstance is Array a)//
            {
                array = a;
                countInfo = propertyInfo.PropertyType.GetProperty("Length");
            }
            else
            {
                countInfo = propertyInfo.PropertyType.GetProperty("Count");
            }
            if (countInfo == null || countInfo.GetValue(propertyInstance) is not int count)
                return;
            for (int i = 0; i < count; i++)
            {
                object? item;
                Type? type;
                if (array is not null)
                {
                    item = array.GetValue(i);
                    type = propertyInfo.PropertyType.GetElementType();
                }
                else
                {
                    PropertyInfo? itemPropertyInfo = propertyInstance.GetType().GetProperty("Item");
                    if (itemPropertyInfo == null)
                        return;
                    item = itemPropertyInfo.GetValue(propertyInstance, [i]);
                    type = itemPropertyInfo.PropertyType;
                }
                if (type is null)
                    return;
                AssignItemToTreeNode(item, string.Empty, type, ValueStatus.Read_Write, treeNodes[^1].ChildNodes, i, newTreeNode);
            }
        }

        /// <summary>
        /// 搜索 instance 中的所有公开字段、且视为只读
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="instanceType"></param>
        /// <param name="treeNodes"></param>
        /// <param name="parent"></param>
        private void FetchFieldInfo(object instance, Type instanceType, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            //公开字段、只给读好。且不再向下搜索
            FieldInfo[] fieldInfos = instanceType.GetFields();
            if (fieldInfos.Length < 1)
                return;
            //GetMethod 必须为 Public；且不能为静态
            var targetFieldInfos = fieldInfos.Where(x => x.IsPublic
                                                                                    && !x.IsStatic);
            foreach (var fieldInfo in targetFieldInfos)
            {
                var fieldInstance = fieldInfo.GetValue(instance);

                var newTreeNode = new TreeNode(fieldInfo.Name, fieldInstance, fieldInfo.FieldType, ValueStatus.ReadOnly, parent);
                treeNodes.Add(newTreeNode);
            }
        }

        /// <summary>
        /// 搜索实例的所有带返回值无参方法,且将搜索到的返回值再扔进<see cref="FetchMemberInfo"/>递归搜索更多
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="instanceType"></param>
        /// <param name="treeNodes"></param>
        /// <param name="parent"></param>
        private void FetchMethodInfo(object instance, Type instanceType, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            MethodInfo[] methods = instanceType.GetMethods();
            var targetMethods = methods.Where(x => x.IsPublic &&//必须公开
                                                                         !x.IsStatic &&//必须非静态
                                                                         !x.IsConstructor &&//必须非构造器
                                                                         !x.ContainsGenericParameters &&//必须非泛型
                                                                          x.GetParameters().Length == 0 &&//必须无参
                                                                          x.ReturnType.IsPublic &&
                                                                         !x.ReturnType.IsPointer &&
                                                                          x.ReturnType != typeof(void) &&//必须带返回值
                                                                          x.ReturnType != typeof(Mat) &&//必须非返回Mat
                                                                         !x.IsSpecialName &&//需要区分GetType和ToString  GetHashCode()
                                                                         !x.Name.Contains("Clone", StringComparison.OrdinalIgnoreCase) &&
                                                                         !x.Name.Contains("GetType", StringComparison.OrdinalIgnoreCase) &&
                                                                         !x.Name.Contains("To", StringComparison.OrdinalIgnoreCase) &&
                                                                         //!x.Name.Contains("ToString", StringComparison.OrdinalIgnoreCase) &&
                                                                         !x.Name.Contains("GetHashCode", StringComparison.OrdinalIgnoreCase));
            foreach (var method in targetMethods)
            {
                try
                {
                    object? returnValue = method.Invoke(instance, null);

                    string path = method.Name + "()";
                    treeNodes.Add(new TreeNode(path, returnValue, method.ReturnType, ValueStatus.ReadOnly, parent));
                    FetchMemberInfo(returnValue, treeNodes[^1].ChildNodes, parent);
                }
                catch (Exception ex)
                {
                    //一些 openCv 的异常似乎无解
                    if (ex.InnerException is not OpenCVException)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 搜索实例的所有属性和无参带返回值方法、且继续向下寻找知道没有更多为止、并添加到 <see cref="TreeNodes"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="treeNodes"></param>
        /// <param name="parent"></param>
        private void FetchMemberInfo(object? instance, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            // 返回方法的还没完成！！！！！
            if (instance is null)
                return;

            Type instanceType = instance.GetType();
            if (IsVisitedOrNoAllow(instance, instanceType))
                return;

            // 获取所有属性
            FetchPropertyInfo(instance, instanceType, treeNodes, parent);
            // 获取公开字段
            FetchFieldInfo(instance, instanceType, treeNodes, parent);
            // 获取所有无参带返回值方法
            FetchMethodInfo(instance, instanceType, treeNodes, parent);
        }

        /// <summary>
        /// 搜索实例的所有属性,且将搜索到的实例再扔进<see cref="FetchMemberInfo"/>递归搜索更多
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="instanceType"></param>
        /// <param name="treeNodes"></param>
        /// <param name="parent"></param>
        private void FetchPropertyInfo(object instance, Type instanceType, ObservableCollection<TreeNode> treeNodes, TreeNode? parent)
        {
            PropertyInfo[] propertyInfos = instanceType.GetProperties();
            if (propertyInfos.Length < 1)
                return;
            var targetPropertyInfos = propertyInfos.Where(x => x.GetMethod is not null &&
                                                                                x.GetMethod.IsPublic && //必须为 Public
                                                                                !x.GetMethod.IsStatic && //不能为静态
                                                                                x.Name != "Item");//即自带引索器的
            foreach (var propertyInfo in targetPropertyInfos)
            {
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(ThresholdIgnoreAttribute)) is not null ||
                    Attribute.GetCustomAttribute(propertyInfo, typeof(JsonPropertyAttribute)) is not null)
                    continue;
                if (propertyInfo.PropertyType.IsAssignableTo(typeof(IList)))
                {
                    FetchArrayOrIListPropertyInfo(instance, propertyInfo, treeNodes, parent);
                }
                else if (propertyInfo.GetIndexParameters().Length > 0)//如果自定义类带引锁器,过滤。。。
                    continue;
                else//否则视为普通 object?
                {
                    var propertyInstance = propertyInfo.GetValue(instance);
                    AssignPropertyTreeNode(propertyInstance, propertyInfo, treeNodes, parent);
                }
            }
        }
        /// <summary>
        /// 判断实例是否搜索过防止无限递归、且判断是否允许该类型向下所搜
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsVisitedOrNoAllow(object instance, Type type)
        {
            bool isContains = visited!.Contains(instance);
            if (!isContains && type.IsClass)
                visited!.Add(instance);
            return isContains ||
                   instance is DisposableObject disposable && disposable.IsDisposed ||
                   IsNoAllowType(type);
        }
        [RelayCommand]
        private void TreeNodeSelected(TreeNode treeNode)
        {
            SelectedNode = treeNode;
        }
    }
}