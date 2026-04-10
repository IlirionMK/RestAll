using System.Windows;
using RestAll.Desktop.App.ViewModels;

namespace RestAll.Desktop.App.Views;

public partial class KitchenView : Window
{
    public KitchenView(KitchenViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
