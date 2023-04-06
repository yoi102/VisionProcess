using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VisonProcess.Tools.ViewModels;

namespace VisonProcess.ViewModels
{
    public partial class OperationsMenuViewModel : ObservableObject
    {

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();

        }

        public OperationsMenuViewModel()
        {
            List<string> list = new();
            //Assembly assembly = Assembly.Load("./VisonProcess.Tools.ViewModels.dll");
            Assembly assembly = Assembly.GetAssembly(typeof(AcquireImageViewModel))!;//获取 AcquireImageViewModel 中的程序集
            //var assemblyAllTypes = assembly.GetTypes();//获取该程序集中的所有类型
            var assemblyAllTypes = GetTypesInNamespace(assembly, "VisonProcess.Tools.ViewModels");//获取该程序集中的所有类型
            foreach (var itemType in assemblyAllTypes)//遍历所有类型进行查找
            {
                list.Add(itemType.Name.Replace("ViewModel", string.Empty));
            }
            AvailableOperations = list;
        }
        public event Action? Closed;

        [ObservableProperty]
        private Point _location;

        [ObservableProperty]
        private bool _isVisible;
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


        private void CreateOperation()
        {

        }






    }
}
