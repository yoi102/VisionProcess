using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nodify;
using System.Windows;
using VisonProcess.Core.Extentions;

namespace VisonProcess.Core.Mvvm
{
    public partial class ProcessModel : ObservableObject
    {
        public ProcessModel()
        {




        }





        #region Properties

        private NodifyObservableCollection<OperationModel> _operations = new();
        private NodifyObservableCollection<OperationModel> _selectedOperations = new();

        public NodifyObservableCollection<ConnectionModel> Connections { get; } = new();
        public PendingConnectionModel PendingConnection { get; set; } = new();

        public NodifyObservableCollection<OperationModel> Operations
        {
            get => _operations;
            set => SetProperty(ref _operations, value);
        }

        public NodifyObservableCollection<OperationModel> SelectedOperations
        {
            get => _selectedOperations;
            set => SetProperty(ref _selectedOperations, value);
        }
        #endregion Properties

        #region Commands

        [RelayCommand]
        private void DeleteSelection()
        {
            //var selected = SelectedOperations.ToList();
            SelectedOperations.ForEach(o => Operations.Remove(o));

        }

        [RelayCommand]
        private void DisconnectConnector(ConnectorModel connector)
        {
            var connections = Connections.Where(c => c.Input == connector || c.Output == connector);
            connections.ForEach(c => Connections.Remove(c));
        }

        [RelayCommand(CanExecute = nameof(CanGroupSelection))]
        private void GroupSelection()
        {
            //var selected = SelectedOperations.ToList();
            var bounding = SelectedOperations.GetBoundingBox(50);

            Operations.Add(new OperationGroupModel
            {
                Title = "Operations",
                Location = bounding.Location,
                GroupSize = new Size(bounding.Width, bounding.Height)
            });

        }

        private bool CanGroupSelection()
        {
            return SelectedOperations.Count > 0;

        }

        [RelayCommand]
        private void StartConnection()
        {
            PendingConnection.IsVisible = true;
        }
        #endregion Commands
    }
}