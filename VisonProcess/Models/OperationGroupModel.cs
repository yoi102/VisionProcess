using System.Windows;

namespace VisonProcess.Models
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