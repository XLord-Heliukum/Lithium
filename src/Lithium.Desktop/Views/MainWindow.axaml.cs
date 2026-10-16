using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Lithium.Desktop.ViewModels;
using LithiumKit.Models;

namespace Lithium.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        NewButton.Click += (_, _) => OpenNewBottle();
        SettingsButton.Click += (_, _) => OpenSettings();
        AboutButton.Click += (_, _) => OpenAbout();
        AddProgramButton.Click += AddProgramClicked;
        RemoveProgramButton.Click += RemoveProgramClicked;
        DeleteBottleButton.Click += DeleteBottleClicked;
        ProgramList.DoubleTapped += ProgramListDoubleTapped;
    }

    private MainViewModel? Vm => DataContext as MainViewModel;

    private async void OpenNewBottle()
    {
        if (Vm is null) return;
        var dlg = new NewBottleDialog { DataContext = new NewBottleViewModel(Vm.Engines) };
        await dlg.ShowDialog(this);
        if (dlg.DialogResult == true
            && dlg.DataContext is NewBottleViewModel vm
            && vm.SelectedEngine is not null
            && !string.IsNullOrWhiteSpace(vm.Name))
        {
            Vm.CreateBottle(vm.Name.Trim(), vm.SelectedEngine);
        }
    }

    private async void OpenSettings()
    {
        var dlg = new SettingsWindow { DataContext = new SettingsViewModel() };
        await dlg.ShowDialog(this);
    }

    private async void OpenAbout()
    {
        var dlg = new AboutWindow();
        await dlg.ShowDialog(this);
    }

    private async void AddProgramClicked(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedBottle is null) return;
        var dlg = new AddProgramDialog();
        await dlg.ShowDialog(this);
        if (dlg.DialogResult == true && !string.IsNullOrWhiteSpace(dlg.ExecutablePath))
            Vm.AddProgramTo(Vm.SelectedBottle, dlg.ExecutablePath, dlg.DisplayName);
    }

    private void RemoveProgramClicked(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedBottle is { } bottle && ProgramList.SelectedItem is BottleProgram p)
            Vm.RemoveProgramFrom(bottle, p);
    }

    private void DeleteBottleClicked(object? sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedBottle is { } bottle)
            Vm.DeleteBottle(bottle);
    }

    private void ProgramListDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (ProgramList.SelectedItem is BottleProgram p)
            Vm?.RunProgramCommand.Execute(p);
    }
}
