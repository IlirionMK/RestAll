using System.Windows;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using RestAll.Desktop.Core.Auth;
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

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add logging
        services.AddLogging(builder => 
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
        });
        
        // Register Core services
        services.AddSingleton<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
        services.AddTransient<IGetMenuUseCase, GetMenuUseCase>();
        services.AddTransient<ITableManagementUseCase, TableManagementUseCase>();
        services.AddTransient<IManageOrdersUseCase, ManageOrdersUseCase>();
        services.AddTransient<IManageKitchenUseCase, ManageKitchenUseCase>();
        services.AddTransient<IManageReservationsUseCase, ManageReservationsUseCase>();
        services.AddSingleton<IManageProfileUseCase, ManageProfileUseCase>();
        
        // Register Cache service
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        
        // Register Realtime service
        services.AddSingleton<IRealtimeService, WebSocketRealtimeService>();
        
        // Register Offline service
        services.AddSingleton<IOfflineStorage, SqliteOfflineStorage>();
        
        // Register Performance service
        services.AddSingleton<IPerformanceMonitor, PerformanceMonitor>();
        
        // Register Infrastructure services
        var apiOptions = configuration.GetSection("Api").Get<RestAllApiOptions>() ?? new RestAllApiOptions { BaseUrl = "http://localhost:8000/api" };
        services.AddSingleton(apiOptions);
        services.AddSingleton<IErrorHandler, ErrorHandler>();
        
        // Register Session storage
        services.AddSingleton<ISessionStorage, SqliteSessionStorage>();

        // Configure HTTP clients with one shared cookie container and handler
        services.AddSingleton(new CookieContainer());
        services.AddSingleton<HttpClientHandler>(sp => CreateHttpClientHandler(sp.GetRequiredService<CookieContainer>()));
        services.AddTransient<ICsrfTokenService, CookieCsrfTokenService>();
        services.AddTransient<CsrfHeaderHandler>();

        services.AddHttpClient("csrf-cookie", client => ConfigureApiClient(client, apiOptions))
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());

        services.AddHttpClient<IAuthGateway, HttpAuthGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());
        
        services.AddHttpClient<IMenuGateway, HttpMenuGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());
        
        services.AddHttpClient<ITableGateway, HttpTableGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());
        
        services.AddHttpClient<IOrderGateway, HttpOrderGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());
        
        services.AddHttpClient<IKitchenGateway, HttpKitchenGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());
        
        services.AddHttpClient<IReservationGateway, HttpReservationGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());
        
        services.AddHttpClient<IProfileGateway, HttpProfileGateway>(client => ConfigureApiClient(client, apiOptions))
            .AddHttpMessageHandler<CsrfHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<HttpClientHandler>());
        
        // Register ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<TablesViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<KitchenViewModel>();
        services.AddTransient<ReservationsViewModel>();
        services.AddTransient<ProfileViewModel>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        // Initialize authentication after services are built
        var authService = _serviceProvider.GetRequiredService<IAuthenticateUserUseCase>();
        authService.InitializeAsync();
        
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
}
