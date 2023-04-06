using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VisonProcess.Core.ToolBase;
using VisonProcess.Models;
using VisonProcess.Tools.ViewModels;

namespace VisonProcess.ViewModels
{
    public partial class OperationsMenuViewModel : ObservableObject
    {

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();

        }

        public OperationsMenuViewModel(ProcessModel processModel)
        {
            //前提，需要规范命名
            List<string> list = new();
            Assembly assembly = Assembly.GetAssembly(typeof(AcquireImageViewModel))!;//获取 AcquireImageViewModel 中的程序集
            var assemblyAllTypes = GetTypesInNamespace(assembly, "VisonProcess.Tools.ViewModels");//获取该程序集命名空间中的所有类型
            foreach (var itemType in assemblyAllTypes)//遍历所有类型进行查找
            {
                list.Add(itemType.Name.Replace("ViewModel", string.Empty));
            }
            AvailableOperations = list;
            this.processModel = processModel;
        }
        public event Action? Closed;

        [ObservableProperty]
        private Point _location;

        [ObservableProperty]
        private bool _isVisible;
        private readonly ProcessModel processModel;

        public IEnumerable<string> AvailableOperations { get; }



        public void OpenAt(Point targetLocation)
        {
            Close();
            Location = targetLocation;
            IsVisible = true;
        }

        public void Close()
        {
            IsVisible = false;
        }

        [RelayCommand]
        private void CreateOperation(string operationName)
        {
            //前提，需要规范命名
            Assembly assembly = typeof(AcquireImageViewModel).Assembly;
            var type = assembly.GetType("VisonProcess.Tools.ViewModels." + operationName + "ViewModel");
            var instance = Activator.CreateInstance(type!);
            processModel.Operations.Add(new OperationModel() { Operation = (IOperation)instance!, Location = Location ,Title = operationName });
            IsVisible = false;
        }






    }
}
