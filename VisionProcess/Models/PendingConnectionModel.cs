using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace VisionProcess.Models
{
    public class PendingConnectionModel : ObservableObject
    {
        private ConnectorModel source = default!;

        public ConnectorModel Source
        {
            get => source;
            set => SetProperty(ref source, value);
        }

        private ConnectorModel? target;

        public ConnectorModel? Target
        {
            get => target;
            set => SetProperty(ref target, value);
        }

        private bool isVisible;

        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty(ref isVisible, value);
        }

        private Point targetLocation;

        public Point TargetLocation
        {
            get => targetLocation;
            set => SetProperty(ref targetLocation, value);
        }
    }
}