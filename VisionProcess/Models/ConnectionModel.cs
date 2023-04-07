using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionProcess.Models
{
    public class ConnectionModel : ObservableObject
    {
        private ConnectorModel _input = default!;

        public ConnectorModel Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        private ConnectorModel _output = default!;

        public ConnectorModel Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }
    }
}