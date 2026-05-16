using System.Windows;

namespace RestAll.Desktop.App.Views;

public partial class MenuManagementView : Window
{
    public MenuManagementView(ViewModels.MenuManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
