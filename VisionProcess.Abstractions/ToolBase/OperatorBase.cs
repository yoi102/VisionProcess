using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using VisionProcess.Abstractions.Attributes;

namespace VisionProcess.Abstractions.ToolBase
{
    public abstract partial class OperatorBase<TInputs, TOutputs, TGraphics>
        : ObservableObject, IOperator, IConnectorProvider
        where TInputs : IInputs, new() where TOutputs : IOutputs, new() where TGraphics : IGraphics, new()
    {
        [ObservableProperty]
        private string? name;
        private readonly object locker = new object();
        private readonly Stopwatch stopwatch = new();
        private bool isRealTime;

        //子类可依赖注入
        protected OperatorBase()
        {
        }

        protected OperatorBase(TInputs inputs, TOutputs outputs, TGraphics graphics, RunStatus runStatus)
        {
            Inputs = inputs;
            Outputs = outputs;
            Graphics = graphics;
            RunStatus = runStatus;
        }
        public event EventHandler? Executed;

        public event EventHandler? Executing;
        [ThresholdIgnore]
        public TGraphics Graphics { get; set; } = new TGraphics();
        IGraphics IOperator.Graphics => Graphics;
        public TInputs Inputs { get; set; } = new TInputs();
        IInputs IOperator.Inputs => Inputs;

        public bool IsRealTime
        {
            get { return isRealTime; }
            set
            {
                if (value)
                {
                    Inputs.PropertyChanged -= ExecuteWhenInputs_PropertyChanged;
                    Inputs.PropertyChanged += ExecuteWhenInputs_PropertyChanged;
                }
                else
                {
                    Inputs.PropertyChanged -= ExecuteWhenInputs_PropertyChanged;
                }
                SetProperty(ref isRealTime, value);
            }
        }

        IOutputs IOperator.Outputs => Outputs;
        public TOutputs Outputs { get; set; } = new TOutputs();
        //[ThresholdIgnore]
        public ObservableCollection<Record> Records { get; set; } = new ObservableCollection<Record>();

        public RunStatus RunStatus { get; set; } = new RunStatus();

        private void Execute()
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

        /// <summary>
        /// 返回算子默认提供的输入/输出连接器。
        /// 派生类重写此方法以替代 <see cref="DefaultToolConnectorAttribute"/> 的反射读取。
        /// </summary>
        protected virtual IEnumerable<DefaultConnectorInfo> GetDefaultConnectors() => Enumerable.Empty<DefaultConnectorInfo>();

        IEnumerable<DefaultConnectorInfo> IOperator.GetDefaultConnectors() => GetDefaultConnectors();

        protected virtual void OnExecuted()
        {
            Executed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnExecuting()
        {
            Executing?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        public async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                lock (locker)
                {
                    Execute();
                }
            });
        }

        private void ExecuteWhenInputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Execute();
        }

        #region IConnectorProvider

        IEnumerable<IConnectorDescriptor> IConnectorProvider.GetConnectors()
        {
            foreach (var descriptor in Inputs.GetConnectors())
            {
                yield return new ConnectorDescriptor(
                    "Inputs." + descriptor.Path,
                    descriptor.Title,
                    descriptor.ValueType,
                    () => Inputs.GetValue(descriptor.Path),
                    descriptor.CanWrite ? v => Inputs.SetValue(descriptor.Path, v) : null);
            }

            foreach (var descriptor in Outputs.GetConnectors())
            {
                yield return new ConnectorDescriptor(
                    "Outputs." + descriptor.Path,
                    descriptor.Title,
                    descriptor.ValueType,
                    () => Outputs.GetValue(descriptor.Path),
                    null);
            }
        }

        object? IConnectorProvider.GetValue(string path)
        {
            if (TrySplitConnectorPath(path, out var provider, out var subPath))
            {
                return provider.GetValue(subPath);
            }
            return null;
        }

        void IConnectorProvider.SetValue(string path, object? value)
        {
            if (TrySplitConnectorPath(path, out var provider, out var subPath))
            {
                provider.SetValue(subPath, value);
            }
        }

        Type? IConnectorProvider.GetValueType(string path)
        {
            if (TrySplitConnectorPath(path, out var provider, out var subPath))
            {
                return provider.GetValueType(subPath);
            }
            return null;
        }

        private bool TrySplitConnectorPath(string path, out IConnectorProvider provider, out string subPath)
        {
            provider = null!;
            subPath = string.Empty;

            const string inputsPrefix = "Inputs.";
            const string outputsPrefix = "Outputs.";

            if (path.StartsWith(inputsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                provider = Inputs;
                subPath = path[inputsPrefix.Length..];
                return true;
            }

            if (path.StartsWith(outputsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                provider = Outputs;
                subPath = path[outputsPrefix.Length..];
                return true;
            }

            return false;
        }

        #endregion IConnectorProvider
    }
}