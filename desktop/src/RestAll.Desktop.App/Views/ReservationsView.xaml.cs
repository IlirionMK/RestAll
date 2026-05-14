using System.Windows;
using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App.Views;

public partial class ReservationsView : Window
{
    private readonly ReservationsViewModel _viewModel;

    public ReservationsView(ReservationsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
    }
}
