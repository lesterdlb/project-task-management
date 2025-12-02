using System.Reflection;

namespace ProjectManagement.Api.Infrastructure.Slices;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddSlices()
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
        }
    }
}
