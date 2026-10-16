using CommunityToolkit.Mvvm.ComponentModel;
using LithiumKit.Services;

namespace Lithium.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsManager _settings = new();

    [ObservableProperty] private string? _preferredWinePath;
    [ObservableProperty] private bool _checkForUpdates;

    public SettingsViewModel()
    {
        PreferredWinePath = _settings.Current.PreferredWinePath;
        CheckForUpdates = _settings.Current.CheckForUpdates;
    }

    public void Save()
    {
        _settings.Current.PreferredWinePath = PreferredWinePath;
        _settings.Current.CheckForUpdates = CheckForUpdates;
        _settings.Save();
    }
}
