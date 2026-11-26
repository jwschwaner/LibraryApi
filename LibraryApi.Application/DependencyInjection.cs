using FluentValidation;
using LibraryApi.Application.Common.Behaviors;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
                .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly)
                .AddMediator(opts =>
                {
                    opts.ServiceLifetime = ServiceLifetime.Scoped;
                })
                // Registration order is execution order: logging wraps validation, so rejected
                // requests are logged too. DomainExceptionBehavior sits right around the handler,
                // so a broken business rule becomes a Conflict result
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(DomainExceptionBehavior<,>))
            ;
    }
}
