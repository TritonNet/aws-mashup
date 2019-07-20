using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.Core;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Cumulus.Aws.Common;
using Cumulus.Aws.Common.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cumulus.Aws.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy(CumulusConstants.CorsPolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory(CumulusConstants.CorsPolicyName));
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddControllersAsServices();

            services.AddAutofac();

            return this.GetServiceProviderAsync(services).Result;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseCors(CumulusConstants.CorsPolicyName);
        }

        private async Task<AutofacServiceProvider> GetServiceProviderAsync(IServiceCollection services)
        {
            var serviceContext = new ServiceContext();

            serviceContext.Populate(services);

            (await serviceContext
                    .RegisterAsync(e =>
                    {
                        var context = e.Resolve<IHttpContextAccessor>();
                        return ((ILambdaContext)context.HttpContext.Items[APIGatewayProxyFunction.LAMBDA_CONTEXT]);

                    }, CancellationToken.None))
                    .As<ILambdaContext>()
                    .InstancePerLifetimeScope();

            await serviceContext.InitializeAsync(CancellationToken.None);

            var lifeTimeScope = serviceContext.BeginLifetimeScope();

            return new AutofacServiceProvider(lifeTimeScope);
        }
    }
}
