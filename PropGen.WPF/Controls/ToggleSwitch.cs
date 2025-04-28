using System.Windows.Controls.Primitives;
using System.Windows;

namespace PropGen.WPF.Controls
{
    public class ToggleSwitch : ToggleButton
    {
        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }
    }
}
