using System.Windows;

namespace VisionProcess.Runtime.Models
{
    public class OperationGroupModel : OperationModel
    {
        public OperationGroupModel() : base()
        {
        }

        public OperationGroupModel(Guid id) : base(id)
        {
        }

        private Size _size;

        public Size GroupSize
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }
    }
}