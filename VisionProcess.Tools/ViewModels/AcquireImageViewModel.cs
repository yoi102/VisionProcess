using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Strings;
using VisionProcess.Core.ToolBase;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.ViewModels
{
    [DefaultToolConnector(false, "Image", "Outputs.Image")]
    public partial class AcquireImageViewModel : OperationBase<InputsEmpty, AcquireImageOutput, GraphicsEmpty>
    {
        public AcquireImageViewModel() : base()
        {
            Init();
        }

        [JsonConstructor]
        public AcquireImageViewModel(RunStatus runStatus) : base(runStatus)
        {
        }

        private int currentIndex = 0;
        private List<string>? imagePaths;

        public int CurrentIndex
        {
            get { return currentIndex; }
            set { currentIndex = value; }
        }
        public List<string>? ImagePaths
        {
            get { return imagePaths; }
            set { imagePaths = value; }
        }

        protected override bool InternalExecute(out string message)
        {
            if (imagePaths == null || imagePaths.Count < 1)
            {
                message = Strings.PleaseSelectFiles;
                //message = "Please select image files";
                return false;
            }

            if (currentIndex > imagePaths.Count - 1)
            {
                currentIndex = 0;
            }
            Outputs.Image = new Mat(imagePaths[currentIndex]);
            Records[^1].DisplayImage = Outputs.Image.ToBitmapSource();

            currentIndex++;
            ////延时
            //Thread.Sleep(1000);
            message = Strings.Success;
            return true;
        }

        [property: JsonIgnore]
        [RelayCommand]
        private void AcquireLocalImages()
        {
            var dialog = new OpenFileDialog();
            //dialog.FileName = "Document"; // Default file name
            dialog.Multiselect = true;
            //dialog.DefaultExt = ".txt"; // Default file extension
            dialog.Filter = $"{Strings.ImageFiles}  (*.jpg*.bmp*.png)|*.jpg;*.bmp;*.png"; // Filter files by extension
            dialog.Title = $"{Strings.PleaseSelectFiles}";
            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                //string filename = dialog.FileName;
                imagePaths = dialog.FileNames.ToList();
                currentIndex = 0;
                Execute();
            }
        }

        private void Init()
        {
            Records.Add(new() { Title = Strings.OutputImage });
        }
    }
}