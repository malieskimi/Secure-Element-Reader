using Microsoft.Extensions.Configuration;
using PCSC;
using PCSC.Iso7816;
using PCSC.Monitoring;
using SecureElementReader.App.Interfaces;
using SecureElementReader.App.Models.Configurations;
using SecureElementReader.App.Proxies;
using SecureElementReader.App.Services;
using SecureElementReader.App.ViewModels;
using SecureElementReader.App.ViewModels.Interfaces;
using SecureElementReader.App.ViewModels.Services;
using Serilog;
using Serilog.Extensions.Logging;
using Splat;
using Splat.Microsoft.Extensions.Logging;
using System.IO;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SecureElementReader.App.DependencyInjection
{
    public static class ServicesBootstrapper
    {
        public static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            RegisterLogging(services, resolver);

            var configuration = BuildConfiguration();
            RegisterDefaultThemeConfiguration(services, configuration);
            RegisterThemesNamesConfiguration(services, configuration);
            RegisterLoggingConfiguration(services, configuration);
            RegisterSelectedLanguagesConfiguration(services, configuration);


            services.Register(() => new AboutDialogViewModel());
            services.Register(() => new VerificationInfoDialogViewModel(
                resolver.GetRequiredService<ICertDetailsViewModel>()
                ));
            services.Register(() => new LoadingViewModel());
            services.Register(() => new VerifyPinDialogViewModel(
                resolver.GetRequiredService<ICardReaderService>(),
                resolver.GetRequiredService<IMainWindowProvider>()
                ));
            services.RegisterLazySingleton<ISCardContext>(() => new SCardContext());
            services.RegisterLazySingleton<ISCardReader>(() => new SCardReader(
                resolver.GetRequiredService<ISCardContext>()));
            services.RegisterLazySingleton<IIsoReader>(() => new IsoReader(
                resolver.GetRequiredService<ISCardReader>()
                ));
            services.RegisterLazySingleton<IMainWindowProvider>(() => new MainWindowProvider());
            services.RegisterLazySingleton<IApduCommandService>(() => new ApduCommandService());
            services.RegisterLazySingleton<ITaxCoreApiProxy>(() => new TaxCoreApiProxy());
            services.RegisterLazySingleton<ICardReaderService>(() => new CardReaderService(
                resolver.GetRequiredService<IApduCommandService>(),
                resolver.GetRequiredService<ILogger>(),
                 resolver.GetRequiredService<IContextFactory>()

                ));
            services.RegisterLazySingleton<IDialogService>(() => new DialogService(
                    resolver.GetRequiredService<IMainWindowProvider>()
                ));
            services.RegisterLazySingleton<IApplicationDispatcher>(() => new AvaloniaDispatcher());
            services.RegisterLazySingleton<IContextFactory>(() => new ContextFactory());
            services.RegisterLazySingleton<IMonitorFactory>(() => new MonitorFactory(
                resolver.GetRequiredService<IContextFactory>()));
            services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
                resolver.GetRequiredService<IDialogService>(),
                resolver.GetRequiredService<ICardReaderService>(),
                resolver.GetRequiredService<IMenuViewModel>(),
                resolver.GetRequiredService<ICertDetailsViewModel>(),
                resolver.GetRequiredService<IMonitorFactory>(),
                resolver.GetRequiredService<IApplicationDispatcher>(),
                resolver.GetRequiredService<IMainWindowProvider>(),
                resolver.GetRequiredService<ITaxCoreApiProxy>()
                ));
            services.RegisterLazySingleton<IApplicationCloser>(() => new ApplicationCloser());

            services.RegisterLazySingleton<IMenuViewModel>(() => new MenuViewModel(
                    resolver.GetRequiredService<IApplicationCloser>(),
                    resolver.GetRequiredService<IDialogService>(),
                    resolver.GetRequiredService<ILocalizationService>(),
                    resolver.GetRequiredService<SelectedLanguageConfiguration>()
                ));
            services.RegisterLazySingleton<ICertDetailsViewModel>(() => new CertDetailsViewModel(
                    resolver.GetRequiredService<IDialogService>()
                ));


            services.RegisterLazySingleton<ILocalizationService>(() => new LocalizationService(
                    resolver.GetRequiredService<SelectedLanguageConfiguration>()
                ));
        }

        private static void RegisterLogging(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton(() =>
            {
                var config = resolver.GetRequiredService<LoggingConfiguration>();
                var logFilePath = GetLogFileName(config);
                var logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Default", config.DefaultLogLevel)
                    .MinimumLevel.Override("Microsoft", config.MicrosoftLogLevel)
                    .WriteTo.Console()
                    .WriteTo.RollingFile(logFilePath, fileSizeLimitBytes: config.LimitBytes)
                    .CreateLogger();
                var factory = new SerilogLoggerFactory(logger);

                Locator.CurrentMutable.UseMicrosoftExtensionsLoggingWithWrappingFullLogger(factory);

                return factory.CreateLogger("Default");
            });
        }

        private static string GetLogFileName(LoggingConfiguration config)
        {
            string logDirectory = Directory.GetCurrentDirectory();

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            return Path.Combine(logDirectory, config.LogFileName);
        }

        private static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        }

        private static void RegisterLoggingConfiguration(IMutableDependencyResolver services,
            IConfiguration configuration)
        {
            var config = new LoggingConfiguration();
            configuration.GetSection("Logging").Bind(config);
            services.RegisterConstant(config);
        }

        private static void RegisterDefaultThemeConfiguration(IMutableDependencyResolver services,
            IConfiguration configuration)
        {
            var config = new DefaultThemeConfiguration();
            configuration.GetSection("Themes").Bind(config);
            services.RegisterConstant(config);
        }

        private static void RegisterThemesNamesConfiguration(IMutableDependencyResolver services,
            IConfiguration configuration)
        {
            var config = new ThemesNamesConfiguration();
            configuration.GetSection("Themes").Bind(config);
            services.RegisterConstant(config);
        }

        private static void RegisterSelectedLanguagesConfiguration(IMutableDependencyResolver services,
            IConfiguration configuration)
        {
            var config = new SelectedLanguageConfiguration();
            configuration.GetSection("SelectedLanguage").Bind(config);
            services.RegisterConstant(config);
        }
    }
}
