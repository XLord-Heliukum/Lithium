using Avalonia.Controls;
using Avalonia.Interactivity;
using Lithium.Desktop.ViewModels;

namespace Lithium.Desktop.Views;

public partial class NewBottleDialog : Window
{
    public bool? DialogResult { get; private set; }

    public NewBottleDialog()
    {
        InitializeComponent();
        CancelButton.Click += (_, _) =>
        {
            DialogResult = false;
            Close();
        };
        CreateButton.Click += (_, _) =>
        {
            if (DataContext is NewBottleViewModel vm && vm.SelectedEngine is not null && !string.IsNullOrWhiteSpace(vm.Name))
            {
                DialogResult = true;
                Close();
            }
        };
    }
}
