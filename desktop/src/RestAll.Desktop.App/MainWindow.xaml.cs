using System.Windows;
using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        Closed += OnClosed;
    }

    private void OpenMenu_Click(object sender, RoutedEventArgs e)
    {
        var menuView = ((App)Application.Current).CreateMenuView();
        menuView.Closed += (s, args) => ((CancelableViewModelBase)menuView.DataContext).Dispose();
        menuView.Show();
    }

    private void OpenTables_Click(object sender, RoutedEventArgs e)
    {
        var tablesView = ((App)Application.Current).CreateTablesView();
        tablesView.Closed += (s, args) => ((CancelableViewModelBase)tablesView.DataContext).Dispose();
        tablesView.Show();
    }

    private void OpenOrders_Click(object sender, RoutedEventArgs e)
    {
        var ordersView = ((App)Application.Current).CreateOrdersView();
        ordersView.Closed += (s, args) => ((CancelableViewModelBase)ordersView.DataContext).Dispose();
        ordersView.Show();
    }

    private void OpenKitchen_Click(object sender, RoutedEventArgs e)
    {
        var kitchenView = ((App)Application.Current).CreateKitchenView();
        kitchenView.Closed += (s, args) => ((CancelableViewModelBase)kitchenView.DataContext).Dispose();
        kitchenView.Show();
    }

    private void OpenReservations_Click(object sender, RoutedEventArgs e)
    {
        var reservationsView = ((App)Application.Current).CreateReservationsView();
        reservationsView.Closed += (s, args) => ((CancelableViewModelBase)reservationsView.DataContext).Dispose();
        reservationsView.Show();
    }

    private void OpenProfile_Click(object sender, RoutedEventArgs e)
    {
        var profileView = ((App)Application.Current).CreateProfileView();
        profileView.Closed += (s, args) => ((CancelableViewModelBase)profileView.DataContext).Dispose();
        profileView.Show();
    }

    private void OpenAdmin_Click(object sender, RoutedEventArgs e)
    {
        var adminView = ((App)Application.Current).CreateAdminDashboardView();
        adminView.Closed += (s, args) => ((CancelableViewModelBase)adminView.DataContext).Dispose();
        adminView.Show();
    }

    private void OpenMenuManagement_Click(object sender, RoutedEventArgs e)
    {
        var menuManagementView = ((App)Application.Current).CreateMenuManagementView();
        menuManagementView.Closed += (s, args) => ((CancelableViewModelBase)menuManagementView.DataContext).Dispose();
        menuManagementView.Show();
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.LogoutCommand.ExecuteAsync();
        Close();
        
        var loginView = ((App)Application.Current).CreateLoginView();
        loginView.Show();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.Dispose();
    }
}
