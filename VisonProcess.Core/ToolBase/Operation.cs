using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace VisonProcess.Core.ToolBase
{
    public abstract class Operation<T1, T2, T3> : IOperation where T1 : class, IInputs, new() where T2 : class, IOutputs, new() where T3 : class, IGraphics, new()
    {

        public Operation()
        {
            Inputs = new T1();
            Outputs = new T2();
            Graphic = new T3();

        }
        public event EventHandler? Ran;
        public event EventHandler? Running;

        public  RunStatus RunStatus { get; protected set; } = new RunStatus();
        private Stopwatch? sw;

        public T1 Inputs { get; protected set; }

        public T2 Outputs { get; protected set; }
        public T3 Graphic { get; protected set; }


        public void Execute()
        {
            OnRunning();

            sw ??= new Stopwatch();
            sw.Reset();
            sw.Start();

            RunStatus.Exception = null;
            RunStatus.Message = "";
            RunStatus.ProcessingTime = 0;
            RunStatus.Result = true;

            try
            {
                RunStatus.LastTime = DateTime.Now;
                RunStatus.Result = Run(out string message);
                RunStatus.Message = message;

            }

            catch (Exception ex)
            {
                RunStatus.Result = false;
                RunStatus.Exception = ex;
                RunStatus.Message = ex.Message;

            }
            finally
            {
                sw.Stop();
                RunStatus.ProcessingTime = sw.ElapsedMilliseconds;
                OnRan();
            }
        }



        protected abstract bool Run(out string message);

        protected virtual void OnRunning()
        {
            Running?.Invoke(this, new EventArgs());
        }

        protected virtual void OnRan()
        {
            Ran?.Invoke(this, new EventArgs());
        }







    }
}
