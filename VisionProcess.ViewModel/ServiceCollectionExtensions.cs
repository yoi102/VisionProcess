using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using VisionProcess.Runtime.Models;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.ViewModel.ViewModels.Toolboxes;

namespace VisionProcess.ViewModel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<EditorViewModel>();
        services.AddTransient<FolderExplorerToolboxViewModel>();
        services.AddTransient<PropertiesToolboxViewModel>();

        return services;
    }
}
