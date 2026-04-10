using System.Windows;
using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App.Views;

public partial class MenuView : Window
{
    public MenuView(MenuViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
