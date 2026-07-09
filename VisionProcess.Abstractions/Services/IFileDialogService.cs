namespace VisionProcess.Abstractions.Services
{
    /// <summary>
    /// 文件选择服务的抽象，使 ViewModel/算子可以在需要时打开文件对话框，
    /// 而不直接引用 WPF 或 Windows API。
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// 弹出图片多选对话框，返回选中的文件路径；取消或失败时返回 <c>null</c>。
        /// </summary>
        IReadOnlyList<string>? PickImageFiles();
    }
}
