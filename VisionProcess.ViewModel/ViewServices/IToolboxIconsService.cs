using System;
using System.Collections.Generic;
using System.Text;

namespace VisionProcess.ViewModel.ViewServices;

public interface IToolboxIconsService
{
    object Explorer { get; }
    object Layers { get; }
    object Terminal { get; }
    object Search { get; }
    object Git { get; }
    object Problems { get; }
}
