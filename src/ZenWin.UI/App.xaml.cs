using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZenWin.Core;
using ZenWin.Services;
using ZenWin.UI.ViewModels;
using Forms = System.Windows.Forms;

namespace ZenWin.UI;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;
    private Forms.NotifyIcon? _trayIcon;
    private HotkeyWindow? _hotkeyWindow;
    private ToolbarWindow? _toolbar;
    private MainWindow? _mainWindow;
    private DimOverlayManager? _dimOverlay;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _services = ConfigureServices();
        var settingsStore = _services.GetRequiredService<SettingsStore>();
        var settings = await settingsStore.LoadAsync();
        _services.GetRequiredService<MainViewModel>().Settings = settings;

        _mainWindow = _services.GetRequiredService<MainWindow>();
        _toolbar = _services.GetRequiredService<ToolbarWindow>();
        _dimOverlay = _services.GetRequiredService<DimOverlayManager>();
        ConfigureTray();
        ConfigureHotkeys();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());
        services.AddSingleton<WindowManager>();
        services.AddSingleton<TaskbarManager>();
        services.AddSingleton<DesktopManager>();
        services.AddSingleton<CursorManager>();
        services.AddSingleton<WallpaperManager>();
        services.AddSingleton<NotificationManager>();
        services.AddSingleton<AnimationManager>();
        services.AddSingleton<StartupManager>();
        services.AddSingleton<AudioManager>();
        services.AddSingleton<ProfileManager>();
        services.AddSingleton<SettingsStore>();
        services.AddSingleton<ZenModeController>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DimOverlayManager>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ToolbarWindow>();
        return services.BuildServiceProvider();
    }

    private void ConfigureTray()
    {
        var viewModel = _services!.GetRequiredService<MainViewModel>();
        _trayIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "ZenWin",
            Visible = true,
            ContextMenuStrip = new Forms.ContextMenuStrip()
        };
        _trayIcon.ContextMenuStrip.Items.Add("Toggle Zen", null, (_, _) => viewModel.ToggleZenCommand.Execute(null));
        _trayIcon.ContextMenuStrip.Items.Add("Settings", null, (_, _) => ShowSettings());
        _trayIcon.ContextMenuStrip.Items.Add("About", null, (_, _) => System.Windows.MessageBox.Show("ZenWin 0.1.0\nUniversal Zen Mode for Windows", "About ZenWin"));
        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (_, _) => ExitApp());
        _trayIcon.DoubleClick += (_, _) => ShowSettings();
    }

    private void ConfigureHotkeys()
    {
        var viewModel = _services!.GetRequiredService<MainViewModel>();
        _hotkeyWindow = new HotkeyWindow();
        _hotkeyWindow.HotkeyPressed += (_, id) =>
        {
            if (id == 1) viewModel.ToggleZenCommand.Execute(null);
            if (id == 2) ToggleToolbar();
        };
        _hotkeyWindow.RegisterDefaults();
        _services.GetRequiredService<ZenModeController>().ActiveChanged += (_, active) =>
        {
            if (active)
            {
                _dimOverlay!.ShowForActiveWindow();
                _toolbar!.Show();
            }
            else
            {
                _toolbar!.Hide();
                _dimOverlay!.Hide();
            }
        };
    }

    private void ToggleToolbar()
    {
        if (_toolbar is null)
            return;
        if (_toolbar.IsVisible) _toolbar.Hide();
        else _toolbar.Show();
    }

    private void ShowSettings()
    {
        _mainWindow ??= _services!.GetRequiredService<MainWindow>();
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void ExitApp()
    {
        _services!.GetRequiredService<ZenModeController>().Exit();
        _trayIcon?.Dispose();
        _hotkeyWindow?.Dispose();
        Shutdown();
    }
}
