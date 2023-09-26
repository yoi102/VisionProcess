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
            this.message = message;
        }

        public RunStatus(Exception exception)
        {
            this.exception = exception;
        }

        private Exception? exception;
        private DateTime lastTime = DateTime.Now;
        private string message = Strings.Strings.Success;
        private double processingTime = 0;
        private bool result = false;

        public Exception? Exception
        {
            get { return exception; }
            internal set { SetProperty(ref exception, value); }
        }

        public DateTime LastTime
        {
            get { return lastTime; }
            internal set { SetProperty(ref lastTime, value); }
        }

        public string Message
        {
            get { return message; }
            internal set { SetProperty(ref message, value); }
        }

        public double ProcessingTime
        {
            get { return processingTime; }
            internal set { SetProperty(ref processingTime, value); }
        }

        public bool Result
        {
            get { return result; }
            internal set { SetProperty(ref result, value); }
        }
    }
}