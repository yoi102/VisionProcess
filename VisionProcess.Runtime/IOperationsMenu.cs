using VisionProcess.Common.Primitives;

namespace VisionProcess.Runtime
{
    /// <summary>
    /// 操作菜单的抽象，用于在 Runtime 的 <see cref="Models.ProcessModel"/> 中保持对菜单的弱引用，
    /// 避免 Runtime 直接依赖 Application 层。
    /// </summary>
    public interface IOperationsMenu
    {
        void OpenAt(VpPoint location);

        void Close();
    }
}
