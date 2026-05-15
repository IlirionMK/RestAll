using System.Windows;

namespace RestAll.Desktop.App.Views;

public partial class ForgotPasswordView : Window
{
    public ForgotPasswordView(ViewModels.ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
