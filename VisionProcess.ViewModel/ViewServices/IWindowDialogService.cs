using VisionProcess.Runtime.Models;

namespace VisionProcess.ViewModel.ViewServices;

/// <summary>
/// Opens view-specific dialogs requested by editor commands.
/// </summary>
public interface IWindowDialogService
{
    void OpenIOConnectorDialog(OperationModel operationModel);
}
