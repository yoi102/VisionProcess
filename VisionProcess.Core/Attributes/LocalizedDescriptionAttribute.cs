using System.ComponentModel;
using System.Resources;

namespace VisionProcess.Core.Attributes
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private ResourceManager resourceManager;
        private string resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            resourceManager = new ResourceManager(resourceType);
            this.resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string? description = resourceManager.GetString(resourceKey);
                return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", resourceKey) : description;
            }
        }
    }
}