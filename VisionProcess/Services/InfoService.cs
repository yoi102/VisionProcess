using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionProcess.Services
{
    public sealed class InfoService
    {
        private InfoService()
        {
        }
        private static readonly InfoService instance = new();
        public static InfoService Instance
        {
            get
            {
                return instance;
            }
        }

        public IEnumerable<Type>? ToolViewModelTypes { get; set; }
        public IServiceProvider? Services { get; set; }


    }
}
