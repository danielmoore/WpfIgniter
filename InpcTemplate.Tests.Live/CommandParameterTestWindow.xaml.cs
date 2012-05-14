using System.Windows;

namespace NorthHorizon.Samples.InpcTemplate.Tests.Live
{
    [LiveTest("Command Parameter")]
    public partial class CommandParameterTestWindow : Window
    {
        public CommandParameterTestWindow()
        {
            DataContext = new CommandParameterTestViewModel();

            InitializeComponent();
        }
    }
}
