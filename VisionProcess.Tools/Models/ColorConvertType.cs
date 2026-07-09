using System.ComponentModel;
using VisionProcess.Common;

namespace VisionProcess.Tools.Models
{
    /// <summary>
    /// 颜色转换类型。这是业务/视图层自己的枚举，避免 View 直接绑定 OpenCvSharp 的枚举。
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ColorConvertType
    {
        [Description("BGR to Gray")]
        BGR2GRAY,

        [Description("BGR to RGB")]
        BGR2RGB,

        [Description("BGR to HSV")]
        BGR2HSV,

        [Description("BGR to HLS")]
        BGR2HLS,

        [Description("BGR to YUV")]
        BGR2YUV,

        [Description("YUV to BGR")]
        YUV2BGR,

        [Description("BGR to Lab")]
        BGR2Lab,

        [Description("Lab to BGR")]
        Lab2BGR,

        [Description("RGB to Gray")]
        RGB2GRAY,

        [Description("RGB to BGR")]
        RGB2BGR,

        [Description("Gray to BGR")]
        GRAY2BGR,
    }
}
