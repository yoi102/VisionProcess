using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionProcess.Core.ToolBase
{
    /// <summary>
    /// To be added
    /// </summary>
    public abstract class OutputsBase : ObservableObject, IOutputs
    {
    }

    public class OutputsEmpty : OutputsBase
    {
    }
}