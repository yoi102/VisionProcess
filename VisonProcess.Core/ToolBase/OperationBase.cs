using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace VisonProcess.Core.ToolBase
{
    public abstract partial class OperationBase<T1, T2, T3> : ObservableObject, IOperation where T1 : class, IInputs, new() where T2 : class, IOutputs, new() where T3 : class, IGraphics, new()
    {

        public OperationBase()
        {
            Inputs = new T1();
            Outputs = new T2();
            Graphic = new T3();
        }
        public event EventHandler? Executed;
        public event EventHandler? Executing;

        public RunStatus RunStatus { get; } = new RunStatus();
        public ObservableCollection<Record> Records {  get; } = new ObservableCollection<Record>();
        private Stopwatch? sw;

        public T1 Inputs { get; protected set; }

        public T2 Outputs { get; protected set; }
        public T3 Graphic { get; protected set; }

        [RelayCommand]
        public void Execute()
        {
            OnExecutng();

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
                RunStatus.Result = InternalExecute(out string message);
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
                OnExecuted();
            }
        }





        protected abstract bool InternalExecute(out string message);

        protected virtual void OnExecutng()
        {
            Executing?.Invoke(this, new EventArgs());
        }

        protected virtual void OnExecuted()
        {
            Executed?.Invoke(this, new EventArgs());
        }







    }
}
