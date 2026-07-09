using System;
using System.Collections.Generic;
using System.Text;

namespace VisionProcess.Common.Events;

public record class ThemeChangedEvent(bool IsDark);

