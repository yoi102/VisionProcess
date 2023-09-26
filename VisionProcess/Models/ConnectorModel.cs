using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;

namespace VisionProcess.Models
{
    public class ConnectorModel : ObservableObject
    {
        public ConnectorModel(string title, Type valueType,
            string valuePath, bool isInput, Guid ownerGuid)
        {
            this.title = title;
            this.valueType = valueType;
            this.valuePath = valuePath;
            this.isInput = isInput;
            this.ownerGuid = ownerGuid;
        }

        private Point anchor = default;
        private bool isConnected = false;
        private readonly bool isInput;
        private Guid ownerGuid;
        private string title;
        private string valuePath;
        private Type valueType;

        public string ValuePath
        {
            get { return valuePath; }
            protected set { SetProperty(ref valuePath, value); }
        }

        public Point Anchor
        {
            get => anchor;
            set => SetProperty(ref anchor, value);
        }

        public bool IsConnected
        {
            get => isConnected;
            set => SetProperty(ref isConnected, value);
        }

        public bool IsInput
        {
            get => isInput;
            //set => SetProperty(ref isInput, value);
        }

        public Guid OwnerGuid
        {
            get => ownerGuid;
            set => SetProperty(ref ownerGuid, value);
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        [JsonIgnore]
        public List<ConnectorModel> ValueObservers { get; } = new();

        [JsonIgnore]
        public Type ValueType
        {
            get => valueType;
            protected set => SetProperty(ref valueType, value);
        }
    }
}