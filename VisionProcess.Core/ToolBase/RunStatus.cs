using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionProcess.Core.ToolBase
{
    public class RunStatus : ObservableObject
    {
        public RunStatus()
        {
        }

        public RunStatus(string message)
        {
            _message = message;
        }

        public RunStatus(Exception exception)
        {
            _exception = exception;
        }

        private Exception? _exception;
        private DateTime _lastTime = DateTime.Now;
        private string _message = Strings.Strings.Success;
        private double _processingTime = 0;
        private bool _result = false;

        public Exception? Exception
        {
            get { return _exception; }
            internal set { SetProperty(ref _exception, value); }
        }

        public DateTime LastTime
        {
            get { return _lastTime; }
            internal set { SetProperty(ref _lastTime, value); }
        }

        public string Message
        {
            get { return _message; }
            internal set { SetProperty(ref _message, value); }
        }

        public double ProcessingTime
        {
            get { return _processingTime; }
            internal set { SetProperty(ref _processingTime, value); }
        }

        public bool Result
        {
            get { return _result; }
            internal set { SetProperty(ref _result, value); }
        }
    }
}