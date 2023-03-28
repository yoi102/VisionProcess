using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisonProcess.Core.ToolBase
{
    public interface IOperation
    {

        RunStatus RunStatus { get; }

        event EventHandler? Ran;
        event EventHandler? Running;


        void Execute( );

    }
}
