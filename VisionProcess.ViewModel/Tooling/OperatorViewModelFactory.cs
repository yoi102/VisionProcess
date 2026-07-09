using System;
using VisionProcess.Abstractions.Services;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Operators;
using VisionProcess.ViewModel.ViewModels.Tools;

namespace VisionProcess.ViewModel.Tooling
{
    /// <summary>
    /// 根据算子实例创建对应 ViewModel 的工厂实现。
    /// </summary>
    public sealed class OperatorViewModelFactory : IOperatorViewModelFactory
    {
        private readonly IFileDialogService? _fileDialogService;

        public OperatorViewModelFactory(IFileDialogService? fileDialogService = null)
        {
            _fileDialogService = fileDialogService;
        }

        /// <summary>
        /// 为指定算子创建对应的 ViewModel。
        /// </summary>
        public object Create(IOperator @operator)
        {
            return @operator switch
            {
                AcquireImageOperator op => new AcquireImageViewModel(op, _fileDialogService),
                ColorConvertOperator op => new ColorConvertViewModel(op),
                ImageFilterOperator op => new ImageFilterViewModel(op),
                ImageThresholdOperator op => new ImageThresholdViewModel(op),
                _ => throw new ArgumentException($"Unknown operator type '{@operator.GetType()}'.", nameof(@operator))
            };
        }
    }
}
