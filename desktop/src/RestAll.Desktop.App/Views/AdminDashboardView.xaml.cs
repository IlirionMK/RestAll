using System.Windows;
using System.Windows.Controls;

using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App.Views;

public partial class AdminDashboardView : Window
{
    private readonly object _viewModel;

    public AdminDashboardView(object viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (_viewModel is AdminDashboardViewModel vm)
        {
            await Task.WhenAll(
                Task.Run(() => vm.LoadStaffCommand.Execute(null)),
                Task.Run(() => vm.LoadAnalyticsCommand.Execute(null)),
                Task.Run(() => vm.LoadLogsCommand.Execute(null))
            );
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (_viewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private void NewStaffPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is AdminDashboardViewModel vm && sender is PasswordBox passwordBox)
        {
            vm.NewStaffPassword = passwordBox.Password;
        }
    }
}

