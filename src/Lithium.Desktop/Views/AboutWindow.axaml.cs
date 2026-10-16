using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lithium.Desktop.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        CloseButton.Click += (_, _) => Close();
    }
}
