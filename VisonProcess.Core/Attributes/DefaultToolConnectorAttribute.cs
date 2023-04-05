namespace VisonProcess.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class DefaultToolConnectorAttribute : Attribute
    {
        public DefaultToolConnectorAttribute(bool isInput, string title, string path)
        {
            IsInput = isInput;
            Title = title;
            Path = path;
        }

        public bool IsInput { get; }
        public string Title { get; }
        public string Path { get; }
    }
}