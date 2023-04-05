using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VisonProcess.ViewModels
{
    public partial class OperationsMenuViewModel:ObservableObject
    {
        public OperationsMenuViewModel()
        {
            


        }
        public event Action? Closed;

        [ObservableProperty]
        private Point _location;

        [ObservableProperty]
        private bool _isVisible;



        public void OpenAt(Point targetLocation)
        {
            Close();
            Location = targetLocation;
            IsVisible = true;
        }

        public void Close()
        {
            IsVisible = false;
        }


        private void CreateOperation()
        {

        }






    }
}
