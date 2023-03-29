using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisonProcess.Core.ToolBase
{
    public interface IOperation<T1, T2, T3> where T1 : class, IInputs, new() where T2 : class, IOutputs, new() where T3 : class, IGraphics, new()
    {

        RunStatus RunStatus { get; }

        event EventHandler? Executed;
        event EventHandler? Executing;
        T1 Inputs { get;}
        T2 Outputs { get;}
        T3 Graphic { get;}

        void Execute();

    }
}
