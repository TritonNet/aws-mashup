#region - References -
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SQS;
using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using Cumulus.Aws.Common.Abstraction;
using Cumulus.Aws.Common.Implementation;
using Cumulus.Aws.Common.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
#endregion

#region - Attributes -

[assembly: InternalsVisibleTo("Cumulus.Aws.Common.Tests")]

#endregion

namespace Cumulus.Aws.Common
{
    public class ServiceContext
    {
        #region - Private Properties -

        private IContainer serviceContainer;

        private ContainerBuilder containerBuilder;

        private bool servicesLoaded;

        private bool containerBuilt;

        #endregion

        #region - Constructor -

        public ServiceContext()
        {
            this.containerBuilder = new ContainerBuilder();
            this.servicesLoaded = false;
            this.containerBuilt = false;
        }

        #endregion

        #region - Public Methods -

        public async Task InjectServiceAsync<TService>(TService service, CancellationToken cancellationToken) where TService : class
        {
            if (this.containerBuilt)
                throw new InvalidOperationException("Container is already initialised and service injection is not allowed.");

            if (!this.servicesLoaded)
                await this.LoadServicesAsync(cancellationToken);

            this.containerBuilder.RegisterInstance(service).As<TService>();
        }


        public async Task<IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle>> RegisterAsync<T>(Func<IComponentContext, T> @delegate, CancellationToken cancellationToken)
        {
            if (this.containerBuilt)
                throw new InvalidOperationException("Service injection after container build is not allowed");

            if (!this.servicesLoaded)
                await this.LoadServicesAsync(cancellationToken);

            return this.containerBuilder.Register(@delegate);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (!this.containerBuilt)
            {
                if (!this.servicesLoaded)
                    await this.LoadServicesAsync(cancellationToken);

                this.serviceContainer = this.containerBuilder.Build();

                this.containerBuilt = true;
            }
        }

        public async Task<TService> GetServiceAsync<TService>(CancellationToken cancellationToken)
        {
            if (!this.containerBuilt)
                await this.InitializeAsync(cancellationToken);

            return this.serviceContainer.Resolve<TService>();
        }

        public void Populate(IEnumerable<ServiceDescriptor> descriptors)
        {
            this.containerBuilder.Populate(descriptors);
        }

        public ILifetimeScope BeginLifetimeScope()
        {
            return this.serviceContainer.BeginLifetimeScope();
        }

        #endregion

        #region - Private Methods -

        private async Task LoadServicesAsync(CancellationToken cancellationToken)
        {
            var secretManagerClient = new AmazonSecretsManagerClient("AKIAJGI5DMKDX5WJUMMQ", "8+2jrKXcdCGBJeqk0F8b0fuXoOZ8wO5Yt6pPH+JB", RegionEndpoint.EUCentral1);
            var getSecretValueRequest = new GetSecretValueRequest
            {
                SecretId = CumulusConstants.SecretManagerKey.VstsReleaseManagerUserSecretKey,
            };

            var getSecretValueResult = await secretManagerClient.GetSecretValueAsync(getSecretValueRequest, cancellationToken);

            var credential = new BasicAWSCredentials("AKIAIAN24QJPLXLSCKPA", getSecretValueResult.SecretString);

            var dynamodbCredential = credential;
#if ODEBUG
            dynamodbCredential = new BasicAWSCredentials("fakeMyKeyId", "fakeSecretAccessKey");

            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000",
            };
#else
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.EUCentral1,
            };
#endif
            this.containerBuilder
                .RegisterType<AmazonDynamoDBClient>()
                .As<IAmazonDynamoDB>()
                .WithParameter("credentials", dynamodbCredential)
                .WithParameter("clientConfig", config)
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<AmazonS3Client>()
                .As<IAmazonS3>()
                .WithParameter("credentials", credential)
                .WithParameter("region", RegionEndpoint.EUCentral1)
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<AmazonSQSClient>()
                .As<IAmazonSQS>()
                .WithParameter("credentials", credential)
                .WithParameter("region", RegionEndpoint.EUCentral1)
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<Repository>()
                .As<IRepository>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<DateTimeService>()
                .As<IDateTimeService>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<ProductService>()
                .As<IProductService>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<CsvConverter>()
                .As<ICsvConverter>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<EurexService>()
                .As<IEurexService>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<XetraService>()
                .As<IXetraService>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<InstallService>()
                .As<IInstallService>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<EncryptionService>()
                .As<IEncryptionService>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<ManagerService>()
                .As<IManagerService>()
                .InstancePerLifetimeScope();

            this.containerBuilder
                .RegisterType<MaintenanceService>()
                .As<IMaintenanceService>()
                .InstancePerLifetimeScope();

            this.servicesLoaded = true;
        }

        #endregion
    }
}
