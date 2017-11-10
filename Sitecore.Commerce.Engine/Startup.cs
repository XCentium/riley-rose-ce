// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Engine
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.DataProtection.XmlEncryption;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.OData.Builder;
    using Microsoft.AspNetCore.OData.Extensions;
    using Microsoft.AspNetCore.OData.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Serilog;
    using Serilog.Events;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Authentication;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.Core.Logging;
    using Sitecore.Commerce.Provider.FileSystem;
    using Sitecore.Framework.Diagnostics;
    using Sitecore.Framework.Rules;

    /// <summary>
    /// Defines the engine startup.
    /// </summary>
    public class Startup
    {
        private readonly string _nodeInstanceId = Guid.NewGuid().ToString("N");
        private readonly IServiceProvider _hostServices;
        private readonly IHostingEnvironment _hostEnv;
        private volatile CommerceEnvironment _environment;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private volatile NodeContext _nodeContext;
        private IServiceCollection _services;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="hostEnv">The host env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public Startup(IServiceProvider serviceProvider, IHostingEnvironment hostEnv, ILoggerFactory loggerFactory)
        {
            this._hostEnv = hostEnv;
            this._hostServices = serviceProvider;
            this._logger = loggerFactory.CreateLogger("Startup");
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(hostEnv.WebRootPath)
                .AddJsonFile("config.json", false, true)
                .AddJsonFile($"config.{this._hostEnv.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            if (this._hostEnv.IsDevelopment())
            {
                builder.AddApplicationInsightsSettings(true);
            }
            
            this.Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the node context.
        /// </summary>
        /// <value>
        /// The node context.
        /// </value>
        public NodeContext NodeContext => this._nodeContext ?? this.InitializeNodeContext();
        
        /// <summary>
        /// Gets or sets the Initial Startup Environment. This will tell the Node how to behave
        /// This will be overloaded by the Environment stored in configuration.
        /// </summary>
        /// <value>
        /// The startup environment.
        /// </value>
        public CommerceEnvironment StartupEnvironment
        {
            get
            {
                return this._environment ?? (this._environment = new CommerceEnvironment { Name = "Bootstrap" });
            }

            set
            {
                this._environment = value;
            }
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this._services = services;

            this.SetupDataProtection(services);

            var serializer = new EntitySerializerCommand(this._hostServices);
            this.StartupEnvironment = this.GetGlobalEnvironment(serializer);
            this.NodeContext.Environment = this.StartupEnvironment;

            this._services.AddSingleton(this.StartupEnvironment);
            this._services.AddSingleton(this.NodeContext);

            // Add the ODataServiceBuilder to the  services collection
            services.AddOData();

            // Add MVC services to the services container.
            services.AddMvc();

            services.Configure<LoggingSettings>(options => this.Configuration.GetSection("Logging").Bind(options));
            services.AddApplicationInsightsTelemetry(this.Configuration);
            services.Configure<ApplicationInsightsSettings>(options => this.Configuration.GetSection("ApplicationInsights").Bind(options));

            TelemetryConfiguration.Active.DisableTelemetry = true;

            this._logger.LogInformation("BootStrapping Services...");

            services.Sitecore()
                .Eventing()
                .Caching(config => config
                    .AddMemoryStore("GlobalEnvironment")
                    .ConfigureCaches("GlobalEnvironment.*", "GlobalEnvironment"))
                .Rules(config => config
                    .IgnoreNamespaces(n => n.Equals("Sitecore.Commerce.Plugin.Tax")))
                .RulesSerialization();
            services.Add(new ServiceDescriptor(typeof(IRuleBuilderInit), typeof(RuleBuilder), ServiceLifetime.Transient));

            this._logger.LogInformation("BootStrapping application...");
            services.Sitecore().Bootstrap(this._hostServices);

            services.Add(new ServiceDescriptor(typeof(TelemetryClient), typeof(TelemetryClient), ServiceLifetime.Singleton));
            this.NodeContext.AddObject(services);

            services.AddSingleton(this._logger);

            services.Configure<CertificatesSettings>(this.Configuration.GetSection("Certificates"));
            services.AddSingleton<IConfiguration>(Configuration);
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="contextPipeline">The context pipeline.</param>
        /// <param name="startNodePipeline">The start node pipeline.</param>
        /// <param name="contextOpsServiceApiPipeline">The context ops service API pipeline.</param>
        /// <param name="startEnvironmentPipeline">The start environment pipeline.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="loggingSettings">The logging settings.</param>
        /// <param name="applicationInsightsSettings">The application insights settings.</param>
        /// <param name="certificatesSettings">The certificates settings.</param>
        public void Configure(
            IApplicationBuilder app,
            IConfigureServiceApiPipeline contextPipeline,
            IStartNodePipeline startNodePipeline,
            IConfigureOpsServiceApiPipeline contextOpsServiceApiPipeline,
            IStartEnvironmentPipeline startEnvironmentPipeline,
            ILoggerFactory loggerFactory,
            IOptions<LoggingSettings> loggingSettings,
            IOptions<ApplicationInsightsSettings> applicationInsightsSettings,
            IOptions<CertificatesSettings> certificatesSettings)
        {
            app.UseDiagnostics();

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Set the error page
            if (this._hostEnv.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePages();
            }
            
            if (certificatesSettings.Value != null && certificatesSettings.Value.ValidationEnabled)
            {
                app.UseClientCertificateValidationMiddleware(certificatesSettings);
            }

            Task.Run(() => startNodePipeline.Run(this.NodeContext, this.NodeContext.GetPipelineContextOptions())).Wait();

            var environmentName = this.Configuration.GetSection("AppSettings:EnvironmentName").Value;
            if (!string.IsNullOrEmpty(environmentName))
            {
                this.NodeContext.AddDataMessage("EnvironmentStartup", $"StartEnvironment={environmentName}");
                Task.Run(() => startEnvironmentPipeline.Run(environmentName, this.NodeContext.GetPipelineContextOptions())).Wait();
            }

            // Initialize plugins OData contexts
            app.InitializeODataBuilder();
            var modelBuilder = new ODataConventionModelBuilder();

            // Run the pipeline to configure the plugin's OData context
            var contextResult = Task.Run(() => contextPipeline.Run(modelBuilder, this.NodeContext.GetPipelineContextOptions())).Result;
            contextResult.Namespace = "Sitecore.Commerce.Engine";

            // Get the model and register the ODataRoute
            var model = contextResult.GetEdmModel();
            app.UseRouter(new ODataRoute("Api", model));

            // Register the bootstrap context for the engine
            modelBuilder = new ODataConventionModelBuilder();
            var contextOpsResult = Task.Run(() => contextOpsServiceApiPipeline.Run(modelBuilder, this.NodeContext.GetPipelineContextOptions())).Result;
            contextOpsResult.Namespace = "Sitecore.Commerce.Engine";

            // Get the model and register the ODataRoute
            model = contextOpsResult.GetEdmModel();
            app.UseRouter(new ODataRoute("CommerceOps", model));

            //// Setup logging
            if (loggingSettings.Value != null)
            {
                if (loggingSettings.Value.SerilogLoggingEnabled)
                {
                    var loggingConfig = new LoggerConfiguration()
                        .ReadFrom.Configuration(this.Configuration)
                        .Enrich.With(new ScLogEnricher());
                    var logsPath = Path.Combine(this._hostEnv.WebRootPath, "logs");
                    loggingConfig.WriteTo.RollingFile(
                        $@"{logsPath}\SCF.{DateTimeOffset.UtcNow:yyyyMMdd}.log.{this._nodeInstanceId}.txt",
                        this.GetSerilogLogLevel(),
                        "{ThreadId} {Timestamp:HH:mm:ss} {ScLevel} {Message}{NewLine}{Exception}");
                    loggerFactory.AddSerilog(loggingConfig.CreateLogger());
                }

                if (loggingSettings.Value.ApplicationInsightsLoggingEnabled)
                {
                    var appInsightsSettings = applicationInsightsSettings.Value;
                    appInsightsSettings.DeveloperMode = this._hostEnv.IsDevelopment();
                    loggerFactory.AddApplicationInsights(appInsightsSettings);
                }
            }
        }

        /// <summary>
        /// Gets the serilog log level.
        /// </summary>
        /// <returns>Serilog configured log level</returns>
        private LogEventLevel GetSerilogLogLevel()
        {
            var level = LogEventLevel.Verbose;
            var configuredLevel = this.Configuration.GetSection("Serilog:MinimumLevel").Value;
            if (string.IsNullOrEmpty(configuredLevel))
            {
                configuredLevel = this.Configuration.GetSection("Serilog:MinimumLevel:Default").Value;
            }

            if (string.IsNullOrEmpty(configuredLevel))
            {
                return level;
            }

            if (configuredLevel.Equals(LogEventLevel.Debug.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                level = LogEventLevel.Debug;
            }
            else if (configuredLevel.Equals(LogEventLevel.Information.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                level = LogEventLevel.Information;
            }
            else if (configuredLevel.Equals(LogEventLevel.Warning.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                level = LogEventLevel.Warning;
            }
            else if (configuredLevel.Equals(LogEventLevel.Error.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                level = LogEventLevel.Error;
            }
            else if (configuredLevel.Equals(LogEventLevel.Fatal.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                level = LogEventLevel.Fatal;
            }

            return level;
        }

        /// <summary>
        /// Initializes the node context.
        /// </summary>
        /// <returns>A <see cref="NodeContext"/></returns>
        private NodeContext InitializeNodeContext()
        {
            this._nodeContext = new NodeContext(this._logger, new TelemetryClient())
            {
                CorrelationId = this._nodeInstanceId,
                ConnectionId = "Node_Global",
                ContactId = "Node_Global",
                GlobalEnvironment = this.StartupEnvironment,
                Environment = this.StartupEnvironment,
                WebRootPath = this._hostEnv.WebRootPath,
                LoggingPath = this._hostEnv.WebRootPath + @"\logs\"
            };
            return this._nodeContext;
        }

        /// <summary>
        /// Setups the data protection storage and encryption protection type
        /// </summary>
        /// <param name="services">The services.</param>
        private void SetupDataProtection(IServiceCollection services)
        {
            var builder = services.AddDataProtection();
            var pathToKeyStorage = this.Configuration.GetSection("AppSettings:EncryptionKeyStorageLocation").Value;
            
            // Persist keys to a specific directory (should be a network location in distributed application)
            builder.PersistKeysToFileSystem(new DirectoryInfo(pathToKeyStorage));

            var protectionType = this.Configuration.GetSection("AppSettings:EncryptionProtectionType").Value.ToUpperInvariant();

            switch (protectionType)
            {
                case "DPAPI-SID":
                    var storageSid = this.Configuration.GetSection("AppSettings:EncryptionSID").Value.ToUpperInvariant();
                    //// Uses the descriptor rule "SID=S-1-5-21-..." to encrypt with domain joined user
                    builder.ProtectKeysWithDpapiNG($"SID={storageSid}", flags: DpapiNGProtectionDescriptorFlags.None);
                    break;
                case "DPAPI-CERT":
                    var storageCertificateHash = this.Configuration.GetSection("AppSettings:EncryptionCertificateHash").Value.ToUpperInvariant();
                    //// Searches the cert store for the cert with this thumbprint
                    builder.ProtectKeysWithDpapiNG(
                        $"CERTIFICATE=HashId:{storageCertificateHash}",
                        DpapiNGProtectionDescriptorFlags.None);
                    break;
                case "LOCAL":
                    //// Only the local user account can decrypt the keys
                    builder.ProtectKeysWithDpapiNG();
                    break;
                case "MACHINE":
                    //// All user accounts on the machine can decrypt the keys
                    builder.ProtectKeysWithDpapi(true);
                    break;
                default:
                    //// All user accounts on the machine can decrypt the keys
                    builder.ProtectKeysWithDpapi(true);
                    break;
            }
        }

        /// <summary>
        /// Gets the global environment.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A <see cref="CommerceEnvironment"/></returns>
        private CommerceEnvironment GetGlobalEnvironment(EntitySerializerCommand serializer)
        {
            CommerceEnvironment environment;

            this._logger.LogInformation($"Loading Global Environment using Filesystem Provider from: {this._hostEnv.WebRootPath} s\\Bootstrap\\");

            // Use the default File System provider to setup the environment
            this.NodeContext.BootstrapProviderPath = this._hostEnv.WebRootPath + @"\Bootstrap\";
            var bootstrapProvider = new FileSystemEntityProvider(this.NodeContext.BootstrapProviderPath, serializer);

            var bootstrapFile = this.Configuration.GetSection("AppSettings:BootStrapFile").Value;

            if (!string.IsNullOrEmpty(bootstrapFile))
            {
                this.NodeContext.BootstrapEnvironmentPath = bootstrapFile;

                this.NodeContext.AddDataMessage("NodeStartup", $"GlobalEnvironmentFrom='Configuration: {bootstrapFile}'");
                environment = Task.Run(() => bootstrapProvider.Find<CommerceEnvironment>(this.NodeContext, bootstrapFile, false)).Result;
            }
            else
            {
                // Load the NodeContext default
                bootstrapFile = "Global";
                this.NodeContext.BootstrapEnvironmentPath = bootstrapFile;
                this.NodeContext.AddDataMessage("NodeStartup", $"GlobalEnvironmentFrom='{bootstrapFile}.json'");
                environment = Task.Run(() => bootstrapProvider.Find<CommerceEnvironment>(this.NodeContext, bootstrapFile, false)).Result;
            }

            this.NodeContext.BootstrapEnvironmentPath = bootstrapFile;

            this.NodeContext.GlobalEnvironmentName = environment.Name;
            this.NodeContext.AddDataMessage("NodeStartup", $"Status='Started',GlobalEnvironmentName='{NodeContext.GlobalEnvironmentName}'");

            if (this.Configuration.GetSection("AppSettings:BootStrapFile").Value != null)
            {
                this.NodeContext.ContactId = this.Configuration.GetSection("AppSettings:NodeId").Value;
            }

            if (!string.IsNullOrEmpty(environment.GetPolicy<DeploymentPolicy>().DeploymentId))
            {
                this.NodeContext.ContactId = $"{environment.GetPolicy<DeploymentPolicy>().DeploymentId}_{this._nodeInstanceId}";
            }

            return environment;
        }
    }
}
