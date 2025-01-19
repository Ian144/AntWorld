using System.Windows.Controls;

namespace AntWorldGui;

public partial class AntWorldDisplayControl : UserControl
{
    public AntWorldDisplayControl()
    {
        InitializeComponent();
    }

    public void Redraw()
    {
        antWorldRender.InvalidateVisual();
    }
}