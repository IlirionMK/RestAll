using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RestAll.Desktop.App;
using RestAll.Desktop.Core.Auth;
using RestAll.Desktop.Core.Cache;
using RestAll.Desktop.Core.Kitchen;
using RestAll.Desktop.Core.Menu;
using RestAll.Desktop.Core.Orders;
using RestAll.Desktop.Core.Tables;
using RestAll.Desktop.Core.Realtime;
using RestAll.Desktop.Infrastructure.Auth;
using RestAll.Desktop.Infrastructure.Cache;
using RestAll.Desktop.Infrastructure.Kitchen;
using RestAll.Desktop.Infrastructure.Menu;
using RestAll.Desktop.Infrastructure.Orders;
using RestAll.Desktop.Infrastructure.Tables;
using RestAll.Desktop.App.ViewModels;
using Xunit;

namespace RestAll.Desktop.Tests.App;

public class AppTests
{
    [Fact]
    public void OnStartup_ShouldRegisterAllCoreServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register logging
        services.AddLogging();
        
        // Register Cache service
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<RestAll.Desktop.Core.Offline.IOfflineStorage>(_ => Mock.Of<RestAll.Desktop.Core.Offline.IOfflineStorage>());
        var realtimeServiceMock = new Mock<IRealtimeService>();
        realtimeServiceMock.Setup(s => s.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        realtimeServiceMock.Setup(s => s.DisconnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        realtimeServiceMock.Setup(s => s.IsConnectedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        services.AddSingleton(realtimeServiceMock.Object);
        
        // Register Core services
        services.AddTransient<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
        services.AddTransient<IGetMenuUseCase, GetMenuUseCase>();
        services.AddSingleton<IManageProfileUseCase>(_ => Mock.Of<IManageProfileUseCase>());
        services.AddTransient<ITableManagementUseCase, TableManagementUseCase>();
        services.AddTransient<IManageOrdersUseCase, ManageOrdersUseCase>();
        services.AddTransient<IManageKitchenUseCase, ManageKitchenUseCase>();
        
        // Register Infrastructure services
        services.AddSingleton<RestAllApiOptions>();
        services.AddHttpClient<IAuthGateway, HttpAuthGateway>();
        services.AddSingleton<ISessionStorage, SqliteSessionStorage>();
        services.AddHttpClient<IMenuGateway, HttpMenuGateway>();
        services.AddHttpClient<ITableGateway, HttpTableGateway>();
        services.AddHttpClient<IOrderGateway, HttpOrderGateway>();
        services.AddHttpClient<IKitchenGateway, HttpKitchenGateway>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        serviceProvider.GetRequiredService<IAuthenticateUserUseCase>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IGetMenuUseCase>().Should().NotBeNull();
        serviceProvider.GetRequiredService<ITableManagementUseCase>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IManageOrdersUseCase>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IManageKitchenUseCase>().Should().NotBeNull();
    }

    [Fact]
    public void OnStartup_ShouldRegisterAllInfrastructureServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register logging
        services.AddLogging();
        
        services.AddSingleton<RestAllApiOptions>();
        services.AddHttpClient<IAuthGateway, HttpAuthGateway>();
        services.AddSingleton<ISessionStorage, SqliteSessionStorage>();
        services.AddHttpClient<IMenuGateway, HttpMenuGateway>();
        services.AddHttpClient<ITableGateway, HttpTableGateway>();
        services.AddHttpClient<IOrderGateway, HttpOrderGateway>();
        services.AddHttpClient<IKitchenGateway, HttpKitchenGateway>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        serviceProvider.GetRequiredService<IAuthGateway>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IMenuGateway>().Should().NotBeNull();
        serviceProvider.GetRequiredService<ITableGateway>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IOrderGateway>().Should().NotBeNull();
        serviceProvider.GetRequiredService<IKitchenGateway>().Should().NotBeNull();
    }

    [Fact]
    public void OnStartup_ShouldRegisterAllViewModels()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register logging
        services.AddLogging();
        
        // Register Cache service
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<RestAll.Desktop.Core.Offline.IOfflineStorage>(_ => Mock.Of<RestAll.Desktop.Core.Offline.IOfflineStorage>());
        var realtimeServiceMock = new Mock<IRealtimeService>();
        realtimeServiceMock.Setup(s => s.ConnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        realtimeServiceMock.Setup(s => s.DisconnectAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        realtimeServiceMock.Setup(s => s.IsConnectedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        services.AddSingleton(realtimeServiceMock.Object);
        
        // Register Core services
        services.AddTransient<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
        services.AddTransient<IGetMenuUseCase, GetMenuUseCase>();
        services.AddSingleton<IManageProfileUseCase>(_ => Mock.Of<IManageProfileUseCase>());
        services.AddTransient<ITableManagementUseCase, TableManagementUseCase>();
        services.AddTransient<IManageOrdersUseCase, ManageOrdersUseCase>();
        services.AddTransient<IManageKitchenUseCase, ManageKitchenUseCase>();
        
        // Register Infrastructure services
        services.AddSingleton<RestAllApiOptions>();
        services.AddHttpClient<IAuthGateway, HttpAuthGateway>();
        services.AddSingleton<ISessionStorage, SqliteSessionStorage>();
        services.AddHttpClient<IMenuGateway, HttpMenuGateway>();
        services.AddHttpClient<ITableGateway, HttpTableGateway>();
        services.AddHttpClient<IOrderGateway, HttpOrderGateway>();
        services.AddHttpClient<IKitchenGateway, HttpKitchenGateway>();
        
        // Register ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<TablesViewModel>();
        services.AddTransient<OrdersViewModel>();
        services.AddTransient<KitchenViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        serviceProvider.GetRequiredService<LoginViewModel>().Should().NotBeNull();
        serviceProvider.GetRequiredService<MainWindowViewModel>().Should().NotBeNull();
        serviceProvider.GetRequiredService<MenuViewModel>().Should().NotBeNull();
        serviceProvider.GetRequiredService<TablesViewModel>().Should().NotBeNull();
        serviceProvider.GetRequiredService<OrdersViewModel>().Should().NotBeNull();
        serviceProvider.GetRequiredService<KitchenViewModel>().Should().NotBeNull();
    }

    [Fact]
    public void OnStartup_ShouldRegisterAllServicesAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register logging
        services.AddLogging();
        
        // Register Cache service
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<RestAll.Desktop.Core.Offline.IOfflineStorage>(_ => Mock.Of<RestAll.Desktop.Core.Offline.IOfflineStorage>());
        
        services.AddTransient<IAuthenticateUserUseCase, AuthenticateUserUseCase>();
        services.AddTransient<IGetMenuUseCase, GetMenuUseCase>();
        services.AddSingleton<IManageProfileUseCase>(_ => Mock.Of<IManageProfileUseCase>());
        services.AddTransient<ITableManagementUseCase, TableManagementUseCase>();
        services.AddTransient<IManageOrdersUseCase, ManageOrdersUseCase>();
        services.AddTransient<IManageKitchenUseCase, ManageKitchenUseCase>();
        services.AddSingleton<RestAllApiOptions>();
        services.AddHttpClient<IAuthGateway, HttpAuthGateway>();
        services.AddSingleton<ISessionStorage, SqliteSessionStorage>();
        services.AddHttpClient<IMenuGateway, HttpMenuGateway>();
        services.AddHttpClient<ITableGateway, HttpTableGateway>();
        services.AddHttpClient<IOrderGateway, HttpOrderGateway>();
        services.AddHttpClient<IKitchenGateway, HttpKitchenGateway>();
        services.AddTransient<LoginViewModel>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var useCase1 = serviceProvider.GetRequiredService<IAuthenticateUserUseCase>();
        var useCase2 = serviceProvider.GetRequiredService<IAuthenticateUserUseCase>();
        var viewModel1 = serviceProvider.GetRequiredService<LoginViewModel>();
        var viewModel2 = serviceProvider.GetRequiredService<LoginViewModel>();

        // Assert
        useCase1.Should().NotBeSameAs(useCase2);
        viewModel1.Should().NotBeSameAs(viewModel2);
    }

    [Fact]
    public void OnStartup_ShouldRegisterRestAllApiOptionsAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<RestAllApiOptions>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options1 = serviceProvider.GetRequiredService<RestAllApiOptions>();
        var options2 = serviceProvider.GetRequiredService<RestAllApiOptions>();

        // Assert
        options1.Should().BeSameAs(options2);
    }
}
