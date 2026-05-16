using System.Windows;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Serilog;
using Serilog.Events;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Admin;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Tables;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Core.Reservations;
using RestAll.Desktop.Core.Exceptions;
using RestAll.Desktop.Core.Realtime;
using RestAll.Desktop.Core.Offline;
using RestAll.Desktop.Core.Performance;
using RestAll.Desktop.Core.Cache;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Admin;
using RestAll.Desktop.Infrastructure.Menu;
using RestAll.Desktop.Infrastructure.Tables;
using RestAll.Desktop.Infrastructure.Orders;
using RestAll.Desktop.Infrastructure.Kitchen;
using RestAll.Desktop.Infrastructure.Reservations;
using RestAll.Desktop.Infrastructure.Realtime;
using RestAll.Desktop.Infrastructure.Offline;
using RestAll.Desktop.Infrastructure.Performance;
using RestAll.Desktop.Infrastructure.Cache;
using RestAll.Desktop.Infrastructure.Http;
using RestAll.Desktop.Infrastructure;
using RestAll.Desktop.App.Views;
using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App;

public partial class App : Application
{
    private IServiceProvider _serviceProvider = null!;

    public IServiceProvider ServiceProvider => _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Serilog to write to file
        var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http", LogEventLevel.Information)
            .WriteTo.File(
                Path.Combine(logDirectory, "restall-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        var services = new ServiceCollection();
        
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add logging with Serilog
        services.AddLogging(builder => 
        {
            builder.AddSerilog(dispose: false);
        });
        
        // Register Core services
        services.AddSingleton<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
        services.AddTransient<IGetMenuUseCase, GetMenuUseCase>();
        services.AddTransient<IManageMenuUseCase, ManageMenuUseCase>();
        services.AddTransient<ITableManagementUseCase, TableManagementUseCase>();
        services.AddTransient<IManageOrdersUseCase, ManageOrdersUseCase>();
        services.AddTransient<IManageKitchenUseCase, ManageKitchenUseCase>();
        services.AddTransient<IManageReservationsUseCase, ManageReservationsUseCase>();
        services.AddSingleton<IManageProfileUseCase, ManageProfileUseCase>();
        services.AddTransient<IManageStaffUseCase, ManageStaffUseCase>();
        services.AddTransient<IGetAnalyticsSummaryUseCase, GetAnalyticsSummaryUseCase>();
        services.AddTransient<IGetAuditLogsUseCase, GetAuditLogsUseCase>();
        
        // Register Cache service
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        
        // Register Offline service (register concrete implementation so SyncManager can use specialized methods)
        services.AddSingleton<SqliteOfflineStorage>();
        services.AddSingleton<IOfflineStorage>(sp => sp.GetRequiredService<SqliteOfflineStorage>());
        
        // Register Performance service
        services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
        
        // Register Infrastructure services
        var apiOptions = configuration.GetSection("Api").Get<RestAllApiOptions>() ?? new RestAllApiOptions { BaseUrl = "http://localhost:8000/api" };
        services.AddSingleton(apiOptions);
        var realtimeOptions = configuration.GetSection("Realtime").Get<RealtimeOptions>() ?? new RealtimeOptions();
        services.AddSingleton(realtimeOptions);
        
        // Register Google Auth service
        var googleAuthOptions = configuration.GetSection("GoogleAuth");
        var googleClientId = googleAuthOptions.GetValue<string>("ClientId") ?? string.Empty;
        var googleClientSecret = googleAuthOptions.GetValue<string>("ClientSecret") ?? string.Empty;
        var googleCallbackPort = googleAuthOptions.GetValue<int>("CallbackPort", 8765);
        services.AddSingleton(sp => new GoogleAuthBrowserService(
            sp.GetRequiredService<ILogger<GoogleAuthBrowserService>>(),
            googleClientId,
            googleClientSecret,
            googleCallbackPort
        ));
        services.AddHttpClient("broadcasting-auth", client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());
        services.AddSingleton<IRealtimeService, WebSocketRealtimeService>();
        services.AddSingleton<IErrorHandler, ErrorHandler>();
        // Sync manager for offline operation queue
        services.AddSingleton<ISyncManager, RestAll.Desktop.Infrastructure.Sync.SyncManager>();
        
        // Register Session storage
        services.AddSingleton<ISessionStorage, SqliteSessionStorage>();

        // Configure HTTP clients with one shared cookie container
        // IMPORTANT: Do NOT register HttpClientHandler as singleton - IHttpClientFactory manages handler lifecycle
        // Each HttpClient gets its own handler instance, but they all share the same CookieContainer
        var sharedCookieContainer = new CookieContainer();
        services.AddSingleton(sharedCookieContainer);
        services.AddTransient<ICsrfTokenService, CookieCsrfTokenService>();
        services.AddTransient<CsrfHeaderHandler>();

        services.AddHttpClient("csrf-cookie", client => ConfigureApiClient(client, apiOptions))
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });

        services.AddHttpClient<IAuthGateway, HttpAuthGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });
        
        services.AddHttpClient<IMenuGateway, HttpMenuGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });
        
        services.AddHttpClient<ITableGateway, HttpTableGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });
        
        services.AddHttpClient<IOrderGateway, HttpOrderGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });
        
        services.AddHttpClient<IKitchenGateway, HttpKitchenGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });

        services.AddHttpClient<IAdminGateway, HttpAdminGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });
        
        services.AddHttpClient<IReservationGateway, HttpReservationGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });
        
        services.AddHttpClient<IProfileGateway, HttpProfileGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = sp.GetRequiredService<CookieContainer>()
            });
        
        // Register ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ForgotPasswordViewModel>();
        services.AddTransient<ResetPasswordViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<MenuManagementViewModel>();
        services.AddTransient<TablesViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<KitchenViewModel>();
        services.AddTransient<ReservationsViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<AdminDashboardViewModel>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        // Initialize authentication after services are built
        var authService = _serviceProvider.GetRequiredService<IAuthenticateUserUseCase>();
        authService.InitializeAsync();

        // Start background sync manager (flush queued offline operations)
        try
        {
            var sync = _serviceProvider.GetRequiredService<ISyncManager>();
            sync.Start();
        }
        catch (Exception ex)
        {
            var logger = _serviceProvider.GetService<ILogger<App>>();
            logger?.LogWarning(ex, "Failed to start SyncManager");
        }

        if (authService.State == AuthFlowState.Authenticated)
        {
            var mainWindow = CreateMainWindow();
            mainWindow.Show();
            return;
        }

        var loginView = CreateLoginView();
        loginView.Show();
    }

    public LoginView CreateLoginView()
    {
        var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
        return new LoginView(loginViewModel);
    }

    public MainWindow CreateMainWindow()
    {
        var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        return new MainWindow(mainWindowViewModel);
    }

    public MenuView CreateMenuView()
    {
        var menuViewModel = _serviceProvider.GetRequiredService<MenuViewModel>();
        return new MenuView(menuViewModel);
    }

    public MenuManagementView CreateMenuManagementView()
    {
        var menuManagementViewModel = _serviceProvider.GetRequiredService<MenuManagementViewModel>();
        var view = new MenuManagementView(menuManagementViewModel);
        _ = menuManagementViewModel.InitializeAsync();
        return view;
    }

    public TablesView CreateTablesView()
    {
        var tablesViewModel = _serviceProvider.GetRequiredService<TablesViewModel>();
        return new TablesView(tablesViewModel);
    }

    public OrdersView CreateOrdersView()
    {
        var ordersViewModel = _serviceProvider.GetRequiredService<OrdersViewModel>();
        return new OrdersView(ordersViewModel);
    }

    public KitchenView CreateKitchenView()
    {
        var kitchenViewModel = _serviceProvider.GetRequiredService<KitchenViewModel>();
        return new KitchenView(kitchenViewModel);
    }

    public ReservationsView CreateReservationsView()
    {
        var reservationsViewModel = _serviceProvider.GetRequiredService<ReservationsViewModel>();
        return new ReservationsView(reservationsViewModel);
    }

    public ProfileView CreateProfileView()
    {
        var profileViewModel = _serviceProvider.GetRequiredService<ProfileViewModel>();
        return new ProfileView(profileViewModel);
    }

    public AdminDashboardView CreateAdminDashboardView()
    {
        var adminViewModel = _serviceProvider.GetRequiredService<AdminDashboardViewModel>();
        return new AdminDashboardView(adminViewModel);
    }

    private static void ConfigureApiClient(HttpClient client, RestAllApiOptions options)
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

        var origin = BuildOrigin(options.BaseUrl);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", origin);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Referer", $"{origin}/");
    }

    private static string BuildOrigin(string apiBaseUrl)
    {
        var uri = new Uri(apiBaseUrl, UriKind.Absolute);
        return $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? string.Empty : $":{uri.Port}")}";
    }

    private static HttpClientHandler CreateHttpClientHandler(CookieContainer cookieContainer)
    {
        return new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = cookieContainer
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
