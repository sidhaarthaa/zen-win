using System.Drawing;
using System.Windows;

namespace ZenWin.UI;

public partial class DimOverlayWindow : Window
{
    public DimOverlayWindow(Rectangle bounds, double opacity)
    {
        InitializeComponent();
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        Opacity = Math.Clamp(1.0 - opacity, 0.15, 0.95);
    }
}
