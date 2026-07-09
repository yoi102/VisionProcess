using VisionProcess.Runtime.Models;

namespace VisionProcess.Runtime.Services
{
    /// <summary>
    /// 对话框服务的抽象，使 Runtime 中的模型可以在需要时打开 UI 对话框，
    /// 而不直接引用 WPF 或 Application 层。
    /// </summary>
    public interface IWindowDialogService
    {
        void OpenIOConnectorDialog(OperationModel operationModel);
    }
}
