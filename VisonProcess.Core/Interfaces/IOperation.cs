using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisonProcess.Core.Interfaces
{
    public interface IOperation
    {
        bool Execute(out string message);

    }
}
