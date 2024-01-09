﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace VisionProcess.Core.ToolBase
{
    public abstract partial class OperatorBase<TInputs, TOutputs, TGraphics>
        : ObservableObject, IOperator
        where TInputs : IInputs, new() where TOutputs : IOutputs, new() where TGraphics : IGraphics, new()
    {
        [ObservableProperty]
        public string? name;

        private readonly Stopwatch stopwatch = new();
        private bool isRealTime;

        //子类可依赖注入
        protected OperatorBase()
        {
            SubscribePropertyChanged();
        }

        protected OperatorBase(TInputs inputs, TOutputs outputs, TGraphics graphics, RunStatus runStatus)
        {
            Inputs = inputs;
            Outputs = outputs;
            Graphics = graphics;
            RunStatus = runStatus;
            SubscribePropertyChanged();
        }

        public event EventHandler? Executed;

        public event EventHandler? Executing;

        public event PropertyChangedEventHandler? GraphicsPropertyChanged;

        public event PropertyChangedEventHandler? InputsPropertyChanged;

        public event PropertyChangedEventHandler? OutputsPropertyChanged;

        public TGraphics Graphics { get; } = new TGraphics();

        public TInputs Inputs { get; } = new TInputs();

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

        public TOutputs Outputs { get; } = new TOutputs();

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

        private void ExecuteWhenInputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Execute();
        }

        private void Graphics_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            GraphicsPropertyChanged?.Invoke(sender, e);
        }

        private void Inputs_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            InputsPropertyChanged?.Invoke(sender, e);
        }

        private void Outputs_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OutputsPropertyChanged?.Invoke(sender, e);
        }

        private void SubscribePropertyChanged()
        {
            //生命周期一样，不需要取消订阅了
            Inputs.PropertyChanged += Inputs_PropertyChanged;
            Outputs.PropertyChanged += Outputs_PropertyChanged;
            Graphics.PropertyChanged += Graphics_PropertyChanged; ;
        }
    }
}