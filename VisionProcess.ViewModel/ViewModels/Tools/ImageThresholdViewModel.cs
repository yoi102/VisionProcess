using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Models;
using VisionProcess.Tools.Operators;

namespace VisionProcess.ViewModel.ViewModels.Tools
{
    /// <summary>
    /// 图像阈值算子的 ViewModel。只负责 View 绑定，运行逻辑由 <see cref="ImageThresholdOperator"/> 处理。
    /// </summary>
    public sealed class ImageThresholdViewModel : ObservableObject
    {
        private readonly ImageThresholdOperator _operator;

        /// <summary>
        /// 供序列化/视图创建使用的无参构造函数。
        /// </summary>
        public ImageThresholdViewModel() : this(new ImageThresholdOperator())
        {
        }

        public ImageThresholdViewModel(ImageThresholdOperator @operator)
        {
            _operator = @operator;
            _operator.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
        }

        public ImageThresholdInputs Inputs => _operator.Inputs;

        public ImageThresholdOutputs Outputs => _operator.Outputs;

        public ObservableCollection<Record> Records => _operator.Records;

        public RunStatus RunStatus => _operator.RunStatus;
    }
}
