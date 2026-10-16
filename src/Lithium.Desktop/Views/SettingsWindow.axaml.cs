using Avalonia.Controls;
using Avalonia.Interactivity;
using Lithium.Desktop.ViewModels;

namespace Lithium.Desktop.Views;

public partial class SettingsWindow : Window
{
    public bool? DialogResult { get; private set; }

    public SettingsWindow()
    {
        InitializeComponent();
        CancelButton.Click += (_, _) =>
        {
            DialogResult = false;
            Close();
        };
        SaveButton.Click += (_, _) =>
        {
            if (DataContext is SettingsViewModel vm)
                vm.Save();
            DialogResult = true;
            Close();
        };
    }
}
