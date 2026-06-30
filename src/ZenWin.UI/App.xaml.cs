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
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _services = ConfigureServices();
        _mainWindow = _services.GetRequiredService<MainWindow>();
        ConfigureTray();
        ConfigureHotkeys();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());
        services.AddSingleton<WindowManager>();
        services.AddSingleton<ZenModeController>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
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
        _trayIcon.ContextMenuStrip.Items.Add("Toggle Frameless", null, (_, _) => viewModel.ToggleZenCommand.Execute(null));
        _trayIcon.ContextMenuStrip.Items.Add("Settings", null, (_, _) => ShowSettings());
        _trayIcon.ContextMenuStrip.Items.Add("About", null, (_, _) => System.Windows.MessageBox.Show("ZenWin\nFrameless windows for Windows", "About ZenWin"));
        _trayIcon.ContextMenuStrip.Items.Add("Exit", null, (_, _) => ExitApp());
        _trayIcon.DoubleClick += (_, _) => ShowSettings();
    }

    private void ConfigureHotkeys()
    {
        var services = _services!;
        var viewModel = services.GetRequiredService<MainViewModel>();
        _hotkeyWindow = new HotkeyWindow();
        _hotkeyWindow.HotkeyPressed += (_, id) =>
        {
            if (id == 1) viewModel.ToggleZenCommand.Execute(null);
        };
        _hotkeyWindow.RegisterDefaults();
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
