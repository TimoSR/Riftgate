using System.Reflection;
using CodingPatterns.ApplicationLayer.ApplicationServices;

namespace API._DIRegister;


// Error was fixes by using executing assembly instead of assembly.
public static class CommandHandlerRegister
{
    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        var commandHandlerType = typeof(ICommandHandler<>);
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerType))
            .ToList();

        foreach (var handler in types)
        {
            var interfaceTypes = handler.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandHandlerType);

            foreach (var interfaceType in interfaceTypes)
            {
                var genericArguments = interfaceType.GetGenericArguments();
                if (genericArguments.Length == 1)
                {
                    var commandType = genericArguments[0].Name;
                    services.AddScoped(interfaceType, handler);
                    Console.WriteLine($"Registered command handler: {handler.Name} for ICommandHandler<{commandType}>");
                }
            }
        }

        return services;
    }
}