using System.Reflection;
using CodingPatterns.ApplicationLayer.ApplicationServices;

namespace API._DIRegister;

public static class QueryHandlerRegister
{
    public static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        var queryHandlerType = typeof(IQueryHandler<,>);
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == queryHandlerType))
            .ToList();

        foreach (var handler in types)
        {
            var interfaceTypes = handler.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == queryHandlerType);

            foreach (var interfaceType in interfaceTypes)
            {
                var genericArguments = interfaceType.GetGenericArguments();
                if (genericArguments.Length == 2)
                {
                    var queryType = genericArguments[0].Name;
                    var resultType = genericArguments[1].Name;
                    services.AddScoped(interfaceType, handler);
                    Console.WriteLine($"Registered query handler: {handler.Name} for IQueryHandler<{queryType}, {resultType}>");
                }
            }
        }

        return services;
    }
}