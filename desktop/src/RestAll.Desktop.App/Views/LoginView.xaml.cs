using System.Windows;
using System.Windows.Controls;
using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App.Views;

public partial class LoginView : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.LoginSuccessful += OnLoginSuccessful;
        Closed += OnWindowClosed;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = ((PasswordBox)sender).Password;
        }
    }

    private void OnLoginSuccessful(object? sender, EventArgs e)
    {
        var mainWindow = ((App)Application.Current).CreateMainWindow();
        mainWindow.Show();
        Close();
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        _viewModel.LoginSuccessful -= OnLoginSuccessful;
        _viewModel.Cancel();
    }
}
