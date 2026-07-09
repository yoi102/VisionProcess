using System;
using System.Collections.Generic;
using System.Text;

namespace VisionProcess.ViewModel.ViewServices;

public interface IThemeSettingService
{
    bool IsDarkTheme { get; }

    void ToggleThemeLightDark();

    void ApplyThemeLightDark(bool isDarkTheme);
}
