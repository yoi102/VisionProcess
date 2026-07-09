using MaterialDesignThemes.Wpf;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Text;
using VisionProcess.Common.Events;
using VisionProcess.ViewModel.ViewServices;

namespace VisionProcess.wpf.Services;



internal class ThemeSettingService : IThemeSettingService
{
    private readonly PaletteHelper _paletteHelper;
    private readonly Theme _theme;
    private readonly IPublisher<ThemeChangedEvent> _publisher;

    public ThemeSettingService(IPublisher<ThemeChangedEvent> publisher)
    {
        _paletteHelper = new PaletteHelper();
        _theme = _paletteHelper.GetTheme();
        _publisher = publisher;
    }

    public bool IsDarkTheme
    {
        get
        {
            var currentBaseTheme = _theme.GetBaseTheme();
            return currentBaseTheme == BaseTheme.Dark;
        }
    }

    public void ApplyThemeLightDark(bool isDarkTheme)
    {
        if (isDarkTheme)
        {
            _theme.SetDarkTheme();
        }
        else
        {
            _theme.SetLightTheme();
        }
        _paletteHelper.SetTheme(_theme);
        _publisher.Publish(new ThemeChangedEvent(isDarkTheme));
    }

    public void ToggleThemeLightDark()
    {
        var currentBaseTheme = _theme.GetBaseTheme();
        if (currentBaseTheme != BaseTheme.Dark)
        {
            _theme.SetDarkTheme();
        }
        else
        {
            _theme.SetLightTheme();
        }
        _paletteHelper.SetTheme(_theme);
    }
}
