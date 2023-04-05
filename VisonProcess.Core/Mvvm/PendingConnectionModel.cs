using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace VisonProcess.Core.Mvvm
{
    public class PendingConnectionModel : ObservableObject
    {
        private ConnectorModel _source = default!;

        public ConnectorModel Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        private ConnectorModel? _target;

        public ConnectorModel? Target
        {
            get => _target;
            set => SetProperty(ref _target, value);
        }

        private bool _isVisible;

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        private Point _targetLocation;

        public Point TargetLocation
        {
            get => _targetLocation;
            set => SetProperty(ref _targetLocation, value);
        }
    }
}