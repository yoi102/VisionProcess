﻿using Newtonsoft.Json;
using OpenCvSharp;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ImageFilterOutputs : OutputsBase
    {
        private Mat? image;

        [JsonIgnore]
        public Mat? Image
        {
            get
            {
                return image;
            }
            internal set
            {
                if (image != value)
                {
                    image?.Dispose();
                    image = value;
                }
            }
        }
        [ThresholdIgnore]
        [JsonProperty(nameof(Image))]
        public byte[]? ImageBytes
        {
            get { return Image?.ToBytes(); }
            set
            {
                if (value is not null)
                {
                    Image = Cv2.ImDecode(value, ImreadModes.Unchanged);
                }
            }
        }
    }
}