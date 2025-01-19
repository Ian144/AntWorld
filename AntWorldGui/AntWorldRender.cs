using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AntWorldGui;

public class AntWorldRender : FrameworkElement
{
    public static readonly DependencyProperty BackgroundProperty =
        Panel.BackgroundProperty.AddOwner(typeof(AntWorldRender));

    public static readonly DependencyProperty BrushesProperty = DependencyProperty.Register(
        nameof(Brushes),
        typeof(Brush[]),
        typeof(AntWorldRender),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable<VisibleEntity>),
        typeof(AntWorldRender));

    public Brush Background
    {
        set => SetValue(BackgroundProperty, value);
        get => (Brush)GetValue(BackgroundProperty);
    }

    public Brush[] Brushes
    {
        set => SetValue(BrushesProperty, value);
        get => (Brush[])GetValue(BrushesProperty);
    }

    public IEnumerable<VisibleEntity> ItemsSource
    {
        set => SetValue(ItemsSourceProperty, value);
        get => (IEnumerable<VisibleEntity>)GetValue(ItemsSourceProperty);
    }

    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Background, null, new Rect(RenderSize));

        if (ItemsSource == null || Brushes == null)
            return;

        foreach (var dp in ItemsSource)
        {
            var brushIndex = (int)dp.Type;

            var point = new Point(dp.VariableX, dp.VariableY);
            var brush = Brushes[brushIndex];
            dc.DrawEllipse(brush, null, point, dp.Radius, dp.Radius);
        }
    }
}