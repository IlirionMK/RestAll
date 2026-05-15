using System.Windows;
using System.Windows.Controls;

namespace RestAll.Desktop.App.Views;

public partial class ResetPasswordView : Window
{
    public ResetPasswordView(ViewModels.ResetPasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ResetPasswordViewModel vm)
        {
            vm.Password = ((PasswordBox)sender).Password;
        }
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ResetPasswordViewModel vm)
        {
            vm.PasswordConfirmation = ((PasswordBox)sender).Password;
        }
    }
}
