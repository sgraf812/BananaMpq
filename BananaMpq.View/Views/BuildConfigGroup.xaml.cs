using System.Windows;
using System.Windows.Controls;

namespace BananaMpq.View.Views
{
    /// <summary>
    /// Interaktionslogik für BuildConfigGroup.xaml
    /// </summary>
    public partial class BuildConfigGroup : UserControl
    {
        public BuildConfigGroup()
        {
            InitializeComponent();
        }

        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add
            {
                _value.ValueChanged += value;
            } 
            remove
            {
                _value.ValueChanged -= value;
            }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(BuildConfigGroup), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(double), typeof(BuildConfigGroup), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(BuildConfigGroup), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register("CurrentValue", typeof(double), typeof(BuildConfigGroup), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(BuildConfigGroup), new PropertyMetadata(default(string)));

        public string LabelText { get { return (string)GetValue(LabelTextProperty); } set { SetValue(LabelTextProperty, value); } }
        public double MaxValue { get { return (double)GetValue(MaxValueProperty); } set { SetValue(MaxValueProperty, value); } }
        public double MinValue { get { return (double)GetValue(MinValueProperty); } set { SetValue(MinValueProperty, value); } }
        public double CurrentValue { get { return (double)GetValue(CurrentValueProperty); } set { SetValue(CurrentValueProperty, value); } }
        public double Step { get { return (double)GetValue(StepProperty); } set { SetValue(StepProperty, value); } }
    }
}
