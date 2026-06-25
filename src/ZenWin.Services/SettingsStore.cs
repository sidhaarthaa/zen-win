using System.Text.Json;
using ZenWin.Models;

namespace ZenWin.Services;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public string SettingsPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZenWin", "settings.json");

    public async Task<ZenWinSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(SettingsPath))
            return new ZenWinSettings();

        await using var stream = File.OpenRead(SettingsPath);
        return await JsonSerializer.DeserializeAsync<ZenWinSettings>(stream, Options, cancellationToken) ?? new ZenWinSettings();
    }

    public async Task SaveAsync(ZenWinSettings settings, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        await using var stream = File.Create(SettingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, Options, cancellationToken);
    }
}
