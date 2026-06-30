using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ZenWin.Core;

namespace ZenWin.UI.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ZenModeController _controller;

    public MainViewModel(ZenModeController controller)
    {
        _controller = controller;
        ToggleZenCommand = new RelayCommand(() =>
        {
            _controller.Toggle();
            OnPropertyChanged(nameof(IsZenActive));
            OnPropertyChanged(nameof(ToggleButtonText));
            OnPropertyChanged(nameof(StatusMessage));
        });
        _controller.ActiveChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsZenActive));
            OnPropertyChanged(nameof(ToggleButtonText));
            OnPropertyChanged(nameof(StatusMessage));
        };
    }

    public bool IsZenActive => _controller.IsActive;
    public string ToggleButtonText => IsZenActive ? "Restore Window Frame" : "Make Active Window Frameless";
    public string StatusMessage => _controller.StatusMessage;
    public ICommand ToggleZenCommand { get; }
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
