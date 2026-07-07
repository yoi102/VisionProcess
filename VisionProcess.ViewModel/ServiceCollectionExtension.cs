using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using VisionProcess.Application.ViewModels;

namespace VisionProcess.ViewModel;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();


        return services;
    }
}
