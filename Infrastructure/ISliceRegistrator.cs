using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;
public interface ISliceRegistrator
{
    public Task RegisterWebApplicationBuilder(IHostApplicationBuilder builder);
    public Task RegisterWebApplication(IApplicationBuilder app);

    public static IEnumerable<ISliceRegistrator> GetAllRegistrators()
    {
        return Assembly.GetCallingAssembly()
            .GetTypes()
            .Where(type => type.IsClass && type.IsAssignableTo(typeof(ISliceRegistrator)))
            .Select(type => (ISliceRegistrator)Activator.CreateInstance(type)!)
            .ToList();
    }
}
