using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VisonProcess.Core.ToolBase
{
    public abstract partial class OperationBase<T1, T2, T3> : ObservableObject, IOperation where T1 : IInputs, new() where T2 : IOutputs, new() where T3 : IGraphics, new()
    {
        public OperationBase()
        {
            Inputs = new T1();
            Outputs = new T2();
            Graphic = new T3(); 


        }

        private Stopwatch? sw;

        public event EventHandler? Executed;

        public event EventHandler? Executing;

        public T3 Graphic { get; protected set; }
        public T1 Inputs { get; protected set; }
        public T2 Outputs { get; protected set; }
        public ObservableCollection<Record> Records { get; } = new ObservableCollection<Record>();
        public RunStatus RunStatus { get; } = new RunStatus();

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

        protected virtual void OnExecuted()
        {
            Executed?.Invoke(this, new EventArgs());
        }

        protected virtual void OnExecutng()
        {
            Executing?.Invoke(this, new EventArgs());
        }
    }
}