using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using VisionProcess.ViewModel.Tooling;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.ViewModel.ViewModels.Toolboxes;

namespace VisionProcess.ViewModel;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<IEditorViewModelFactory, EditorViewModelFactory>();
        services.AddTransient<FolderExplorerToolboxViewModel>();
        services.AddTransient<PropertiesToolboxViewModel>();

        return services;
    }
}
