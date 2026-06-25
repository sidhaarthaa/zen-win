using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ZenWin.Core;
using ZenWin.Models;
using ZenWin.Services;

namespace ZenWin.UI.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ZenModeController _controller;
    private readonly SettingsStore _settingsStore;
    private ZenWinSettings _settings = new();

    public MainViewModel(ZenModeController controller, SettingsStore settingsStore)
    {
        _controller = controller;
        _settingsStore = settingsStore;
        ToggleZenCommand = new RelayCommand(() =>
        {
            _controller.Toggle(Settings);
            OnPropertyChanged(nameof(IsZenActive));
        });
        SaveCommand = new RelayCommand(async () => await _settingsStore.SaveAsync(Settings));
        _controller.ActiveChanged += (_, _) => OnPropertyChanged(nameof(IsZenActive));
    }

    public ZenWinSettings Settings
    {
        get => _settings;
        set { _settings = value; OnPropertyChanged(); }
    }

    public bool IsZenActive => _controller.IsActive;
    public ICommand ToggleZenCommand { get; }
    public ICommand SaveCommand { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class RelayCommand(Action execute) : ICommand
{
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => execute();
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }
}
