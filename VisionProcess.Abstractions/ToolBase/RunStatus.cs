using CommunityToolkit.Mvvm.ComponentModel;
using VisionProcess.Abstractions.Attributes;

namespace VisionProcess.Abstractions.ToolBase
{
    public class RunStatus : ObservableObject
    {
        private Exception? exception;

        private DateTime lastTime = DateTime.Now;

        private string message = "";

        private double processingTime = 0;

        private bool result = false;

        public RunStatus()
        {
        }

        public RunStatus(string message)
        {
            this.message = message;
        }

        public RunStatus(Exception exception)
        {
            this.exception = exception;
        }

        [ThresholdIgnore]
        public Exception? Exception
        {
            get { return exception; }
            internal set { SetProperty(ref exception, value); }
        }

        public DateTime LastTime
        {
            get { return lastTime; }
            set { SetProperty(ref lastTime, value); }
        }

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        public double ProcessingTime
        {
            get { return processingTime; }
            set { SetProperty(ref processingTime, value); }
        }

        public bool Result
        {
            get { return result; }
            set { SetProperty(ref result, value); }
        }
    }
}