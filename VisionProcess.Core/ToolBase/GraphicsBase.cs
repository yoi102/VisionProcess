using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionProcess.Core.ToolBase
{
    /// <summary>
    /// To be added
    /// </summary>
    public abstract class GraphicsBase : ObservableObject, IGraphics
    {
    }

    public class GraphicsEmpty : GraphicsBase
    {
    }
}