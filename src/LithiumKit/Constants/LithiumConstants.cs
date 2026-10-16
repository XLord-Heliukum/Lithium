namespace LithiumKit.Constants;

/// <summary>
/// Project-wide constants. Lithium is a cross-platform reimplementation of
/// Whisky (Whisky-App/Whisky), originally a macOS-only SwiftUI Wine wrapper.
/// </summary>
public static class LithiumConstants
{
    public const string AppName = "Lithium";
    public const string AppNameFull = "Lithium — A modern Wine wrapper for Anywhere";
    public const string Tagline = "A modern Wine wrapper for Anywhere, built with .NET and Avalonia.";

    // Provenance: Lithium is derived from Whisky (GPL-3.0).
    public const string UpstreamProject = "Whisky-App/Whisky";
    public const string UpstreamUrl = "https://github.com/Whisky-App/Whisky";
    public const string RepositoryUrl = "https://github.com/XLord-Heliukum/Lithium";

    // On-disk layout inside a bottle directory.
    public const string ConfigFileName = "bottle.json";
    public const string ProgramsFileName = "bottle.programs.json";
}
