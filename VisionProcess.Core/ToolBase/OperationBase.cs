using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VisionProcess.Core.ToolBase
{
    public abstract partial class OperationBase<TInputs, TOutputs, TGraphic> : ObservableObject, IOperation where TInputs : InputsBase, new() where TOutputs : OutputsBase, new() where TGraphic : GraphicsBase, new()
    {

        //子类可依赖注入
        protected OperationBase()
        {

        }

        private Stopwatch? sw;

        public event EventHandler? Executed;

        public event EventHandler? Executing;

        public TGraphic Graphic { get; protected set; } = new TGraphic();

        public TInputs Inputs { get; protected set; } = new TInputs();

        public TOutputs Outputs { get; protected set; } = new TOutputs();

        public ObservableCollection<Record> Records { get; } = new ObservableCollection<Record>();

        public RunStatus RunStatus { get; } = new RunStatus();

        private bool _IsRealTime;
        public bool IsRealTime
        {
            get { return _IsRealTime; }
            set
            {
                if (value)
                {
                    Inputs.PropertyChanged -= Inputs_PropertyChanged;
                    Inputs.PropertyChanged += Inputs_PropertyChanged;
                }
                else
                {
                    Inputs.PropertyChanged -= Inputs_PropertyChanged;
                }
                SetProperty(ref _IsRealTime, value);
            }
        }





        [ObservableProperty]
        public string? name;

        [RelayCommand]
        private async Task ExecuteAsync()
        {
            await Task.Run(() => Execute());
        }

        public void Execute()
        {
            OnExecutng();

            sw ??= new Stopwatch();
            sw.Reset();
            sw.Start();

            RunStatus.Exception = null;

            try
            {
                RunStatus.LastTime = DateTime.Now;
                RunStatus.Result = InternalExecute(out string message);
                RunStatus.Message = message;
            }
            catch (OpenCVException ex)
            {
                RunStatus.Result = false;
                RunStatus.Exception = ex;
                RunStatus.Message = ex.Message;
            }
            catch (ArgumentNullException ex)
            {
                RunStatus.Result = false;
                RunStatus.Exception = ex;
                RunStatus.Message = ex.Message;
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
            Executed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnExecutng()
        {
            Executing?.Invoke(this, EventArgs.Empty);
        }
        private void Inputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Execute();
        }
    }
}