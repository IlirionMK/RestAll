using System.Windows;
using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App.Views;

public partial class OrdersView : Window
{
    public OrdersView(OrdersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
