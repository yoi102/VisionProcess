using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionProcess.Models
{
    public class ConnectionModel : ObservableObject
    {
        private ConnectorModel? input;

        public ConnectorModel? Input
        {
            get => input;
            set => SetProperty(ref input, value);
        }

        private ConnectorModel? output;

        public ConnectorModel? Output
        {
            get => output;
            set => SetProperty(ref output, value);
        }
    }
}