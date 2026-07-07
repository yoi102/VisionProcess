using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using VisionProcess.Lang.Strings;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Extensions;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.ViewModels
{
    public partial class AcquireImageViewModel : OperatorBase<InputsEmpty, AcquireImageOutputs, GraphicsEmpty>
    {
        public AcquireImageViewModel() : base()
        {
            Init();
        }

        [JsonConstructor]
        public AcquireImageViewModel(InputsEmpty inputs, AcquireImageOutputs outputs, GraphicsEmpty graphics, RunStatus runStatus)
            : base(inputs, outputs, graphics, runStatus)
        {
        }

        /// <summary>
        /// 当前读取到的图片路径索引。会随文档一起保存。
        /// </summary>
        [JsonProperty]
        public int CurrentIndex { get; set; }

        /// <summary>
        /// 已选择的本地图片路径列表。会随文档一起保存。
        /// </summary>
        [JsonProperty]
        public List<string>? ImagePaths { get; set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Outputs.Image is not null && Records.Count > 0)
            {
                Records[^1].DisplayImage = Outputs.Image.ToFrozenBitmapSource();
            }
        }

        protected override bool InternalExecute(out string message)
        {
            if (ImagePaths == null || ImagePaths.Count < 1)
            {
                message = Strings.PleaseSelectFiles;
                return false;
            }

            if (CurrentIndex > ImagePaths.Count - 1)
            {
                CurrentIndex = 0;
            }
            Outputs.Image = new Mat(ImagePaths[CurrentIndex]);
            Records[^1].DisplayImage = Outputs.Image.ToFrozenBitmapSource();

            CurrentIndex++;
            message = Strings.Success;
            return true;
        }

        protected override IEnumerable<DefaultConnectorInfo> GetDefaultConnectors()
        {
            yield return new DefaultConnectorInfo(false, "Image", "Outputs.Image");
        }

        [property: JsonIgnore]
        [RelayCommand]
        private async Task AcquireLocalImagesAsync()
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
                ImagePaths = dialog.FileNames.ToList();
                CurrentIndex = 0;
                await ExecuteAsync();
            }
        }

        private void Init()
        {
            Records.Add(new() { Title = Strings.OutputImage });
        }
    }
}