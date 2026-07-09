using VisionProcess.Common.Primitives;

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

        private VpSize _size;

        public VpSize GroupSize
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }
    }
}