using System.Windows;
using System.Windows.Controls;
using RestAll.Desktop.App.ViewModels;
using RestAll.Desktop.Core.Tables;

namespace RestAll.Desktop.App.Views;

public partial class TablesView : Window
{
    public TablesView(TablesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void StatusComboBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox comboBox || comboBox.DataContext is not Table table)
        {
            return;
        }

        comboBox.SelectedIndex = table.Status switch
        {
            TableStatus.Available => 0,
            TableStatus.Occupied => 1,
            TableStatus.Reserved => 2,
            TableStatus.Cleaning => 3,
            _ => -1
        };
    }

    private async void StatusComboBox_DropDownClosed(object sender, EventArgs e)
    {
        try
        {
            if (sender is not ComboBox comboBox || comboBox.DataContext is not Table table || DataContext is not TablesViewModel viewModel)
            {
                return;
            }

            if (comboBox.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not string tag || !int.TryParse(tag, out var statusValue))
            {
                return;
            }

            var status = statusValue switch
            {
                0 => TableStatus.Available,
                1 => TableStatus.Occupied,
                2 => TableStatus.Reserved,
                3 => TableStatus.Cleaning,
                _ => table.Status
            };

            if (status == table.Status)
            {
                return;
            }

            await viewModel.UpdateTableStatusCommand.ExecuteAsync(table.Id, status);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating table status: {ex.Message}");
        }
    }
}
