using MessagePack;
using VisionProcess.Tools.Models;

namespace VisionProcess.Persistence.Operators
{
    /// <summary>
    /// 颜色转换算子的持久化数据。
    /// </summary>
    [MessagePackObject]
    public class ColorConvertOperatorDto : OperatorDto
    {
        [Key(2)]
        public ColorConvertType ColorConversionCodes { get; set; }
    }
}
