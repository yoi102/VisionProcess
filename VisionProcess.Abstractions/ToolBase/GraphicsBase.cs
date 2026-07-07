using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionProcess.Abstractions.ToolBase
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