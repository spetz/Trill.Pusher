using Convey;
using Convey.Auth;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.HTTP;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.Prometheus;
using Convey.Tracing.Jaeger;
using Convey.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trill.Pusher.Channels;
using Trill.Pusher.Decorators;
using Trill.Pusher.Logging;
using Trill.Pusher.Services;

namespace Trill.Pusher
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConvey()
                .AddHttpClient()
                .AddJwt()
                .AddRabbitMq()
                .AddEventHandlers()
                .AddInMemoryEventDispatcher()
                .AddCommandHandlers()
                .AddInMemoryCommandDispatcher()
                .AddPrometheus()
                .AddJaeger()
                .Build();

            services.AddScoped<LogContextMiddleware>();
            services.AddSingleton<ICorrelationIdFactory, CorrelationIdFactory>();
            services.AddSingleton<StorySentChannels>();
            services.AddSingleton<ActionRejectedChannels>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddGrpc();
            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            }));
            
            services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingCommandHandlerDecorator<>));
            services.TryDecorate(typeof(IEventHandler<>), typeof(LoggingEventHandlerDecorator<>));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<LogContextMiddleware>();
            app.UseConvey();
            app.UseJaeger();
            app.UsePrometheus();
            app.UseRabbitMq();

            app.UseRouting();
            app.UseGrpcWeb();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<NotifierService>().EnableGrpcWeb().RequireCors("AllowAll");

                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync(context.RequestServices.GetService<AppOptions>().Name);
                    });
            });
        }
    }
}
