namespace VisionProcess.Abstractions.Attributes
{
    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class ThresholdIgnoreAttribute : Attribute
    { }
}