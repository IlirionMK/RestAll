using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
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
        viewModel.ForgotPasswordRequested += OnForgotPasswordRequested;
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
        _viewModel.ForgotPasswordRequested -= OnForgotPasswordRequested;
        _viewModel.Cancel();
    }

    private void OnForgotPasswordRequested(object? sender, EventArgs e)
    {
        var forgotPasswordViewModel = ((App)Application.Current).ServiceProvider.GetRequiredService<ForgotPasswordViewModel>();
        var forgotPasswordView = new ForgotPasswordView(forgotPasswordViewModel);
        
        forgotPasswordViewModel.BackToLoginRequested += (s, args) =>
        {
            forgotPasswordView.Close();
        };

        Hide();
        forgotPasswordView.ShowDialog();
        Show();
    }
}
