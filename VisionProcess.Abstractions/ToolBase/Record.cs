using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 算子运行记录。运行时用于缓存显示图像等 UI 数据；持久化时不直接保存图像，由 Persistence 层按需转换。
    /// </summary>
    public class Record : ObservableObject
    {
        private string? title;

        public string? Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private object? displayImage;

        /// <summary>
        /// 运行时显示图像，类型由具体算子决定（如 WPF 的 BitmapSource）。
        /// 不直接序列化到持久化数据。
        /// </summary>
        [JsonIgnore]
        public object? DisplayImage
        {
            get => displayImage;
            set => SetProperty(ref displayImage, value);
        }
    }
}
