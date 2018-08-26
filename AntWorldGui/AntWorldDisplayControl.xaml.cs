using System.Windows.Controls;
using System.Collections.Generic;


namespace AntWorldGui
{

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
}
