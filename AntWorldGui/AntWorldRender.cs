using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;



namespace AntWorldGui
{

    public class AntWorldRender : FrameworkElement
    {
        public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(AntWorldRender));

        public Brush Background
        {
            set { SetValue(BackgroundProperty, value); }
            get { return (Brush)GetValue(BackgroundProperty); }
        }


        public static readonly DependencyProperty BrushesProperty = DependencyProperty.Register(
                                                                        "Brushes",
                                                                        typeof(Brush[]),
                                                                        typeof(AntWorldRender),
                                                                        new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.AffectsRender) );

        public Brush[] Brushes
        {
            set { SetValue(BrushesProperty, value); }
            get { return (Brush[])GetValue(BrushesProperty); }
        }


        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
                                                                                "ItemsSource",
                                                                                typeof(IEnumerable<VisibleEntity>),
                                                                                typeof(AntWorldRender));


        public IEnumerable<VisibleEntity> ItemsSource
        {
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
            get
            {
                return (IEnumerable<VisibleEntity>)GetValue(ItemsSourceProperty);
            }
        }



        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle( Background, null, new Rect(RenderSize) );

            if (ItemsSource == null || Brushes == null)
                return;


            foreach (var dp in ItemsSource)
            {
                int brushIndex = (int)dp.Type;

                var point = new Point(dp.VariableX, dp.VariableY);
                var brush = this.Brushes[brushIndex];
                dc.DrawEllipse(brush, null, point, dp.Radius, dp.Radius);
            }
        }
    }
}
