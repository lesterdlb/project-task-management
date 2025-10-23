using System.Reflection;

namespace ProjectManagement.Api.Common.Slices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSlices(this IServiceCollection services)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        var slices = currentAssembly.GetTypes().Where(t =>
            typeof(ISlice).IsAssignableFrom(t) &&
            t != typeof(ISlice) &&
            t is { IsPublic: false, IsAbstract: false });

        foreach (var slice in slices)
        {
            services.AddSingleton(typeof(ISlice), slice);
        }

        return services;
    }
}
