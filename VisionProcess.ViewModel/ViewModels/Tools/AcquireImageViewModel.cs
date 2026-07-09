using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VisionProcess.Abstractions.Services;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Models;
using VisionProcess.Tools.Operators;

namespace VisionProcess.ViewModel.ViewModels.Tools
{
    /// <summary>
    /// 图片采集算子的 ViewModel。只负责 View 绑定和文件对话框交互，运行逻辑由 <see cref="AcquireImageOperator"/> 处理。
    /// </summary>
    public sealed partial class AcquireImageViewModel : ObservableObject
    {
        private readonly AcquireImageOperator _operator;
        private readonly IFileDialogService? _fileDialogService;

        /// <summary>
        /// 供序列化/视图创建使用的无参构造函数。
        /// </summary>
        public AcquireImageViewModel() : this(new AcquireImageOperator(), Ioc.Default.GetService<IFileDialogService>())
        {
        }

        public AcquireImageViewModel(AcquireImageOperator @operator, IFileDialogService? fileDialogService = null)
        {
            _operator = @operator;
            _fileDialogService = fileDialogService;
            _operator.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
        }

        public InputsEmpty Inputs => _operator.Inputs;

        public AcquireImageOutputs Outputs => _operator.Outputs;

        public ObservableCollection<Record> Records => _operator.Records;

        public RunStatus RunStatus => _operator.RunStatus;

        /// <summary>
        /// 当前读取到的图片路径索引。
        /// </summary>
        public int CurrentIndex
        {
            get => _operator.CurrentIndex;
            set => _operator.CurrentIndex = value;
        }

        /// <summary>
        /// 已选择的本地图片路径列表。
        /// </summary>
        public List<string>? ImagePaths
        {
            get => _operator.ImagePaths;
            set => _operator.ImagePaths = value;
        }

        [RelayCommand]
        private async Task AcquireLocalImagesAsync()
        {
            var files = _fileDialogService?.PickImageFiles();
            if (files is null || files.Count == 0)
                return;

            ImagePaths = files.ToList();
            CurrentIndex = 0;
            await _operator.ExecuteAsync();
        }
    }
}
