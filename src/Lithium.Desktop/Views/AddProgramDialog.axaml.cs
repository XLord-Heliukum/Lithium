using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lithium.Desktop.Views;

public partial class AddProgramDialog : Window
{
    public bool? DialogResult { get; private set; }

    public string? ExecutablePath => PathBox.Text;
    public string? DisplayName => string.IsNullOrWhiteSpace(NameBox.Text) ? null : NameBox.Text;

    public AddProgramDialog()
    {
        InitializeComponent();
        CancelButton.Click += (_, _) =>
        {
            DialogResult = false;
            Close();
        };
        AddButton.Click += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(PathBox.Text))
            {
                DialogResult = true;
                Close();
            }
        };
    }
}
