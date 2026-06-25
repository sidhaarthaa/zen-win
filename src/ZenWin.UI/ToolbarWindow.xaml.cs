using System.Windows;
using System.Windows.Threading;
using ZenWin.UI.ViewModels;

namespace ZenWin.UI;

public partial class ToolbarWindow : Window
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private TimeSpan _remaining = TimeSpan.FromMinutes(25);

    public static readonly DependencyProperty ClockTextProperty = DependencyProperty.Register(nameof(ClockText), typeof(string), typeof(ToolbarWindow));
    public static readonly DependencyProperty BatteryTextProperty = DependencyProperty.Register(nameof(BatteryText), typeof(string), typeof(ToolbarWindow));
    public static readonly DependencyProperty TimerTextProperty = DependencyProperty.Register(nameof(TimerText), typeof(string), typeof(ToolbarWindow));

    public string ClockText { get => (string)GetValue(ClockTextProperty); set => SetValue(ClockTextProperty, value); }
    public string BatteryText { get => (string)GetValue(BatteryTextProperty); set => SetValue(BatteryTextProperty, value); }
    public string TimerText { get => (string)GetValue(TimerTextProperty); set => SetValue(TimerTextProperty, value); }

    public ToolbarWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => PositionAtTop();
        MouseLeave += (_, _) => Hide();
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
        Tick();
    }

    private void Tick()
    {
        ClockText = DateTime.Now.ToString("HH:mm");
        BatteryText = $"{System.Windows.Forms.SystemInformation.PowerStatus.BatteryLifePercent:P0}";
        if (_remaining > TimeSpan.Zero)
            _remaining -= TimeSpan.FromSeconds(1);
        TimerText = _remaining.ToString(@"mm\:ss");
    }

    private void PositionAtTop()
    {
        Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
        Top = 10;
    }
}
