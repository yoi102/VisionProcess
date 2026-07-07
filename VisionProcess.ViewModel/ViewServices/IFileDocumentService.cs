using VisionProcess.Application.ViewModels;

namespace VisionProcess.ViewModel.ViewServices
{
    /// <summary>
    /// 提供单个文档（Editor）的保存与加载能力。
    /// </summary>
    public interface IFileDocumentService
    {
        /// <summary>
        /// 保存指定文档。若文档已有 <see cref="EditorViewModel.FilePath"/> 则直接覆盖，否则弹出保存对话框。
        /// </summary>
        void SaveDocument(EditorViewModel document);

        /// <summary>
        /// 始终弹出保存对话框，将文档另存为新文件。
        /// </summary>
        void SaveDocumentAs(EditorViewModel document);

        /// <summary>
        /// 从用户选择的文件加载一个文档，并返回其 ViewModel；取消或失败时返回 <c>null</c>。
        /// </summary>
        EditorViewModel? LoadDocument();
    }
}
