using System.Windows;

namespace NorthHorizon.Samples.InpcTemplate.Tests.Live
{
    /// <summary>
    /// Interaction logic for CoerceTestWindow.xaml
    /// </summary>
    [LiveTest("Property Coercion")]
    public partial class CoerceTestWindow : Window
    {
        public CoerceTestWindow()
        {
            DataContext = new CoerceTestViewModel();

            InitializeComponent();
        }
    }
}
