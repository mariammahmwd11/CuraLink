using CuraLink.Application.Common.Behavior;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CuraLink.Application
{
    public static class ApplicationRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(
                     typeof(ApplicationRegistration).Assembly);
                config.AddOpenBehavior(
                    typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(
                  typeof(ApplicationRegistration).Assembly);
           

            return services;
        }
    }
}