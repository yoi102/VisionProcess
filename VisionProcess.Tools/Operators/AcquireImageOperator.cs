using System;
using System.Collections.Generic;
using OpenCvSharp;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Lang.Strings;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.Operators
{
    /// <summary>
    /// 图片采集算子。负责从本地图片路径读取图像。
    /// </summary>
    public sealed class AcquireImageOperator : OperatorBase<InputsEmpty, AcquireImageOutputs, GraphicsEmpty>
    {
        public AcquireImageOperator()
        {
            Init();
        }

        /// <summary>
        /// 当前读取到的图片路径索引。
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// 已选择的本地图片路径列表。
        /// </summary>
        public List<string>? ImagePaths { get; set; }

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
            Records[^1].DisplayImage = Outputs.Image;

            CurrentIndex++;
            message = Strings.Success;
            return true;
        }

        protected override IEnumerable<DefaultConnectorInfo> GetDefaultConnectors()
        {
            yield return new DefaultConnectorInfo(false, "Image", "Outputs.Image");
        }

        private void Init()
        {
            Records.Add(new() { Title = Strings.OutputImage });
        }
    }
}
