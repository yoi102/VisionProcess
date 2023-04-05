using System.Windows;

namespace VisonProcess.Core.Mvvm
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