using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VisionProcess.Core.ToolBase
{
    public abstract partial class OperationBase<TInputs, TOutputs, TGraphic> : ObservableObject, IOperation where TInputs : InputsBase, new() where TOutputs : OutputsBase, new() where TGraphic : GraphicsBase, new()
    {
        [ObservableProperty]
        public string? name;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private bool isRealTime;

        //子类可依赖注入
        protected OperationBase()
        {
        }

        protected OperationBase(TGraphic graphic, TInputs inputs,
            TOutputs outputs, bool isRealTime, ObservableCollection<Record> records, RunStatus runStatus)
        {
            Graphic = graphic;
            Inputs = inputs;
            Outputs = outputs;
            IsRealTime = isRealTime;
            Records = records;
            RunStatus = runStatus;
        }

        public event EventHandler? Executed;

        public event EventHandler? Executing;

        public TGraphic Graphic { get; protected set; } = new TGraphic();

        public TInputs Inputs { get; protected set; } = new TInputs();

        public bool IsRealTime
        {
            get { return isRealTime; }
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
                SetProperty(ref isRealTime, value);
            }
        }

        public TOutputs Outputs { get; protected set; } = new TOutputs();

        public ObservableCollection<Record> Records { get; } = new ObservableCollection<Record>();

        public RunStatus RunStatus { get; } = new RunStatus();

        public void Execute()
        {
            OnExecuting();

            stopwatch.Reset();
            stopwatch.Start();

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
                stopwatch.Stop();
                RunStatus.ProcessingTime = stopwatch.ElapsedMilliseconds;
                OnExecuted();
            }
        }

        protected abstract bool InternalExecute(out string message);

        protected virtual void OnExecuted()
        {
            Executed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnExecuting()
        {
            Executing?.Invoke(this, EventArgs.Empty);
        }

        [property: JsonIgnore]
        [RelayCommand]
        private async Task ExecuteAsync()
        {
            await Task.Run(() => Execute());
        }

        private void Inputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Execute();
        }
    }
}