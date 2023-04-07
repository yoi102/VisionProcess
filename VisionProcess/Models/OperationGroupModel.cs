using System.Windows;

namespace VisionProcess.Models
{
    public class OperationGroupModel : OperationModel
    {
        private Size _size;

        public Size GroupSize
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }
    }
}