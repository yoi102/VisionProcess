using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Models;
using VisionProcess.Tools.Operators;

namespace VisionProcess.ViewModel.ViewModels.Tools
{
    /// <summary>
    /// 颜色转换算子的 ViewModel。只负责 View 绑定，运行逻辑由 <see cref="ColorConvertOperator"/> 处理。
    /// </summary>
    public sealed class ColorConvertViewModel : ObservableObject
    {
        private readonly ColorConvertOperator _operator;

        /// <summary>
        /// 供序列化/视图创建使用的无参构造函数。
        /// </summary>
        public ColorConvertViewModel() : this(new ColorConvertOperator())
        {
        }

        public ColorConvertViewModel(ColorConvertOperator @operator)
        {
            _operator = @operator;
            _operator.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
        }

        public ColorConvertInputs Inputs => _operator.Inputs;

        public ColorConvertOutputs Outputs => _operator.Outputs;

        public ObservableCollection<Record> Records => _operator.Records;

        public RunStatus RunStatus => _operator.RunStatus;
    }
}
