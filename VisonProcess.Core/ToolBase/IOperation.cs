namespace VisonProcess.Core.ToolBase
{
    public interface IOperation
    {
        event EventHandler? Executed;

        event EventHandler? Executing;

        //IGraphics Graphic { get; }
        //IInputs Inputs { get; }
        //IOutputs Outputs { get; }
        RunStatus RunStatus { get; }

        void Execute();
    }
}