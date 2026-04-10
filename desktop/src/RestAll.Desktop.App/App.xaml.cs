using System.Windows;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Tables;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Menu;
using RestAll.Desktop.Infrastructure.Tables;
using RestAll.Desktop.Infrastructure.Orders;
using RestAll.Desktop.Infrastructure.Kitchen;
using RestAll.Desktop.App.Views;
using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App;

public partial class App : Application
{
    private IServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        
        // Register Core services
        services.AddTransient<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
        services.AddTransient<IGetMenuUseCase, GetMenuUseCase>();
        services.AddTransient<ITableManagementUseCase, TableManagementUseCase>();
        services.AddTransient<IManageOrdersUseCase, ManageOrdersUseCase>();
        services.AddTransient<IManageKitchenUseCase, ManageKitchenUseCase>();
        
        // Register Infrastructure services
        services.AddSingleton<RestAllApiOptions>();
        services.AddHttpClient<IAuthGateway, HttpAuthGateway>(ConfigureApiClient);
        services.AddHttpClient<IMenuGateway, HttpMenuGateway>(ConfigureApiClient);
        services.AddHttpClient<ITableGateway, HttpTableGateway>(ConfigureApiClient);
        services.AddHttpClient<IOrderGateway, HttpOrderGateway>(ConfigureApiClient);
        services.AddHttpClient<IKitchenGateway, HttpKitchenGateway>(ConfigureApiClient);
        
        // Register ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<TablesViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<KitchenViewModel>();
        
        _serviceProvider = services.BuildServiceProvider();
        
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

    private static void ConfigureApiClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
    }
}
