﻿using System.ComponentModel;
using System.Resources;

namespace VisionProcess.Core.Attributes
{
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private ResourceManager _resourceManager;
        private string _resourceKey;

        public LocalizedDescriptionAttribute(string resourceKey, Type resourceType)
        {
            _resourceManager = new ResourceManager(resourceType);
            _resourceKey = resourceKey;
        }

        public override string Description
        {
            get
            {
                string? description = _resourceManager.GetString(_resourceKey);
                return string.IsNullOrWhiteSpace(description) ? string.Format("[[{0}]]", _resourceKey) : description;
            }
        }
    }
}